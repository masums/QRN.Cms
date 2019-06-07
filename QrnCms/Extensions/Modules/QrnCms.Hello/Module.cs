using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using QrnCms.Lib.Cms.Modules;
using System;

namespace QrnCms.Hello
{
    public class Module : BaseModule, IModule
    {

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
    }
}
