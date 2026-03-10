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

using MOGWAI.Exceptions;
using MOGWAI.Objects;
using MOGWAI.Primitives;
using System.Globalization;
using System.Text;

namespace MOGWAI.Engine
{
    internal class Parser
    {
        private char _currentChar;
        private int _currentIndex;
        private int _pos;
        private string _code = string.Empty;
        private StringBuilder _currentItem = new StringBuilder();
        private List<MOGObject> _parsedObjects = new();

        private int Pos
        {
            get => _pos;

            set
            {
                _pos = value;
                LastStartErrorPosition = value;
            }
        }

        public static int LastStartErrorPosition { get; private set; } = -1;

        public static int LastEndErrorPosition { get; private set; } = -1;

        public static MogwaiExecutionContext? LastExecutionContext { get; private set; }

        public List<MOGObject> ParsedObjects => _parsedObjects;

        public void Parse(MogwaiEngine engine, string code, int offsetPosition, MogwaiExecutionContext? context)
        {
            _code = code;
            _currentIndex = 0;
            _currentItem.Clear();
            _parsedObjects.Clear();

            LastExecutionContext = context;

            while (_currentIndex < _code.Length)
            {
                do
                {
                    Pos = _currentIndex + offsetPosition;
                    _currentChar = _code[_currentIndex++];

                } while (_currentIndex < _code.Length && (_currentChar == ' ' || _currentChar == '\n' || _currentChar == '\r' || _currentChar == '\t'));

                if (_currentChar == ' ' || _currentChar == '\n' || _currentChar == '\r' || _currentChar == '\t')
                    break;

                if (_currentChar == '#')
                {
                    // On cherche la fin de ligne ou la fin du code

                    do
                    {
                        Pos = _currentIndex + offsetPosition;
                        _currentChar = _code[_currentIndex++];

                    } while (_currentIndex < _code.Length && _currentChar != '\n' && _currentChar != '\r');
                }
                else if (_currentChar == '(')
                {
                    // LIST

                    MOGObject? prefix = null;

                    if (_currentItem.Length > 0)
                    {
                        // Cas de figure où un mot est suivi d'une liste sans espace.
                        // On peut traduire ça par une application du mot à tous les éléments de la liste.

                        var p = new Parser();
                        p.Parse(engine, _currentItem.ToString(), Pos - _currentItem.Length, context);

                        if (p.ParsedObjects.Count != 1 || (p.ParsedObjects[0] is not MOGWord && p._parsedObjects[0] is not MOGPrimitive))
                        {
                            LastStartErrorPosition = Pos - _currentItem.Length;
                            LastEndErrorPosition = Pos;
                            throw new MogwaiParseErrorException("unexpected character '('");
                        }

                        prefix = p.ParsedObjects[0];
                        _currentItem.Clear();
                    }

                    GetEnclosedItem('(', ')');
                    var l = new MOGList(engine, _currentItem.ToString(), Pos + 1, context);                                   

                    if (prefix != null)
                    {
                        foreach (var item in l.Items)
                            _parsedObjects.Add(item);

                        _parsedObjects.Add(prefix);
                    }
                    else
                    {
                        _parsedObjects.Add(l);
                    }

                    _currentItem.Clear();
                }
                else if (_currentChar == '{')
                {
                    // CODE

                    if (_currentItem.Length > 0)
                    {
                        LastStartErrorPosition = Pos;
                        LastEndErrorPosition = Pos;
                        throw new MogwaiParseErrorException("unexpected character '{'");
                    }

                    GetEnclosedItem('{', '}');
                    var c = new MOGCode(engine, _currentItem.ToString(), Pos + 1, context);
                    _parsedObjects.Add(c);
                    _currentItem.Clear();
                }
                else if (_currentChar == '«')
                {
                    // FUNCTION

                    if (_currentItem.Length > 0)
                    {
                        LastStartErrorPosition = Pos;
                        LastEndErrorPosition = Pos;
                        throw new MogwaiParseErrorException("unexpected character '«'");
                    }

                    GetEnclosedItem('«', '»');
                    var f = new MOGFunction(engine, _currentItem.ToString(), Pos + 1, context);
                    _parsedObjects.Add(f);
                    _currentItem.Clear();
                }
                else if (_currentChar == '[')
                {
                    // RECORD

                    MOGObject? prefix = null;

                    if (_currentItem.Length > 0)
                    {
                        // Cas de figure où un mot est suivi d'un record sans espace.
                        // On peut traduire ça par une application du mot au record, ce qui est un cas fréquent notamment pour les messages à la Objective C (ex: myObject doSomethingWith: arg1 and: arg2)
                        // Dans ce cas on ajoute le mot comme 1er item du record, ce qui permet de faire le lien entre le mot et le record et d'avoir un code plus naturel à écrire et à lire.

                        var p = new Parser();  
                        p.Parse(engine, _currentItem.ToString(), Pos - _currentItem.Length, context);

                        if (p.ParsedObjects.Count != 1 || (p.ParsedObjects[0] is not MOGWord && p._parsedObjects[0] is not MOGPrimitive))
                        {
                            LastStartErrorPosition = Pos - _currentItem.Length;
                            LastEndErrorPosition = Pos; 
                            throw new MogwaiParseErrorException("unexpected character '['");
                        }

                        prefix = p.ParsedObjects[0];
                        _currentItem.Clear();  
                    }

                    GetEnclosedItem('[', ']');

                    var parser = new Parser();
                    parser.Parse(engine, _currentItem.ToString(), Pos + 1, context);
                    var items = parser.ParsedObjects;

                    MOGObject? item0 = null;
                    MOGObject? autoEval = null;

                    if (items.Count > 0)
                    {
                        for (int i = 0; i < items.Count; i++)
                        {
                            item0 = items[i];

                            if (item0 is MOGKey)
                                break;

                            if (item0 is MOGWord word && word.Value == "!")
                            {
                                autoEval = word;
                                continue;
                            }

                            break;
                        }

                        if (item0 != null && item0 is not MOGKey)
                        {
                            // Le 1er item (sauf !) n'est pas une clé
                            // Il faut le sortir du record et le placer après (cas de l'appel style message Objective C)
                            // Si un prefix est déjà présent on est en face d'une erreur de syntaxe

                            if (prefix != null)
                            {
                                LastStartErrorPosition = Pos;
                                LastEndErrorPosition = Pos;
                                throw new MogwaiParseErrorException("unabled to define the function to call 2 times");
                            }

                            items.Remove(item0);
                        }
                        else
                        {
                            item0 = null;
                        }

                        if (autoEval != null)
                            items.Remove(autoEval);                        
                    }

                    var r = new MOGRecord(engine, items);
                    r.StartPos = Pos;
                    r.EndPos = Pos + _currentItem.Length + 1;
                    r.ExecutionContext = context;
                    r.AutoEval = autoEval != null;

                    _parsedObjects.Add(r);

                    _currentItem.Clear();

                    if (item0 != null)
                        _parsedObjects.Add(item0);

                    if (prefix != null)
                        _parsedObjects.Add(prefix);
                }
                else if (_currentChar == '"')
                {
                    // STRING

                    if (_currentItem.Length > 0)
                    {
                        LastStartErrorPosition = Pos;
                        LastEndErrorPosition = Pos;
                        throw new MogwaiParseErrorException("unexpected character '\"'");
                    }

                    GetEnclosedItem('"', '"');
                    var s = new MOGString(engine, _currentItem.ToString(), Pos);
                    s.ExecutionContext = context;
                    _parsedObjects.Add(s);
                    _currentItem.Clear();
                }
                else if (_currentChar == '\'')
                {
                    // NAME

                    if (_currentItem.Length > 0)
                    {
                        LastStartErrorPosition = Pos;
                        LastEndErrorPosition = Pos;
                        throw new MogwaiParseErrorException("unexpected character '");
                    }

                    GetEnclosedItem('\'', '\'');
                    var name = _currentItem.ToString();

                    if (name.Length == 0 || !engine.IsValidName(name))
                    {
                        LastStartErrorPosition = Pos;
                        LastEndErrorPosition = Pos + _currentItem.Length + 1;
                        throw new MogwaiParseErrorException($"invalid name '{name}'");
                    }

                    var n = new MOGName(engine, name, Pos);
                    n.ExecutionContext = context;
                    _parsedObjects.Add(n);

                    _currentItem.Clear();
                }
                else
                {
                    _currentItem.Append(_currentChar);

                    char c = ' ';

                    if (_currentIndex < _code.Length)
                    {
                        c = _code[_currentIndex];
                    }

                    if (c == ' ' || c == '\n' || c == '\r' || c == '\t')
                    {
                        var item = _currentItem.ToString();

                        if (item.Length > 0)
                            _parsedObjects.AddRange(ParseBasicWord(engine, item, Pos + 1 - item.Length, context));

                        _currentItem.Clear();
                    }
                }
            }

            var item2 = _currentItem.ToString();

            if (item2.Length > 0)
                _parsedObjects.AddRange(ParseBasicWord(engine, item2, Pos, context));

            _currentItem.Clear();

            UpdateForSugarItems(engine);
        }

