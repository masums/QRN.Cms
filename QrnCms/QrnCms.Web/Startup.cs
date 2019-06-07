using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.EntityFrameworkCore;
using QrnCms.Web.Data;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using QrnCms.Lib.Cms.Modules;
using System.IO;
using McMaster.NETCore.Plugins;
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using QrnCms.Lib.App;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using QrnCms.Lib.App.Providers;

namespace QrnCms.Web
{
    public class Startup
    {
        public static ModuleLoader _moduleContext;
        private List<IModule> _plugins = new List<IModule>();

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            var ModulePath = Path.Combine(AppContext.BaseDirectory, "Modules");
            foreach (var pluginDir in Directory.GetDirectories(ModulePath))
            {

                var dirName = Path.GetFileName(pluginDir);
                var pluginFile = Path.Combine(pluginDir, "bin","Debug","netcoreapp3.0", dirName + ".dll");
                _moduleContext = ModuleLoader.CreateFromAssemblyFile(pluginFile,
                    // this ensures that the plugin resolves to the same version of DependencyInjection
                    // and ASP.NET Core that the current app uses
                    sharedTypes: new[]
                    {
                        typeof(IApplicationBuilder),
                        typeof(IModule),
                        typeof(IServiceCollection),
                    }, (opt) => { opt.PreferSharedTypes = true; opt.IsUnloadable = true; });

                GlobalContext.ModuleContext = _moduleContext;

                var pluginAssembly = _moduleContext.LoadDefaultAssembly();
                foreach (var type in pluginAssembly
                    .GetTypes()
                    .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsAbstract))
                {
                    Debug.WriteLine("Found plugin " + type.Name);
                    var plugin = (IModule)Activator.CreateInstance(type);
                    _plugins.Add(plugin);
                }
            }

        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
            });

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(
                    Configuration.GetConnectionString("DefaultConnection")));
            services.AddDefaultIdentity<IdentityUser>()
                .AddEntityFrameworkStores<ApplicationDbContext>();

            services.AddControllersWithViews()
                .AddNewtonsoftJson();
            services.AddRazorPages();

            foreach (var plugin in _plugins)
            {
                plugin.ConfigureServices(services);
            }

            var mvcBuilder = services.AddMvc(cfg=> { cfg.EnableEndpointRouting = false; });
            GlobalContext.MvcBuilder = mvcBuilder;

            var pluginAssembly = _moduleContext.LoadDefaultAssembly();

            var partFactory = ApplicationPartFactory.GetApplicationPartFactory(pluginAssembly);
            foreach (var part in partFactory.GetApplicationParts(pluginAssembly))
            {
                Debug.WriteLine($"* {part.Name}");
                mvcBuilder.PartManager.ApplicationParts.Add(part);
            }

            // This piece finds and loads related parts, such as MvcAppPlugin1.Views.dll.
            var relatedAssembliesAttrs = pluginAssembly.GetCustomAttributes(typeof(RelatedAssemblyAttribute),true);
            foreach (RelatedAssemblyAttribute attr in relatedAssembliesAttrs)
            {
                var assembly = _moduleContext.LoadAssembly(attr.AssemblyFileName);
                partFactory = ApplicationPartFactory.GetApplicationPartFactory(assembly);
                foreach (var part in partFactory.GetApplicationParts(assembly))
                {
                    Console.WriteLine($"  * {part.Name}");
                    mvcBuilder.PartManager.ApplicationParts.Add(part);
                }
            }

            services.AddSingleton<IActionDescriptorChangeProvider>(QrnActionDescriptorChangeProvider.Instance);
            services.AddSingleton(QrnActionDescriptorChangeProvider.Instance);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseCookiePolicy();

            app.UseMvc(cfg => { });

            app.UseMvcWithDefaultRoute();
            //app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            foreach (var plugin in _plugins)
            {
                plugin.Configure(app);
            }

            /*
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
            */

        }
    }
}
