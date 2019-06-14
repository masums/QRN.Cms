using Microsoft.Extensions.DependencyInjection;
using QrnCms.Shell.Loaders;
using QrnCms.Shell.Modules;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace QrnCms.Shell
{
    public class GlobalContext
    { 
        public static IMvcBuilder MvcBuilder { get; set; }
        public static List<ModuleEntry> SiteModules { get; set; }
        public static List<ModuleEntry> AdminModules { get; set; }
    }
}
