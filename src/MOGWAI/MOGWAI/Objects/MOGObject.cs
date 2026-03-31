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

namespace MOGWAI.Objects
{
    public abstract class MOGObject
    {
        public MogwaiEngine Engine { get; init; }

        public string Code { get; init; } = string.Empty;

        public bool AutoEval { get; set; }

        public bool PauseAllowed { get; set; } = true;

        public int StartPos { get; set; } = -1;

        public int EndPos { get; set; } = -1;

        public MOGType Type { get; set; }

        public MogwaiExecutionContext? ExecutionContext { get; set; }

        public MOGObject? Bag { get; set; } 

        public MOGObject(MogwaiEngine engine)
        {
            Engine = engine;
            Type = engine.GetType(typeof(MOGObject));
        }

        public abstract MOGObject Clone();

        public virtual void UpdateFromOther(MOGObject other)
        {
            Type = other.Type;
            StartPos = other.StartPos;
            EndPos = other.EndPos;
            AutoEval = other.AutoEval;
            PauseAllowed = other.PauseAllowed;
            ExecutionContext = other.ExecutionContext;
            Bag = other.Bag;
        }

        public virtual Task<EvalResult> EngineEval()
        {
            Engine.StackPush(this);
            return Task.FromResult(EvalResult.NoError);
        }

        public virtual async Task<EvalResult> UserEval()
        {
            return await EngineEval();
        }

        public void RemoveFromDebugMechanism()
        {
            PauseAllowed = false;
            StartPos = -1;
            EndPos = -1;
        }

        public override string ToString() => "MogwaiObject";

        public virtual string ToJson() => ToString() + "!";
    }
}
