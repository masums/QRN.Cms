﻿// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.
 
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace QrnCms.Shell.Loaders
{
    /// <summary>
    /// Represents the configuration for a .NET Core plugin.
    /// </summary>
    public class ModuleLoaderConfig
    {
        /// <summary>
        /// Initializes a new instance of <see cref="ModuleLoaderConfig" />
        /// </summary>
        /// <param name="mainAssemblyPath">The full file path to the main assembly for the plugin.</param>
        public ModuleLoaderConfig(string mainAssemblyPath)
        {
            if (string.IsNullOrEmpty(mainAssemblyPath))
            {
                throw new ArgumentException("Value must be null or not empty", nameof(mainAssemblyPath));
            }

            if (!Path.IsPathRooted(mainAssemblyPath))
            {
                throw new ArgumentException("Value must be an absolute file path", nameof(mainAssemblyPath));
            }

            MainAssemblyPath = mainAssemblyPath;
        }

        /// <summary>
        /// The file path to the main assembly.
        /// </summary>
        public string MainAssemblyPath { get; }

        /// <summary>
        /// A list of assemblies which should be treated as private.
        /// </summary>
        public ICollection<AssemblyName> PrivateAssemblies { get; protected set; } = new List<AssemblyName>();

        /// <summary>
        /// A list of assemblies which should be unified between the host and the plugin.
        /// </summary>
        public ICollection<AssemblyName> SharedAssemblies { get; protected set; } = new List<AssemblyName>();

        /// <summary>
        /// Attempt to unify all types from a plugin with the host.
        /// <para>
        /// This does not guarantee types will unify.
        /// </para>
        /// </summary>
        public bool PreferSharedTypes { get; set; }

 
        /// <summary>
        /// The plugin can be unloaded from memory.
        /// </summary>
        public bool IsUnloadable { get; set; }
 
    }
}
