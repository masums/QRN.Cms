// Copyright (c) Nate McMaster.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.Loader;
using McMaster.NETCore.Plugins.Loader;

namespace McMaster.NETCore.Plugins
{    
    public class ModuleLoader : IDisposable
    {
        public static ModuleLoader CreateFromAssemblyFile(string assemblyFile, bool isUnloadable, Type[] sharedTypes)
            => CreateFromAssemblyFile(assemblyFile, isUnloadable, sharedTypes, _ => { });

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
        private readonly AssemblyLoadContext _context;
        private volatile bool _disposed;


        public ModuleLoader(ModuleLoaderConfig config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _context = CreateLoadContext(config);
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
    }
}
