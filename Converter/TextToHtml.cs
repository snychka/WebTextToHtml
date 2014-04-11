// Copyright Stefan Nychka, BSD 3-Clause license, LICENSE.txt
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.IO;
using System.Web.UI;
using System.Web.Security.AntiXss;

namespace Text2Html
{
    public class TextToHtml
    {
        // DEFAULT_BULLET_PREFIX + NUMBER_SPACES_AFTER_BULLET_PREFIX should be the same as
        // DEFAULT_SPACES_TAB_LENGTH
        private const string DEFAULT_BULLET_PREFIX = "-";
        private const int NUMBER_SPACES_AFTER_BULLET_PREFIX = 1;
        private const string LINK_PREFIX = @"http://";
        private const char BLANK = ' ';
        private const int DEFAULT_SPACES_TAB_LENGTH = 2;
        private const string TAB = "  ";

        // may still need to do this to look at protected tag vars.
        //// solely to use OutputTabs ... documented way caused stackoverflow
        //class HtmlWriter : HtmlTextWriter
        //{
        //    private string TabChar;

        //    // passing in "" to ensure default tab char isn't used
        //    public HtmlWriter(TextWriter t, String tabChar)
        //        : base(t, tabChar)
        //    {
        //        this.TabChar = tabChar;
        //    }
     
        //    public void WriteOutTabs()
        //    {
        //        //// Output the DefaultTabString for the number 
        //        //// of times specified in the Indent property. 
        //        //for (int i = 0; i < Indent; i++)
        //        //    base.Write(TabChar); // this kept on triggering the overriden OutputTabs
        //    }
        //}

        private string SpacesTabValue;

        public string SpacesTab
        {
            get { return SpacesTabValue; }
            set { SpacesTabValue = value; }
        }

        public TextToHtml(int spacesTabLength = DEFAULT_SPACES_TAB_LENGTH)
        {
        }

        public static TextReader OpenReaderFromFile(string inputTextFilePath)
        {
            return new StreamReader(File.OpenRead(inputTextFilePath));
        }

        public static TextWriter OpenWriterToFile(string outputHtmlFilePath)
        {
            return new StreamWriter(File.OpenWrite(outputHtmlFilePath));

        }

        public static TextReader OpenReaderFromString(string inputText)
        {
            return new StringReader(inputText);
        }

        public string ReadAndConvert(TextReader reader)
        {
            StringWriter htmlTextOutput = new StringWriter();
            using (HtmlTextWriter htmlBuilder = new HtmlTextWriter(htmlTextOutput, TAB))
            {
                // consider catching System.InvalidOperationException whose Source is System.Web
                // indicates malformed text
                ReadAndConvert(reader, htmlBuilder);
            }
            return htmlTextOutput.ToString();
        }

        // does not handle text in an li after an inner ul
        private void HandleLists(
            HtmlTextWriter output,
            string line, string trimmedLine, int lineNumber,
            bool inBullet)
        {
            
            int indexOfBullet = line.IndexOf(DEFAULT_BULLET_PREFIX);

            if (indexOfBullet % DEFAULT_SPACES_TAB_LENGTH != 0)
            {
                throw new IncorrectTabLengthException(
                    "line " + lineNumber + ":  incorrect number of spaces used");
            }

            int currentLevel = indexOfBullet / DEFAULT_SPACES_TAB_LENGTH + 1;

            if (!inBullet)
            {
                output.RenderBeginTag(HtmlTextWriterTag.Ul);
            }
            else if (currentLevel > output.Indent) // start sub-list
            {
                if ((currentLevel - 1) > output.Indent)
                {
                    throw new IncorrectTabIncreaseException(
                        "line " + lineNumber + ":  increased indent by more than one tab");
                }
                output.RenderBeginTag(HtmlTextWriterTag.Ul);
            }
            else if (currentLevel < output.Indent) // end sub-list
            {
                this.CloseLists(output, currentLevel);
            }
            else // same level
            {
                output.RenderEndTag(); // li
                output.WriteLine();
            }

            output.RenderBeginTag(HtmlTextWriterTag.Li);
            output.WriteLine();
            string content = trimmedLine.Substring(NUMBER_SPACES_AFTER_BULLET_PREFIX).Trim();
            output.WriteLine(content);

        }

