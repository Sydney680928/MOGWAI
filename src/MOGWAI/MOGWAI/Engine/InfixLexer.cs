using System;
using System.Collections.Generic;
using System.Globalization;

namespace MOGWAI.Engine
{
    /// <summary>
    /// Type d'un token issu du lexer infixe.
    /// </summary>
    public enum InfixTokenKind
    {
        Number,         // littéral numérique : 3.14, -2, 1e6
        Word,           // identifiant : X, sin, pow, E, PI…
        Operator,       // + - * / ^ %
        ParenOpen,      // (
        ParenClose,     // )
        Comma           // , séparateur d'arguments de fonction
    }

    /// <summary>
    /// Token produit par <see cref="InfixLexer"/>.
    /// </summary>
    public readonly struct InfixToken
    {
        public InfixTokenKind Kind  { get; }
        public string         Value { get; }

        public InfixToken(InfixTokenKind kind, string value)
        {
            Kind  = kind;
            Value = value;
        }

        public override string ToString() => $"[{Kind}:{Value}]";
    }

    /// <summary>
    /// Transforme une expression infixe en liste de tokens.
    /// Gère : nombres (y compris notation scientifique et signe unaire),
    /// identifiants (variables et fonctions MOGWAI), opérateurs, parenthèses, virgules.
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

                // -- espaces ignorés
                if (char.IsWhiteSpace(c)) { i++; continue; }

                // -- nombre : commence par un chiffre ou '.' 
                if (char.IsDigit(c) || c == '.')
                {
                    tokens.Add(ReadNumber(expression, ref i));
                    continue;
                }

                // -- identifiant : lettre ou '_'
                if (char.IsLetter(c) || c == '_')
                {
                    tokens.Add(ReadWord(expression, ref i));
                    continue;
                }

                // -- opérateurs et ponctuations
                switch (c)
                {
                    case '+':
                    case '-':
                        // Signe unaire ? Oui si le token précédent est absent,
                        // un opérateur, ou une parenthèse ouvrante.
                        if (IsUnary(tokens))
                        {
                            i++;
                            // Consomme le nombre qui suit (ex. -3.14 ou +2)
                            if (i < len && (char.IsDigit(expression[i]) || expression[i] == '.'))
                            {
                                var num = ReadNumber(expression, ref i);
                                tokens.Add(new InfixToken(InfixTokenKind.Number,
                                    c == '-' ? "-" + num.Value : num.Value));
                            }
                            else
                            {
                                // Signe unaire devant une parenthèse ou variable :
                                // on insère 0 et l'opérateur pour que Shunting-yard gère ça
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

                    case '*': case '/': case '%':
                        tokens.Add(new InfixToken(InfixTokenKind.Operator, c.ToString()));
                        i++;
                        break;

                    case '(':
                        tokens.Add(new InfixToken(InfixTokenKind.ParenOpen,  "("));
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
                            $"Caractère inattendu '{c}' à la position {i} dans \"{expression}\"");
                }
            }

            return tokens;
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        private static InfixToken ReadNumber(string s, ref int i)
        {
            int start = i;
            // Partie entière
            while (i < s.Length && char.IsDigit(s[i])) i++;
            // Partie décimale
            if (i < s.Length && s[i] == '.')
            {
                i++;
                while (i < s.Length && char.IsDigit(s[i])) i++;
            }
            // Exposant scientifique : 1e6, 2.5E-3
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
