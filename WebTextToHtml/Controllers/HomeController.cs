// Copyright Stefan Nychka, BSD 3-Clause license, LICENSE.txt
using System;
using System.Collections.Generic;
using System.Data.Entity.Core;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security.AntiXss;
using System.Web.UI;
using Text2Html;
using WebTextToHtml.Models;

namespace WebTextToHtml.Controllers
{
    public class HomeController : Controller
    {
        readonly static string ContentForViewTDKey = "Content";
        readonly static string OriginalContentTDKey = "OriginalContent";
        readonly static string SavedMessageTDKey = "SavedMessage";
        const string SavedMessage = "Your content has been saved!";
        const string ConvertButtonName = "ConvertText";

        // max. size of upload in bytes
        const int MaxContentSize = 102400;

        Text2HtmlDb context = new Text2HtmlDb();

        public ActionResult Index()
        {
            PrepareIndexView();
            return View();
        }
        
        [HttpPost]
        [TargetAction(Button = ConvertButtonName)]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Index(string unEncodedText)
        {
            if (System.Text.Encoding.UTF32.GetByteCount(unEncodedText) > MaxContentSize)
            {
                ModelState.AddModelError("", "Pasted text too long.  Max size " + MaxContentSize/1024 + "KB");
                PrepareIndexView();
                return View();
            }
            if (!PrepareText(unEncodedText, "Textarea was empty", "Text did not correspond to required format"))
            {
                PrepareIndexView();
                return View();
            }
            return RedirectToAction("Display");
        }

        // likely fix-up redundant error checking
        [HttpPost]
        [TargetAction(Button = "Upload")]
        [ValidateInput(false)]
        [ValidateAntiForgeryToken]
        public ActionResult Index(HttpPostedFileBase textFile)
        {
            if (textFile == null)
            {
                ModelState.AddModelError("", "No file selected");
            }
            else if (textFile.ContentLength == 0)
            {
                ModelState.AddModelError("", "Empty file chosen");
            }
            else if (textFile.ContentLength > MaxContentSize)
            {
                ModelState.AddModelError("", "Uploaded file too large.  Max size " + MaxContentSize / 1024 + "KB");
            }
            else if (textFile.ContentType != "text/plain")
            {
                ModelState.AddModelError("", "Uploaded file must be a text file (*.txt)");
            }
            if (!ModelState.IsValid)
            {
                PrepareIndexView();
                return View();
            }

            string unEncodedText = GetFileContents(textFile);
            if (!PrepareText(unEncodedText,
                "No file chosen for upload, or file is empty",
                "File did not correspond to required format"))
            {
                PrepareIndexView();
                return View();
            }
            return RedirectToAction("Display");
        }


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Retrieve()
        {
            try
            {
                int? id = this.context.UserProfiles
                            .Where(u => u.UserName == User.Identity.Name)
                            .Select(u => u.UserId)
                            .SingleOrDefault();
                string contents;
                if (!id.HasValue)
                {
                    contents = null;
                }
                else
                {
                    contents = this.context.Files
                        .Where(f => f.UserId == id)
                        .Select(f => f.Contents)
                        .SingleOrDefault();
                }
                // better to deal with error more consistently ... later
                TempData[OriginalContentTDKey] = contents ?? "NO STORED FILE WAS FOUND.";
            }
            catch (InvalidOperationException)
            {
                ModelState.AddModelError("", "Error connecting to database");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An unexpected error");
            }

            if (!ModelState.IsValid)
            {
                PrepareIndexView();
                return View("Index");
            }
            return RedirectToAction("Index");
        }