        private void CloseLists(HtmlTextWriter output, int targetLevel)
        {
            for (int i = output.Indent; i > targetLevel; i--)
            {
                output.RenderEndTag(); // li
                output.RenderEndTag(); // ul ... apparently, likely this puts in pre-newline
                output.WriteLine();
            }
            if (targetLevel != 0) // not at top level, so must close containing li
            {
                output.RenderEndTag();
                output.WriteLine();
            }
        }

        private void ReadAndConvert(TextReader reader, HtmlTextWriter output)
        {
            string orgLine;
            bool inPar = false;
            bool inBullet = false;
            int lineNumber = 1;


            while ((orgLine = reader.ReadLine()) != null)
            {
                string orgLineReplacedLinks = ReplaceLinks(orgLine);
                string trimmedLineReplacedLinks = orgLineReplacedLinks.Trim();

                if (trimmedLineReplacedLinks.StartsWith(DEFAULT_BULLET_PREFIX))
                {
                    if (inPar)
                    {
                        inPar = false;
                        output.RenderEndTag(); // p
                        output.WriteLine();
                    }

                    HandleLists(output, orgLineReplacedLinks, trimmedLineReplacedLinks, lineNumber, inBullet); //, out indexOfBullet); //, out sameLevel);

                    inBullet = true;

                }
                else if (trimmedLineReplacedLinks == string.Empty)
                {
                    CloseAllTags(output, inPar, inBullet);
                    inPar = false;
                    inBullet = false;
                }
                else // first line in par., or non-first line of text
                {
                // need to check for errors here, maybe ... need to be same indentation level as before

                    if (!inPar && !inBullet) // first line is a p
                    {
                        inPar = true;
                        output.RenderBeginTag(HtmlTextWriterTag.P);
                        output.WriteLine();
                    }

                    // "resetting" back to p, but no preceding blank line
                    if (inBullet && !orgLineReplacedLinks.StartsWith(BLANK.ToString()))
                    {
                        inBullet = false;
                        inPar = true;
                        CloseLists(output, 0);
                        output.RenderBeginTag(HtmlTextWriterTag.P);
                        output.WriteLine();
                    }

                    output.WriteLine(trimmedLineReplacedLinks);

                }

                lineNumber++;
            }
            CloseAllTags(output, inPar, inBullet);
        }

        private void CloseAllTags(HtmlTextWriter output, bool inPar, bool inBullet)
        {
            if (inPar)
            {
                output.RenderEndTag();
                output.WriteLine();
            }
            if (inBullet)
            {
                CloseLists(output, 0);
            }
        }


        // http://a.b.c >> <a href="http://a.b.c" target="_blank">http://a.b.c</a>
        private string ReplaceLinks(string line)
        {
            int start = 0;
            start = line.IndexOf(LINK_PREFIX, start);

            if (start < 0)
            {
                return line;
            }

            StringWriter targetHtml = new StringWriter();
            using (HtmlTextWriter output = new HtmlTextWriter(targetHtml))
            {
                int startNonLink = 0;
                string link;

                do
                {
                    string lineSegment = line.Substring(startNonLink, start - startNonLink);
                    output.Write(lineSegment);
                    int endOfLink = line.IndexOf(BLANK, start);
                    if (endOfLink < 0)
                    {
                        endOfLink = line.Length;
                    }
                    link = line.Substring(start, endOfLink - start);
                    this.CreateLink(output, link);
                    startNonLink = endOfLink;
                } while ((start = line.IndexOf(LINK_PREFIX, startNonLink)) > 0);

                if (startNonLink < line.Length)
                {
                    output.Write(line.Substring(startNonLink));
                }
            }

            return targetHtml.ToString();
        }

