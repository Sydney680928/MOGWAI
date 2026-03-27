// Copyright 2015-2026 Stéphane Sibué
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Runtime.Loader;

namespace MOGWAI.Engine
{
    internal class PluginLoadContext : AssemblyLoadContext
    {
        private AssemblyDependencyResolver _Resolver;

        public PluginLoadContext(string pluginPath) : base(isCollectible: true) 
        {
            _Resolver = new AssemblyDependencyResolver(pluginPath);
        }

        [RequiresUnreferencedCode("Plugin loading is not trim-compatible by design.")]
        #pragma warning disable IL2046 // 'RequiresUnreferencedCodeAttribute' annotations must match across all interface implementations or overrides.
        protected override Assembly? Load(AssemblyName assemblyName)
        #pragma warning restore IL2046 // 'RequiresUnreferencedCodeAttribute' annotations must match across all interface implementations or overrides.
        {
            string? assemblyPath = _Resolver.ResolveAssemblyToPath(assemblyName);
            
            if (assemblyPath != null)
                return LoadFromAssemblyPath(assemblyPath);
            
            return null;
        }

        protected override IntPtr LoadUnmanagedDll(string unmanagedDllName)
        {
            string? libraryPath = _Resolver.ResolveUnmanagedDllToPath(unmanagedDllName);
            
            if (libraryPath != null)
                return LoadUnmanagedDllFromPath(libraryPath);

            return IntPtr.Zero;
        }
    }
}
