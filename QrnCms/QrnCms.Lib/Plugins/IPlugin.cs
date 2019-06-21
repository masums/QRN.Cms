/*************************************************************
 *          Project: QRN CMS                                 *
 *              Web: http://tecrt.com/cms                    *
 *           Author: Qumruzzaman, Rashidul, Nazmul           *
 *          Website: byronbd.com, masums.com, gmnazmul.com   *
 *            Email: tecrt.com@gmail.com                     *
 *          License: AGPL v3                                 *
 *************************************************************/

using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text; 

namespace QrnCms.Shell.Modules
{
    public interface IPlugin
    {
        int Order { get; set; }

        string Title { get; set; }
        Version Version { get; set; } 
        Version MinCmsVersion { get; set; }

        string Author { get; set; }
        string Email { get; set; }
        string Website { get; set; }
        string DemoUrl { get; set; }
        string Description { get; set; }

        string Category { get; set; }

        string TablePrefix { get; set; }

        string Area { get; }
        List<string> SupportedLanguages { get; }
        List<string> SupportedDatabases { get; }

        void Configure(IApplicationBuilder appBuilder);
        void ConfigureServices(IServiceCollection services);

        bool Install();
        bool Uninstall();
        bool Update();

        bool Activate();
        bool Inactivate();

        bool RemoveTables();
    }

    public class BasePlugin : IPlugin
    {
        public int Order { get; set; }
        public string Title { get; set; }
        public Version Version { get; set; }
        public Version MinCmsVersion { get; set; }
        public string Author { get; set; }
        public string Email { get; set; }
        public string Website { get; set; }
        public string DemoUrl { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string TablePrefix { get; set; }

        public string Area => "";

        public List<string> SupportedLanguages => new List<string>() { "en" };

        public List<string> SupportedDatabases => new List<string>() { "MySql" };

        public virtual bool Activate()
        {
            return false;
        }

        public virtual void Configure(IApplicationBuilder appBuilder)
        {
             
        }

        public virtual void ConfigureServices(IServiceCollection services)
        {
             
        }

        public virtual bool Inactivate()
        {
            return false;
        }

        public virtual bool Install()
        {
            return false;
        }

        public virtual bool RemoveTables()
        {
            return false;
        }

        public virtual bool Uninstall()
        {
            return false;
        }

        public virtual bool Update()
        {
            return false;
        }
    }
}
