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

using MOGWAI.Engine;
using MOGWAI.Exceptions;

namespace MOGWAI.Objects
{
    public class MOGClass
    {
        public MogwaiEngine Engine { get; init; }

        public string Name { get; init; }

        public Dictionary<string, MOGType> PrivateProperties { get; } = new();

        public Dictionary<string, MOGType> PublicProperties { get; } = new();

        public Dictionary<string, MOGFunction> PrivateFunctions { get; } = new();

        public Dictionary<string, MOGFunction> PublicFunctions { get; } = new();

        public MOGClass(MogwaiEngine engine, string name, MOGRecord defRecord)
        {
            Engine = engine;    
            Name = name;

            var names = new List<string>();

            // Section Privates  

            var record = defRecord.GetItem("private");

            if (record is MOGRecord privatesRecord)
            {
                foreach (var key in privatesRecord.Items.Keys)
                {                 
                    var value = privatesRecord.Items[key];

                    if (value is MOGType type)
                    {
                        if (names.Contains(key))
                            throw new MogwaiClasseDefinitionException($"duplicate name: {key}");

                        PrivateProperties.Add(key, type);
                        names.Add(key); 
                    }
                    else if (value is MOGCode code)
                    {
                        if (names.Contains(key))
                            throw new MogwaiClasseDefinitionException($"duplicate name: {key}");

                        PrivateFunctions.Add(key, code.ToFunction());
                        names.Add(key);
                    }
                }
            }

            // Section Publics  

            record = defRecord.GetItem("public");

            if (record is MOGRecord publicsRecord)
            {
                foreach (var key in publicsRecord.Items.Keys)
                {
                    var value = publicsRecord.Items[key];

                    if (value is MOGType type)
                    {
                        if (names.Contains(key))
                            throw new MogwaiClasseDefinitionException($"duplicate name: {key}");

                        PublicProperties.Add(key, type);
                        names.Add(key);
                    }
                    else if (value is MOGCode code)
                    {
                        if (names.Contains(key))
                            throw new MogwaiClasseDefinitionException($"duplicate name: {key}");

                        PublicFunctions.Add(key, code.ToFunction());
                        names.Add(key);
                    }
                }
            }
        }

        public MOGInstance CreateInstance(int instance) => new MOGInstance(Engine, this, instance);
    }
}
