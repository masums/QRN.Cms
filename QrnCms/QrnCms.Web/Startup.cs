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
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc.ApplicationParts;
using QrnCms.Lib.App;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using QrnCms.Lib.App.Providers;
using QrnCms.Lib.App.Loaders;

namespace QrnCms.Web
{
    public class Startup
    {
        List<ModuleEntry> _adminModules = new List<ModuleEntry>();
        List<ModuleEntry> _siteModules = new List<ModuleEntry>();

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            var siteModulePath = Path.Combine(AppContext.BaseDirectory, "Modules");

            _siteModules = LoadModules(siteModulePath, new[]
                        {
                            typeof(IApplicationBuilder),
                            typeof(IModule),
                            typeof(IServiceCollection),
                        });

            var adminModulePath = Path.Combine(AppContext.BaseDirectory, "Modules","Core");

            _adminModules = LoadModules(adminModulePath, new[]
                        {
                            typeof(IApplicationBuilder),
                            typeof(IModule),
                            typeof(IServiceCollection),
                        });

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

            var mvcBuilder = services.AddMvc(cfg => { cfg.EnableEndpointRouting = false; });
            GlobalContext.MvcBuilder = mvcBuilder;

            foreach (var me in _siteModules)
            {
                me.Module.ConfigureServices(services);
                var pluginAssembly = me.Loader.LoadDefaultAssembly();

                var partFactory = ApplicationPartFactory.GetApplicationPartFactory(pluginAssembly);
                foreach (var part in partFactory.GetApplicationParts(pluginAssembly))
                {
                    Debug.WriteLine($"* {part.Name}");
                    mvcBuilder.PartManager.ApplicationParts.Add(part);
                }

                // This piece finds and loads related parts, such as MvcAppPlugin1.Views.dll.
                var relatedAssembliesAttrs = pluginAssembly.GetCustomAttributes(typeof(RelatedAssemblyAttribute), true);
                foreach (RelatedAssemblyAttribute attr in relatedAssembliesAttrs)
                {
                    var assembly = me.Loader.LoadAssembly(attr.AssemblyFileName);
                    partFactory = ApplicationPartFactory.GetApplicationPartFactory(assembly);
                    foreach (var part in partFactory.GetApplicationParts(assembly))
                    {
                        Console.WriteLine($"  * {part.Name}");
                        mvcBuilder.PartManager.ApplicationParts.Add(part);
                    }
                }
            }

            services.AddSingleton<IActionDescriptorChangeProvider>(QrnActionDescriptorChangeProvider.Instance);
            services.AddSingleton(QrnActionDescriptorChangeProvider.Instance);

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseBranchWithServices("/admin",
            (service) => {

            },
            (app) => {

            });

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

            foreach (var sme in _siteModules)
            {
                sme.Module.Configure(app);
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

        private List<ModuleEntry> LoadModules(string modulePath, Type[] sharedTypes)
        {
            var moduleEntries = new List<ModuleEntry>();
            foreach (var pluginDir in Directory.GetDirectories(modulePath))
            {
                var dirName = Path.GetFileName(pluginDir);
                var pluginFile = Path.Combine(pluginDir, "bin", "Debug", "netcoreapp3.0", dirName + ".dll");

                if (File.Exists(pluginFile))
                {
                    var moduleEntry = new ModuleEntry();

                    moduleEntry.Loader = ModuleLoader.CreateFromAssemblyFile(pluginFile,
                        // this ensures that the plugin resolves to the same version of DependencyInjection
                        // and ASP.NET Core that the current app uses
                        sharedTypes: sharedTypes,
                        (opt) => { opt.PreferSharedTypes = true; opt.IsUnloadable = true; }
                    );

                    var pluginAssembly = moduleEntry.Loader.LoadDefaultAssembly();
                    var type = pluginAssembly.GetTypes().Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsAbstract).FirstOrDefault();
                    if(type != null)
                    {
                        Debug.WriteLine("Found plugin " + type.Name);
                        moduleEntry.Module = (IModule)Activator.CreateInstance(type);
                        moduleEntries.Add(moduleEntry);
                    }
                }
            }
            return moduleEntries;
        }
    }
}
