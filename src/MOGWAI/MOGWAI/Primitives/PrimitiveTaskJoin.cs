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

namespace MOGWAI.Primitives
{
    internal class PrimitiveTaskJoin : MOGPrimitive
    {
        public PrimitiveTaskJoin(MogwaiEngine engine, string name) : base(engine, name)
        {

        }

        public override MOGPrimitive Duplicate()
        {
            var obj = new PrimitiveTaskJoin(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> EngineEval()
        {
            // list tasks.join

            var s = Engine.StackSign(1);

            if (s.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);

            if (s[0] == typeof(MOGList))
            {
                var list = Engine.StackPopList();
                var tasks = new List<MOGTask>();

                // La liste doit être composée que de noms de tâches existantes

                foreach (var item in list.Items)
                {
                    if (item is MOGName name)
                    {
                        var task = Engine.GetTask(name.Value);

                        if (task != null)
                        {
                            tasks.Add(task);
                        }
                        else
                        {
                            return EvalResult.Failure(Engine, Error.UnknownNameError, Name, name.ToString());
                        }

                    }
                    else
                    {
                        return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, item.ToString());
                    }
                }

                if (tasks.Count > 0)
                {
                    var stopWaiting = false;

                    while (!stopWaiting)
                    {
                        stopWaiting = true;

                        foreach (var task in tasks)
                        {
                            if (task.Status == MOGTask.TaskStatus.Running)
                            {
                                stopWaiting = false;
                                break;
                            }
                        }

                        var result = await Engine.ExecuteWaitingFireObjects();

                        if (result != EvalResult.NoError)
                            return result;
                    }
                }

                return EvalResult.NoError;
            }

            return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name);
        }
    }
}
