using QrnCms.Lib.App.Loaders;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace QrnCms.Lib.Cms.Modules
{
    public class ModuleEntry
    {
        public IModule Module { get; set; }
        public ModuleLoader Loader { get; set; }
        public Assembly Assembly { get; set; }
    }
}
