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

using MOGWAI.Objects;

namespace MOGWAI.Engine
{
    public class VarContext
    {
        private Dictionary<string, Var> _vars = new();

        public string Name { get; init; }

        public string[] Keys => _vars.Keys.ToArray();

        public VarContext(string name)
        {
            Name = name;
        }

        public void Clear()
        {
            _vars.Clear();
        }

        public void Declare(string name, MOGObject value)
        {
            _vars[name] = new Var(name, value, value.Type);
        }

        public void DeclareForType(string name, MOGType type)
        {
            var v = new Var(name, new MOGNull(type.Engine));
            v.StrongType = type;

            _vars[name] = v;
        }

        public bool Write(string name, MOGObject value)
        {
            if (_vars.ContainsKey(name))
            {
                var v = _vars[name];

                if (v.StrongType == null || v.StrongType == value.Type || v.StrongType.Code == "any")
                {
                    v.Value = value;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                _vars[name] = new Var(name, value);
                return true;
            }
        }

        public MOGObject? Read(string name, bool clone = true)
        {
            if (_vars.ContainsKey(name))
                return clone ? _vars[name].Value.Clone() : _vars[name].Value;

            return null;
        }

        public bool Exists(string name) => _vars.ContainsKey(name);

        public bool Purge(string name)
        {
            if (_vars.ContainsKey(name))
            {
                _vars.Remove(name);
                return true;
            }

            return false;
        }
    }
}