        private List<MOGObject> ParseBasicWord(MogwaiEngine engine, string item, int offsetPosition, MogwaiExecutionContext? context)
        {
            if (item.Length > 0)
            {
                var hostFunctions = engine.HostFunctions;

                if (double.TryParse(item, CultureInfo.InvariantCulture, out double n1))
                {
                    var number = new MOGNumber(engine, n1, offsetPosition);
                    number.ExecutionContext = context;

                    return [number];
                }
                else if (item.StartsWith("0x"))
                {
                    if (item.Length > 2 && long.TryParse(item.Substring(2), NumberStyles.HexNumber, CultureInfo.InvariantCulture, out long n2))
                    {
                        var number = new MOGNumber(engine, n2, offsetPosition);
                        number.ExecutionContext = context;
                        return [number];
                    }
                    else
                    {
                        LastStartErrorPosition = offsetPosition;
                        LastEndErrorPosition = offsetPosition + item.Length - 1;
                        throw new MogwaiParseErrorException("invalid conversion operation");
                    }
                }
                else if (item.StartsWith("0b"))
                {
                    var content = item.Substring(2);

                    if (content.Length > 0)
                    {
                        try
                        {
                            var n = Convert.ToInt64(content, 2);
                            var number = new MOGNumber(engine, n, offsetPosition);
                            number.ExecutionContext = context;
                            return [number];
                        }
                        catch
                        {
                            LastStartErrorPosition = offsetPosition;
                            LastEndErrorPosition = offsetPosition + item.Length - 1;
                            throw new MogwaiParseErrorException("invalid conversion operation");
                        }
                    }
                    else
                    {
                        LastStartErrorPosition = offsetPosition;
                        LastEndErrorPosition = offsetPosition + item.Length - 1;
                        throw new MogwaiParseErrorException("invalid conversion operation");
                    }
                }
                else if (item.StartsWith("D:"))
                {
                    var content = item.Substring(2);

                    if (content.Length == 0)
                    {
                        var data = new MOGData(engine);
                        data.ExecutionContext = context;
                        return [data];
                    }
                    else
                    {
                        var data = new MOGData(engine, content, offsetPosition);
                        data.ExecutionContext = context;

                        return [data];
                    }
                }
                else if (item.StartsWith("B:"))
                {
                    var content = item.Substring(2);

                    if (content.Length > 0)
                    {
                        try
                        {
                            return [new MOGBinaryNumber(engine, content, offsetPosition)];
                        }
                        catch
                        {
                            LastStartErrorPosition = offsetPosition;
                            LastEndErrorPosition = offsetPosition + item.Length - 1;
                            throw;
                        }
                    }
                    else
                    {
                        LastStartErrorPosition = offsetPosition;
                        LastEndErrorPosition = offsetPosition + item.Length - 1;
                        throw new MogwaiParseErrorException("empty binary not allowed");
                    }
                }
                else if (item.StartsWith("&"))
                {
                    // REF

                    if (item.Length > 1)
                    {
                        var name = item.Substring(1);

                        if (name.Length == 0)
                        {
                            LastStartErrorPosition = offsetPosition;
                            LastEndErrorPosition = offsetPosition + item.Length - 1;
                            throw new MogwaiParseErrorException($"illegal reference name");
                        }

                        try
                        {
                            var t = new MOGRef(engine, name, offsetPosition);
                            t.ExecutionContext = context;
                            return [t];
                        }
                        catch
                        {
                            LastStartErrorPosition = offsetPosition;
                            LastEndErrorPosition = offsetPosition + item.Length - 1;
                            throw;
                        }
                    }
                    else
                    {
                        LastStartErrorPosition = offsetPosition;
                        LastEndErrorPosition = offsetPosition + item.Length - 1;
                        throw new MogwaiParseErrorException("empty reference name not allowed");
                    }
                }
                else if (item == "null")
                {
                    try
                    {
                        var n = new MOGNull(engine, offsetPosition);
                        n.ExecutionContext = context;
                        return [n];
                    }
                    catch
                    {
                        LastStartErrorPosition = offsetPosition;
                        LastEndErrorPosition = offsetPosition + item.Length - 1;
                        throw;
                    }
                }
                else if (item == "empty")
                {
                    try
                    {
                        var n = new MOGEmpty(engine, offsetPosition);
                        n.ExecutionContext = context;
                        return [n];
                    }
                    catch
                    {
                        LastStartErrorPosition = offsetPosition;
                        LastEndErrorPosition = offsetPosition + item.Length - 1;
                        throw;
                    }
                }
                else if (item == "true" || item == "false")
                {
                    try
                    {
                        var b = new MOGBoolean(engine, item == "true", offsetPosition);
                        b.ExecutionContext = context;
                        return [b];
                    }
                    catch
                    {
                        LastStartErrorPosition = offsetPosition;
                        LastEndErrorPosition = offsetPosition + item.Length - 1;
                        throw;
                    }

                }
                else if (item.StartsWith("."))
                {
                    // TYPE

                    if (item.Length > 1)
                    {
                        var name = item.Substring(1);

                        if (name.Length == 0 || !engine.TypeExists(name))
                        {
                            LastStartErrorPosition = offsetPosition;
                            LastEndErrorPosition = offsetPosition + item.Length - 1;
                            throw new MogwaiParseErrorException($"unknown type .{name}");
                        }

                        try
                        {
                            var t = new MOGType(engine, name, offsetPosition);
                            t.ExecutionContext = context;
                            return [t];
                        }
                        catch
                        {
                            LastStartErrorPosition = offsetPosition;
                            LastEndErrorPosition = offsetPosition + item.Length - 1;
                            throw;
                        }
                    }
                    else
                    {
                        LastStartErrorPosition = offsetPosition;
                        LastEndErrorPosition = offsetPosition + item.Length - 1;
                        throw new MogwaiParseErrorException("empty type name not allowed");
                    }
                }
                else if (item.Contains("->") && !item.StartsWith("->") && !item.EndsWith("->"))
                {
                    var fields = item.Split("->");

                    if (fields.Length != 2)
                    {
                        LastStartErrorPosition = Pos - item.Length + 1;
                        LastEndErrorPosition = Pos;
                        throw new MogwaiParseErrorException("invalid record->key notation");
                    }

                    if (!engine.IsValidName(fields[1]))
                    {
                        LastStartErrorPosition = Pos - item.Length + 1;
                        LastEndErrorPosition = Pos;
                        throw new MogwaiParseErrorException("invalid name for key in record->key notation");
                    }

                    var item1 = new MOGWord(engine, fields[0], offsetPosition);
                    item1.ExecutionContext = context;

                    var item2 = new MOGKey(engine, fields[1], offsetPosition + fields[0].Length + "->".Length);
                    item2.ExecutionContext = context;

                    var primitive = engine.GetPrimitive(typeof(PrimitiveGet));

                    if (primitive == null)
                    {
                        LastStartErrorPosition = Pos - item.Length + 1;
                        LastEndErrorPosition = Pos;
                        throw new MogwaiParseErrorException("internal error with get primitive in record->key notation");
                    }

                    primitive.ExecutionContext = context;

                    return [item1, item2, primitive];
                }
                else if (item.Contains("<-") && !item.StartsWith("<-") && !item.EndsWith("<-"))
                {
                    var fields = item.Split("<-");

                    if (fields.Length != 2)
                    {
                        LastStartErrorPosition = Pos - item.Length + 1;
                        LastEndErrorPosition = Pos;
                        throw new MogwaiParseErrorException("invalid record<-key notation");
                    }

                    if (!engine.IsValidName(fields[1]))
                    {
                        LastStartErrorPosition = Pos - item.Length + 1;
                        LastEndErrorPosition = Pos;
                        throw new MogwaiParseErrorException("invalid name for key in record<-key notation");
                    }

                    var item1 = new MOGWord(engine, fields[0], offsetPosition);
                    item1.ExecutionContext = context;

                    var item2 = new MOGKey(engine, fields[1], offsetPosition + fields[0].Length + "<-".Length);
                    item2.ExecutionContext = context;

                    var primitive = engine.GetPrimitive(typeof(PrimitiveSet));

                    if (primitive == null)
                    {
                        LastStartErrorPosition = Pos - item.Length + 1;
                        LastEndErrorPosition = Pos;
                        throw new MogwaiParseErrorException("internal error with set primitive in record<-key notation");
                    }

                    primitive.ExecutionContext = context;

                    if (_parsedObjects.Count > 0)
                    {
                        var value = _parsedObjects.Last();
                        _parsedObjects.RemoveAt(_parsedObjects.Count - 1);

                        return [item1, item2, value, primitive];
                    }
                    else
                    {
                        LastStartErrorPosition = Pos - item.Length + 1;
                        LastEndErrorPosition = Pos;
                        throw new MogwaiParseErrorException("missing value for record<-key notation");
                    }
                }
                else if (item.EndsWith(":") && item.Length > 1)
                {
                    // KEY

                    var name = item[..^1];

                    var k = new MOGKey(engine, name, offsetPosition);
                    k.ExecutionContext = context;

                    return [k];
                }
                else
                {
                    var p = engine.GetPrimitive(item);

                    if (p != null)
                    {
                        if (p.IsPrivate && !engine.AllowPrivatePrimitives)
                        {
                            LastStartErrorPosition = Pos;
                            LastEndErrorPosition = Pos + item.Length;
                            throw new MogwaiParseErrorException("private primitive not allowed");
                        }

                        p.ExecutionContext = context;
                        p.StartPos = offsetPosition;
                        p.EndPos = offsetPosition + p.Name.Length - 1;

                        return [p];
                    }
                    else
                    {
                        if (hostFunctions.Contains(item))
                        {
                            var hfunc = new MOGHostFunction(engine, item, offsetPosition);
                            hfunc.ExecutionContext = context;
                            return [hfunc];
                        }

                        var w = new MOGWord(engine, item, offsetPosition);
                        w.ExecutionContext = context;

                        return [w];
                    }
                }
            }
            else
            {
                throw new MogwaiEmptyWordException();
            }
        }

