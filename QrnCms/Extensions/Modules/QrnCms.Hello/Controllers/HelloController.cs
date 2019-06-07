using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Text; 

namespace QrnCms.Hello.Controllers
{
    public class HelloController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