        // no point to url encoding as not putting a url into a url
        private void CreateLink(HtmlTextWriter output, string link)
        {
            output.AddAttribute(HtmlTextWriterAttribute.Href, link);
            output.AddAttribute(HtmlTextWriterAttribute.Target, "_blank");
            output.RenderBeginTag(HtmlTextWriterTag.A);
            output.Write(link);
            output.RenderEndTag();
        }

    }

}


//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Diagnostics;
//using System.IO;
//using System.Web.UI;
//namespace Text2Html
//{
//    public class TextToHtml
//    {
//        private const int DEFAULT_SPACES_TAB_LENGTH = 2;
//        private const string DEFAULT_BULLET_PREFIX = "-";
//        private const string LINK_PREFIX = @"http://";
//        private const string BLANK = " ";

//        private const string OPENING_PAR = "<p>";
//        private const string CLOSING_PAR = "</p>";
//        private const string OPENING_LIST = "<ul>";
//        private const string CLOSING_LIST = "</ul>";
//        private const string OPENING_BULLET = "<li>";
//        private const string CLOSING_BULLET = "</li>";
//        private const string OPENING_LINK = "<a href=\"";
//        private const string MIDDLE_LINK = "\" target=\"_blank\">";
//        private const string CLOSING_LINK = "</a>";

//        private string SpacesTabValue;

//        public string SpacesTab
//        {
//            get { return SpacesTabValue; }
//            set { SpacesTabValue = value; }
//        }

//        public TextToHtml(int spacesTabLength = DEFAULT_SPACES_TAB_LENGTH)
//        {
//        }

//        public static TextReader OpenReaderFromFile(string inputTextFilePath)
//        {
//            return new StreamReader(File.OpenRead(inputTextFilePath));
//        }

//        public static TextWriter OpenWriterToFile(string outputHtmlFilePath)
//        {
//            return new StreamWriter(File.OpenWrite(outputHtmlFilePath));

//        }

//        public static TextReader OpenReaderFromString(string inputText)
//        {
//            return new StringReader(inputText);
//        }

//        public string ReadAndConvert(TextReader reader)
//        {
//            string htmlText = null;
//            StringWriter htmlTextOutput = new StringWriter();
//            using (HtmlTextWriter htmlBuilder = new HtmlTextWriter(htmlTextOutput))
//            {
//                // consider catching System.InvalidOperationException whose Source is System.Web
//                // indicates malformed text
//                htmlText = ReadAndConvert(reader, htmlBuilder);
//            }
//            return htmlTextOutput.ToString();
//            //return htmlText;
//        }

//        private string ReadAndConvert(TextReader reader, HtmlTextWriter output)
//        {
//            StringBuilder outputOld = new StringBuilder();

//            string orgLine;
//            int previousLevel = 0, currentLevel = 0;
//            bool previousLineWasBullet = false;
//            bool previousLineWasOther = false;
//            int lineNumber = 1;

//            while ((orgLine = reader.ReadLine()) != null)
//            {
//                if (orgLine.StartsWith(LINK_PREFIX))
//                {
//                    if (lineNumber != 1)
//                    {
//                        CloseLists(
//                            output,
//                            outputOld,
//                            out previousLineWasBullet,
//                            out previousLineWasOther,
//                            out previousLevel,
//                            ref currentLevel);
//                    }
//                    output.RenderBeginTag(HtmlTextWriterTag.P);
//                    outputOld.AppendLine(OPENING_PAR + ReplaceLinks(orgLine, output) + CLOSING_PAR);
//                    output.RenderEndTag();

//                    // a bit of a presumptious kludge
//                    output.Write('\n'); // ideally replace with entity that reps. a newline
//                    output.RenderBeginTag(HtmlTextWriterTag.Ul);
//                    outputOld.AppendLine(OPENING_LIST);
//                }
//                else if (orgLine.Trim().StartsWith(DEFAULT_BULLET_PREFIX))
//                {
//                    int indexOfBullet;
//                    HandleSubLists(
//                        output,
//                        outputOld,
//                        orgLine, lineNumber,
//                        previousLineWasOther, previousLineWasBullet,
//                        out indexOfBullet,
//                        out currentLevel, ref previousLevel);

