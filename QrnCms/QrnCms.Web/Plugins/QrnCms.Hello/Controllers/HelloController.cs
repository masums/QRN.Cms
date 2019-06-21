using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using QrnCms.Shell;
using QrnCms.Shell.Modules;
using System;
using System.Collections.Generic;
using System.Text; 

namespace QrnCms.Hello.Controllers
{
    public class HelloController : Controller
    {
        public ContentResult Index()
        {
            var moduleVersion = new Plugin().Version.ToString();
            var sl = SupportedLanguages.En;
            var me = new ModuleEntry();

            return new ContentResult() { Content = $" Hello from Index. CMS Version:{CmsInfo.Version.ToString()}  Module Version:  " + moduleVersion , ContentType = "text/html; charset=utf-8", StatusCode = 200 };
        }
    }
}
