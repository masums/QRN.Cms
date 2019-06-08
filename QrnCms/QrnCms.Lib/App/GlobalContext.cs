using Microsoft.Extensions.DependencyInjection;
using QrnCms.Lib.App.Loaders;
using System;
using System.Collections.Generic;
using System.Text;

namespace QrnCms.Lib.App
{
    public class GlobalContext
    {
        public static IMvcBuilder MvcBuilder { get; set; }
        public static ModuleLoader ModuleContext { get; set; }
    }
}
