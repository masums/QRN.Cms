// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace QrnCms.Shell.Loaders
{
    /// <summary>
    /// Options for how <see cref="PluginLoader"/> behaves.
    /// </summary>
    [Flags]
    [Obsolete("This API is obsolete and will be removed in a future version. The recommended replacement is PluginConfig")]
    public enum ModuleLoaderOptions
    {
        /// <summary>
        /// Use the default behavior.
        /// </summary>
        None = 0,

        /// <summary>
        /// Attempt to unify all types from a plugin with the host.
        /// <para>
        /// This does not guarantee types will unify.
        /// </para>
        /// </summary>
        PreferSharedTypes = 1 << 0,
    }
}
