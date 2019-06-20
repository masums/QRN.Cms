using QrnCms.Shell;
using QrnCms.Shell.Loaders;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace QrnCms.Shell.Modules
{
    public class ModuleEntry
    {
        public string ModuleName { get; set; }
        public IModule Module { get; set; }
        public ModuleLoader Loader { get; set; }
        public Assembly Assembly { get; set; }
        public string Path { get; set; }
    }
}
