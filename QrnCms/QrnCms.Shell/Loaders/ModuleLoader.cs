// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using QrnCms.Shell.Modules;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;

namespace QrnCms.Shell.Loaders
{    
    public class ModuleLoader : IDisposable
    {
        public WeakReference Reference { get; set; }

        public static ModuleLoader CreateFromAssemblyFile(string assemblyFile, bool isUnloadable, Type[] sharedTypes)
            => CreateFromAssemblyFile(assemblyFile, isUnloadable, sharedTypes, _ => { });

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ModuleLoader CreateFromAssemblyFile(string assemblyFile, bool isUnloadable, Type[] sharedTypes, Action<ModuleLoaderConfig> configure)
        {
            return CreateFromAssemblyFile(assemblyFile,
                    sharedTypes,
                    config =>
                    {
                        config.IsUnloadable = isUnloadable;
                        configure(config);
                    });
        }

        public static ModuleLoader CreateFromAssemblyFile(string assemblyFile, Type[] sharedTypes)
            => CreateFromAssemblyFile(assemblyFile, sharedTypes, _ => { });

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ModuleLoader CreateFromAssemblyFile(string assemblyFile, Type[] sharedTypes, Action<ModuleLoaderConfig> configure)
        {
            return CreateFromAssemblyFile(assemblyFile,
                    config =>
                    {
                        if (sharedTypes != null)
                        {
                            foreach (var type in sharedTypes)
                            {
                                config.SharedAssemblies.Add(type.Assembly.GetName());
                            }
                        }
                        configure(config);
                    });
        }


        public static ModuleLoader CreateFromAssemblyFile(string assemblyFile)
            => CreateFromAssemblyFile(assemblyFile, _ => { });

        [MethodImpl(MethodImplOptions.NoInlining)]
        public static ModuleLoader CreateFromAssemblyFile(string assemblyFile, Action<ModuleLoaderConfig> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var config = new ModuleLoaderConfig(assemblyFile);
            configure(config);
            return new ModuleLoader(config);
        }

        private readonly ModuleLoaderConfig _config;
        private AssemblyLoadContext _context;
        private volatile bool _disposed;

        [MethodImpl(MethodImplOptions.NoInlining)]
        public ModuleLoader(ModuleLoaderConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _context = CreateLoadContext(config);
            Reference = new WeakReference(_context, true);
        }

        public bool IsUnloadable
        {
            get
            {
                return _context.IsCollectible;
            }
        }

        internal AssemblyLoadContext LoadContext => _context;

        public Assembly LoadDefaultAssembly()
        {
            EnsureNotDisposed();
            return _context.LoadFromAssemblyPath(_config.MainAssemblyPath);
        }

        public Assembly LoadAssembly(AssemblyName assemblyName)
        {
            EnsureNotDisposed();
            return _context.LoadFromAssemblyName(assemblyName);
        }

        public Assembly LoadAssembly(string assemblyName)
        {
            EnsureNotDisposed();
            return LoadAssembly(new AssemblyName(assemblyName));
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (_context.IsCollectible)
            {
                _context.Unload(); 
            }
        }

        private void EnsureNotDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(ModuleLoader));
            }
        }

        [MethodImpl(MethodImplOptions.NoInlining)]
        private static AssemblyLoadContext CreateLoadContext(ModuleLoaderConfig config)
        {
            var builder = new AssemblyLoadContextBuilder(); 

            builder.SetMainAssemblyPath(config.MainAssemblyPath);

            foreach (var ext in config.PrivateAssemblies)
            {
                builder.PreferLoadContextAssembly(ext);
            }

            if (config.PreferSharedTypes)
            {
                builder.PreferDefaultLoadContext(true);
            }

            if (config.IsUnloadable)
            {
                builder.EnableUnloading();
            }

            foreach (var assemblyName in config.SharedAssemblies)
            {
                builder.PreferDefaultLoadContextAssembly(assemblyName);
            }

            return builder.Build();
        }

        public static  List<ModuleEntry> LoadModules(string modulePath, Type[] sharedTypes)
        {
            var moduleEntries = new List<ModuleEntry>();
            foreach (var moduleDir in Directory.GetDirectories(modulePath))
            {
                var dirName = Path.GetFileName(moduleDir);
                var moduleFile = Path.Combine(moduleDir, "bin", "Debug", "netcoreapp3.0", dirName + ".dll");

                if (File.Exists(moduleFile))
                {
                    var moduleEntry = new ModuleEntry();
                    moduleEntry.Loader =  CreateFromAssemblyFile(moduleFile, true, sharedTypes, (cfg) => { cfg.PreferSharedTypes = true; cfg.IsUnloadable = true; });
                    var moduleAssembly = moduleEntry.Loader.LoadDefaultAssembly();
                    moduleEntry.Assembly = moduleAssembly;
                    moduleEntry.Path = moduleFile;
                    moduleEntry.ModuleName = dirName;

                    var type = moduleAssembly.GetTypes().Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsAbstract).FirstOrDefault();
                    if (type != null)
                    {
                        Debug.WriteLine("Found Module " + type.FullName);
                        moduleEntry.Module = (IModule)Activator.CreateInstance(type);
                        moduleEntries.Add(moduleEntry);
                    }
                }
            }
            return moduleEntries;
        }
    }
}
