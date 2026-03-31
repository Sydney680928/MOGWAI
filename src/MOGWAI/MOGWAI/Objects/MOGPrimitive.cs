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
using MOGWAI.Objects;

namespace MOGWAI
{
    public abstract class MOGPrimitive : MOGObject
    {
        public const string CATEGORY_MATHS = "MH";
        public const string CATEGORY_GENERAL = "GE";
        public const string CATEGORY_STACK = "SK";
        public const string CATEGORY_RUNTIME = "RT";
        public const string CATEGORY_DEBUG = "DG";
        public const string CATEGORY_ERROR = "ER";

        public string Name { get; init; }

        public string FriendlyName { get; set; }

        public string HelpText { get; init; } = string.Empty;

        public bool IsPrivate { get; set; }

        public string Category { get; set; } = CATEGORY_GENERAL;

        public MOGPrimitive(MogwaiEngine engine, string name) : base(engine)
        {
            Engine = engine;
            Name = name;
            FriendlyName = name;
            PauseAllowed = true;
            Type = engine.GetType(typeof(MOGPrimitive));
        }


        public MOGPrimitive(MogwaiEngine engine, string name, string friendyName) : this(engine, name)
        {
            FriendlyName = name;
        }

        public MOGPrimitive(MogwaiEngine engine, string name, bool isPrivate = false, string helpText = "") : this(engine, name)
        {
            IsPrivate = isPrivate;
            HelpText = helpText;
        }

        public override string ToString()
        {
            return Name;
        }

        public override void UpdateFromOther(MOGObject other)
        {
            if (other is MOGPrimitive p)
            {
                FriendlyName = p.FriendlyName;
                Category = p.Category;
                IsPrivate = p.IsPrivate;
                AutoEval = p.AutoEval;
                PauseAllowed = p.PauseAllowed;
                ExecutionContext = p.ExecutionContext;
                StartPos = p.StartPos;
                EndPos = p.EndPos;
                Bag = p.Bag;    
            }
        }         

        public override MOGObject Clone()
        {
            return this;
        }

        public abstract MOGPrimitive Duplicate();
    }
}
