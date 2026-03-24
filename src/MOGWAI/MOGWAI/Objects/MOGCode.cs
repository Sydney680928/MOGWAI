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
using System.Text;

namespace MOGWAI.Objects
{
    public class MOGCode : MOGBaseItems
    {
        public MOGCode(MogwaiEngine engine, string content, int originPosition, MogwaiExecutionContext? context) : base(engine, content, originPosition, context)
        {
            Type = engine.GetType(typeof(MOGCode));
            PauseAllowed = false;

            if (Items.Count > 0 && Items[0] is MOGWord word && word.Value == "!")
            {
                AutoEval = true;
                Items.RemoveAt(0);
            }
        }

        public MOGCode(MogwaiEngine engine, List<MOGObject> items) : base(engine, items)
        {
            Type = engine.GetType(typeof(MOGCode));
            PauseAllowed = false;

            if (Items.Count > 0 && Items[0] is MOGWord word && word.Value == "!")
            {
                AutoEval = true;
                Items.RemoveAt(0);
            }
        }

        public virtual async Task<EvalResult> Execute()
        {
            var isBrowser = OperatingSystem.IsBrowser();    

            EvalResult result = EvalResult.NoError;
            var ExecutionContextAllowDebugMode = true;

            if (Items.Count > 0)
            {
                var counter = 0;

                foreach (var item in Items)
                {
                    if (item.ExecutionContext != null)
                        ExecutionContextAllowDebugMode = item.ExecutionContext.AllowDebugMode;

                    if (Engine.HasWaitingFireObjects)
                        result = await Engine.ExecuteWaitingFireObjects();

                    if (result != EvalResult.NoError)
                        break;               

                    if (Engine.ExitRequested || Engine.BreakRequested || Engine.ReturnRequested)
                        break;

                    if (Engine.HaltRequested)
                    {
                        result = EvalResult.Failure(Engine, Error.HaltEncountedError);
                        break;
                    }

                    Engine.CurrentEvalObject = item.Clone();

                    if (Engine.DebugMode && ExecutionContextAllowDebugMode)
                    {
                        if (item.PauseAllowed && Engine.DebugPauseMode)
                        {
                            // Debug.stop

                            await Engine.SendProgramPause();

                            if (Engine.Delegate != null)
                            {
                                var r = await Engine.Delegate.EngineDidPause(Engine);

                                if (r != EvalResult.NoError)
                                {
                                    result = r;
                                    break;
                                }
                            }

                            if (Engine.IsSocketServerServiceRunning)
                            {
                                await Engine.SendProgramInformations(Engine.CurrentEvalObject, item.ExecutionContext?.CodeFilename ?? null);
                                await Engine.SendTrace();
                            }

                            while (!Engine.DebugResumeSignal && !Engine.DebugNextStepSignal && !Engine.HaltRequested)
                                await Task.Delay(250);

                            if (Engine.IsSocketServerRunning)
                                await Engine.SendProgramResume();

                            if (Engine.Delegate != null)
                            {
                                var r = await Engine.Delegate.EngineDidPause(Engine);

                                if (r != EvalResult.NoError)
                                {
                                    result = r;
                                    break;
                                }
                            }

                            if (Engine.DebugResumeSignal)
                                Engine.DebugPauseMode = false;

                            Engine.DebugExtinguishSignals();
                        }
                        else
                        {
                            if (Engine.IsSocketServerServiceRunning && Engine.TronValue > 0 && item.PauseAllowed)
                            {
                                await Engine.SendProgramInformations(Engine.CurrentEvalObject, item.ExecutionContext?.CodeFilename ?? null);
                                await Engine.SendTrace();
                                await Task.Delay(Engine.TronValue);
                            }
                        }
                    }

                    try
                    {
                        result = await Engine.CurrentEvalObject.EngineEval();
                    }
                    catch (Exception ex)
                    {
                        result = EvalResult.Failure(Engine, Error.FatalError, ex.Message);
                    }

                    if (result.IsError)
                        break;

                    if (isBrowser)
                        counter++;

                    if (isBrowser && counter % 100 == 0)
                        await Task.Delay(1);
                }

                if (isBrowser && counter % 100 != 0)
                    await Task.Delay(1);

                return result;
            }
            else
            {
                if (isBrowser)
                    await Task.Delay(1);

                return await Engine.ExecuteWaitingFireObjects();
            }
        }

        public async Task<(EvalResult result, MOGObject? value)> ExecuteScalar()
        {
            // Execute code and get one value as result
            // The first value in the stack after code execution is the value returned

            MOGObject? value = null;

            var result = await Execute();

            if (result == EvalResult.NoError)
            {
                if (Engine.StackSize == 0)
                {
                    result = EvalResult.Failure(Engine, Error.StackSizeError, "error about scalar execution code.");
                }
                else
                {
                    value = Engine.StackPop();
                }
            }

            return (result, value);
        }

        public override async Task<EvalResult> EngineEval()
        {
            if (AutoEval)
            {
                return await Execute();
            }
            else
            {
                return await base.EngineEval();
            }
        }

        public override async Task<EvalResult> UserEval()
        {
            return await Execute();
        }

        public override MOGCode Clone() => this;

        public override string ToString()
        {
            return "{" + ToStringCode() + "}";
        }

        public MOGFunction ToFunction()
        {
            var obj = new MOGFunction(Engine, Items);
            obj.UpdateFromOther(this);
            return obj;
        }

        public string ToStringCode()
        {
            var sb = new StringBuilder();

            if (AutoEval)
                sb.Append("! ");

            for (int i = 0; i < Items.Count; i++)
            {
                sb.Append(Items[i].ToString());

                if (i < Items.Count - 1)
                {
                    sb.Append(" ");
                }
            }

            return sb.ToString();
        }
    }
}
