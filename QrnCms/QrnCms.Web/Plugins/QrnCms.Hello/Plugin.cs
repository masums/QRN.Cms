using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using QrnCms.Shell.Modules;
using System;

namespace QrnCms.Hello
{
    public class Plugin : BasePlugin, IPlugin
    {
        public Plugin()
        {
            Version = new Version(1, 0, 1, 1);
        }
        public override void Configure(IApplicationBuilder appBuilder)
        {
            //appBuilder.Map(new Microsoft.AspNetCore.Http.PathString("/plugins/v1"), cfg => {
            //    cfg.Run(async (ctx) =>
            //    {
            //        await ctx.Response.WriteAsync("This plugin LOADED");
            //    });
            //});
            base.Configure(appBuilder);
        }

        public override void ConfigureServices(IServiceCollection service)
        {

        }    
    }
}
