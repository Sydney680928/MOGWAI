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

        public Dictionary<string, MOGType> Privates { get; } = new();

        public Dictionary<string, MOGType> Properties { get; } = new();

        public Dictionary<string, MOGFunction> Funcs { get; } = new();

        public MOGClass(MogwaiEngine engine, string name, MOGRecord defRecord)
        {
            Engine = engine;    
            Name = name;

            // Section Properties:  

            var propertiesRecord = defRecord.GetItem("props");

            if (propertiesRecord != null)
            {
                if (propertiesRecord is MOGRecord properties)
                {
                    foreach (var key in properties.Items.Keys)
                    {
                        var item = properties.Items[key];

                        if (item is MOGType type)
                        {
                            Properties.Add(key, type);
                        }
                        else
                        {
                            throw new MogwaiClasseDefinitionException($"the 'properties' section of a class definition must only contain types");
                        }
                    }
                }
                else
                {
                    throw new MogwaiClasseDefinitionException($"the 'properties' section of a class definition must be a record");
                }
            }

            // Section Privates:
            // Les noms des propriétés privées d'une classe sont uniques, et ne peuvent pas être les mêmes que les noms des propriétés de la classe.

            var privatesRecord = defRecord.GetItem("privates"); 

            if (privatesRecord != null)
            {
                if (privatesRecord is MOGRecord privates)
                {
                    foreach (var key in privates.Items.Keys)
                    {
                        if (Properties.ContainsKey(key))
                            throw new MogwaiClasseDefinitionException($"the name '{key}' is already used as a property name in the class definition, it cannot be used as a private property name");

                        var item = privates.Items[key]; 

                        if (item is MOGType type)
                        {                        
                            Privates.Add(key, type);
                        }
                        else
                        {
                            throw new MogwaiClasseDefinitionException($"the 'privates' section of a class definition must only contain types");
                        }
                    }
                }
                else
                {
                    throw new MogwaiClasseDefinitionException($"the 'privates' section of a class definition must be a record");
                }
            }

            // Section Funcs:
            // Les noms des fonctions d'une classe sont uniques, et ne peuvent pas être les mêmes que les noms des propriétés ou des propriétés privées de la classe.   

            var funcsRecord = defRecord.GetItem("funcs");

            if (funcsRecord != null)
            {
                if (funcsRecord is MOGRecord funcs)
                {
                    foreach (var key in funcs.Items.Keys)
                    {
                        if (Properties.ContainsKey(key))
                            throw new MogwaiClasseDefinitionException($"the name '{key}' is already used as a property name in the class definition, it cannot be used as a function name");

                        if (Privates.ContainsKey(key))
                            throw new MogwaiClasseDefinitionException($"the name '{key}' is already used as a private property name in the class definition, it cannot be used as a function name");

                        var item = funcs.Items[key];

                        if (item is MOGCode code)
                        {
                            Funcs.Add(key, code.ToFunction());
                        }
                        else
                        {
                            throw new MogwaiClasseDefinitionException($"the 'funcs' section of a class definition must only contain code");
                        }
                    }
                }
                else
                {
                    throw new MogwaiClasseDefinitionException($"the 'funcs' section of a class definition must be a record");
                }
            }
        }

        public MOGInstance CreateInstance(int instance) => new MOGInstance(Engine, this, instance);
    }
}
