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
using System.Text.Json;

namespace MOGWAI.Objects
{
    public class MOGString : MOGBaseString
    {
        public MOGString(MogwaiEngine engine, string value) : base(engine, value)
        {
            Type = engine.GetType(typeof(MOGString));
        }

        public MOGString(MogwaiEngine engine, string value, int originPosition) : this(engine, value)
        {
            if (originPosition > -1)
            {
                StartPos = originPosition;
                EndPos = originPosition + Value.Length + 1;
            }
        }

        public override async Task<EvalResult> UserEval()
        {
            var result = await Eval();

            if (result.IsError)
                return result;

            return await base.UserEval();
        }

        public override MOGString Clone()
        {
            var obj = new MOGString(Engine, Value);
            obj.UpdateFromOther(this);
            return obj;
        }

        public override string ToString()
        {
            return $"\"{Value}\"";
        }

        public override string ToJson()
        {
            return $"\"{Tools.ToUnicodeEscaped(Value)}\"";
        }

        public async Task<EvalResult> Eval()
        {
            // name = "Stéphane" (string)
            // age = 57 (number)
            // "Hello {! name} you are {! age}" ---> "Hello Stéphane you are 57"

            var r = UnescapeString(Engine, Value);    

            if (r.result.IsError)
            {
                Engine.LastParserStartErrorPosition = StartPos;
                Engine.LastParserEndErrorPosition = EndPos;
                return r.result;
            }

            Value = r.unescaped ?? Value;

            var items = ParseStringFormat();

            if (items != null)
            {
                var sb = new StringBuilder();

                foreach (var item in items)
                {
                    if (item is MOGString s)
                    {
                        sb.Append(s.Value);
                    }
                    else if (item is MOGCode c)
                    {
                        if (c.AutoEval)
                        {
                            try
                            {
                                Engine.AddNewStack();

                                var result = await c.Execute();

                                if (result != EvalResult.NoError)
                                {
                                    Engine.LastParserStartErrorPosition = StartPos;
                                    Engine.LastParserEndErrorPosition = EndPos;
                                    return result;
                                }

                                if (Engine.StackSize != 1)
                                {
                                    Engine.LastParserStartErrorPosition = StartPos;
                                    Engine.LastParserEndErrorPosition = EndPos;
                                    return EvalResult.Failure(Engine, Error.StackSizeError, "stack size differs from 1 during string eval.");
                                }

                                var obj = Engine.StackPop();

                                if (obj == null)
                                {
                                    Engine.LastParserStartErrorPosition = StartPos;
                                    Engine.LastParserEndErrorPosition = EndPos;
                                    return EvalResult.Failure(Engine, Error.StackSizeError, "unabled to get stack value during string eval.");
                                }

                                if (obj is MOGString s2)
                                {
                                    sb.Append(s2.Value);
                                }
                                else
                                {
                                    sb.Append(obj.ToString());
                                }
                            }
                            finally
                            {
                                Engine.RemoveLastStack();
                            }
                        }
                        else
                        {
                            sb.Append(item.ToString());
                        }
                    }
                }

                /*

                try
                {
                    Value = sb.ToString();

                    //var bytes = Encoding.UTF8.GetBytes($"\"{sb}\"");
                    //var reader = new Utf8JsonReader(bytes);
                    //reader.Read();
                    //Value = reader.GetString() ?? sb.ToString();
                }
                catch
                {
                    Value = sb.ToString();
                }

                */

                Value = sb.ToString();

                return EvalResult.NoError;
            }

            return EvalResult.NoError;
        }

        private List<MOGObject>? ParseStringFormat()
        {
            // "Hello {! name} you are {! age}" ---> "Hello " {! name} " you are " {! age} = 4 items

            int index = 0;
            List<MOGObject> items = new();
            StringBuilder currentItem = new();
            bool inCode = false;

            while (index < Value.Length)
            {
                var c = Value[index++];

                if (c == '{')
                {
                    if (inCode)
                        return null;

                    inCode = true;

                    if (currentItem.Length > 0)
                    {
                        var s = new MOGString(Engine, currentItem.ToString(), 0);
                        items.Add(s);
                        currentItem.Clear();
                    }
                }
                else if (c == '}')
                {
                    if (!inCode)
                        return null;

                    var code = new MOGCode(Engine, currentItem.ToString(), 0, ExecutionContext);
                    
                    if (Bag != null && Bag is MOGCode code2)
                        code.Instance = code2.Instance;

                    foreach (var item in code.Items)
                        item.RemoveFromDebugMechanism();

                    items.Add(code);
                    currentItem.Clear();
                    inCode = false;
                }
                else
                {
                    currentItem.Append(c);
                }
            }

            if (currentItem.Length > 0)
            {
                var s = new MOGString(Engine, currentItem.ToString(), -1);
                items.Add(s);
            }

            return items;
        }

        private static (EvalResult result, string? unescaped) UnescapeString(MogwaiEngine engine, string input)
        {
            if (input.IndexOf('\\') < 0)
                return (EvalResult.NoError, input);

            var sb = new StringBuilder(input.Length);
            int i = 0;
            int len = input.Length;

            while (i < len)
            {
                char c = input[i];

                if (c == '\\')
                {
                    if (i + 1 >= len)
                    {
                        // backslash en fin de string sans caractère suivant

                        return (EvalResult.Failure(engine, Error.BadArgumentValueError, $"Unterminated escape sequence at position {i}"), null) ;
                    }

                    char next = input[i + 1];
                    switch (next)
                    {
                        case 'r': 
                            sb.Append('\r'); 
                            break;
                        
                        case 'n': 
                            sb.Append('\n'); 
                            break;

                        case 't': 
                            sb.Append('\t'); 
                            break;

                        case 'b': 
                            sb.Append('\b'); 
                            break;

                        case 'f': 
                            sb.Append('\f'); 
                            break;

                        case 'v': 
                            sb.Append('\v'); 
                            break;

                        case 'a': 
                            sb.Append('\a'); 
                            break;

                        case '0': 
                            sb.Append('\0'); 
                            break;

                        case '\\': 
                            sb.Append('\\'); 
                            break;

                        case '"': 
                            sb.Append('"'); 
                            break;
                        
                        default:
                            return (EvalResult.Failure(engine, Error.BadArgumentValueError, $"Unknown escape sequence '\\{next}' at position {i}"), null);            
                    }

                    i += 2; // on consomme \ ET le caractère échappé, jamais réexaminés
                }
                else
                {
                    sb.Append(c);
                    i++;
                }
            }

            return (EvalResult.NoError, sb.ToString());
        }
    }
}
