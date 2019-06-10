using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text; 

namespace QrnCms.Hello.Controllers
{
    public class HelloController : Controller
    {
        public ContentResult Index()
        {
            return new ContentResult() { Content = " Hello from Index ", ContentType = "text/html; charset=utf-8", StatusCode = 200 };
        }
    }
}
