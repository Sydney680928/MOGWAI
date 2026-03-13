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
    internal class PrimitiveSub : MOGPrimitive
    {
        public PrimitiveSub(MogwaiEngine engine, string name) : base(engine, name)
        {
        }

        public override MOGObject Clone()
        {
            var obj = new PrimitiveSub(Engine, Name);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override async Task<EvalResult> EngineEval()
        {
            // "ABCDE" 1 1 sub ---> "B"
            // "ABCDE" 2 0 sub   ---> "CDE"

            // (1 2 3 4 5) 1 1 sub ---> (2)
            // (1 2 3 4 5) 2 0 sub   ---> (3 4 5)

            // DATA:FFBBEE 0 2 sub ---> DATA:FFBB

            // BIN:110011 0 2 sub ---> BIN:011

            var sign = Engine.StackSign(3);

            if (sign.Count == 0)
                return EvalResult.Failure(Engine, Error.TooFewArgumentsError, Name);

            if (sign[0] != typeof(MOGNumber) || sign[1] != typeof(MOGNumber) || (sign[2] != typeof(MOGString) && sign[2] != typeof(MOGList) && sign[2] != typeof(MOGData) && sign[2] != typeof(MOGBinaryNumber) && sign[2] != typeof(MOGRef)))
                return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name);

            var n0 = Engine.StackPopNumber();
            var n1 = Engine.StackPopNumber();
            var n2 = Engine.StackPop();

            var start = n1.IntValue;
            var count = n0.IntValue;

            if (start < 0 || count < 0)
                return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name);

            if (n2 is MOGString s)
            {
                if (count <= 0)
                    count = s.Value.Length;

                if (start + count >= s!.Value.Length)
                    count = s.Value.Length - start;

                try
                {
                    Engine.StackPushString(s.Value.Substring(start, count));
                    return EvalResult.NoError;
                }
                catch (Exception ex)
                {
                    return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name, ex.Message);
                }
            }
            else if (n2 is MOGList l)
            {
                if (start < 0 || start >= l.Items.Count)
                    return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name);

                if (count <= 0)
                    count = l.Size;

                if (start + count >= l.Size)
                    count = l.Size - start;

                var l2 = new MOGList(Engine);

                for (int i = 0; i < count; i++)
                    l2.Items.Add(l.Items[start + i]);

                Engine.StackPush(l2);
                return EvalResult.NoError;
            }
            else if (n2 is MOGData d)
            {
                if (start < 0 || start >= d.Items.Count)
                    return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name);

                if (count <= 0)
                    count = d.Items.Count;

                if (start + count >= d.Items.Count)
                    count = d.Items.Count - start;

                var d2 = new MOGData(Engine);

                for (int i = 0; i < count; i++)
                    d2.Items.Add(d.Items[start + i]);

                Engine.StackPush(d2);
                return EvalResult.NoError;
            }
            else if (n2 is MOGBinaryNumber b)
            {
                if (start < 0 || start >= b.Items.Count)
                    return EvalResult.Failure(Engine, Error.BadArgumentValueError, Name);

                if (count <= 0)
                    count = b.Items.Count;

                if (start + count >= b.Items.Count)
                    count = b.Items.Count - start;

                var d2 = new MOGBinaryNumber(Engine, count);

                for (int i = 0; i < count; i++)
                    d2.Items[i] = b.Items[start + i];

                Engine.StackPush(d2);
                return EvalResult.NoError;
            }
            else if (n2 is MOGRef r)
            {
                var value = Engine.VarRead(r.Value, false);

                if (value == null)
                    return EvalResult.Failure(Engine, Error.UnknownNameError, Name, r.ToString());

                Engine.StackPush(value);
                Engine.StackPush(n1);
                Engine.StackPush(n0);   

                return await EngineEval();
            }

            return EvalResult.Failure(Engine, Error.BadArgumentTypeError, Name);
        }
    }
}