        public ActionResult Display()
        {
            PrepareDisplayView();
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        // http://weblog.west-wind.com/posts/2012/May/30/Rendering-ASPNET-MVC-Views-to-String
        public ActionResult Download()
        {
            ViewBag.Content = TempData[ContentForViewTDKey];
            TempData.Keep(ContentForViewTDKey);

            var templateViewResult = ViewEngines.Engines.FindPartialView(
                this.ControllerContext, "~/Views/Home/_DownloadTemplate.cshtml");
            var templateView = templateViewResult.View;
            string fileContents = null;

            using (StringWriter writer = new StringWriter())
            {
                var templateViewContext = new ViewContext(
                    this.ControllerContext,
                    templateView,
                    this.ControllerContext.Controller.ViewData,
                    this.ControllerContext.Controller.TempData,
                    writer);
                templateView.Render(templateViewContext, writer);
                fileContents = writer.ToString();
            }

            return File(System.Text.Encoding.UTF8.GetBytes(fileContents), "text/html", "convertedText.html");
        }


        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public ActionResult Save()
        {

            TempData.Keep(OriginalContentTDKey);
            TempData.Keep(ContentForViewTDKey);

            try
            {

                int? userId = this.context.UserProfiles
                    .Where(u => u.UserName == User.Identity.Name)
                    .Select(u => u.UserId)
                    .SingleOrDefault();
                if (userId == null)
                {
                    ModelState.AddModelError("", "No such user " + User.Identity.Name + " found");
                    PrepareDisplayView();
                    return View("Display");
                }

                FileContents contentsToSave = this.context.Files.SingleOrDefault(u => u.UserId == userId.Value);

                bool exists = true;
                // needs to be inserted as opposed to updated
                if (contentsToSave == null)
                {
                    exists = false;
                    contentsToSave = new FileContents();
                    contentsToSave.UserId = userId.Value;
                }

                contentsToSave.DateModified = DateTime.Now;
                contentsToSave.Contents = TempData[OriginalContentTDKey] as string;

                if (exists)
                {
                    this.context.Entry(contentsToSave).State = System.Data.Entity.EntityState.Modified;
                }
                else
                {
                    this.context.Files.Add(contentsToSave);
                }
                this.context.SaveChanges();
            }
            catch (EntityCommandExecutionException)
            {
                ModelState.AddModelError("", "Error saving text");
            }
            catch (EntitySqlException)
            {
                ModelState.AddModelError("", "An unexpected general database error");
            }
            catch (SqlException)
            {
                ModelState.AddModelError("", "An unexpected database error");
            }
            catch (Exception)
            {
                ModelState.AddModelError("", "An unexpected error");
            }

            if (!ModelState.IsValid)
            {
                PrepareDisplayView();
                return View("Display");
            }

            TempData[SavedMessageTDKey] = SavedMessage;

            return RedirectToAction("Display");
        }

        public ActionResult About()
        {
            return View();
        }

        public ActionResult Copyright()
        {
            return View();
        }

        private void PrepareIndexView()
        {
            ViewBag.ConvertButtonName = ConvertButtonName;
            object contents = TempData[OriginalContentTDKey];
            if (contents != null)
            {
                ViewBag.Content = TempData[OriginalContentTDKey];
            }
        }

        private void PrepareDisplayView()
        {
            ViewBag.Content = TempData[ContentForViewTDKey];
            ViewBag.SavedMessage = TempData[SavedMessageTDKey];
            TempData.Keep(OriginalContentTDKey);
            TempData.Keep(ContentForViewTDKey);
        }

        // escapes and converts unEncodedText, putting it in TempData[ContentForViewTDKey].
        // adds errorMessage and returns false if unEncodedText is null
        // not quite focused enough.
        private bool PrepareText(string unEncodedText, string nullErrorMessage, string formattingErrorMessage)
        {
            TempData.Keep(OriginalContentTDKey);
            if (unEncodedText == null)
            {
                ModelState.AddModelError("", nullErrorMessage);
                return false;
            }
            TempData[OriginalContentTDKey] = unEncodedText;
            // likely not portable.  may be better to use stringbuilder
            string text = AntiXssEncoder.HtmlEncode(unEncodedText, false)
                .Replace("&#10;", "\n")
                .Replace("&#13;", "\r");

            string convertedText;
            try
            {
                convertedText = ConvertText(text);
            }
            // must improve this.
            catch (Exception)
            {
                ModelState.AddModelError("", formattingErrorMessage);
                return false;
            }
            TempData[ContentForViewTDKey] = convertedText
                ?? "Text not converted; possible formatting error";
            return true;
        }

        // converts text to html
        private string ConvertText(string text)
        {
            string convertedString = null;
            using (TextReader reader = TextToHtml.OpenReaderFromString(text))
            {
                TextToHtml converter = new TextToHtml();
                convertedString = converter.ReadAndConvert(reader);
            }
            return convertedString;
        }

        // returns null if no file or file contents are empty
        private string GetFileContents(HttpPostedFileBase textFile)
        {
            if (textFile == null)
            {
                return null;
            }
            int fileLength = textFile.ContentLength;
            string fileContents = null;
            if (fileLength > 0)
            {
                byte[] byteContents = new byte[fileLength];
                textFile.InputStream.Read(byteContents, 0, fileLength);
                fileContents = System.Text.Encoding.Default.GetString(byteContents);
            }
            return fileContents;
        }

    }
}