//                    // should check if line contains only a -
//                    string newLine = orgLine.Substring(indexOfBullet + 1).Trim();
//                    output.Write('\n'); // ideally replace with entity that reps. a newline
//                    output.RenderBeginTag(HtmlTextWriterTag.Li);
//                    newLine = ReplaceLinks(newLine, output);

//                    previousLineWasBullet = true;
//                    previousLineWasOther = false;
//                    outputOld.AppendLine(OPENING_BULLET + newLine);
//                }
//                // doesn't allow for a bullet to have text after sub-bullets
//                else
//                {
//                    previousLineWasOther = true;
//                    previousLineWasBullet = false;
//                    outputOld.AppendLine(ReplaceLinks(orgLine, output));
//                }
//                lineNumber++;
//            }
//            CloseLists(
//                            output,
//                            outputOld,
//                            out previousLineWasBullet,
//                            out previousLineWasOther,
//                            out previousLevel,
//                            ref currentLevel);


//            return outputOld.ToString();

//        }

//        private void CloseLists(
//            HtmlTextWriter output,
//            StringBuilder outputOld,
//            out bool previousLineWasBullet, out bool previousLineWasOther,
//            out int previousLevel, ref int currentLevel)
//        {
//            previousLineWasBullet = false;
//            previousLineWasOther = false;
//            output.RenderEndTag();

//            //output.RenderEndTag();
//            try { output.RenderEndTag(); }
//            catch (Exception e) { }
//            outputOld.AppendLine(CLOSING_BULLET);
//            outputOld.AppendLine(CLOSING_LIST);
//            while (currentLevel > 0)
//            {
//                output.RenderEndTag();
//                output.RenderEndTag();
//                outputOld.AppendLine(CLOSING_BULLET);
//                outputOld.AppendLine(CLOSING_LIST);
//                currentLevel--;
//            }
//            previousLevel = 0;
//        }

//        // might be able to clean this up, as don't here need to know which end tag is being rendered,
//        // since HtmlTextWriter takes care of that
//        private void HandleSubLists(
//            HtmlTextWriter output,
//            StringBuilder oldOutput,
//            string line, int lineNumber,
//            bool previousLineWasOther, bool previousLineWasBullet,
//            out int indexOfBullet,
//            out int currentLevel, ref int previousLevel)
//        {
//            indexOfBullet = line.IndexOf(DEFAULT_BULLET_PREFIX);
//            if (indexOfBullet % DEFAULT_SPACES_TAB_LENGTH != 0)
//                throw new IncorrectTabLengthException(
//                    "line " + lineNumber + ":  incorrect number of spaces used");
//            currentLevel = indexOfBullet / DEFAULT_SPACES_TAB_LENGTH;
//            // should check to see in/decreased by a proper amount
//            if (currentLevel > previousLevel) // start sub-list
//            {
//                output.RenderBeginTag(HtmlTextWriterTag.Ul);
//                oldOutput.AppendLine(OPENING_LIST);
//            }
//            else if (currentLevel < previousLevel) // end sub-list
//            {
//                output.RenderEndTag();
//                output.RenderEndTag();
//                output.RenderEndTag();
//                oldOutput.AppendLine(CLOSING_BULLET);
//                oldOutput.AppendLine(CLOSING_LIST);
//                oldOutput.AppendLine(CLOSING_BULLET);
//            }
//            else if (previousLineWasOther || previousLineWasBullet) // same level
//            {
//                output.RenderEndTag();
//                oldOutput.AppendLine(CLOSING_BULLET);
//            }

//            previousLevel = currentLevel;

//        }

//        // http://a.b.c >> <a href="http://a.b.c" target="_blank">http://a.b.c</a>
//        private string ReplaceLinks(string line, HtmlTextWriter output)
//        {
//            string newLine = "";
//            int start = 0;
//            start = line.IndexOf(LINK_PREFIX, start);

//            if (start < 0)
//            {
//                output.Write(line);
//                return line;
//            }

