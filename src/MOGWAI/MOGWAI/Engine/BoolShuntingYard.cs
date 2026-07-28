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
    /// Converts a list of boolean infix tokens (produced by <see cref="BoolLexer"/>)
    /// into a list of <see cref="MOGObject"/> ready to be executed by the MOGWAI engine.
    ///
    /// Algorithm: Dijkstra's Shunting-yard with multi-argument function support.
    ///
    /// Operator precedence (high to low):
    ///   * /          → 5
    ///   + -          → 4
    ///   < > <= >= == !=  → 3
    ///   and xor      → 2
    ///   or           → 1
    ///
    /// not(...) is handled as a unary function via the existing isFunction mechanism.
    /// </summary>
    public static class BoolShuntingYard
    {
        // ── Precedence and associativity table ─────────────────────────────────

        private static readonly Dictionary<string, (int Precedence, bool RightAssoc)> Operators = new()
        {
            ["*"]  = (5, false),
            ["/"]  = (5, false),
            ["+"]  = (4, false),
            ["-"]  = (4, false),
            ["<"]  = (3, false),
            [">"]  = (3, false),
            ["<="] = (3, false),
            [">="] = (3, false),
            ["=="] = (3, false),
            ["!="] = (3, false),
            ["and"] = (2, false),
            ["xor"] = (2, false),
            ["or"]  = (1, false),
        };

        // ── Entry point ────────────────────────────────────────────────────────

        /// <summary>
        /// Converts a boolean infix expression into a <see cref="List{MOGObject}"/>
        /// ready to be wrapped in a MOGWAI block and executed.
        /// The block, when executed, leaves a boolean value on the stack.
        /// </summary>
        /// <param name="expression">Boolean infix expression, e.g. "a &lt; 10 and b &gt; 20"</param>
        /// <param name="engine">MOGWAI engine instance (for GetPrimitive)</param>
        public static List<MOGObject> Convert(string expression, MogwaiEngine engine, int startPos, int endPos)
        {
            var tokens  = BoolLexer.Tokenize(expression);
            var output  = new List<MOGObject>();    // RPN output queue
            var opStack = new Stack<BoolToken>();   // operator stack

            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                switch (token.Kind)
                {
                    // ── Numeric literal → direct output
                    case BoolTokenKind.Number:
                        output.Add(ParseNumber(engine, token.Value, startPos, endPos));
                        break;

                    // ── Word: boolean operator (and/or/xor) → operator stack
                    //         function (followed by '(') → operator stack
                    //         otherwise → direct output (primitive, variable, constant)
                    case BoolTokenKind.Word:
                        if (Operators.ContainsKey(token.Value))
                        {
                            // Boolean keyword operator: treat like a binary operator
                            PushOperator(output, opStack, token, engine, startPos, endPos);
                            opStack.Push(token);
                        }
                        else
                        {
                            bool isFunction = i + 1 < tokens.Count
                                              && tokens[i + 1].Kind == BoolTokenKind.ParenOpen;
                            if (isFunction)
                                opStack.Push(token);             // will be popped at the matching ')'
                            else
                                PushToOutput(output, token, engine, startPos, endPos); // PI, E, X, @$X…
                        }
                        break;

                    // ── Comma: end of argument, pop until '('
                    case BoolTokenKind.Comma:
                        while (opStack.Count > 0 && opStack.Peek().Kind != BoolTokenKind.ParenOpen)
                            PushToOutput(output, opStack.Pop(), engine, startPos, endPos);
                        if (opStack.Count == 0)
                            throw new InvalidOperationException("Missing parenthesis or misplaced comma.");
                        break;

                    // ── Operator
                    case BoolTokenKind.Operator:
                        PushOperator(output, opStack, token, engine, startPos, endPos);
                        opStack.Push(token);
                        break;

                    // ── Opening parenthesis → pushed as-is
                    case BoolTokenKind.ParenOpen:
                        opStack.Push(token);
                        break;

                    // ── Closing parenthesis
                    case BoolTokenKind.ParenClose:
                        while (opStack.Count > 0 && opStack.Peek().Kind != BoolTokenKind.ParenOpen)
                            PushToOutput(output, opStack.Pop(), engine, startPos, endPos);
                        if (opStack.Count == 0)
                            throw new InvalidOperationException("Closing parenthesis without matching opening parenthesis.");
                        opStack.Pop(); // discard '('

                        // If a function is on top → pop it to output
                        if (opStack.Count > 0 && opStack.Peek().Kind == BoolTokenKind.Word)
                            PushToOutput(output, opStack.Pop(), engine, startPos, endPos);
                        break;
                }
            }

            // ── Drain the operator stack
            while (opStack.Count > 0)
            {
                var top = opStack.Pop();
                if (top.Kind == BoolTokenKind.ParenOpen)
                    throw new InvalidOperationException("Opening parenthesis without matching closing parenthesis.");
                PushToOutput(output, top, engine, startPos, endPos);
            }

            return output;
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        /// <summary>
        /// Applies Shunting-yard precedence rules before pushing an operator onto the stack.
        /// </summary>
        private static void PushOperator(List<MOGObject> output, Stack<BoolToken> opStack, BoolToken token, MogwaiEngine engine, int startPos, int endPos)
        {
            var (prec, rightAssoc) = Operators[token.Value];
            while (opStack.Count > 0)
            {
                var top = opStack.Peek();
                if (!Operators.ContainsKey(top.Value)) break;
                var (topPrec, _) = Operators[top.Value];
                if (rightAssoc ? topPrec > prec : topPrec >= prec)
                    PushToOutput(output, opStack.Pop(), engine, startPos, endPos);
                else
                    break;
            }
        }

        /// <summary>
        /// Maps a token to a <see cref="MOGObject"/> and appends it to the output.
        /// Rule: first check with the engine whether it is a primitive.
        /// If yes → MOGPrimitive (copy). Otherwise → MOGVar (@), MOGRef (&), or MOGWord.
        /// </summary>
        private static void PushToOutput(List<MOGObject> output, BoolToken token, MogwaiEngine engine, int startPos, int endPos)
        {
            // Operator or boolean keyword: retrieve as primitive
            if (token.Kind == BoolTokenKind.Operator || Operators.ContainsKey(token.Value))
            {
                var prim = engine.GetPrimitive(token.Value, true);
                if (prim != null)
                    output.Add(prim);
                else
                    throw new InvalidOperationException(
                        $"Operator '{token.Value}' not found in the MOGWAI engine.");
                return;
            }

            // Word: known primitive → direct output, otherwise MOGVar (@), MOGRef (&), or MOGWord
            if (token.Kind == BoolTokenKind.Word)
            {
                var prim = engine.GetPrimitive(token.Value, true);
                if (prim != null)
                {
                    output.Add(prim); // Primitive: sin, cos, PI, E, not…
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
