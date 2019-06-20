using System;

namespace QrnCms.Shell
{
    public class CmsInfo
    {
        public static string Name { get; } = "QRN CMS";
        public static string Slogan { get; } = "Developer friendly CMS";
        public static Version Version { get; } = new Version(0, 0, 1, 1);
        public static string Description { get; } = "A Content Management System developed using ASP.NET Core.";
        public static string Website { get; } = "http://tecrt.com/cms";
        public static string Email { get; } = "tecrt.com@gmail.com";
        public static string Author { get; } = "Qumruzzaman, Rashidul, Nazmul";
        public static string CoreModuleFolder { get; } = "Core";
        public static string ModuleFolder { get; } = "Modules";
        public static string ThemeFolder { get; } = "Themes";
        public static string LogFolder { get; } = "__Logs";
        public static string DevelopedBy { get; } = "TecRT";
    }
}
