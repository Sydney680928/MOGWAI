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
using System.Globalization;

namespace MOGWAI.Engine
{
    /// <summary>
    /// Converts a list of infix tokens (produced by <see cref="InfixLexer"/>)
    /// into a list of <see cref="MOGObject"/> ready to be executed by the MOGWAI engine.
    ///
    /// Algorithm: Dijkstra's Shunting-yard with multi-argument function support.
    /// </summary>
    public static class ShuntingYard
    {
        // ── Precedence and associativity table ─────────────────────────────────

        private static readonly Dictionary<string, (int Precedence, bool RightAssoc)> Operators = new()
        {
            ["+"] = (1, false),
            ["-"] = (1, false),
            ["*"] = (2, false),
            ["/"] = (2, false),
        };

        // ── Entry point ────────────────────────────────────────────────────────

        /// <summary>
        /// Converts an infix expression into a <see cref="List{MOGObject}"/>
        /// ready to be wrapped in a MOGWAI block and executed.
        /// </summary>
        /// <param name="expression">Infix expression, e.g. "5 * X + (7 + sin(Y))"</param>
        /// <param name="engine">MOGWAI engine instance (for GetPrimitive)</param>
        /// <param name="startPos">Start position for popping tokens</param>
        /// <param name="endPos">End position for popping tokens</param>
        public static List<MOGObject> Convert(string expression, MogwaiEngine engine, int startPos, int endPos)
        {
            var tokens = InfixLexer.Tokenize(expression);
            var output = new List<MOGObject>();    // RPN output queue
            var opStack = new Stack<InfixToken>(); // operator stack

            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                switch (token.Kind)
                {
                    // ── Numeric literal → direct output
                    case InfixTokenKind.Number:
                        output.Add(ParseNumber(engine, token.Value, startPos, endPos));
                        break;

                    // ── Word: function (followed by '(') → stack, otherwise → direct output
                    case InfixTokenKind.Word:
                        bool isFunction = i + 1 < tokens.Count
                                          && tokens[i + 1].Kind == InfixTokenKind.ParenOpen;
                        if (isFunction)
                            opStack.Push(token);             // will be popped at the matching ')'
                        else
                            PushToOutput(output, token, engine, startPos, endPos); // PI, E, X, @$X…
                        break;

                    // ── Comma: end of argument, pop until '('
                    case InfixTokenKind.Comma:
                        while (opStack.Count > 0 && opStack.Peek().Kind != InfixTokenKind.ParenOpen)
                            PushToOutput(output, opStack.Pop(), engine, startPos, endPos);
                        if (opStack.Count == 0)
                            throw new InvalidOperationException("Missing parenthesis or misplaced comma.");
                        break;

                    // ── Operator
                    case InfixTokenKind.Operator:
                        var (prec, rightAssoc) = Operators[token.Value];
                        while (opStack.Count > 0)
                        {
                            var top = opStack.Peek();
                            if (top.Kind != InfixTokenKind.Operator) break;
                            var (topPrec, _) = Operators[top.Value];
                            if (rightAssoc ? topPrec > prec : topPrec >= prec)
                                PushToOutput(output, opStack.Pop(), engine, startPos, endPos);
                            else
                                break;
                        }
                        opStack.Push(token);
                        break;

                    // ── Opening parenthesis → pushed as-is
                    case InfixTokenKind.ParenOpen:
                        opStack.Push(token);
                        break;

                    // ── Closing parenthesis
                    case InfixTokenKind.ParenClose:
                        while (opStack.Count > 0 && opStack.Peek().Kind != InfixTokenKind.ParenOpen)
                            PushToOutput(output, opStack.Pop(), engine, startPos, endPos);
                        if (opStack.Count == 0)
                            throw new InvalidOperationException("Closing parenthesis without matching opening parenthesis.");
                        opStack.Pop(); // discard '('

                        // If a function is on top → pop it to output
                        if (opStack.Count > 0 && opStack.Peek().Kind == InfixTokenKind.Word)
                            PushToOutput(output, opStack.Pop(), engine, startPos, endPos);
                        break;
                }
            }

            // ── Drain the operator stack
            while (opStack.Count > 0)
            {
                var top = opStack.Pop();
                
                if (top.Kind == InfixTokenKind.ParenOpen)
                    throw new InvalidOperationException("Opening parenthesis without matching closing parenthesis.");
               
                PushToOutput(output, top, engine, startPos, endPos);
            }

            return output;
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        /// <summary>
        /// Maps a token to a <see cref="MOGObject"/> and appends it to the output.
        /// Rule: first check with the engine whether it is a primitive.
        /// If yes → MOGPrimitive (copy). Otherwise → MOGVar (@), MOGRef (&), or MOGWord.
        /// </summary>
        private static void PushToOutput(List<MOGObject> output, InfixToken token, MogwaiEngine engine, int startPos, int endPos)
        {
            // Standard operator: +, -, *, /
            if (token.Kind == InfixTokenKind.Operator)
            {
                var prim = engine.GetPrimitive(token.Value, true);
                
                if (prim != null)
                {
                    prim.PauseAllowed = false;
                    output.Add(prim);
                }
                else
                {
                    throw new InvalidOperationException($"Operator '{token.Value}' not found in the MOGWAI engine.");
                }

                return;
            }

            // Word: known primitive → direct output, otherwise MOGVar (@), MOGRef (&), or MOGWord
            if (token.Kind == InfixTokenKind.Word)
            {
                var prim = engine.GetPrimitive(token.Value, true);
                if (prim != null)
                {
                    prim.PauseAllowed = false;
                    output.Add(prim); // Primitive: sin, cos, PI, E, pow…
                    return;
                }

                // Detect the ! sigil (auto-evaluation) and strip it from the name
                bool autoEval = token.Value.StartsWith('!');
                string name = autoEval ? token.Value.Substring(1) : token.Value;

                MOGObject obj;

                if (name.StartsWith('@'))
                {
                    obj = new MOGVar(engine, name.Substring(1)); // @X→X, @$X→$X
                }
                else if (name.StartsWith('&'))
                {
                    obj = new MOGRef(engine, name.Substring(1)); // &X→X, &$X→$X
                }
                else
                {
                    obj = new MOGWord(engine, name);             // X, $X, free word… 
                }

                obj.PauseAllowed = false;
                obj.StartPos = startPos;
                obj.EndPos = endPos;
                
                if (autoEval) 
                    obj.AutoEval = true;

                output.Add(obj);
                
                return;
            }

            throw new InvalidOperationException($"Unexpected token in output: {token}");
        }

        /// <summary>
        /// Parses a numeric literal into a <see cref="MOGObject"/>.
        /// </summary>
        private static MOGObject ParseNumber(MogwaiEngine engine, string value, int startPos, int endPos)
        {
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
            {
                var n = new MOGNumber(engine, d);
                
                n.StartPos = startPos;
                n.EndPos = endPos;
                n.PauseAllowed = false;
                
                return n;
            }

            throw new InvalidOperationException($"Cannot parse number '{value}'.");
        }
    }
}
