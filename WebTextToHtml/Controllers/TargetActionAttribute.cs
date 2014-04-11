// Copyright Stefan Nychka, BSD 3-Clause license, LICENSE.txt
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace WebTextToHtml.Controllers
{
    // http://stackoverflow.com/questions/442704/how-do-you-handle-multiple-submit-buttons-in-asp-net-mvc-framework
    // mine is to be used to distinguish between actions of the same name.
    // assumes, reasonably, that name attributes have unique values
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class TargetActionAttribute : ActionNameSelectorAttribute
    {
        public string Button { get; set; }

        public override bool IsValidName(   ControllerContext controllerContext, string actionName,
                                            System.Reflection.MethodInfo methodInfo)
        {
            return controllerContext.Controller.ValueProvider.GetValue(Button) != null;
        }
    }
}