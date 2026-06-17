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

namespace MOGWAI.Engine
{
    /// <summary>
    /// Token type produced by the infix lexer.
    /// </summary>
    public enum InfixTokenKind
    {
        Number,         // numeric literal: 3.14, -2, 1e6
        Word,           // identifier: X, sin, pow, E, PI…
        Operator,       // + - * /
        ParenOpen,      // (
        ParenClose,     // )
        Comma           // , function argument separator
    }

    /// <summary>
    /// Token produced by <see cref="InfixLexer"/>.
    /// </summary>
    public readonly struct InfixToken
    {
        public InfixTokenKind Kind { get; }
        public string Value { get; }

        public InfixToken(InfixTokenKind kind, string value)
        {
            Kind = kind;
            Value = value;
        }

        public override string ToString() => $"[{Kind}:{Value}]";
    }

    /// <summary>
    /// Transforms an infix expression into a list of tokens.
    /// Handles: numbers (including scientific notation and unary sign),
    /// identifiers (local variables, global variables $X, MOGWAI functions),
    /// operators, parentheses, commas.
    /// </summary>
    public static class InfixLexer
    {
        public static List<InfixToken> Tokenize(string expression)
        {
            var tokens = new List<InfixToken>();
            int i = 0;
            int len = expression.Length;

            while (i < len)
            {
                char c = expression[i];

                // -- skip whitespace
                if (char.IsWhiteSpace(c)) { i++; continue; }

                // -- number: starts with a digit or '.'
                if (char.IsDigit(c) || c == '.')
                {
                    tokens.Add(ReadNumber(expression, ref i));
                    continue;
                }

                // -- identifier: letter, '_' or MOGWAI sigil ($, &, @, !)
                if (char.IsLetter(c) || c == '_' || c == '$' || c == '&' || c == '@' || c == '!')
                {
                    tokens.Add(ReadWord(expression, ref i));
                    continue;
                }

                // -- operators and punctuation
                switch (c)
                {
                    case '+':
                    case '-':
                        // Unary sign? Yes if no previous token,
                        // or previous token is an operator or opening parenthesis.
                        if (IsUnary(tokens))
                        {
                            i++;
                            // Consume the following number (e.g. -3.14 or +2)
                            if (i < len && (char.IsDigit(expression[i]) || expression[i] == '.'))
                            {
                                var num = ReadNumber(expression, ref i);
                                tokens.Add(new InfixToken(InfixTokenKind.Number,
                                    c == '-' ? "-" + num.Value : num.Value));
                            }
                            else
                            {
                                // Unary sign before a parenthesis or variable:
                                // insert 0 and the operator so Shunting-yard handles it
                                tokens.Add(new InfixToken(InfixTokenKind.Number, "0"));
                                tokens.Add(new InfixToken(InfixTokenKind.Operator, c.ToString()));
                            }
                        }
                        else
                        {
                            tokens.Add(new InfixToken(InfixTokenKind.Operator, c.ToString()));
                            i++;
                        }
                        break;

                    case '*':
                    case '/':
                        tokens.Add(new InfixToken(InfixTokenKind.Operator, c.ToString()));
                        i++;
                        break;

                    case '(':
                        tokens.Add(new InfixToken(InfixTokenKind.ParenOpen, "("));
                        i++;
                        break;

                    case ')':
                        tokens.Add(new InfixToken(InfixTokenKind.ParenClose, ")"));
                        i++;
                        break;

                    case ',':
                        tokens.Add(new InfixToken(InfixTokenKind.Comma, ","));
                        i++;
                        break;

                    default:
                        throw new InvalidOperationException(
                            $"Unexpected character '{c}' at position {i} in \"{expression}\"");
                }
            }

            return tokens;
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static InfixToken ReadNumber(string s, ref int i)
        {
            int start = i;
            // Integer part
            while (i < s.Length && char.IsDigit(s[i])) i++;
            // Decimal part
            if (i < s.Length && s[i] == '.')
            {
                i++;
                while (i < s.Length && char.IsDigit(s[i])) i++;
            }
            // Scientific notation: 1e6, 2.5E-3
            if (i < s.Length && (s[i] == 'e' || s[i] == 'E'))
            {
                i++;
                if (i < s.Length && (s[i] == '+' || s[i] == '-')) i++;
                while (i < s.Length && char.IsDigit(s[i])) i++;
            }
            return new InfixToken(InfixTokenKind.Number, s[start..i]);
        }

        private static InfixToken ReadWord(string s, ref int i)
        {
            int start = i;
            // Consume all consecutive sigils ($, &, @, !) then letters/digits/'_'/'.'
            while (i < s.Length && (s[i] == '$' || s[i] == '&' || s[i] == '@' || s[i] == '!'))
                i++;
            while (i < s.Length && (char.IsLetterOrDigit(s[i]) || s[i] == '_' || s[i] == '.'))
                i++;
            return new InfixToken(InfixTokenKind.Word, s[start..i]);
        }

        private static bool IsUnary(List<InfixToken> tokens)
        {
            if (tokens.Count == 0) return true;
            var last = tokens[^1];
            return last.Kind == InfixTokenKind.Operator
                || last.Kind == InfixTokenKind.ParenOpen
                || last.Kind == InfixTokenKind.Comma;
        }
    }
}
