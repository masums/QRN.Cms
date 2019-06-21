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
        public string PluginName { get; set; }
        public IPlugin Plugin { get; set; }
        public PluginLoader Loader { get; set; }
        public Assembly Assembly { get; set; }
        public string Path { get; set; }
    }
}