//            int startNonLink = 0;
//            string link;

//            // didn't test this working on a line with multiple links
//            do
//            {
//                string lineSegment = line.Substring(startNonLink, start - startNonLink);
//                output.Write(lineSegment);
//                newLine += lineSegment;
//                int endOfLink = line.IndexOf(BLANK, start);
//                if (endOfLink < 0) // link is last thing on line
//                {
//                    link = line.Substring(start);
//                    CreateLink(output, link);
//                    newLine += OPENING_LINK + link + MIDDLE_LINK + link + CLOSING_LINK;
//                    return newLine;
//                }
//                link = line.Substring(start, endOfLink - start);
//                CreateLink(output, link);
//                newLine += OPENING_LINK + link + MIDDLE_LINK + link + CLOSING_LINK;
//                startNonLink = endOfLink;
//            } while ((start = line.IndexOf(LINK_PREFIX, startNonLink)) > 0);

//            return newLine + line.Substring(startNonLink);

//        }

//        private static void CreateLink(HtmlTextWriter output, string link)
//        {
//            output.AddAttribute(HtmlTextWriterAttribute.Href, link);
//            output.AddAttribute(HtmlTextWriterAttribute.Target, "_blank");
//            output.RenderBeginTag(HtmlTextWriterTag.A);
//            output.Write(link);
//            output.RenderEndTag();
//        }


//    }

//}



//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Diagnostics;
//using System.IO;

//namespace Text2Html
//{
//    public class TextToHtml
//    {
//        private const int DEFAULT_SPACES_TAB_LENGTH = 2;
//        private const string DEFAULT_BULLET_PREFIX = "-";
//        private const string LINK_PREFIX = @"http://";
//        private const string BLANK = " ";

//        private const string OPENING_PAR = "<p>";
//        private const string CLOSING_PAR = "</p>"; 
//        private const string OPENING_LIST = "<ul>";
//        private const string CLOSING_LIST = "</ul>";
//        private const string OPENING_BULLET = "<li>";
//        private const string CLOSING_BULLET = "</li>";
//        private const string OPENING_LINK = "<a href=\"";
//        private const string MIDDLE_LINK = "\" target=\"_blank\">";
//        private const string CLOSING_LINK = "</a>";

//        private string SpacesTabValue;

//        public string SpacesTab
//        {
//            get { return SpacesTabValue; }
//            set { SpacesTabValue = value; }
//        }

//        public TextToHtml(int spacesTabLength = DEFAULT_SPACES_TAB_LENGTH)
//        {
//        }

//        public static TextReader OpenReaderFromFile(string inputTextFilePath)
//        {
//            return new StreamReader(File.OpenRead(inputTextFilePath));
//        }

//        public static TextWriter OpenWriterToFile(string outputHtmlFilePath)
//        {
//            return new StreamWriter(File.OpenWrite(outputHtmlFilePath));

//        }

//        public static TextReader OpenReaderFromString(string inputText)
//        {
//            return new StringReader(inputText);
//        }


//        public string ReadAndConvert(TextReader reader)
//        {
//            StringBuilder output = new StringBuilder();

//            string orgLine;
//            int previousLevel = 0, currentLevel = 0;
//            bool previousLineWasBullet = false;
//            bool previousLineWasOther = false;
//            int lineNumber = 1;

//            while ((orgLine = reader.ReadLine()) != null)
//            {
//                if (orgLine.StartsWith(LINK_PREFIX))
//                {
//                    if (lineNumber != 1)
//                    {
//                        CloseLists(
//                            output,
//                            out previousLineWasBullet,
//                            out previousLineWasOther,
//                            out previousLevel,
//                            ref currentLevel);
//                    }
//                    output.AppendLine(OPENING_PAR + ReplaceLinks(orgLine) + CLOSING_PAR);
//                    // a bit of a presumptious kludge
//                    output.AppendLine(OPENING_LIST);
//                }
//                else if (orgLine.Trim().StartsWith(DEFAULT_BULLET_PREFIX))
//                {
//                    int indexOfBullet;
//                    HandleSubLists(
//                        output,
//                        orgLine, lineNumber,
//                        previousLineWasOther, previousLineWasBullet,
//                        out indexOfBullet,
//                        out currentLevel, ref previousLevel);
                            
