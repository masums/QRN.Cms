/*************************************************************
 *          Project: QRN CMS                                 *
 *              Web: http://tecrt.com/qrncms                 *
 *           Author: Qumruzzaman, Rashidul, Nazmul           *
 *          Website: byronbd.com, masums.com, gmnazmul.com   *
 *            Email: tecrt.com@gmail.com                     *
 *          License: AGPL v3                                 *
 *************************************************************/

using System;
using System.Collections.Generic;
using System.Text;

namespace QrnCms.Lib.Cms.Modules
{
    public interface IModule
    {
        int Order { get; set; }

        string Title { get; set; }
        float Version { get; set; }
        string VersionDetails { get; set; }
        int MinCmsVersion { get; set; }

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

        bool Install();
        bool Uninstall();
        bool Update();

        bool Activate();
        bool Inactivate();

        bool RemoveTables();
    }
}