        private void GetEnclosedItem(char firstChar, char lastChar)
        {
            int level = 0;
            char currentChar = '\0';

            while (_currentIndex < _code.Length)
            {
                currentChar = _code[_currentIndex++];
                
                if (currentChar == lastChar)
                {
                    if (_currentIndex > 1 && _code[_currentIndex - 2] == '\\')
                    {
                        // Caractère d'échappement, on n'augmente pas le niveau
                    }
                    else if (level == 0 || --level < 0)
                    {
                        return;
                    }
                }
                else if (currentChar == firstChar)
                {
                    if (_currentIndex > 1 && _code[_currentIndex - 2] == '\\')
                    {
                        // Caractère d'échappement, on n'augmente pas le niveau
                    }
                    else
                    {
                        level++;
                    }
                }

                _currentItem.Append(currentChar);
            }

            if (currentChar != lastChar)
            {
                LastStartErrorPosition = Pos;
                LastEndErrorPosition = Pos + _currentItem.Length;
                throw new MogwaiParseErrorException($"missing closing character '{lastChar}'");
            }
        }

        private void UpdateForSugarItems(MogwaiEngine engine)
        {
            for (int i = 0; i < _parsedObjects.Count; i++)
            {
                var item = _parsedObjects[i];

                if (item is MOGWord word)
                {
                    bool result = true;

                    if (word.Value == "if")
                    {
                        result = UpdateForIfSugar(engine, i);
                    }
                    else if (word.Value == "foreach")
                    {
                        result = UpdateForForeachSugar(engine, i);
                    }
                    else if (word.Value == "for")
                    {
                        result = UpdateForForSugar(engine, i);
                    }
                    else if (word.Value == "repeat")
                    {
                        result = UpdateForRepeatSugar(engine, i);
                    }
                    else if (word.Value == "while")
                    {
                        result = UpdateForWhileSugar(engine, i);
                    }
                    else if (word.Value == "do")
                    {
                        result = UpdateForDoWhileSugar(engine, i);
                    }
                    else if (word.Value == "to")
                    {
                        result = UpdateForDefuncSugar(engine, i);
                    }
                    else if (word.Value == "forever")
                    {
                        result = UpdateForForeverSugar(engine, i);
                    }
                    else if (word.Value == "timer")
                    {
                        result = UpdateForTimerSugar(engine, i);
                    }
                    else if (word.Value == "onEvent")
                    {
                        result = UpdateForOnEventSugar(engine, i);
                    }
                    else if (word.Value == "during")
                    {
                        result = UpdateForDuringSugar(engine, i);
                    }
                    else if (word.Value == "trap")
                    {
                        result = UpdateForTrapSugar(engine, i);
                    }
                    else if (word.Value == "guard")
                    {
                        result = UpdateForGuardSugar(engine, i);
                    }
                    else if (word.Value == "->" || word.Value == "->+" || word.Value == "->-" || word.Value == "->*" || word.Value == "->/")
                    {
                        result = UpdateForStoOperationsSugar(engine, i, word.Value);
                    }
                    else if (word.Value == "=>")
                    {
                        result = UpdateForDeclareSugar(engine, i);
                    }
                    else if (word.Value == "after")
                    {
                        result = UpdateForAfterSugar(engine, i);
                    }
                    else if (word.Value == "switch")
                    {
                        result = UpdateForSwitchSugar(engine, i);
                    }
                    else if (word.Value == "task")
                    {
                        result = UpdateForTaskSugar(engine, i);
                    }

                    if (!result)
                    {
                        LastStartErrorPosition = word.StartPos;
                        LastEndErrorPosition = word.EndPos;

                        throw new MogwaiParseErrorException();
                    }
                }
            }
        }

