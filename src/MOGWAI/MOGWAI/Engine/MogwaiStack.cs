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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOGWAI.Engine
{
    internal class MogwaiStack : List<MOGObject>
    {
        public MogwaiStack()
        {

        }

        public MOGObject? Pop()
        {
            if (Count > 0)
            {
                var v = this[^1];
                RemoveAt(this.Count - 1);
                return v;
            }

            return null;
        }

        public MOGObject? Peek()
        {
            if (Count > 0)
                return this[^1];

            return null;
        }

        public void Push(MOGObject item) => Add(item);

        public List<Type> Sign(int size)
        {
            List<Type> types = new();

            if (Count >= size)
            {             
                for (int i = Count - 1; i >= Count - size; i--)
                    types.Add(this[i].GetType());
            }

            return types;
        }
    }
}
