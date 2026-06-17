using MOGWAI.Objects;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace MOGWAI.Engine
{
    /// <summary>
    /// Convertit une liste de tokens infixes (produits par <see cref="InfixLexer"/>)
    /// en une liste de <see cref="MOGObject"/> prête à être exécutée par le moteur MOGWAI.
    ///
    /// Algorithme : Shunting-yard de Dijkstra avec support des fonctions multi-arguments.
    /// </summary>
    public static class ShuntingYard
    {
        // ── Table de priorité et associativité ─────────────────────────────────

        private static readonly Dictionary<string, (int Precedence, bool RightAssoc)> Operators = new()
        {
            ["+"] = (1, false),
            ["-"] = (1, false),
            ["*"] = (2, false),
            ["/"] = (2, false),
            ["%"] = (2, false),
        };

        // ── Point d'entrée ─────────────────────────────────────────────────────

        /// <summary>
        /// Convertit une expression infixe en <see cref="List{MOGObject}"/>
        /// prête à être wrappée dans un bloc MOGWAI et exécutée.
        /// </summary>
        /// <param name="expression">Expression infixe, ex. "5 * X + (7 + sin(Y))"</param>
        /// <param name="engine">Instance du moteur MOGWAI (pour GetPrimitive)</param>
        public static List<MOGObject> Convert(string expression, MogwaiEngine engine)
        {
            var tokens  = InfixLexer.Tokenize(expression);
            var output  = new List<MOGObject>();    // file de sortie RPN
            var opStack = new Stack<InfixToken>(); // pile d'opérateurs

            for (int i = 0; i < tokens.Count; i++)
            {
                var token = tokens[i];
                switch (token.Kind)
                {
                    // ── Nombre littéral → sortie directe
                    case InfixTokenKind.Number:
                        output.Add(ParseNumber(engine, token.Value));
                        break;

                    // ── Mot : fonction (suivi de '(') → pile, sinon → sortie directe
                    case InfixTokenKind.Word:
                        bool isFunction = i + 1 < tokens.Count
                                          && tokens[i + 1].Kind == InfixTokenKind.ParenOpen;
                        if (isFunction)
                            opStack.Push(token);             // sera dépilé à la ')' correspondante
                        else
                            PushToOutput(output, token, engine); // PI, E, X, myVar…
                        break;

                    // ── Virgule : fin d'argument, dépile jusqu'à '('
                    case InfixTokenKind.Comma:
                        while (opStack.Count > 0 && opStack.Peek().Kind != InfixTokenKind.ParenOpen)
                            PushToOutput(output, opStack.Pop(), engine);
                        if (opStack.Count == 0)
                            throw new InvalidOperationException("Parenthèse manquante ou virgule mal placée.");
                        break;

                    // ── Opérateur
                    case InfixTokenKind.Operator:
                        var (prec, rightAssoc) = Operators[token.Value];
                        while (opStack.Count > 0)
                        {
                            var top = opStack.Peek();
                            if (top.Kind != InfixTokenKind.Operator) break;
                            var (topPrec, _) = Operators[top.Value];
                            if (rightAssoc ? topPrec > prec : topPrec >= prec)
                                PushToOutput(output, opStack.Pop(), engine);
                            else
                                break;
                        }
                        opStack.Push(token);
                        break;

                    // ── Parenthèse ouvrante → empilée telle quelle
                    case InfixTokenKind.ParenOpen:
                        opStack.Push(token);
                        break;

                    // ── Parenthèse fermante
                    case InfixTokenKind.ParenClose:
                        while (opStack.Count > 0 && opStack.Peek().Kind != InfixTokenKind.ParenOpen)
                            PushToOutput(output, opStack.Pop(), engine);
                        if (opStack.Count == 0)
                            throw new InvalidOperationException("Parenthèse fermante sans ouvrante correspondante.");
                        opStack.Pop(); // retire '('

                        // Si une fonction est en sommet → la dépiler vers la sortie
                        if (opStack.Count > 0 && opStack.Peek().Kind == InfixTokenKind.Word)
                            PushToOutput(output, opStack.Pop(), engine);
                        break;
                }
            }

            // ── Vider la pile d'opérateurs
            while (opStack.Count > 0)
            {
                var top = opStack.Pop();
                if (top.Kind == InfixTokenKind.ParenOpen)
                    throw new InvalidOperationException("Parenthèse ouvrante sans fermante correspondante.");
                PushToOutput(output, top, engine);
            }

            return output;
        }

        // ── Helpers ────────────────────────────────────────────────────────────

        /// <summary>
        /// Mappe un token vers un <see cref="MOGObject"/> et l'ajoute à la sortie.
        /// Règle : on demande d'abord au moteur si c'est une primitive.
        /// Si oui → MOGPrimitive (copie). Sinon → MOGWord (résolu à l'exécution).
        /// </summary>
        private static void PushToOutput(List<MOGObject> output, InfixToken token, MogwaiEngine engine)
        {
            // Opérateur standard : +, -, *, /, %
            if (token.Kind == InfixTokenKind.Operator)
            {
                var prim = engine.GetPrimitive(token.Value, true);
                if (prim != null)
                    output.Add(prim);
                else
                    throw new InvalidOperationException(
                        $"Opérateur '{token.Value}' introuvable dans le moteur MOGWAI.");
                return;
            }

            // Mot : primitive connue → sortie directe, sinon MOGWord résolu à l'exécution
            if (token.Kind == InfixTokenKind.Word)
            {
                var prim = engine.GetPrimitive(token.Value, true);
                if (prim != null)
                    output.Add(prim);                     // Primitive : sin, cos, PI, E, pow…
                else
                    output.Add(new MOGWord(engine, token.Value)); // Mot libre : variable, proc…
                return;
            }

            throw new InvalidOperationException($"Token inattendu en sortie : {token}");
        }

        /// <summary>
        /// Parse un littéral numérique en <see cref="MOGObject"/>.
        /// </summary>
        private static MOGObject ParseNumber(MogwaiEngine engine, string value)
        {
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double d))
                return new MOGNumber(engine, d);
            throw new InvalidOperationException($"Impossible de parser le nombre '{value}'.");
        }
    }
}