        private bool UpdateForTaskSugar(MogwaiEngine engine, int index)
        {
            // 0 task
            // 1 name (objet to name)
            // 2 do
            // 3 code

            // 0 task
            // 1 name (objet to name)
            // 2 start
            // 3 with
            // 4 objet (parameter)

            // 0 task
            // 1 name
            // 2 send
            // 3 object (message)

            try
            {
                if (_parsedObjects.Count - index >= 4)
                {
                    var name = _parsedObjects[index + 1];

                    var doOrStartOrSendWord = _parsedObjects[index + 2] as MOGWord;

                    if (doOrStartOrSendWord != null)
                    {
                        if (doOrStartOrSendWord.Value == "do")
                        {
                            var code = _parsedObjects[index + 3] as MOGCode;

                            if (code != null)
                            {
                                var primitive = engine.GetPrimitive(typeof(PrimitiveTASKDEF));

                                if (primitive != null)
                                {
                                    primitive.StartPos = _parsedObjects[index].StartPos;
                                    primitive.EndPos = _parsedObjects[index].EndPos;
                                    primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;

                                    for (int i = 0; i < 4; i++)
                                        _parsedObjects.RemoveAt(index);

                                    // name
                                    // function
                                    // TASK.DEF

                                    _parsedObjects.Insert(index, primitive);
                                    _parsedObjects.Insert(index, code.ToFunction());
                                    _parsedObjects.Insert(index, name);

                                    return true;
                                }
                            }
                        }
                        else if (doOrStartOrSendWord.Value == "send")
                        {
                            var message = _parsedObjects[index + 3];

                            var primitive = engine.GetPrimitive(typeof(PrimitiveTASKSEND));

                            if (primitive != null)
                            {
                                primitive.StartPos = _parsedObjects[index].StartPos;
                                primitive.EndPos = _parsedObjects[index].EndPos;
                                primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;

                                for (int i = 0; i < 4; i++)
                                    _parsedObjects.RemoveAt(index);

                                // name
                                // message
                                // TASK.SEND

                                _parsedObjects.Insert(index, primitive);
                                _parsedObjects.Insert(index, message);
                                _parsedObjects.Insert(index, name);

                                return true;
                            }
                        }
                        else if (doOrStartOrSendWord.Value == "start" && _parsedObjects.Count - index >= 5)
                        {
                            var withWord = _parsedObjects[index + 3] as MOGWord;

                            if (withWord != null && withWord.Value == "with")
                            {
                                var parameter = _parsedObjects[index + 4];

                                if (parameter != null)
                                {
                                    var primitive = engine.GetPrimitive(typeof(PrimitiveTASKSTART));

                                    if (primitive != null)
                                    {
                                        primitive.StartPos = _parsedObjects[index].StartPos;
                                        primitive.EndPos = _parsedObjects[index].EndPos;
                                        primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;

                                        for (int i = 0; i < 5; i++)
                                            _parsedObjects.RemoveAt(index);

                                        // name
                                        // parameter
                                        // TASKSTART

                                        _parsedObjects.Insert(index, primitive);
                                        _parsedObjects.Insert(index, parameter);
                                        _parsedObjects.Insert(index, name);

                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {

            }

            return false;
        }

        private bool UpdateForSwitchSugar(MogwaiEngine engine, int index)
        {
            // 0 switch
            // 1 code

            // code =
            // {
            //  (test) then {code}
            //  (test) then {code}
            //  (test) then {code}
            // }

            try
            {
                if (_parsedObjects.Count - index >= 1)
                {
                    var globalCode = _parsedObjects[index + 1] as MOGCode;

                    if (globalCode != null)
                    {
                        // Il faut un nombre paire d'items dans le code

                        if (globalCode.Items.Count % 3 == 0)
                        {
                            // Il faut une succession d'une liste (condition) et du mot then et d'un code

                            for (int i = 0; i < globalCode.Items.Count; i += 3)
                            {
                                var condition = globalCode.Items[i] as MOGList;
                                var wordThen = globalCode.Items[i + 1] as MOGWord;
                                var code = globalCode.Items[i + 2] as MOGCode;

                                if (condition == null || wordThen == null || wordThen.Value != "then" || code == null)
                                    return false;
                            }

                            var primitive = engine.GetPrimitive(typeof(PrimitiveSWITCH));

                            if (primitive != null)
                            {
                                // On transforme les listes en code pour la vraie syntax de SWITCH

                                primitive.StartPos = _parsedObjects[index].StartPos;
                                primitive.EndPos = _parsedObjects[index].EndPos;
                                primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;

                                var finalCode = new MOGCode(engine, "", 0, _parsedObjects[index].ExecutionContext);
                                finalCode.StartPos = globalCode.StartPos;
                                finalCode.EndPos = globalCode.EndPos;

                                for (int i = 0; i < globalCode.Items.Count; i += 3)
                                {
                                    var condition = (globalCode.Items[i] as MOGList)!.ToCode();
                                    var code = globalCode.Items[i + 2] as MOGCode;

                                    finalCode.Items.Add(condition!);
                                    finalCode.Items.Add(code!);
                                }

                                // On fabrique le code final

                                _parsedObjects.RemoveAt(index);
                                _parsedObjects.RemoveAt(index);

                                _parsedObjects.Insert(index, primitive);
                                _parsedObjects.Insert(index, finalCode);

                                return true;
                            }
                        }
                    }
                }
            }
            catch
            {

            }

            return false;
        }

        private bool UpdateForAfterSugar(MogwaiEngine engine, int index)
        {
            // 0 after
            // 1 object as number
            // 2 do
            // 3 code

            try
            {
                if (_parsedObjects.Count - index >= 4)
                {
                    var interval = _parsedObjects[index + 1];

                    if (interval != null)
                    {
                        var doWord = _parsedObjects[index + 2] as MOGWord;

                        if (doWord != null && doWord.Value == "do")
                        {
                            var code = _parsedObjects[index + 3] as MOGCode;

                            if (code != null)
                            {
                                var primitive = engine.GetPrimitive(typeof(PrimitiveLATER));

                                if (primitive != null)
                                {
                                    primitive.StartPos = _parsedObjects[index].StartPos;
                                    primitive.EndPos = _parsedObjects[index].EndPos;
                                    primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;

                                    for (int i = 0; i < 4; i++)
                                        _parsedObjects.RemoveAt(index);

                                    // function
                                    // Interval
                                    // LATER

                                    _parsedObjects.Insert(index, primitive);
                                    _parsedObjects.Insert(index, interval);
                                    _parsedObjects.Insert(index, code.ToFunction());

                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {

            }

            return false;
        }

        private bool UpdateForStoOperationsSugar(MogwaiEngine engine, int index, string word)
        {
            // -1 value
            //  0 word -> ->+ ->- ->* ->/
            //  1 name

            try
            {
                if (_parsedObjects.Count - index >= 2)
                {
                    var name = _parsedObjects[index + 1] as MOGName;

                    if (name != null)
                    {
                        MOGPrimitive? primitive = null;

                        switch (word)
                        {
                            case "->":
                                primitive = engine.GetPrimitive(typeof(PrimitiveSTO));
                                break;

                            case "->+":
                                primitive = engine.GetPrimitive(typeof(PrimitiveSTOPLUS));
                                break;

                            case "->-":
                                primitive = engine.GetPrimitive(typeof(PrimitiveSTOSUBSTRACT));
                                break;

                            case "->*":
                                primitive = engine.GetPrimitive(typeof(PrimitiveSTOMULTIPLY));
                                break;

                            case "->/":
                                primitive = engine.GetPrimitive(typeof(PrimitiveSTODIVIDE));
                                break;

                            default:
                                return false;
                        }

                        if (primitive != null)
                        {
                            primitive.StartPos = _parsedObjects[index].StartPos;
                            primitive.EndPos = _parsedObjects[index].EndPos;
                            primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;

                            _parsedObjects.RemoveAt(index);
                            _parsedObjects.RemoveAt(index);

                            _parsedObjects.Insert(index, primitive);
                            _parsedObjects.Insert(index, name);

                            return true;
                        }
                    }
                }
            }
            catch
            {

            }

            return false;
        }

        private bool UpdateForDeclareSugar(MogwaiEngine engine, int index)
        {
            // -1 value
            //  0 =>
            //  1 name

            try
            {
                if (_parsedObjects.Count - index >= 2)
                {
                    var name = _parsedObjects[index + 1] as MOGName;

                    if (name != null)
                    {
                        MOGPrimitive? primitive = engine.GetPrimitive(typeof(PrimitiveDECLARE));

                        if (primitive != null)
                        {
                            primitive.StartPos = _parsedObjects[index].StartPos;
                            primitive.EndPos = _parsedObjects[index].EndPos;
                            primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;

                            _parsedObjects.RemoveAt(index);
                            _parsedObjects.RemoveAt(index);

                            _parsedObjects.Insert(index, primitive);
                            _parsedObjects.Insert(index, name);

                            return true;
                        }
                    }
                }
            }
            catch
            {

            }

            return false;
        }

        private bool UpdateForGuardSugar(MogwaiEngine engine, int index)
        {
            // 0 guard
            // 1 code
            // 2 else
            // 3 errorCode

            try
            {
                if (_parsedObjects.Count - index > 3)
                {
                    var code = _parsedObjects[index + 1] as MOGCode;

                    if (code != null)
                    {
                        var elseWord = _parsedObjects[index + 2] as MOGWord;

                        if (elseWord != null && elseWord.Value == "else")
                        {
                            var errorCode = _parsedObjects[index + 3] as MOGCode;

                            if (errorCode != null)
                            {
                                var primitive = engine.GetPrimitive(typeof(PrimitiveErrorGuard));

                                if (primitive != null)
                                {
                                    primitive.StartPos = _parsedObjects[index].StartPos;
                                    primitive.EndPos = _parsedObjects[index].EndPos;
                                    primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;

                                    for (int i = 0; i < 4; i++)
                                        _parsedObjects.RemoveAt(index);

                                    _parsedObjects.Insert(index, primitive);
                                    _parsedObjects.Insert(index, errorCode);
                                    _parsedObjects.Insert(index, code);

                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {

            }

            return false;
        }

        private bool UpdateForTrapSugar(MogwaiEngine engine, int index)
        {
            // 0 trap
            // 1 code

            try
            {
                if (_parsedObjects.Count - index > 1)
                {
                    var code = _parsedObjects[index + 1] as MOGCode;

                    if (code != null)
                    {
                        var primitive = engine.GetPrimitive(typeof(PrimitiveErrorTrap));

                        if (primitive != null)
                        {
                            primitive.StartPos = _parsedObjects[index].StartPos;
                            primitive.EndPos = _parsedObjects[index].EndPos;
                            primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;

                            _parsedObjects.RemoveAt(index);
                            _parsedObjects.RemoveAt(index);

                            _parsedObjects.Insert(index, primitive);
                            _parsedObjects.Insert(index, code);

                            return true;
                        }
                    }
                }
            }
            catch
            {

            }

            return false;
        }

        private bool UpdateForDuringSugar(MogwaiEngine engine, int index)
        {
            // 0 during
            // 1 object as number
            // 2 do
            // 3 code

            try
            {
                if (_parsedObjects.Count - index > 3)
                {
                    var number = _parsedObjects[index + 1];

                    if (number != null)
                    {
                        var doWord = _parsedObjects[index + 2] as MOGWord;

                        if (doWord != null && doWord.Value == "do")
                        {
                            var code = _parsedObjects[index + 3] as MOGCode;

                            if (code != null)
                            {
                                var primitive = engine.GetPrimitive(typeof(PrimitiveDURING));

                                if (primitive != null)
                                {
                                    primitive.StartPos = _parsedObjects[index].StartPos;
                                    primitive.EndPos = _parsedObjects[index].EndPos;
                                    primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;

                                    for (int i = 0; i < 4; i++)
                                        _parsedObjects.RemoveAt(index);

                                    _parsedObjects.Insert(index, primitive);
                                    _parsedObjects.Insert(index, code);
                                    _parsedObjects.Insert(index, number);

                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {

            }

            return false;
        }

        private bool UpdateForOnEventSugar(MogwaiEngine engine, int index)
        {
            // 0 onEvent
            // 1 name
            // 2 do
            // 3 code

            try
            {
                if (_parsedObjects.Count - index > 3)
                {
                    var name = _parsedObjects[index + 1] as MOGName;

                    if (name != null)
                    {
                        var doWord = _parsedObjects[index + 2] as MOGWord;

                        if (doWord != null && doWord.Value == "do")
                        {
                            var code = _parsedObjects[index + 3] as MOGCode;

                            if (code != null)
                            {
                                var primitive = engine.GetPrimitive(typeof(PrimitiveEVENT));

                                if (primitive != null)
                                {
                                    primitive.StartPos = _parsedObjects[index].StartPos;
                                    primitive.EndPos = _parsedObjects[index].EndPos;
                                    primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;

                                    for (int i = 0; i < 4; i++)
                                        _parsedObjects.RemoveAt(index);

                                    _parsedObjects.Insert(index, primitive);
                                    _parsedObjects.Insert(index, name);
                                    _parsedObjects.Insert(index, code.ToFunction());

                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {

            }

            return false;
        }

        private bool UpdateForTimerSugar(MogwaiEngine engine, int index)
        {
            // 0 timer
            // 1 name
            // 2 every or after
            // 3 interval (object)
            // 4 do
            // 5 code

            try
            {
                if (_parsedObjects.Count - index > 5)
                {
                    var name = _parsedObjects[index + 1] as MOGName;

                    if (name != null)
                    {
                        var afterOrEveryWord = _parsedObjects[index + 2] as MOGWord;

                        if (afterOrEveryWord != null && (afterOrEveryWord.Value == "after" || afterOrEveryWord.Value == "every"))
                        {
                            bool every = afterOrEveryWord.Value == "every";

                            var interval = _parsedObjects[index + 3];

                            if (interval != null)
                            {
                                var doWord = _parsedObjects[index + 4] as MOGWord;

                                if (doWord != null && doWord.Value == "do")
                                {
                                    var code = _parsedObjects[index + 5] as MOGCode;

                                    if (code != null)
                                    {
                                        MOGPrimitive? primitive = null;

                                        if (every)
                                        {
                                            primitive = engine.GetPrimitive(typeof(PrimitiveEVERY));
                                        }
                                        else
                                        {
                                            primitive = engine.GetPrimitive(typeof(PrimitiveAFTER));
                                        }

                                        if (primitive != null)
                                        {
                                            primitive.StartPos = _parsedObjects[index].StartPos;
                                            primitive.EndPos = _parsedObjects[index].EndPos;
                                            primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;

                                            for (int i = 0; i <= 5; i++)
                                                _parsedObjects.RemoveAt(index);

                                            _parsedObjects.Insert(index, primitive);
                                            _parsedObjects.Insert(index, name);
                                            _parsedObjects.Insert(index, interval);
                                            _parsedObjects.Insert(index, code.ToFunction());

                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {

            }

            return false;
        }

        private bool UpdateForForeverSugar(MogwaiEngine engine, int index)
        {
            // 0 forever
            // 1 do
            // 2 code

            try
            {
                if (_parsedObjects.Count - index > 2)
                {
                    var doWord = _parsedObjects[index + 1] as MOGWord;

                    if (doWord != null && doWord.Value == "do")
                    {
                        var code = _parsedObjects[index + 2] as MOGCode;

                        if (code != null)
                        {
                            var primitive = engine.GetPrimitive(typeof(PrimitiveFOREVER));

                            if (primitive != null)
                            {
                                primitive.StartPos = _parsedObjects[index].StartPos;
                                primitive.EndPos = _parsedObjects[index + 1].EndPos;
                                primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;

                                _parsedObjects.RemoveAt(index);
                                _parsedObjects.RemoveAt(index);
                                _parsedObjects.RemoveAt(index);

                                _parsedObjects.Insert(index, primitive);
                                _parsedObjects.Insert(index, code);

                                return true;
                            }
                        }
                    }
                }
            }
            catch
            {

            }

            return false;
        }

        private bool UpdateForDefuncSugar(MogwaiEngine engine, int index)
        {
            var primitiveDEFUNC = engine.GetPrimitive(typeof(PrimitiveDEFUNC));

            if (primitiveDEFUNC == null)
                return false;

            if (engine.CheckCodeFootprint(_parsedObjects, index, "to", null, "do", null))
            {
                // to 'name' do { code }

                // 0 to
                // 1 name
                // 2 do
                // 3 code

                var name = _parsedObjects[index + 1] as MOGName;

                if (name != null)
                {
                    var code = _parsedObjects[index + 3] as MOGCode;

                    if (code != null)
                    {
                        primitiveDEFUNC.StartPos = _parsedObjects[index].StartPos;
                        primitiveDEFUNC.EndPos = _parsedObjects[index].EndPos;
                        primitiveDEFUNC.ExecutionContext = _parsedObjects[index].ExecutionContext;

                        for (int i = 0; i < 4; i++)
                            _parsedObjects.RemoveAt(index);

                        var function = code.ToFunction();
                        function.Name = name.Value;

                        _parsedObjects.Insert(index, primitiveDEFUNC);
                        _parsedObjects.Insert(index, name);
                        _parsedObjects.Insert(index, function);

                        return true;
                    }
                }
            }
            else if (engine.CheckCodeFootprint(_parsedObjects, index, "to", null, "with", null, "do", null))
            {
                // to 'name' with [record] do { code }  

                // 0 to
                // 1 name
                // 2 with
                // 3 record
                // 4 do
                // 5 code

                var name = _parsedObjects[index + 1] as MOGName;

                if (name != null)
                {
                    var paramsRecord = _parsedObjects[index + 3] as MOGRecord;

                    if (paramsRecord != null)
                    {
                        var code = _parsedObjects[index + 5] as MOGCode;

                        if (code != null)
                        {
                            var primitiveStackToSafeVars = engine.GetPrimitive(typeof(PrimitiveStackToSafeVars));

                            if (primitiveStackToSafeVars != null)
                            {
                                primitiveDEFUNC.StartPos = _parsedObjects[index].StartPos;
                                primitiveDEFUNC.EndPos = _parsedObjects[index].EndPos;
                                primitiveDEFUNC.ExecutionContext = _parsedObjects[index].ExecutionContext;

                                primitiveStackToSafeVars.PauseAllowed = false;
                                primitiveStackToSafeVars.ExecutionContext = _parsedObjects[index].ExecutionContext;

                                for (int i = 0; i < 6; i++)
                                    _parsedObjects.RemoveAt(index);

                                var function = code.ToFunction();
                                function.Name = name.Value;

                                // We need to modify the function's code to incorporate the parameters.

                                function.Items.Insert(0, primitiveStackToSafeVars);
                                function.Items.Insert(0, paramsRecord);

                                _parsedObjects.Insert(index, primitiveDEFUNC);
                                _parsedObjects.Insert(index, name);
                                _parsedObjects.Insert(index, function);

                                return true;
                            }
                        }
                    }
                }
            }
            else if (engine.CheckCodeFootprint(_parsedObjects, index, "to", null, "params", null, "do", null))
            {
                // to'name' params [record] do { code }

                // 0 to
                // 1 name
                // 2 params
                // 3 record
                // 4 do
                // 5 code

                var name = _parsedObjects[index + 1] as MOGName;

                if (name != null)
                {
                    var paramsRecord = _parsedObjects[index + 3] as MOGRecord;

                    if (paramsRecord != null)
                    {
                        var code = _parsedObjects[index + 5] as MOGCode;

                        if (code != null)
                        {
                            var primitiveStackToParams = engine.GetPrimitive(typeof(PrimitiveStackToParams));

                            if (primitiveStackToParams != null)
                            {
                                primitiveDEFUNC.StartPos = _parsedObjects[index].StartPos;
                                primitiveDEFUNC.EndPos = _parsedObjects[index].EndPos;
                                primitiveDEFUNC.ExecutionContext = _parsedObjects[index].ExecutionContext;

                                primitiveStackToParams.PauseAllowed = false;
                                primitiveStackToParams.ExecutionContext = _parsedObjects[index].ExecutionContext;

                                for (int i = 0; i < 6; i++)
                                    _parsedObjects.RemoveAt(index);

                                var function = code.ToFunction();
                                function.Name = name.Value;

                                // We need to modify the function's code to incorporate the parameters.

                                function.Items.Insert(0, primitiveStackToParams);
                                function.Items.Insert(0, paramsRecord);

                                _parsedObjects.Insert(index, primitiveDEFUNC);
                                _parsedObjects.Insert(index, name);
                                _parsedObjects.Insert(index, function);

                                return true;
                            }
                        }
                    }
                }
            }
            else if (engine.CheckCodeFootprint(_parsedObjects, index, "to", null, "returns", null, "do", null))
            {
                // to 'name' returns (list) do { code }

                // 0 to
                // 1 name
                // 2 returns
                // 3 list
                // 4 do
                // 5 code

                var name = _parsedObjects[index + 1] as MOGName;

                if (name != null)
                {
                    var returns = _parsedObjects[index + 3] as MOGList;

                    if (returns != null && returns.CheckJusteOneType(typeof(MOGType)))
                    {
                        var code = _parsedObjects[index + 5] as MOGCode;

                        if (code != null)
                        {
                            var primitiveCheck = engine.GetPrimitive(typeof(PrimitiveStackCheck));

                            if (primitiveCheck != null)
                            {
                                primitiveDEFUNC.StartPos = _parsedObjects[index].StartPos;
                                primitiveDEFUNC.EndPos = _parsedObjects[index].EndPos;
                                primitiveDEFUNC.ExecutionContext = _parsedObjects[index].ExecutionContext;

                                for (int i = 0; i < 6; i++)
                                    _parsedObjects.RemoveAt(index);

                                var codeFunction = code.ToFunction();
                                codeFunction.AutoEval = true;
                                codeFunction.Name = name.Value;

                                var bodyItems = new List<MOGObject>();
                                bodyItems.Add(codeFunction);
                                bodyItems.Add(returns);
                                bodyItems.Add(primitiveCheck);

                                var body = new MOGFunction(engine, bodyItems);

                                _parsedObjects.Insert(index, primitiveDEFUNC);
                                _parsedObjects.Insert(index, name);
                                _parsedObjects.Insert(index, body);

                                return true;
                            }
                        }
                    }
                }
            }
            else if (engine.CheckCodeFootprint(_parsedObjects, index, "to", null, "with", null, "returns", null, "do", null))
            {
                // to 'name' with [record] returns (list) do { code }

                // 0 to
                // 1 name
                // 2 with
                // 3 record
                // 4 returns
                // 5 list
                // 6 do
                // 7 code

                var name = _parsedObjects[index + 1] as MOGName;

                if (name != null)
                {
                    var paramsRecord = _parsedObjects[index + 3] as MOGRecord;

                    if (paramsRecord != null)
                    {
                        var returns = _parsedObjects[index + 5] as MOGList;

                        if (returns != null && returns.CheckJusteOneType(typeof(MOGType)))
                        {
                            var code = _parsedObjects[index + 7] as MOGCode;

                            if (code != null)
                            {
                                var primitiveStackToSafeVars = engine.GetPrimitive(typeof(PrimitiveStackToSafeVars));

                                if (primitiveStackToSafeVars != null)
                                {
                                    var primitiveCheck = engine.GetPrimitive(typeof(PrimitiveStackCheck));

                                    if (primitiveCheck != null)
                                    {
                                        primitiveDEFUNC.StartPos = _parsedObjects[index].StartPos;
                                        primitiveDEFUNC.EndPos = _parsedObjects[index].EndPos;
                                        primitiveDEFUNC.ExecutionContext = _parsedObjects[index].ExecutionContext;

                                        primitiveStackToSafeVars.PauseAllowed = false;
                                        primitiveStackToSafeVars.ExecutionContext = _parsedObjects[index].ExecutionContext;

                                        for (int i = 0; i < 8; i++)
                                            _parsedObjects.RemoveAt(index);

                                        var codeFunction = code.ToFunction();
                                        codeFunction.AutoEval = true;
                                        codeFunction.Name = name.Value;

                                        codeFunction.Items.Insert(0, primitiveStackToSafeVars);
                                        codeFunction.Items.Insert(0, paramsRecord);

                                        var bodyItems = new List<MOGObject>();
                                        bodyItems.Add(codeFunction);
                                        bodyItems.Add(returns);
                                        bodyItems.Add(primitiveCheck);

                                        var body = new MOGFunction(engine, bodyItems);

                                        _parsedObjects.Insert(index, primitiveDEFUNC);
                                        _parsedObjects.Insert(index, name);
                                        _parsedObjects.Insert(index, body);

                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else if (engine.CheckCodeFootprint(_parsedObjects, index, "to", null, "params", null, "returns", null, "do", null))
            {
                // to 'name' params [record] returns [list] do { code }

                // 0 to
                // 1 name
                // 2 params
                // 3 record
                // 4 returns
                // 5 list
                // 6 do
                // 7 code

                var name = _parsedObjects[index + 1] as MOGName;

                if (name != null)
                {
                    var paramsRecord = _parsedObjects[index + 3] as MOGRecord;

                    if (paramsRecord != null)
                    {
                        var returns = _parsedObjects[index + 5] as MOGList;

                        if (returns != null && returns.CheckJusteOneType(typeof(MOGType)))
                        {
                            var code = _parsedObjects[index + 7] as MOGCode;

                            if (code != null)
                            {
                                var primitiveStackToParams = engine.GetPrimitive(typeof(PrimitiveStackToParams));

                                if (primitiveStackToParams != null)
                                {
                                    var primitiveCheck = engine.GetPrimitive(typeof(PrimitiveStackCheck));

                                    if (primitiveCheck != null)
                                    {
                                        primitiveDEFUNC.StartPos = _parsedObjects[index].StartPos;
                                        primitiveDEFUNC.EndPos = _parsedObjects[index].EndPos;
                                        primitiveDEFUNC.ExecutionContext = _parsedObjects[index].ExecutionContext;

                                        primitiveStackToParams.PauseAllowed = false;
                                        primitiveStackToParams.ExecutionContext = _parsedObjects[index].ExecutionContext;

                                        for (int i = 0; i < 8; i++)
                                            _parsedObjects.RemoveAt(index);

                                        var codeFunction = code.ToFunction();
                                        codeFunction.AutoEval = true;
                                        codeFunction.Name = name.Value;

                                        codeFunction.Items.Insert(0, primitiveStackToParams);
                                        codeFunction.Items.Insert(0, paramsRecord);

                                        var bodyItems = new List<MOGObject>();
                                        bodyItems.Add(codeFunction);
                                        bodyItems.Add(returns);
                                        bodyItems.Add(primitiveCheck);

                                        var body = new MOGFunction(engine, bodyItems);

                                        _parsedObjects.Insert(index, primitiveDEFUNC);
                                        _parsedObjects.Insert(index, name);
                                        _parsedObjects.Insert(index, body);

                                        return true;
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        private bool UpdateForDoWhileSugar(MogwaiEngine engine, int index)
        {
            // 0 do
            // 1 {code}
            // 2 while
            // 3 (condition)

            try
            {
                if (index >= 1 && _parsedObjects.Count - index >= 3)
                {
                    var code = _parsedObjects[index + 1] as MOGCode;

                    if (code != null)
                    {
                        var whileWord = _parsedObjects[index + 2] as MOGWord;

                        if (whileWord != null && whileWord.Value == "while")
                        {
                            var condition = _parsedObjects[index + 3] as MOGList;

                            if (condition != null)
                            {
                                var primitive = engine.GetPrimitive(typeof(PrimitiveDOWHILE));

                                if (primitive != null)
                                {
                                    primitive.StartPos = _parsedObjects[index].StartPos;
                                    primitive.EndPos = _parsedObjects[index].EndPos;
                                    primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;

                                    for (int i = 0; i < 3; i++)
                                        _parsedObjects.RemoveAt(index);

                                    // condition
                                    // code
                                    // DOWHILE

                                    _parsedObjects.Insert(index, primitive);
                                    _parsedObjects.Insert(index, code);
                                    _parsedObjects.Insert(index, condition.ToCode());

                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {

            }

            return false;
        }

        private bool UpdateForWhileSugar(MogwaiEngine engine, int index)
        {
            // 0 while
            // 1 (condition)
            // 2 do
            // 3 {code}

            try
            {
                if (_parsedObjects.Count - index >= 4)
                {
                    var condition = _parsedObjects[index + 1] as MOGList;

                    if (condition != null)
                    {
                        var doWord = _parsedObjects[index + 2] as MOGWord;

                        if (doWord != null && doWord.Value == "do")
                        {
                            var code = _parsedObjects[index + 3] as MOGCode;

                            if (code != null)
                            {
                                var primitive = engine.GetPrimitive(typeof(PrimitiveWHILE));

                                if (primitive != null)
                                {
                                    primitive.StartPos = _parsedObjects[index].StartPos;
                                    primitive.EndPos = _parsedObjects[index].EndPos;
                                    primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;

                                    for (int i = 0; i < 4; i++)
                                        _parsedObjects.RemoveAt(index);

                                    // number
                                    // code
                                    // WHILE

                                    _parsedObjects.Insert(index, primitive);
                                    _parsedObjects.Insert(index, code);
                                    _parsedObjects.Insert(index, condition.ToCode());

                                    return true;
                                }
                            }
                        }
                    }
                }
            }
            catch
            {

            }

            return false;
        }

        private bool UpdateForRepeatSugar(MogwaiEngine engine, int index)
        {
            // 0 repeat
            // 1 code

            try
            {
                if (_parsedObjects.Count - index >= 1)
                {
                    var code = _parsedObjects[index + 1] as MOGCode;

                    if (code != null)
                    {
                        var primitive = engine.GetPrimitive(typeof(PrimitiveREPEAT));

                        if (primitive != null)
                        {
                            primitive.StartPos = _parsedObjects[index].StartPos;
                            primitive.EndPos = _parsedObjects[index].EndPos;
                            primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;

                            for (int i = 0; i < 2; i++)
                                _parsedObjects.RemoveAt(index);

                            // code
                            // REPEAT

                            _parsedObjects.Insert(index, primitive);
                            _parsedObjects.Insert(index, code);

                            return true;
                        }

                    }
                }
            }
            catch
            {

            }

            return false;
        }

        private bool UpdateForForeachSugar(MogwaiEngine engine, int index)
        {
            // -1 items
            // 0 foreach
            // 1 name
            // 2 do
            // 3 code

            try
            {
                if (engine.CheckCodeFootprint(_parsedObjects, index, "foreach", null, "do", null))
                {
                    var name = _parsedObjects[index + 1] as MOGName;

                    if (name != null)
                    {
                        var code = _parsedObjects[index + 3] as MOGCode;

                        if (code != null)
                        {
                            var primitive = engine.GetPrimitive(typeof(PrimitiveFOREACH));

                            if (primitive != null)
                            {
                                primitive.StartPos = _parsedObjects[index].StartPos;
                                primitive.EndPos = _parsedObjects[index].EndPos;
                                primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;

                                for (int i = 0; i < 4; i++)
                                    _parsedObjects.RemoveAt(index);

                                // On crée le vrai FOREACH
                                // 0 items
                                // 1 name
                                // 2 code
                                // 3 FOREACH

                                _parsedObjects.Insert(index, primitive);
                                _parsedObjects.Insert(index, code);
                                _parsedObjects.Insert(index, name);

                                return true;
                            }
                        }

                    }
                }
                else if (engine.CheckCodeFootprint(_parsedObjects, index, "foreach", null, "transform", null))
                {
                    var name = _parsedObjects[index + 1] as MOGName;

                    if (name != null)
                    {
                        var code = _parsedObjects[index + 3] as MOGCode;

                        if (code != null)
                        {
                            var primitive = engine.GetPrimitive(typeof(PrimitiveFOREACHTRANSFORM));

                            if (primitive != null)
                            {
                                primitive.StartPos = _parsedObjects[index].StartPos;
                                primitive.EndPos = _parsedObjects[index].EndPos;
                                primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;

                                for (int i = 0; i < 4; i++)
                                    _parsedObjects.RemoveAt(index);

                                // On crée le vrai FOREACHTRANSFORM
                                // 0 items
                                // 1 name
                                // 2 code
                                // 3 FOREACHTRANSFORM

                                _parsedObjects.Insert(index, primitive);
                                _parsedObjects.Insert(index, code);
                                _parsedObjects.Insert(index, name);

                                return true;
                            }
                        }
                    }
                }
            }
            catch
            {

            }

            return false;
        }

        private bool UpdateForIfSugar(MogwaiEngine engine, int index)
        {
            // 0 if
            // 1 (condition)
            // 2 then
            // 3 {codeTrue}

            // 0 if
            // 1 (condition)
            // 2 then
            // 3 {codeTrue}
            // 4 else
            // 5 {codeFalse}

            try
            {
                if (_parsedObjects.Count - index >= 3)
                {
                    var condition = _parsedObjects[index + 1] as MOGList;

                    if (condition != null)
                    {
                        var wordThen = _parsedObjects[index + 2] as MOGWord;

                        if (wordThen != null && wordThen.Value == "then")
                        {
                            var codeTrue = _parsedObjects[index + 3] as MOGCode;

                            if (codeTrue != null)
                            {
                                bool withElse = false;

                                if (_parsedObjects.Count - index >= 6)
                                {
                                    var wordElse = _parsedObjects[index + 4] as MOGWord;
                                    withElse = (wordElse != null && wordElse.Value == "else");
                                }

                                if (withElse)
                                {
                                    // IFELSE

                                    var codeFalse = _parsedObjects[index + 5] as MOGCode;

                                    if (codeFalse != null)
                                    {
                                        // 0 if
                                        // 1 (condition)
                                        // 2 then
                                        // 3 {codeTrue}
                                        // 4 else
                                        // 5 {codeFalse}

                                        var primitive = engine.GetPrimitive(typeof(PrimitiveIFELSE));

                                        if (primitive != null)
                                        {
                                            primitive.StartPos = _parsedObjects[index].StartPos;
                                            primitive.EndPos = _parsedObjects[index].EndPos;
                                            primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;

                                            // On retire 6 éléments à partir de l'index

                                            for (int i = 0; i < 6; i++)
                                                _parsedObjects.RemoveAt(index);

                                            // On insère

                                            // 0 (condition) ---> {! condition}
                                            // 1 {codeTrue}
                                            // 2 {codeFalse}
                                            // 3 IFELSE

                                            var codeCondition = condition.ToCode();
                                            codeCondition.AutoEval = true;

                                            _parsedObjects.Insert(index, primitive);
                                            _parsedObjects.Insert(index, codeFalse);
                                            _parsedObjects.Insert(index, codeTrue);
                                            _parsedObjects.Insert(index, codeCondition);

                                            return true;
                                        }
                                    }
                                }
                                else
                                {
                                    // IF

                                    var code = _parsedObjects[index + 3] as MOGCode;

                                    if (code != null)
                                    {
                                        // 0 if
                                        // 1 (condition)
                                        // 2 then
                                        // 3 {code}

                                        var primitive = engine.GetPrimitive("IF");

                                        if (primitive != null)
                                        {
                                            primitive.StartPos = _parsedObjects[index].StartPos;
                                            primitive.EndPos = _parsedObjects[index].EndPos;
                                            primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;

                                            // On retire 4 éléments à partir de l'index

                                            for (int i = 0; i < 4; i++)
                                                _parsedObjects.RemoveAt(index);

                                            // On insère

                                            // 0 (condition) ---> {! condition}
                                            // 1 + eval
                                            // 2 {code}
                                            // 3 IF

                                            var codeCondition = condition.ToCode();
                                            codeCondition.AutoEval = true;

                                            _parsedObjects.Insert(index, primitive);
                                            _parsedObjects.Insert(index, code);
                                            _parsedObjects.Insert(index, codeCondition);

                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {

            }

            return false;
        }

        private bool UpdateForForSugar(MogwaiEngine engine, int index)
        {
            // 0 for
            // 1 name
            // 2 do
            // 3 code

            // ou

            // 0 for
            // 1 name
            // 2 object to number step 
            // 3 number
            // 4 do
            // 5 code

            try
            {
                if (_parsedObjects.Count - index >= 4 && index > 0)
                {
                    var name = _parsedObjects[index + 1] as MOGName;

                    if (name != null)
                    {
                        var doOrStepWord = _parsedObjects[index + 2] as MOGWord;

                        if (doOrStepWord != null && doOrStepWord.Value == "do")
                        {
                            // FOR 

                            var code = _parsedObjects[index + 3] as MOGCode;

                            if (code != null)
                            {
                                var primitive = engine.GetPrimitive(typeof(PrimitiveFOR));

                                if (primitive != null)
                                {
                                    primitive.StartPos = _parsedObjects[index].StartPos;
                                    primitive.EndPos = _parsedObjects[index].EndPos;
                                    primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;

                                    // On enlève les 4 éléments

                                    for (int i = 0; i < 4; i++)
                                        _parsedObjects.RemoveAt(index);

                                    // On crée le vrai FOR
                                    // 0 name
                                    // 1 code
                                    // 2 FOR

                                    _parsedObjects.Insert(index, primitive);
                                    _parsedObjects.Insert(index, code);
                                    _parsedObjects.Insert(index, name);

                                    return true;
                                }
                            }
                        }
                        else
                        {
                            // FORSTEP

                            var stepValue = _parsedObjects[index + 3];

                            if (stepValue != null)
                            {
                                var doWord = _parsedObjects[index + 4] as MOGWord;

                                if (doWord != null && doWord.Value == "do")
                                {
                                    var code = _parsedObjects[index + 5] as MOGCode;

                                    if (code != null)
                                    {
                                        var primitive = engine.GetPrimitive(typeof(PrimitiveFORSTEP));

                                        if (primitive != null)
                                        {
                                            primitive.StartPos = _parsedObjects[index].StartPos;
                                            primitive.EndPos = _parsedObjects[index].EndPos;
                                            primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;

                                            // On enlève les 4 éléments

                                            for (int i = 0; i < 6; i++)
                                                _parsedObjects.RemoveAt(index);

                                            // On crée le vrai FORSTEP
                                            // 0 step
                                            // 1 name
                                            // 2 code
                                            // 3 FOR

                                            _parsedObjects.Insert(index, primitive);
                                            _parsedObjects.Insert(index, code);
                                            _parsedObjects.Insert(index, name);
                                            _parsedObjects.Insert(index, stepValue);

                                            return true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch
            {

            }

            return false;
        }

    }
}
