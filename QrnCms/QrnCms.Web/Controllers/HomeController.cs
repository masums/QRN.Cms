using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using QrnCms.Shell.Providers;
using QrnCms.Web.Models;

namespace QrnCms.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationPartManager _partManager;
        private readonly IHostingEnvironment _hostingEnvironment;
        public HomeController(
            ApplicationPartManager partManager,
            IHostingEnvironment env)
        {
            _partManager = partManager;
            _hostingEnvironment = env;
        }
        
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult Load()
        {
            return View("Index");
        }

        public IActionResult UnLoad()
        {
            var apm = HttpContext.RequestServices.GetService<ApplicationPartManager>();
            var ap = apm.ApplicationParts.Where(x => x.Name == "QrnCms.Hello").FirstOrDefault();
            if(ap != null)
                apm.ApplicationParts.Remove(ap);
           
            QrnActionDescriptorChangeProvider.Instance.HasChanged = true;
            QrnActionDescriptorChangeProvider.Instance.TokenSource.Cancel();
            
            GC.Collect();
            GC.WaitForPendingFinalizers();

            return View("Index");
        }

       
        public IActionResult LoadModule()
        {
            string assemblyPath = @"PATH\ExternalControllers.dll";
            var assembly = AssemblyLoadContext.Default.LoadFromAssemblyPath(assemblyPath);
            if (assembly != null)
            {
                _partManager.ApplicationParts.Add(new AssemblyPart(assembly));
                // Notify change
                QrnActionDescriptorChangeProvider.Instance.HasChanged = true;
                QrnActionDescriptorChangeProvider.Instance.TokenSource.Cancel();
                return Content("1");
            }
            return Content("0");
        }
    }
}
