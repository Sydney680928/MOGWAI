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
using System.Xml.Linq;

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
        private Parser? _localParser = null;

        private Parser LocalParser
        {
            get
            {
                if (_localParser == null)
                    _localParser = new Parser();

                return _localParser;
            }
        }

        public List<MOGObject> ParsedObjects => _parsedObjects;

        public void Parse(MogwaiEngine engine, string code, int offsetPosition, MogwaiExecutionContext? context)
        {
            _code = code;
            _currentIndex = 0;
            _currentItem.Clear();
            _parsedObjects.Clear();

            engine.LastParserExecutionContext = context;

            while (_currentIndex < _code.Length)
            {
                do
                {
                    _pos = _currentIndex + offsetPosition;
                    _currentChar = _code[_currentIndex++];

                } while (_currentIndex < _code.Length && (_currentChar == ' ' || _currentChar == '\n' || _currentChar == '\r' || _currentChar == '\t'));

                if (_currentChar == ' ' || _currentChar == '\n' || _currentChar == '\r' || _currentChar == '\t')
                    break;

                if (_currentChar == '#')
                {
                    // On cherche la fin de ligne ou la fin du code

                    do
                    {
                        _pos = _currentIndex + offsetPosition;
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

                        LocalParser.Parse(engine, _currentItem.ToString(), _pos - _currentItem.Length, context);

                        if (LocalParser.ParsedObjects.Count != 1 || (LocalParser.ParsedObjects[0] is not MOGWord && LocalParser._parsedObjects[0] is not MOGPrimitive))
                        {
                            engine.LastParserStartErrorPosition = _pos - _currentItem.Length;
                            engine.LastParserEndErrorPosition = _pos;
                            throw new MogwaiParseErrorException("unexpected character '('");
                        }

                        prefix = LocalParser.ParsedObjects[0];
                        _currentItem.Clear();
                    }

                    GetEnclosedItem(engine, '(', ')');
                    var l = new MOGList(engine, _currentItem.ToString(), _pos + 1, context);

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
                        engine.LastParserStartErrorPosition = _pos;
                        engine.LastParserEndErrorPosition = _pos;
                        throw new MogwaiParseErrorException("unexpected character '{'");
                    }

                    GetEnclosedItem(engine, '{', '}');
                    var c = new MOGCode(engine, _currentItem.ToString(), _pos + 1, context);
                    _parsedObjects.Add(c);
                    _currentItem.Clear();
                }
                else if (_currentChar == '«')
                {
                    // FUNCTION

                    if (_currentItem.Length > 0)
                    {
                        engine.LastParserStartErrorPosition = _pos;
                        engine.LastParserEndErrorPosition = _pos;
                        throw new MogwaiParseErrorException("unexpected character '«'");
                    }

                    GetEnclosedItem(engine, '«', '»');
                    var f = new MOGFunction(engine, _currentItem.ToString(), _pos + 1, context);
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

                        LocalParser.Parse(engine, _currentItem.ToString(), _pos - _currentItem.Length, context);

                        if (LocalParser.ParsedObjects.Count != 1 || (LocalParser.ParsedObjects[0] is not MOGWord && LocalParser.ParsedObjects[0] is not MOGPrimitive))
                        {
                            engine.LastParserStartErrorPosition = _pos - _currentItem.Length;
                            engine.LastParserEndErrorPosition = _pos;
                            throw new MogwaiParseErrorException("unexpected character '['");
                        }

                        prefix = LocalParser.ParsedObjects[0];
                        _currentItem.Clear();
                    }

                    GetEnclosedItem(engine, '[', ']');

                    LocalParser.Parse(engine, _currentItem.ToString(), _pos + 1, context);
                    var items = LocalParser.ParsedObjects;

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
                                engine.LastParserStartErrorPosition = _pos;
                                engine.LastParserEndErrorPosition = _pos;
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
                    r.StartPos = _pos;
                    r.EndPos = _pos + _currentItem.Length + 1;
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
                        engine.LastParserStartErrorPosition = _pos;
                        engine.LastParserEndErrorPosition = _pos;
                        throw new MogwaiParseErrorException("unexpected character '\"'");
                    }

                    GetEnclosedItem(engine, '"', '"');
                    var s = new MOGString(engine, _currentItem.ToString(), _pos);
                    s.ExecutionContext = context;
                    _parsedObjects.Add(s);
                    _currentItem.Clear();
                }
                else if (_currentChar == '\'')
                {
                    // NAME

                    if (_currentItem.Length > 0)
                    {
                        engine.LastParserStartErrorPosition = _pos;
                        engine.LastParserEndErrorPosition = _pos;
                        throw new MogwaiParseErrorException("unexpected character '");
                    }

                    GetEnclosedItem(engine, '\'', '\'');
                    var name = _currentItem.ToString();

                    if (name.Length == 0 || !engine.IsValidName(name))
                    {
                        engine.LastParserStartErrorPosition = _pos;
                        engine.LastParserEndErrorPosition = _pos + _currentItem.Length + 1;
                        throw new MogwaiParseErrorException($"invalid name '{name}'");
                    }

                    var n = new MOGName(engine, name, _pos);
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
                            _parsedObjects.AddRange(ParseBasicWord(engine, item, _pos + 1 - item.Length, context));

                        _currentItem.Clear();
                    }
                }
            }

            var item2 = _currentItem.ToString();

            if (item2.Length > 0)
                _parsedObjects.AddRange(ParseBasicWord(engine, item2, _pos, context));

            _currentItem.Clear();

            UpdateForSugarItems(engine);
        }

        private List<MOGObject> ParseBasicWord(MogwaiEngine engine, string item, int offsetPosition, MogwaiExecutionContext? context)
        {
            if (item.Length > 0)
            {
                var hostFunctions = engine.HostFunctions;

                var p = engine.GetPrimitive(item, true);

                if (p != null)
                {
                    if (p.IsPrivate && !engine.AllowPrivatePrimitives)
                    {
                        engine.LastParserStartErrorPosition = _pos;
                        engine.LastParserEndErrorPosition = _pos + item.Length;
                        throw new MogwaiParseErrorException("private primitive not allowed");
                    }

                    p.ExecutionContext = context;
                    p.StartPos = offsetPosition;
                    p.EndPos = offsetPosition + p.Name.Length - 1;

                    return [p];
                }
                else if (item.Contains("->") && !item.StartsWith("->") && !item.EndsWith("->"))
                {
                    var fields = item.Split("->");

                    if (fields.Length != 2)
                    {
                        engine.LastParserStartErrorPosition = _pos - item.Length + 1;
                        engine.LastParserEndErrorPosition = _pos;
                        throw new MogwaiParseErrorException("invalid x->y notation");
                    }

                    var items1 = ParseBasicWord(engine, fields[0], offsetPosition, context);

                    if (items1.Count != 1)
                    {
                        engine.LastParserStartErrorPosition = _pos - fields[0].Length + 1;
                        engine.LastParserEndErrorPosition = _pos;
                        throw new MogwaiParseErrorException("internal error with record definition in x->y notation");
                    }

                    var items2 = ParseBasicWord(engine, fields[1], offsetPosition, context);

                    if (items2.Count != 1)
                    {
                        engine.LastParserStartErrorPosition = _pos - fields[1].Length + 1;
                        engine.LastParserEndErrorPosition = _pos;
                        throw new MogwaiParseErrorException("internal error with record definition in x<-y notation");
                    }

                    var primitive = engine.GetPrimitive(typeof(PrimitiveGet), true);

                    if (primitive == null)
                    {
                        engine.LastParserStartErrorPosition = _pos - item.Length + 1;
                        engine.LastParserEndErrorPosition = _pos;
                        throw new MogwaiParseErrorException("internal error with get primitive in x->y notation");
                    }

                    primitive.ExecutionContext = context;

                    items1[0].StartPos = offsetPosition;
                    items1[0].EndPos = offsetPosition + item.Length - 1;

                    items2[0].StartPos = primitive.StartPos = items1[0].StartPos;
                    items2[0].EndPos = primitive.EndPos = items1[0].EndPos;

                    items1[0].PauseAllowed = false;
                    items2[0].PauseAllowed = false;

                    return [items1[0], items2[0], primitive];
                }
                else if (item.Contains("<-") && !item.StartsWith("<-") && !item.EndsWith("<-"))
                {
                    var fields = item.Split("<-");

                    if (fields.Length != 2)
                    {
                        engine.LastParserStartErrorPosition = _pos - item.Length + 1;
                        engine.LastParserEndErrorPosition = _pos;
                        throw new MogwaiParseErrorException("invalid x<-y notation");
                    }

                    var items1 = ParseBasicWord(engine, fields[0], offsetPosition, context);

                    if (items1.Count != 1)
                    {
                        engine.LastParserStartErrorPosition = _pos - fields[0].Length + 1;
                        engine.LastParserEndErrorPosition = _pos;
                        throw new MogwaiParseErrorException("internal error with record definition in x<-y notation");
                    }

                    var items2 = ParseBasicWord(engine, fields[1], offsetPosition, context);

                    if (items2.Count != 1)
                    {
                        engine.LastParserStartErrorPosition = _pos - fields[1].Length + 1;
                        engine.LastParserEndErrorPosition = _pos;
                        throw new MogwaiParseErrorException("internal error with record definition in x<-y notation");
                    } 

                    var primitive = engine.GetPrimitive(typeof(PrimitiveSet), true);

                    if (primitive == null)
                    {
                        engine.LastParserStartErrorPosition = _pos - item.Length + 1;
                        engine.LastParserEndErrorPosition = _pos;
                        throw new MogwaiParseErrorException("internal error with set primitive in x<-y notation");
                    }

                    primitive.ExecutionContext = context;

                    items1[0].StartPos = offsetPosition;
                    items1[0].EndPos = offsetPosition + item.Length - 1;

                    items2[0].StartPos = primitive.StartPos = items1[0].StartPos;
                    items2[0].EndPos = primitive.EndPos = items1[0].EndPos;

                    items1[0].PauseAllowed = false;
                    items2[0].PauseAllowed = false;

                    return [items1[0], items2[0], primitive];
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
                        engine.LastParserStartErrorPosition = offsetPosition;
                        engine.LastParserEndErrorPosition = offsetPosition + item.Length - 1;
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
                            engine.LastParserStartErrorPosition = offsetPosition;
                            engine.LastParserEndErrorPosition = offsetPosition + item.Length - 1;
                            throw new MogwaiParseErrorException("invalid conversion operation");
                        }
                    }
                    else
                    {
                        engine.LastParserStartErrorPosition = offsetPosition;
                        engine.LastParserEndErrorPosition = offsetPosition + item.Length - 1;
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
                            engine.LastParserStartErrorPosition = offsetPosition;
                            engine.LastParserEndErrorPosition = offsetPosition + item.Length - 1;
                            throw;
                        }
                    }
                    else
                    {
                        engine.LastParserStartErrorPosition = offsetPosition;
                        engine.LastParserEndErrorPosition = offsetPosition + item.Length - 1;
                        throw new MogwaiParseErrorException("empty binary not allowed");
                    }
                }
                else if (item.StartsWith("@"))
                {
                    // VAR

                    if (item.Length > 1)
                    {
                        var name = item.Substring(1);

                        if (name.Length == 0)
                        {
                            engine.LastParserStartErrorPosition = offsetPosition;
                            engine.LastParserEndErrorPosition = offsetPosition + item.Length - 1;
                            throw new MogwaiParseErrorException($"illegal var name");
                        }

                        try
                        {
                            var t = new MOGVar(engine, name, offsetPosition);
                            t.ExecutionContext = context;
                            return [t];
                        }
                        catch
                        {
                            engine.LastParserStartErrorPosition = offsetPosition;
                            engine.LastParserEndErrorPosition = offsetPosition + item.Length - 1;
                            throw;
                        }
                    }
                    else
                    {
                        engine.LastParserStartErrorPosition = offsetPosition;
                        engine.LastParserEndErrorPosition = offsetPosition + item.Length - 1;
                        throw new MogwaiParseErrorException("empty var name not allowed");
                    }
                }
                else if (item.StartsWith("!") && item.Length > 1)
                {
                    // VAR AUTOEVAL

                    var name = item.Substring(1);

                    if (name.Length == 0)
                    {
                        engine.LastParserStartErrorPosition = offsetPosition;
                        engine.LastParserEndErrorPosition = offsetPosition + item.Length - 1;
                        throw new MogwaiParseErrorException($"illegal var name");
                    }

                    try
                    {
                        var t = new MOGVar(engine, name, offsetPosition);
                        t.ExecutionContext = context;
                        t.AutoEval = true;
                        return [t];
                    }
                    catch
                    {
                        engine.LastParserStartErrorPosition = offsetPosition;
                        engine.LastParserEndErrorPosition = offsetPosition + item.Length - 1;
                        throw;
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
                            engine.LastParserStartErrorPosition = offsetPosition;
                            engine.LastParserEndErrorPosition = offsetPosition + item.Length - 1;
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
                            engine.LastParserStartErrorPosition = offsetPosition;
                            engine.LastParserEndErrorPosition = offsetPosition + item.Length - 1;
                            throw;
                        }
                    }
                    else
                    {
                        engine.LastParserStartErrorPosition = offsetPosition;
                        engine.LastParserEndErrorPosition = offsetPosition + item.Length - 1;
                        throw new MogwaiParseErrorException("empty reference name not allowed");
                    }
                }
                else if (item.StartsWith("§"))
                {
                    // OBJECT REFERENCE

                    if (item.Length > 1)
                    {
                        var value = item.Substring(1);

                        if (value.Length == 0)
                        {
                            engine.LastParserStartErrorPosition = offsetPosition;
                            engine.LastParserEndErrorPosition = offsetPosition + item.Length - 1;
                            throw new MogwaiParseErrorException($"illegal object reference");
                        }

                        try
                        {
                            var v = int.Parse(value, CultureInfo.InvariantCulture);
                            var t = new MOGObjectReference(engine, v, offsetPosition);
                            t.ExecutionContext = context;
                            return [t];
                        }
                        catch
                        {
                            engine.LastParserStartErrorPosition = offsetPosition;
                            engine.LastParserEndErrorPosition = offsetPosition + item.Length - 1;
                            throw;
                        }
                    }
                    else
                    {
                        engine.LastParserStartErrorPosition = offsetPosition;
                        engine.LastParserEndErrorPosition = offsetPosition + item.Length - 1;
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
                        engine.LastParserStartErrorPosition = offsetPosition;
                        engine.LastParserEndErrorPosition = offsetPosition + item.Length - 1;
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
                        engine.LastParserStartErrorPosition = offsetPosition;
                        engine.LastParserEndErrorPosition = offsetPosition + item.Length - 1;
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
                        engine.LastParserStartErrorPosition = offsetPosition;
                        engine.LastParserEndErrorPosition = offsetPosition + item.Length - 1;
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
                            engine.LastParserStartErrorPosition = offsetPosition;
                            engine.LastParserEndErrorPosition = offsetPosition + item.Length - 1;
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
                            engine.LastParserStartErrorPosition = offsetPosition;
                            engine.LastParserEndErrorPosition = offsetPosition + item.Length - 1;
                            throw;
                        }
                    }
                    else
                    {
                        engine.LastParserStartErrorPosition = offsetPosition;
                        engine.LastParserEndErrorPosition = offsetPosition + item.Length - 1;
                        throw new MogwaiParseErrorException("empty type name not allowed");
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
                else if (double.TryParse(item, CultureInfo.InvariantCulture, out double n1))
                {
                    // NUMBER 

                    var number = new MOGNumber(engine, n1, offsetPosition);
                    number.ExecutionContext = context;

                    return [number];
                }
                else if (hostFunctions.Contains(item))
                {
                    var hfunc = new MOGHostFunction(engine, item, offsetPosition);
                    hfunc.ExecutionContext = context;
                    return [hfunc];
                }
                else
                {
                    var w = new MOGWord(engine, item, offsetPosition);
                    w.ExecutionContext = context;

                    return [w];
                }
            }
            else
            {
                throw new MogwaiEmptyWordException();
            }
        }

        private void GetEnclosedItem(MogwaiEngine engine, char firstChar, char lastChar)
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
                engine.LastParserStartErrorPosition = _pos;
                engine.LastParserEndErrorPosition = _pos + _currentItem.Length;
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

                    switch (word.Value)
                    {
                        case "if":
                            result = UpdateForIfSugar(engine, i);
                            break;

                        case "foreach":
                            result = UpdateForForeachSugar(engine, i);
                            break;

                        case "for":
                            result = UpdateForForSugar(engine, i);
                            break;

                        case "repeat":
                            result = UpdateForRepeatSugar(engine, i);
                            break;

                        case "while":
                            result = UpdateForWhileSugar(engine, i);
                            break;

                        case "do":
                            result = UpdateForDoWhileSugar(engine, i);
                            break;

                        case "to":
                            result = UpdateForDefuncSugar(engine, i);
                            break;

                        case "forever":
                            result = UpdateForForeverSugar(engine, i);
                            break;

                        case "timer":
                            result = UpdateForTimerSugar(engine, i);
                            break;

                        case "onEvent":
                            result = UpdateForOnEventSugar(engine, i);
                            break;

                        case "during":
                            result = UpdateForDuringSugar(engine, i);
                            break;

                        case "trap":
                            result = UpdateForTrapSugar(engine, i);
                            break;

                        case "guard":
                            result = UpdateForGuardSugar(engine, i);
                            break;

                        case "=>":
                            result = UpdateForDeclareSugar(engine, i);
                            break;

                        case "after":
                            result = UpdateForAfterSugar(engine, i);
                            break;

                        case "switch":
                            result = UpdateForSwitchSugar(engine, i);
                            break;

                        case "task":
                            result = UpdateForTaskSugar(engine, i);
                            break;

                        case "->":
                        case "->+":
                        case "->-":
                        case "->*":
                        case "->/":
                            result = UpdateForStoOperationsSugar(engine, i, word.Value);
                            break;

                        case "-->":
                            result = UpdateForPipeRefSugar(engine, i);
                            break;

                        case "class":
                            result = UpdateForClassSugar(engine, i); 
                            break; 
                    }

                    if (!result)
                    {
                        engine.LastParserStartErrorPosition = word.StartPos;
                        engine.LastParserEndErrorPosition = word.EndPos;

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
                            var primitive = engine.GetPrimitive(typeof(PrimitiveTASKDEF), true);

                            if (primitive != null)
                            {
                                primitive.StartPos = _parsedObjects[index].StartPos;
                                primitive.EndPos = _parsedObjects[index].EndPos;
                                primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;
                                primitive.Bag = _parsedObjects[index].Bag;

                                _parsedObjects.RemoveRange(index, 4);
                                _parsedObjects.InsertRange(index, [name, code.ToFunction(), primitive]);

                                return true;
                            }
                        }
                    }
                    else if (doOrStartOrSendWord.Value == "send")
                    {
                        var message = _parsedObjects[index + 3];

                        var primitive = engine.GetPrimitive(typeof(PrimitiveTASKSEND), true);

                        if (primitive != null)
                        {
                            primitive.StartPos = _parsedObjects[index].StartPos;
                            primitive.EndPos = _parsedObjects[index].EndPos;
                            primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;
                            primitive.Bag = _parsedObjects[index].Bag;

                            _parsedObjects.RemoveRange(index, 4);
                            _parsedObjects.InsertRange(index, [name, message, primitive]);

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
                                var primitive = engine.GetPrimitive(typeof(PrimitiveTASKSTART), true);

                                if (primitive != null)
                                {
                                    primitive.StartPos = _parsedObjects[index].StartPos;
                                    primitive.EndPos = _parsedObjects[index].EndPos;
                                    primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;
                                    primitive.Bag = _parsedObjects[index].Bag;

                                    _parsedObjects.RemoveRange(index, 5);
                                    _parsedObjects.InsertRange(index, [name, parameter, primitive]);

                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        private bool UpdateForClassSugar(MogwaiEngine engine, int index)
        {
            var primitiveDEFCLASS = engine.GetPrimitive(typeof(PrimitiveDEFCLASS), true);

            if (primitiveDEFCLASS == null)
                return false;

            if (engine.CheckCodeFootprint(_parsedObjects, index, "class", null, "do", null))
            {
                // 0 class
                // 1 name
                // 2 do
                // 3 code

                var name = _parsedObjects[index + 1];

                var code = _parsedObjects[index + 3] as MOGCode;

                if (code != null)
                {
                    // Le code doit être transformé en record
                    
                    MOGRecord? defRecord;
                    
                    try
                    {
                        defRecord = code.ToRecord();
                    }
                    catch
                    {
                        return false;
                    }

                    // Toutes les clés portant du code doivent être transformées en RECORD 

                    foreach (var key in defRecord.Items.Keys)
                    {
                        var value = defRecord.Items[key];
                        
                        if (value is MOGCode codeValue)
                            defRecord.Items[key] = codeValue.ToRecord();
                    }

                    primitiveDEFCLASS.StartPos = _parsedObjects[index].StartPos;
                    primitiveDEFCLASS.EndPos = _parsedObjects[index].EndPos;
                    primitiveDEFCLASS.ExecutionContext = _parsedObjects[index].ExecutionContext;
                    primitiveDEFCLASS.Bag = _parsedObjects[index].Bag;

                    _parsedObjects.RemoveRange(index, 4);
                    _parsedObjects.InsertRange(index, [name, defRecord, primitiveDEFCLASS]);

                    return true;
                }
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

                        var primitive = engine.GetPrimitive(typeof(PrimitiveSWITCH), true);

                        if (primitive != null)
                        {
                            // On transforme les listes en code pour la vraie syntax de SWITCH

                            primitive.StartPos = _parsedObjects[index].StartPos;
                            primitive.EndPos = _parsedObjects[index].EndPos;
                            primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;
                            primitive.Bag = _parsedObjects[index].Bag;

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

                            _parsedObjects.RemoveRange(index, 2);
                            _parsedObjects.InsertRange(index, [finalCode, primitive]);

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool UpdateForAfterSugar(MogwaiEngine engine, int index)
        {
            // 0 after
            // 1 number
            // 2 do
            // 3 code

            if (_parsedObjects.Count - index >= 4)
            {
                var interval = _parsedObjects[index + 1];

                var doWord = _parsedObjects[index + 2] as MOGWord;

                if (doWord != null && doWord.Value == "do")
                {
                    var code = _parsedObjects[index + 3] as MOGCode;

                    if (code != null)
                    {
                        var primitive = engine.GetPrimitive(typeof(PrimitiveLATER), true);

                        if (primitive != null)
                        {
                            primitive.StartPos = _parsedObjects[index].StartPos;
                            primitive.EndPos = _parsedObjects[index].EndPos;
                            primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;
                            primitive.Bag = _parsedObjects[index].Bag;

                            _parsedObjects.RemoveRange(index, 4);
                            _parsedObjects.InsertRange(index, [code.ToFunction(), interval, primitive]);

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool UpdateForStoOperationsSugar(MogwaiEngine engine, int index, string word)
        {
            // -1 value
            //  0 word -> ->+ ->- ->* ->/
            //  1 name

            if (_parsedObjects.Count - index >= 2)
            {
                MOGPrimitive? primitive = null;

                switch (word)
                {
                    case "->":
                        primitive = engine.GetPrimitive(typeof(PrimitiveSTO), true);
                        break;

                    case "->+":
                        primitive = engine.GetPrimitive(typeof(PrimitiveSTOPLUS), true);
                        break;

                    case "->-":
                        primitive = engine.GetPrimitive(typeof(PrimitiveSTOSUBSTRACT), true );
                        break;

                    case "->*":
                        primitive = engine.GetPrimitive(typeof(PrimitiveSTOMULTIPLY), true);
                        break;

                    case "->/":
                        primitive = engine.GetPrimitive(typeof(PrimitiveSTODIVIDE),true);
                        break;

                    default:
                        return false;
                }

                if (primitive != null)
                {
                    primitive.StartPos = _parsedObjects[index].StartPos;
                    primitive.EndPos = _parsedObjects[index].EndPos;
                    primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;
                    primitive.Bag = _parsedObjects[index].Bag;

                    _parsedObjects[index] = _parsedObjects[index + 1];
                    _parsedObjects[index + 1] = primitive;

                    return true;
                }
            }

            return false;
        }

        private bool UpdateForDeclareSugar(MogwaiEngine engine, int index)
        {
            // -1 value
            //  0 =>
            //  1 name

            if (_parsedObjects.Count - index >= 2)
            {
                var name = _parsedObjects[index + 1];

                MOGPrimitive? primitive = engine.GetPrimitive(typeof(PrimitiveDECLARE), true);

                if (primitive != null)
                {
                    primitive.StartPos = _parsedObjects[index].StartPos;
                    primitive.EndPos = _parsedObjects[index].EndPos;
                    primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;
                    primitive.Bag = _parsedObjects[index].Bag;

                    _parsedObjects[index] = name;
                    _parsedObjects[index + 1] = primitive;

                    return true;
                }
            }

            return false;
        }

        private bool UpdateForPipeRefSugar(MogwaiEngine engine, int index)
        {
            // -1 list
            //  0 word -->
            //  1 ref
            //
            // ref list PIPEREF

            if (_parsedObjects.Count - index >= 2)
            {
                var primitive = engine.GetPrimitive(typeof(PrimitivePIPEREF), true);

                if (primitive != null)
                {
                    primitive.StartPos = _parsedObjects[index].StartPos;
                    primitive.EndPos = _parsedObjects[index].EndPos;
                    primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;
                    primitive.Bag = _parsedObjects[index].Bag;

                    var temp = _parsedObjects[index - 1];

                    _parsedObjects[index - 1] = _parsedObjects[index + 1];
                    _parsedObjects[index] = temp;
                    _parsedObjects[index + 1] = primitive;

                    return true;
                }
            }

            return false;
        }

        private bool UpdateForGuardSugar(MogwaiEngine engine, int index)
        {
            // 0 guard
            // 1 code
            // 2 else
            // 3 errorCode

            if (_parsedObjects.Count - index > 3)
            {
                var code = _parsedObjects[index + 1];

                var elseWord = _parsedObjects[index + 2] as MOGWord;

                if (elseWord != null && elseWord.Value == "else")
                {
                    var errorCode = _parsedObjects[index + 3];

                    var primitive = engine.GetPrimitive(typeof(PrimitiveErrorGuard), true);

                    if (primitive != null)
                    {
                        primitive.StartPos = _parsedObjects[index].StartPos;
                        primitive.EndPos = _parsedObjects[index].EndPos;
                        primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;
                        primitive.Bag = _parsedObjects[index].Bag;

                        _parsedObjects.RemoveRange(index, 4);
                        _parsedObjects.InsertRange(index, [code, errorCode, primitive]);

                        return true;
                    }
                }
            }

            return false;
        }

        private bool UpdateForTrapSugar(MogwaiEngine engine, int index)
        {
            // 0 trap
            // 1 code

            if (_parsedObjects.Count - index > 1)
            {
                var code = _parsedObjects[index + 1];

                var primitive = engine.GetPrimitive(typeof(PrimitiveErrorTrap), true);

                if (primitive != null)
                {
                    primitive.StartPos = _parsedObjects[index].StartPos;
                    primitive.EndPos = _parsedObjects[index].EndPos;
                    primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;
                    primitive.Bag = _parsedObjects[index].Bag;

                    _parsedObjects[index] = code;
                    _parsedObjects[index + 1] = primitive;

                    return true;
                }
            }

            return false;
        }

        private bool UpdateForDuringSugar(MogwaiEngine engine, int index)
        {
            // 0 during
            // 1 number
            // 2 do
            // 3 code

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
                            var primitive = engine.GetPrimitive(typeof(PrimitiveDURING), true);

                            if (primitive != null)
                            {
                                primitive.StartPos = _parsedObjects[index].StartPos;
                                primitive.EndPos = _parsedObjects[index].EndPos;
                                primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;
                                primitive.Bag = _parsedObjects[index].Bag;

                                _parsedObjects.RemoveRange(index, 4);
                                _parsedObjects.InsertRange(index, [number, code, primitive]);

                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private bool UpdateForOnEventSugar(MogwaiEngine engine, int index)
        {
            // 0 onEvent
            // 1 name
            // 2 do
            // 3 code

            if (_parsedObjects.Count - index > 3)
            {
                var name = _parsedObjects[index + 1];

                var doWord = _parsedObjects[index + 2] as MOGWord;

                if (doWord != null && doWord.Value == "do")
                {
                    var code = _parsedObjects[index + 3] as MOGCode;

                    if (code != null)
                    {
                        var primitive = engine.GetPrimitive(typeof(PrimitiveEVENT), true);

                        if (primitive != null)
                        {
                            primitive.StartPos = _parsedObjects[index].StartPos;
                            primitive.EndPos = _parsedObjects[index].EndPos;
                            primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;
                            primitive.Bag = _parsedObjects[index].Bag;

                            _parsedObjects.RemoveRange(index, 4);
                            _parsedObjects.InsertRange(index, [code.ToFunction(), name, primitive]);

                            return true;
                        }
                    }
                }
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

            if (_parsedObjects.Count - index > 5)
            {
                var name = _parsedObjects[index + 1];

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
                                    primitive = engine.GetPrimitive(typeof(PrimitiveEVERY), true);
                                }
                                else
                                {
                                    primitive = engine.GetPrimitive(typeof(PrimitiveAFTER), true);
                                }

                                if (primitive != null)
                                {
                                    primitive.StartPos = _parsedObjects[index].StartPos;
                                    primitive.EndPos = _parsedObjects[index].EndPos;
                                    primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;
                                    primitive.Bag = _parsedObjects[index].Bag;

                                    _parsedObjects.RemoveRange(index, 5);
                                    _parsedObjects.InsertRange(index, [code.ToFunction(), interval, name, primitive]);

                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }

        private bool UpdateForForeverSugar(MogwaiEngine engine, int index)
        {
            // 0 forever
            // 1 do
            // 2 code

            if (_parsedObjects.Count - index > 2)
            {
                var doWord = _parsedObjects[index + 1] as MOGWord;

                if (doWord != null && doWord.Value == "do")
                {
                    var code = _parsedObjects[index + 2] as MOGCode;

                    if (code != null)
                    {
                        var primitive = engine.GetPrimitive(typeof(PrimitiveFOREVER), true);

                        if (primitive != null)
                        {
                            primitive.StartPos = _parsedObjects[index].StartPos;
                            primitive.EndPos = _parsedObjects[index + 1].EndPos;
                            primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;
                            primitive.Bag = _parsedObjects[index].Bag;

                            _parsedObjects.RemoveRange(index, 3);
                            _parsedObjects.InsertRange(index, [code, primitive]);

                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private bool UpdateForDefuncSugar(MogwaiEngine engine, int index)
        {
            var primitiveDEFUNC = engine.GetPrimitive(typeof(PrimitiveDEFUNC), true);

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
                        primitiveDEFUNC.Bag = _parsedObjects[index].Bag;

                        _parsedObjects.RemoveRange(index, 4);

                        var function = code.ToFunction();
                        function.Name = name.Value;

                        _parsedObjects.InsertRange(index, [function, name, primitiveDEFUNC]);

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
                            var primitiveStackToSafeVars = engine.GetPrimitive(typeof(PrimitiveStackToSafeVars), true);

                            if (primitiveStackToSafeVars != null)
                            {
                                primitiveDEFUNC.StartPos = _parsedObjects[index].StartPos;
                                primitiveDEFUNC.EndPos = _parsedObjects[index].EndPos;
                                primitiveDEFUNC.ExecutionContext = _parsedObjects[index].ExecutionContext;
                                primitiveDEFUNC.Bag = _parsedObjects[index].Bag;

                                primitiveStackToSafeVars.PauseAllowed = false;
                                primitiveStackToSafeVars.ExecutionContext = _parsedObjects[index].ExecutionContext;
                                primitiveStackToSafeVars.Bag = _parsedObjects[index].Bag;

                                _parsedObjects.RemoveRange(index, 6);

                                var function = code.ToFunction();
                                function.Name = name.Value;

                                // We need to modify the function's code to incorporate the parameters.

                                function.Items.InsertRange(0, [paramsRecord, primitiveStackToSafeVars]);

                                _parsedObjects.InsertRange(index, [function, name, primitiveDEFUNC]);

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
                            var primitiveStackToParams = engine.GetPrimitive(typeof(PrimitiveStackToParams), true);

                            if (primitiveStackToParams != null)
                            {
                                primitiveDEFUNC.StartPos = _parsedObjects[index].StartPos;
                                primitiveDEFUNC.EndPos = _parsedObjects[index].EndPos;
                                primitiveDEFUNC.ExecutionContext = _parsedObjects[index].ExecutionContext;
                                primitiveDEFUNC.Bag = _parsedObjects[index].Bag;

                                primitiveStackToParams.PauseAllowed = false;
                                primitiveStackToParams.ExecutionContext = _parsedObjects[index].ExecutionContext;
                                primitiveStackToParams.Bag = _parsedObjects[index].Bag;

                                _parsedObjects.RemoveRange(index, 6);

                                var function = code.ToFunction();
                                function.Name = name.Value;

                                // We need to modify the function's code to incorporate the parameters.

                                function.Items.InsertRange(0, [paramsRecord, primitiveStackToParams]);

                                _parsedObjects.InsertRange(index, [function, name, primitiveDEFUNC]);

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
                            var primitiveCheck = engine.GetPrimitive(typeof(PrimitiveStackCheck), true);

                            if (primitiveCheck != null)
                            {
                                primitiveDEFUNC.StartPos = _parsedObjects[index].StartPos;
                                primitiveDEFUNC.EndPos = _parsedObjects[index].EndPos;
                                primitiveDEFUNC.ExecutionContext = _parsedObjects[index].ExecutionContext;
                                primitiveDEFUNC.Bag = _parsedObjects[index].Bag;

                                _parsedObjects.RemoveRange(index, 6);

                                var codeFunction = code.ToFunction();
                                codeFunction.AutoEval = true;
                                codeFunction.Name = name.Value;

                                var bodyItems = new List<MOGObject>
                                {
                                    codeFunction,
                                    returns,
                                    primitiveCheck
                                };

                                var body = new MOGFunction(engine, bodyItems);

                                _parsedObjects.InsertRange(index, [body, name, primitiveDEFUNC]);

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
                                var primitiveStackToSafeVars = engine.GetPrimitive(typeof(PrimitiveStackToSafeVars), true);

                                if (primitiveStackToSafeVars != null)
                                {
                                    var primitiveCheck = engine.GetPrimitive(typeof(PrimitiveStackCheck), true);

                                    if (primitiveCheck != null)
                                    {
                                        primitiveDEFUNC.StartPos = _parsedObjects[index].StartPos;
                                        primitiveDEFUNC.EndPos = _parsedObjects[index].EndPos;
                                        primitiveDEFUNC.ExecutionContext = _parsedObjects[index].ExecutionContext;
                                        primitiveDEFUNC.Bag = _parsedObjects[index].Bag;

                                        primitiveStackToSafeVars.PauseAllowed = false;
                                        primitiveStackToSafeVars.ExecutionContext = _parsedObjects[index].ExecutionContext;
                                        primitiveStackToSafeVars.Bag = _parsedObjects[index].Bag;

                                        _parsedObjects.RemoveRange(index, 8);

                                        var codeFunction = code.ToFunction();
                                        codeFunction.AutoEval = true;
                                        codeFunction.Name = name.Value;
                                        codeFunction.Items.InsertRange(0, [paramsRecord, primitiveStackToSafeVars]);

                                        var bodyItems = new List<MOGObject>
                                        {
                                            codeFunction,
                                            returns,
                                            primitiveCheck
                                        };

                                        var body = new MOGFunction(engine, bodyItems);

                                        _parsedObjects.InsertRange(index, [body, name, primitiveDEFUNC]);

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
                                var primitiveStackToParams = engine.GetPrimitive(typeof(PrimitiveStackToParams), true);

                                if (primitiveStackToParams != null)
                                {
                                    var primitiveCheck = engine.GetPrimitive(typeof(PrimitiveStackCheck), true);

                                    if (primitiveCheck != null)
                                    {
                                        primitiveDEFUNC.StartPos = _parsedObjects[index].StartPos;
                                        primitiveDEFUNC.EndPos = _parsedObjects[index].EndPos;
                                        primitiveDEFUNC.ExecutionContext = _parsedObjects[index].ExecutionContext;
                                        primitiveDEFUNC.Bag = _parsedObjects[index].Bag;

                                        primitiveStackToParams.PauseAllowed = false;
                                        primitiveStackToParams.ExecutionContext = _parsedObjects[index].ExecutionContext;
                                        primitiveStackToParams.Bag = _parsedObjects[index].Bag;

                                        _parsedObjects.RemoveRange(index, 8);

                                        var codeFunction = code.ToFunction();
                                        codeFunction.AutoEval = true;
                                        codeFunction.Name = name.Value;
                                        codeFunction.Items.InsertRange(0, [paramsRecord, primitiveStackToParams]);

                                        var bodyItems = new List<MOGObject>
                                        {
                                            codeFunction,
                                            returns,
                                            primitiveCheck
                                        };

                                        var body = new MOGFunction(engine, bodyItems);

                                        _parsedObjects.InsertRange(index, [body, name, primitiveDEFUNC]);

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
                            var primitive = engine.GetPrimitive(typeof(PrimitiveDOWHILE), true);

                            if (primitive != null)
                            {
                                primitive.StartPos = _parsedObjects[index].StartPos;
                                primitive.EndPos = _parsedObjects[index].EndPos;
                                primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;
                                primitive.Bag = _parsedObjects[index].Bag;

                                _parsedObjects.RemoveRange(index, 3);
                                _parsedObjects.InsertRange(index, [condition.ToCode(), code, primitive]);

                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private bool UpdateForWhileSugar(MogwaiEngine engine, int index)
        {
            // 0 while
            // 1 (condition)
            // 2 do
            // 3 {code}

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
                            var primitive = engine.GetPrimitive(typeof(PrimitiveWHILE), true);

                            if (primitive != null)
                            {
                                primitive.StartPos = _parsedObjects[index].StartPos;
                                primitive.EndPos = _parsedObjects[index].EndPos;
                                primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;
                                primitive.Bag = _parsedObjects[index].Bag;

                                _parsedObjects.RemoveRange(index, 4);
                                _parsedObjects.InsertRange(index, [condition.ToCode(), code, primitive]);

                                return true;
                            }
                        }
                    }
                }
            }

            return false;
        }

        private bool UpdateForRepeatSugar(MogwaiEngine engine, int index)
        {
            // 0 repeat
            // 1 code

            if (_parsedObjects.Count - index >= 1)
            {
                var code = _parsedObjects[index + 1] as MOGCode;

                if (code != null)
                {
                    var primitive = engine.GetPrimitive(typeof(PrimitiveREPEAT), true);

                    if (primitive != null)
                    {
                        primitive.StartPos = _parsedObjects[index].StartPos;
                        primitive.EndPos = _parsedObjects[index].EndPos;
                        primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;
                        primitive.Bag = _parsedObjects[index].Bag;

                        _parsedObjects.RemoveRange(index, 2);
                        _parsedObjects.InsertRange(index, [code, primitive]);

                        return true;
                    }

                }
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

            if (engine.CheckCodeFootprint(_parsedObjects, index, "foreach", null, "do", null))
            {
                var name = _parsedObjects[index + 1] as MOGName;

                if (name != null)
                {
                    var code = _parsedObjects[index + 3] as MOGCode;

                    if (code != null)
                    {
                        var primitive = engine.GetPrimitive(typeof(PrimitiveFOREACH), true);

                        if (primitive != null)
                        {
                            primitive.StartPos = _parsedObjects[index].StartPos;
                            primitive.EndPos = _parsedObjects[index].EndPos;
                            primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;
                            primitive.Bag = _parsedObjects[index].Bag;

                            _parsedObjects.RemoveRange(index, 4);
                            _parsedObjects.InsertRange(index, [name, code, primitive]);

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
                        var primitive = engine.GetPrimitive(typeof(PrimitiveFOREACHTRANSFORM), true);

                        if (primitive != null)
                        {
                            primitive.StartPos = _parsedObjects[index].StartPos;
                            primitive.EndPos = _parsedObjects[index].EndPos;
                            primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;
                            primitive.Bag = _parsedObjects[index].Bag;

                            _parsedObjects.RemoveRange(index, 4);
                            _parsedObjects.InsertRange(index, [name, code, primitive]);

                            return true;
                        }
                    }
                }
            }
            else if (engine.CheckCodeFootprint(_parsedObjects, index, "foreach", null, "filter", null))
            {
                var name = _parsedObjects[index + 1] as MOGName;

                if (name != null)
                {
                    var code = _parsedObjects[index + 3] as MOGCode;

                    if (code != null)
                    {
                        var primitive = engine.GetPrimitive(typeof(PrimitiveFOREACHFILTER), true);

                        if (primitive != null)
                        {
                            primitive.StartPos = _parsedObjects[index].StartPos;
                            primitive.EndPos = _parsedObjects[index].EndPos;
                            primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;
                            primitive.Bag = _parsedObjects[index].Bag;

                            _parsedObjects.RemoveRange(index, 4);
                            _parsedObjects.InsertRange(index, [name, code, primitive]);

                            return true;
                        }
                    }
                }
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

                                    var primitive = engine.GetPrimitive(typeof(PrimitiveIFELSE), true);

                                    if (primitive != null)
                                    {
                                        primitive.StartPos = _parsedObjects[index].StartPos;
                                        primitive.EndPos = _parsedObjects[index].EndPos;
                                        primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;
                                        primitive.Bag = _parsedObjects[index].Bag;

                                        // On retire 6 éléments à partir de l'index

                                        _parsedObjects.RemoveRange(index, 6);

                                        // On insère

                                        // 0 (condition) ---> {! condition}
                                        // 1 {codeTrue}
                                        // 2 {codeFalse}
                                        // 3 IFELSE

                                        var codeCondition = condition.ToCode();
                                        codeCondition.AutoEval = true;

                                        _parsedObjects.InsertRange(index, [codeCondition, codeTrue, codeFalse, primitive]);

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

                                    var primitive = engine.GetPrimitive(typeof(PrimitiveIF), true);

                                    if (primitive != null)
                                    {
                                        primitive.StartPos = _parsedObjects[index].StartPos;
                                        primitive.EndPos = _parsedObjects[index].EndPos;
                                        primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;
                                        primitive.Bag = _parsedObjects[index].Bag;

                                        // On retire 4 éléments à partir de l'index

                                        _parsedObjects.RemoveRange(index, 4);

                                        // On insère

                                        // 0 (condition) ---> {! condition}
                                        // 1 + eval
                                        // 2 {code}
                                        // 3 IF

                                        var codeCondition = condition.ToCode();
                                        codeCondition.AutoEval = true;

                                        _parsedObjects.InsertRange(index, [codeCondition, code, primitive]);

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

            if (_parsedObjects.Count - index >= 4 && index > 0)
            {
                var name = _parsedObjects[index + 1];
                var doOrStepWord = _parsedObjects[index + 2] as MOGWord;

                if (doOrStepWord != null && doOrStepWord.Value == "do")
                {
                    // FOR 

                    var code = _parsedObjects[index + 3] as MOGCode;

                    if (code != null)
                    {
                        var primitive = engine.GetPrimitive(typeof(PrimitiveFOR), true);

                        if (primitive != null)
                        {
                            primitive.StartPos = _parsedObjects[index].StartPos;
                            primitive.EndPos = _parsedObjects[index].EndPos;
                            primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;
                            primitive.Bag = _parsedObjects[index].Bag;

                            // On enlève les 4 éléments

                            _parsedObjects.RemoveRange(index, 4);

                            // On crée le vrai FOR
                            // 0 name
                            // 1 code
                            // 2 FOR

                            _parsedObjects.InsertRange(index, [name, code, primitive]);

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
                                var primitive = engine.GetPrimitive(typeof(PrimitiveFORSTEP), true);

                                if (primitive != null)
                                {
                                    primitive.StartPos = _parsedObjects[index].StartPos;
                                    primitive.EndPos = _parsedObjects[index].EndPos;
                                    primitive.ExecutionContext = _parsedObjects[index].ExecutionContext;
                                    primitive.Bag = _parsedObjects[index].Bag;

                                    // On enlève les 6 éléments

                                    _parsedObjects.RemoveRange(index, 6);

                                    // On crée le vrai FORSTEP
                                    // 0 step
                                    // 1 name
                                    // 2 code
                                    // 3 FOR

                                    _parsedObjects.InsertRange(index, [stepValue, name, code, primitive]);

                                    return true;
                                }
                            }
                        }
                    }
                }
            }

            return false;
        }
    }
}