//                    // should check if line contains only a -
//                    string newLine = orgLine.Substring(indexOfBullet + 1).Trim();
//                    newLine = ReplaceLinks(newLine);

//                    previousLineWasBullet = true;
//                    previousLineWasOther = false;
//                    output.AppendLine(OPENING_BULLET + newLine);
//                }
//                // doesn't allow for a bullet to have text after sub-bullets
//                else
//                {
//                    previousLineWasOther = true;
//                    previousLineWasBullet = false;
//                    output.AppendLine(ReplaceLinks(orgLine));
//                }
//                lineNumber++;
//            }
//            CloseLists(
//                            output,
//                            out previousLineWasBullet,
//                            out previousLineWasOther,
//                            out previousLevel,
//                            ref currentLevel);
               

//            return output.ToString();

//        }

//        private void CloseLists(
//            StringBuilder output,
//            out bool previousLineWasBullet, out bool previousLineWasOther,
//            out int previousLevel, ref int currentLevel)
//        {
//            previousLineWasBullet = false;
//            previousLineWasOther = false;
//            output.AppendLine(CLOSING_BULLET);
//            output.AppendLine(CLOSING_LIST);
//            while (currentLevel > 0)
//            {
//                output.AppendLine(CLOSING_BULLET);
//                output.AppendLine(CLOSING_LIST);
//                currentLevel--;
//            }
//            previousLevel = 0;
//        }

//        private void HandleSubLists(
//            StringBuilder output,
//            string line, int lineNumber, 
//            bool previousLineWasOther, bool previousLineWasBullet,
//            out int indexOfBullet,
//            out int currentLevel, ref int previousLevel)
//        { 
//            indexOfBullet = line.IndexOf(DEFAULT_BULLET_PREFIX);
//            if (indexOfBullet % DEFAULT_SPACES_TAB_LENGTH != 0)
//                throw new IncorrectTabLengthException(
//                    "line " + lineNumber + ":  incorrect number of spaces used");
//            currentLevel = indexOfBullet / DEFAULT_SPACES_TAB_LENGTH;
//            // should check to see in/decreased by a proper amount
//            if (currentLevel > previousLevel) // start sub-list
//            {
//                output.AppendLine(OPENING_LIST);
//            }
//            else if (currentLevel < previousLevel) // end sub-list
//            {
//                output.AppendLine(CLOSING_LIST);
//                output.AppendLine(CLOSING_BULLET);
//            } 
//            else if (previousLineWasOther || previousLineWasBullet) // same level
//            {
//                output.AppendLine(CLOSING_BULLET);
//            }

//            previousLevel = currentLevel;

//        }

//        // http://a.b.c >> <a href="http://a.b.c" target="_blank">http://a.b.c</a>
//        private string ReplaceLinks(string line)
//        {
//            string newLine = "";
//            int start = 0;
//            start = line.IndexOf(LINK_PREFIX, start);

//            if (start < 0) return line;

//            int startNonLink = 0;
//            string link;

//            // didn't test this working on lines with multiple links
//            do
//            {
//                newLine += line.Substring(startNonLink, start - startNonLink);
//                int endOfLink = line.IndexOf(BLANK, start);
//                if (endOfLink < 0) // link is last thing on line
//                {
//                    link = line.Substring(start);
//                    newLine += OPENING_LINK + link + MIDDLE_LINK + link + CLOSING_LINK;
//                    return newLine;
//                }
//                link = line.Substring(start, endOfLink - start);
//                newLine += OPENING_LINK + link + MIDDLE_LINK + link + CLOSING_LINK;
//                startNonLink = endOfLink;
//            } while ((start = line.IndexOf(LINK_PREFIX, startNonLink)) > 0);

//            return newLine + line.Substring(startNonLink);

//        }


//    }
//}