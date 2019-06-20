using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Loader;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using QrnCms.Shell;
using QrnCms.Shell.Helpers;
using QrnCms.Shell.Loaders;
using QrnCms.Shell.Modules;
using QrnCms.Shell.Providers;
using QrnCms.Web.Models;

namespace QrnCms.Web.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationPartManager _partManager;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public HomeController(
            ApplicationPartManager partManager,
            IWebHostEnvironment env)
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

            if (ap != null)
                apm.ApplicationParts.Remove(ap);
           
            QrnActionDescriptorChangeProvider.Instance.HasChanged = true;
            QrnActionDescriptorChangeProvider.Instance.TokenSource.Cancel();

            var modulePath = "";
            var module = GlobalContext.SiteModules.Where(x => x.ModuleName == "QrnCms.Hello").FirstOrDefault();
            if(module != null)
            {
                modulePath = module.Path;

                module.Assembly = null;
                module.Module = null;
                module.Loader.Dispose();

                for (int i = 0; i < 10; i++)
                {
                    if (module.Loader.Reference.IsAlive)
                    { 
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                    }
                }

                module.Loader = null;
                GlobalContext.SiteModules.Remove(module);
            }

            for (int i = 0; i < 10; i++)
            { 
                GC.Collect();
                GC.WaitForPendingFinalizers(); 
            }

            GC.Collect();
            GC.WaitForPendingFinalizers();

            System.IO.File.Delete(modulePath);

            return View("Index");
        }

       
        public IActionResult LoadModule()
        {
            var siteModulePath = Path.Combine(AppContext.BaseDirectory, "Modules");

            var siteModules = ModuleLoader.LoadModules(siteModulePath, new[]
                        {
                            typeof(IApplicationBuilder),
                            typeof(IModule),
                            typeof(IServiceCollection),
                        });

            foreach (var me in siteModules)
            {

                var pluginAssembly = me.Loader.LoadDefaultAssembly();
                var apm = HttpContext.RequestServices.GetService<ApplicationPartManager>();
                var partFactory = ApplicationPartFactory.GetApplicationPartFactory(pluginAssembly);
                foreach (var part in partFactory.GetApplicationParts(pluginAssembly))
                {
                    Debug.WriteLine($"* {part.Name}");
                    apm.ApplicationParts.Add(part);

                }

                QrnActionDescriptorChangeProvider.Instance.HasChanged = true;
                QrnActionDescriptorChangeProvider.Instance.TokenSource.Cancel();
            }

            return Content("0");
        }
    }
}
