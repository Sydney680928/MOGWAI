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

using MOGWAI.Interfaces;

namespace MOGWAI.Engine
{
    internal class PluginInformations
    {
        public IPlugin Plugin { get; init; }

        public PluginLoadContext LoadContext { get; init; }

        public string ID => Plugin.ID;  

        public bool IsUnloadable => Plugin.IsUnloadable;    

        public PluginInformations(IPlugin plugin, PluginLoadContext loadContext)
        {
            Plugin = plugin;
            LoadContext = loadContext;
        }

        public Func<MogwaiEngine, string, Task<EvalResult>>? GetKeyword(string keyword)
        {
            if (Plugin.Keywords.TryGetValue(keyword, out var keywordFunc))
                return keywordFunc;

            return null;
        }
    }
}
