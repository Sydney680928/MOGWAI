# MOGWAI, pas à pas

Une découverte concrète du langage de script MOGWAI, une idée à la fois.

> Ce guide part du principe que tu sais déjà programmer — variables, boucles, fonctions, types de données de base. Ce qu'il ne suppose **pas**, en revanche, c'est une quelconque familiarité avec les langages à pile (RPN). C'est la seule vraie courbe d'apprentissage ici, et on va la prendre en douceur.

---

## 1. Qu'est-ce que MOGWAI ?

MOGWAI est un moteur de script léger et embarquable pour .NET. On l'intègre dans une application — desktop, mobile, serveur, objet connecté — et il lui donne un petit langage de script à elle, sûr et extensible.

C'est une description volontairement large, parce que MOGWAI lui-même est volontairement généraliste. Il n'est lié à aucun type d'application ou secteur d'activité en particulier. On l'utilise par exemple pour :

- laisser les utilisateurs finaux personnaliser le comportement d'une application sans nouvelle publication
- piloter de petits workflows d'automatisation ou des séquences scriptées
- scripter la logique d'un jeu ou de petits jeux entiers (il existe une implémentation complète de Snake écrite en MOGWAI)
- construire de petits outils interactifs — calculatrices, applications TUI, REPL
- exposer une surface de script sûre et sandboxable au sein d'une base de code .NET plus large

Sous le capot, MOGWAI est un **langage concaténatif à pile** — la même famille que Forth, Factor, PostScript, et les calculatrices RPN que certains d'entre nous ont connues. C'est de cet héritage que vient le "RPN" dans sa description, mais ne le laisse pas restreindre ta vision de ce à quoi MOGWAI sert *réellement*. Le fonctionnement à pile est un choix d'implémentation qui rend le langage extrêmement simple et non ambigu — pas de priorité d'opérateurs à mémoriser, pas d'ambiguïté d'analyse syntaxique. C'est un moyen, pas une fin.

Quelques faits pratiques avant de commencer :

- MOGWAI s'exécute au sein d'une **application hôte** écrite en C# / .NET. L'hôte embarque un `MogwaiEngine` et exécute les scripts au travers de lui.
- Chaque script MOGWAI est du texte brut. Les commentaires commencent par `#`.
- MOGWAI embarque **plus de 300 primitives natives** couvrant les mathématiques, les chaînes, les listes, les records, les fichiers, HTTP, les expressions régulières, les dates, les données binaires, et plus encore. Ce tutoriel n'a besoin que d'une poignée d'entre elles pour te mettre à l'aise ; le reste est de la matière de référence pour plus tard.

Tu peux essayer tout ce qui est dans ce tutoriel sans rien installer, en utilisant le [playground en ligne](https://sydney680928.github.io/MOGWAI/), ou en exécutant le CLI MOGWAI en local.

---

## 2. Penser en pile

C'est le seul concept qui mérite qu'on s'y arrête vraiment. Une fois qu'il fait "clic", tout le reste dans MOGWAI vient naturellement.

### Oublie "fonction(arguments)" un instant

Dans la plupart des langages que tu écris, tu imbriques les appels les uns dans les autres : `add(multiply(3, 4), 2)`. Pour lire ça, tu dois travailler de l'intérieur vers l'extérieur, et l'ordre d'évaluation n'est pas l'ordre dans lequel tu lis le texte.

MOGWAI se débarrasse entièrement de ça. Il y a une seule **pile** — pense à une pile d'assiettes. Tu ne peux regarder, ajouter, ou retirer qu'au sommet. Un script MOGWAI est une séquence d'instructions lue strictement de gauche à droite :

- une **valeur** (un nombre, une chaîne, ...) est **empilée** au sommet de la pile
- un **opérateur ou une fonction** **dépile** autant de valeurs que nécessaire, fait son travail, et **empile le résultat**

C'est tout le modèle d'exécution. Aucune exception, aucune règle de priorité.

### Un premier calcul, pas à pas

Traçons `3 4 + 2 *` un jeton à la fois :

```
3            # push 3           → stack: [ 3 ]
4            # push 4           → stack: [ 3 4 ]
+            # pop 4 and 3, push 3+4    → stack: [ 7 ]
2            # push 2           → stack: [ 7 2 ]
*            # pop 2 and 7, push 7*2    → stack: [ 14 ]
```

À la fin, `14` se trouve au sommet de la pile. Écrit sur une seule ligne, ça donne :

```
3 4 + 2 * ?
```

Le `?` final est l'instruction MOGWAI "affiche le sommet de la pile" — on l'utilisera constamment dans ce tutoriel pour voir les résultats. Cette ligne affiche `14`.

C'est ce qu'on appelle la **notation polonaise inversée (RPN)** : l'opérateur vient *après* ses opérandes, au lieu d'être entre eux. `3 4 +` se lit "3, 4, additionne" plutôt que "3 + 4" — mais ça produit exactement le même résultat. Les jetons sur la page sont, littéralement, l'ordre d'exécution. Ce que tu vois est ce qui se passe — rien n'est évalué dans le désordre, rien n'a besoin d'une lecture mentale "de l'intérieur vers l'extérieur".

### Pourquoi s'embêter avec ça ?

Deux bénéfices très concrets découlent de tout ça :

- **Zéro ambiguïté.** Il n'y a aucune priorité d'opérateur à retenir, parce qu'il n'y a aucune priorité du tout — juste une exécution de gauche à droite. `3 4 + 2 *` ne peut avoir qu'un seul sens possible.
- **Composabilité.** De petits morceaux s'enchaînent naturellement. Toute séquence d'instructions qui laisse une seule valeur propre sur la pile peut être insérée dans une séquence plus large, exactement comme on branche un tuyau dans un autre.

### Quelques exemples de plus

```
5 3 - ?          # → 2      (5, 3, subtract)
10 2 /  ?        # → 5      (10, 2, divide)
2 3 4 + * ?      # → 14     (2, then 3+4=7, then 2*7=14)
```

Ce dernier vaut la peine d'être tracé à la main : empile `2`, empile `3`, empile `4`, `+` dépile `4` et `3` et empile `7` — la pile est maintenant `[ 2 7 ]` — puis `*` dépile `7` et `2` et empile `14`.

### Pas encore prêt à convertir chaque formule à la main ? Pas besoin.

MOGWAI inclut une primitive `calc` qui accepte une expression infixe classique — parenthèses, priorité des opérateurs, tout — sous forme de chaîne, et l'évalue pour toi :

```
"5 * 3 + (7 + 2)" calc ?      # → 24
```

C'est un pont vraiment utile pendant que tu construis encore ton intuition RPN, et pas mal de code MOGWAI réel s'appuie dessus pour tout ce qui est calculatoire. On y reviendra plus en détail plus tard. Pour le reste de ce tutoriel, cependant, on va s'en tenir au RPN natif — ça vaut le coup de construire cette habitude tôt, et une fois que tu en as tracé quelques-uns à la main, ça cesse très vite de paraître inhabituel.

---

*Suite : écrire et exécuter ton premier programme MOGWAI complet.*

---

## 3. Ton premier programme

Un script MOGWAI est juste une séquence d'instructions en texte brut, exécutée de haut en bas, de gauche à droite. Pas de fonction `main`, pas de code passe-partout — le script *est* le programme.

### La seule habitude à prendre immédiatement

Avant toute chose, prends l'habitude de commencer chaque script par :

```
mogwai.reset
```

Ça te donne un moteur parfaitement propre : pas de variable restante, pas de timer en cours, pas de tâche en attente — rien qui traîne d'une exécution précédente. Ça compte moins dans un scénario d'intégration en une seule exécution, mais ça compte beaucoup dès que tu expérimentes de façon interactive (le CLI MOGWAI, le playground en ligne) où l'état s'accumulerait sinon silencieusement d'une exécution à l'autre. Habitude peu coûteuse, vrai bénéfice — mets-la toujours en premier.

### Hello, MOGWAI

```
mogwai.reset

"Hello from MOGWAI!" ?
```

Deux choses se passent ici : la chaîne `"Hello from MOGWAI!"` est empilée, et `?` la dépile et l'affiche, suivie d'un retour à la ligne. C'est tout le programme.

### Les deux instructions d'affichage

Tu vas utiliser ces deux-là constamment, donc soyons précis dès le départ :

- `?` — affiche le sommet de la pile, **avec** un retour à la ligne final. Raccourci pour `console.println`.
- `??` — affiche le sommet de la pile, **sans** retour à la ligne final. Raccourci pour `console.print`.

Les deux acceptent *n'importe quel* type directement — un nombre, une chaîne, un booléen, une liste — sans conversion nécessaire :

```
mogwai.reset

"Result: " ??
2 3 + ?
```

Ça affiche :

```
Result: 5
```

> **Remarque si tu utilises le playground en ligne.** Le [playground basé sur Blazor](https://sydney680928.github.io/MOGWAI/) affiche sa sortie ligne par ligne, donc `??` s'y comporte comme `?` — chaque affichage finit sur sa propre ligne, quel que soit celui que tu as utilisé. La distinction est réelle et compte dans la plupart des environnements hôtes (applications console, le CLI, applications embarquées) ; ne sois simplement pas surpris de ne pas la voir spécifiquement dans le playground.

### Un premier programme un peu plus étoffé

Assemblons quelques éléments — rien ici n'a encore été introduit en détail (les variables et les fonctions arrivent juste après), mais ça devrait déjà se lire assez naturellement :

```
mogwai.reset

"MOGWAI says hello!" ?
"The answer to a few small calculations:" ?

3 4 + ?          # → 7
10 2 / ?         # → 5
2 8 * ?          # → 16
```

Exécute-le, et tu devrais voir :

```
MOGWAI says hello!
The answer to a few small calculations:
7
5
16
```

C'est un programme MOGWAI complet et valide. Tout ce qui suit consiste à te donner davantage de briques à mettre à l'intérieur d'un tel programme.

---

## 4. Variables

### Stocker une valeur

Une variable est créée la première fois que tu lui assignes une valeur — aucune étape de déclaration séparée n'est requise. L'assignation utilise l'opérateur `->`, avec la valeur d'abord (elle vient de la pile, rappelle-toi) et le nom de la variable entre guillemets simples :

```
mogwai.reset

500 -> 'A'
A ?              # → 500
```

Remarque l'asymétrie : quand tu *assignes* à `A`, tu écris son nom entre guillemets simples, `'A'` — tu nommes une cible, tu ne lis pas une valeur. Quand tu *lis* `A`, tu l'écris nu, sans guillemets — ça empile sa valeur actuelle.

Une variable n'est pas non plus verrouillée au type de sa première valeur — lui assigner autre chose remplace simplement à la fois la valeur et, si besoin, le type :

```
mogwai.reset

500 -> 'A'
A ?                    # → 500

"Hello!" -> 'A'
A ?                    # → Hello!
```

### Locale vs. globale

Ça, c'est une variable **locale** — elle n'existe que pour la durée de l'exécution du script en cours (ou de l'appel de fonction en cours, comme on le verra plus tard). Si tu préfixes le nom par `$`, elle devient **globale** à la place :

```
mogwai.reset

500 -> '$R'
$R ?             # → 500
```

La différence pratique : quand le moteur hôte est configuré avec `keepAlive: true` (typiquement pour un usage interactif — un REPL, le playground en ligne), les variables globales survivent à travers plusieurs exécutions de script séparées, tandis que les locales sont limitées à une seule exécution. Pour un scénario d'intégration en une seule exécution, cette distinction compte moins — mais ça vaut le coup de savoir ce que signifie le préfixe `$`, puisque tu le verras partout dans le code et les exemples MOGWAI.

### Utiliser une variable dans un calcul

Lire une variable, c'est simplement écrire son nom nu — ça empile sa valeur actuelle comme n'importe quelle autre valeur :

```
mogwai.reset

20 -> 'A'
30 -> 'B'

A B + -> 'C'
C ?              # → 50
```

### Verrouiller un type, exiger des déclarations

Deux choses valent la peine d'être connues même si on ne va pas s'y attarder ici — tu croiseras les deux dans du vrai code MOGWAI :

Une variable peut être verrouillée à un seul type dès le départ, avec `=>` à la place de `->` :

```
500 => 'A'
```

À partir de là, `A` n'accepte plus que des nombres — lui assigner une chaîne lèverait une erreur au lieu de changer silencieusement son type.

Séparément, on peut demander à un moteur d'*exiger* que chaque variable soit déclarée avant d'être utilisée, avec `mogwai.strict` :

```
true mogwai.strict
100 => 'A'
A ?
```

Une fois le mode strict activé, utiliser une variable qui n'a jamais été déclarée lève une erreur au lieu de la créer silencieusement. Les deux sont des filets de sécurité optionnels plutôt que quelque chose dont tu as besoin dès le premier jour — on ne les nomme ici que pour que la notation ne paraisse pas inconnue plus tard.

### Supprimer une variable

```
mogwai.reset

10 -> 'A'
'A' purge
```

`purge` supprime une variable explicitement. En pratique tu en as rarement besoin pour les locales — elles disparaissent automatiquement une fois leur portée terminée — mais c'est là quand tu veux libérer quelque chose délibérément, ou récupérer un nom.

> **Une remarque sur ce qui arrive plus tard.** Tu verras parfois des variables écrites avec des préfixes supplémentaires dans du code MOGWAI — `@A`, `&A`, `!A`. Ce sont toutes encore "la variable A" au fond, juste accédée différemment (une lecture plus rapide, une mutation en place, une évaluation immédiate). On les laisse volontairement de côté pour l'instant — `A` tout simple suffit à te rendre productif, et on reviendra sur les autres une fois que les fonctions et les conteneurs (listes, records) seront sur la table, là où ils commencent réellement à compter.

---

*Suite : la poignée de types de base que porte chaque valeur MOGWAI, et comment les distinguer.*

---

## 5. Types de base

Chaque valeur en MOGWAI porte un type, et chaque nom de type commence par un point : `.number`, `.string`, `.boolean`, etc. Tu peux demander son type à n'importe quelle valeur avec `->type` :

```
mogwai.reset

1567 ->type ?         # → .number
"Hello" ->type ?      # → .string
true ->type ?         # → .boolean
```

Pour ce tutoriel, trois types comptent immédiatement :

| Type | Ce que c'est | Exemple |
|------|------------|---------|
| `.number` | Un nombre — MOGWAI ne distingue pas les entiers des décimaux, c'est un seul type numérique | `154` ou `-56.34` |
| `.string` | Une chaîne de caractères | `"Hello world"` |
| `.boolean` | Une valeur de vérité | `true` / `false` |

Tu utilises déjà les trois sans avoir besoin d'y penser — `3 4 +` fonctionne sur des valeurs `.number`, `"Hello!" ?` fonctionne sur une `.string`.

Une vérification de type sert souvent de base à un branchement, exactement comme tu t'y attendrais :

```
mogwai.reset

234 -> 'A'
if (A ->type .number ==) then { "A is a number" ? } else { "A is not a number" ? }
```

(`if` / `then` / `else` ont droit à leur propre introduction en bonne et due forme dans la section sur le contrôle de flux — pour l'instant, remarque juste que `->type .number ==` se lit naturellement de gauche à droite : récupère le type de `A`, compare-le à `.number`.)

Au-delà de ces trois-là, MOGWAI a plusieurs autres types que tu rencontreras au fil de ce tutoriel — `.list`, `.record`, `.function`, `.code`, `.data`, et quelques autres — chacun avec sa propre section à venir. Pas besoin de mémoriser la liste complète maintenant ; `->type` est toujours là quand tu veux vérifier ce que tu as réellement entre les mains.

---

## 6. Chaînes de caractères

Les chaînes bénéficient d'un support dédié assez conséquent dans MOGWAI — cette section couvre les opérations du quotidien ; il existe une famille bien plus large de primitives `str.*` dans la référence des fonctions pour tout ce qui est plus spécialisé.

### Concaténation

`+` concatène des chaînes — et il est un peu plus malin qu'un simple opérateur de chaînes, puisqu'il accepte aussi un nombre de chaque côté et le convertit automatiquement :

```
mogwai.reset

"HELLO " "WORLD" + ?      # → HELLO WORLD
"HELLO" 3 + ?             # → HELLO3
3 "HELLO" + ?             # → 3HELLO
```

### Extraire une partie d'une chaîne

Une poignée de primitives couvrent les cas courants :

```
mogwai.reset

"HELLO WORLD" 0 5 sub ?       # → HELLO       (from index 0, 5 characters)
"HELLO WORLD" first ?         # → H
"HELLO WORLD" last ?          # → D
"HELLO WORLD" 3 left ?        # → HEL
"HELLO WORLD" 3 right ?       # → RLD
"HELLO WORLD" butfirst ?      # → ELLO WORLD
"HELLO WORLD" butlast ?       # → HELLO WORL
```

### Taille et recherche

```
mogwai.reset

"HELLO WORLD" size ?          # → 11

"HELLO WORLD" "O" where ?     # → (4 7)   — every position where "O" occurs
```

### Casse et jointure

```
mogwai.reset

"HELLO WORLD" ->lower ?              # → hello world
"hello world" ->upper ?              # → HELLO WORLD

("X" "Y" "Z") ";" join ?             # → X;Y;Z
"X;Y;Z" ";" split ?                  # → (X Y Z)
```

### Construire des chaînes à partir de variables — l'interpolation

Plutôt que de concaténer les morceaux à la main, tu peux écrire une chaîne modèle avec des blocs d'interpolation — `{! ... }` — et laisser MOGWAI les remplir pour toi avec `eval` :

```
mogwai.reset

"DOE John" -> 'name'
50 -> 'age'

"{! name} is {! age} years old" eval ?

# → DOE John is 50 years old
```

Tout ce qui se trouve entre `{! }` est évalué comme du code MOGWAI ordinaire, pas juste une variable nue — tu peux donc y enchaîner des opérations :

```
mogwai.reset

"DOE John" -> 'name'

"Name in caps: {! name ->upper}" eval ?

# → Name in caps: DOE JOHN
```

### Séquences d'échappement

À l'intérieur d'un littéral de chaîne, un antislash introduit une séquence d'échappement — les habituelles sont là : `\"` pour un guillemet littéral, `\\` pour un antislash littéral, `\n` pour un retour à la ligne, `\t` pour une tabulation. Elles sont résolues quand la chaîne est évaluée :

```
mogwai.reset

"Hello, \"World\" !" eval ?     # → Hello, "World" !
"Line1\nLine2" eval ?           # → Line1 and Line2, on two separate lines
```

---

*Suite : prendre des décisions et répéter des actions — conditions et boucles.*

---

## 7. Contrôle de flux

### Conditions avec `if` / `then` / `else`

`if` prend un test entre parenthèses, un bloc à exécuter avec `then`, et optionnellement un bloc à exécuter avec `else` :

```
mogwai.reset

50 -> 'A'

if (A 50 ==) then
{
    "A has a value of 50" ?
}
else
{
    "A does not have a value of 50" ?
}
```

Lis le test de la même façon que n'importe quelle expression RPN : `A 50 ==` empile `A`, empile `50`, puis dépile les deux et empile `true` ou `false`. Le test **doit** laisser un booléen sur la pile — `if ("TOTO") then {...}` lève une erreur, puisqu'une chaîne n'est pas une condition valide.

Voici la boîte à outils de comparaison et de logique que tu utiliseras dans ces parenthèses :

| Expression | Signification |
|------------|---------|
| `X Y ==`  | X est-il égal à Y ? |
| `X Y !=`  | X est-il différent de Y ? |
| `X Y >`   | X est-il supérieur à Y ? |
| `X Y <`   | X est-il inférieur à Y ? |
| `X Y >=`  | X est-il supérieur ou égal à Y ? |
| `X Y <=`  | X est-il inférieur ou égal à Y ? |
| `X not`   | NON logique de X |
| `X Y or`  | X OU Y |
| `X Y and` | X ET Y |
| `X Y xor` | X OU-EXCLUSIF Y |

Ils se combinent exactement comme tu t'y attendrais, de gauche à droite :

```
mogwai.reset

15 -> 'age'

if (age 18 >= age 65 < and) then
{
    "Standard rate applies" ?
}
```

### Éviter une cascade de `if` / `else` : `switch`

Quand tu as plusieurs conditions mutuellement exclusives, `switch` se lit mieux qu'une chaîne de `if` / `else if`. C'est une série de paires `(test) then { ... }` ; le **premier** test qui renvoie `true` exécute son bloc, et seulement celui-là :

```
mogwai.reset

150 -> 'a'

switch
{
    (a 100 <) then
    {
        "< 100" ?
    }

    (a 200 <) then
    {
        "< 200" ?
    }

    (true) then
    {
        "DEFAULT" ?
    }
}

# → < 200
```

Ce dernier `(true) then { ... }` est la façon habituelle d'écrire une branche par défaut / attrape-tout — un test qui ne peut jamais échouer.

### Une petite notation à connaître avant de boucler : `++` et `--`

Les boucles ont constamment besoin d'incrémenter un compteur. Plutôt que d'écrire `A 1 + -> 'A'` à chaque fois, MOGWAI te donne un raccourci — passe le nom de variable **entre guillemets simples** à `++` ou `--` et il est incrémenté ou décrémenté en place :

```
mogwai.reset

100 -> 'A'
'A' ++
A ?              # → 101
```

Tu verras `'I' ++` partout dans les exemples de boucles ci-dessous.

### `repeat` — exécuter un bloc un nombre fixe de fois

```
mogwai.reset

0 -> 'I'

10 repeat
{
    'I' ++
    I ?
}

# → 1 2 3 4 5 6 7 8 9 10, each on its own line
```

### `for` — un compteur de boucle géré automatiquement

`for` gère lui-même la variable de compteur — tu lui donnes juste une valeur de départ, une valeur de fin, et un nom :

```
mogwai.reset

1 10 for 'I' do
{
    I ?
}

# Counting down instead, with an explicit step
10 1 for 'I' step -1 do
{
    I ?
}
```

### `while` et `do … while`

`while` teste la condition **avant** chaque itération ; `do … while` la teste **après**, donc le bloc s'exécute toujours au moins une fois :

```
mogwai.reset

0 -> 'I'

while (I 100 <) do
{
    'I' ++
    I ?
}
```

```
mogwai.reset

0 -> 'I'

do
{
    'I' ++
    I ?
} while (I 100 <)
```

### `forever` — et comment l'arrêter réellement

```
mogwai.reset

0 -> 'I'

forever do
{
    'I' ++
    I ?

    if (I 456 ==) then { break }
}
```

`break` sort immédiatement de la boucle la plus imbriquée — ça fonctionne de la même façon dans chaque type de boucle ci-dessus, à chaque fois que tu as besoin de sortir avant la condition de fin naturelle.

### `foreach` — parcourir une liste

`foreach` existe en trois variantes, et celle vers laquelle tu te tournes dépend de ce que tu essaies de produire :

```
mogwai.reset

# Just visit each element — foreach...do
("L1" "L2" "L3") foreach 'item' do { item ? }

# Build a new list by transforming each element — foreach...transform
(1 2 3 4 5) foreach 'item' transform { item 2 * }
# → (2 4 6 8 10)

# Keep only the elements matching a condition — foreach...filter
(1 2 3 4 5 6 7 8 9 10) foreach 'item' filter { item 2 mod 0 == }
# → (2 4 6 8 10)
```

`foreach...do` s'exécute sur la pile principale, exactement comme le reste de ton script — rien de surprenant là-dedans. `foreach...transform` et `foreach...filter` sont un peu plus particuliers : chaque itération s'exécute sur **sa propre pile isolée** plutôt que sur la principale, donc elle peut librement lire les variables locales et globales mais ne peut ni piocher dans, ni laisser quoi que ce soit sur la pile en dehors du bloc. Ce qu'elle laisse derrière elle — la valeur transformée, ou le booléen décidant de l'inclusion — c'est ce que la boucle rassemble dans la liste résultante. On reviendra sur les listes en bonne et due forme dans la section suivante ; ceci suffit juste à donner du sens à ces boucles quand tu les rencontres.

---

*Suite : les listes — les collections ordonnées de MOGWAI, et les opérations qui vont avec.*

---

## 8. Listes

Une liste est une collection ordonnée de valeurs — et contrairement à certains langages, une liste MOGWAI n'est pas typée pour ne contenir qu'un seul genre de chose. Les listes s'écrivent avec des parenthèses, les éléments séparés par des espaces (pas de virgules) :

```
(1 2 7)                              # a list of numbers
("X1" "X2" "X3")                     # a list of strings
("X1" 45 (1 2 3) true)               # a mix — a list can even contain lists
```

### Créer une liste

La notation littérale ci-dessus est la façon la plus simple. Tu peux aussi en construire une à partir de valeurs déjà sur la pile, en disant à `->list` combien en rassembler :

```
mogwai.reset

10 20 30 40 50 5 ->list ?      # → (10 20 30 40 50)
```

### Ajouter un élément

`+` ajoute à une liste — et si tu ajoutes une autre liste, elle entre comme un seul élément imbriqué plutôt que d'être aplatie :

```
mogwai.reset

(10 20 30) 40 + ?              # → (10 20 30 40)
(10 20 30) (100 200) + ?       # → (10 20 30 (100 200))
```

### Lire et écrire par index

Les index commencent à zéro. `get` lit, `set` écrit — et renvoie la liste modifiée plutôt que de muter en place :

```
mogwai.reset

(10 20 30 40 50 60 70) 5 get ?          # → 60

(10 "E" 55 20 30) 2 "Z" set ?           # → (10 "E" "Z" 20 30)
```

Demander à `get` un index hors des limites de la liste lève une erreur (**MW.22**, valeur d'argument invalide) — les listes sont strictes là-dessus. Comme on va le voir juste après, les records sont plus indulgents quand une clé n'existe pas.

### Taille, premier, dernier

```
mogwai.reset

(10 20 30 40) size ?                    # → 4
(10 20 30 40 50 60 70) first ?          # → 10
(10 20 30 40 50 60 70) last ?           # → 70
```

### Trier

`sort` fonctionne quand tous les éléments partagent le même type — nombres, chaînes, et quelques types de type identifiant. Les listes à types mixtes sont renvoyées inchangées plutôt que de lever une erreur :

```
mogwai.reset

(1 10 2 5) sort ?      # → (1 2 5 10)
```

### Rechercher

`contains` répond par un simple oui/non ; `where` te dit à quelles positions une valeur apparaît :

```
mogwai.reset

("L1" "L2" "L3" "L4") "L4" contains ?     # → true

(10 20 "XX" "EA" 670 true "XX") "XX" where ?    # → (2 6)
```

### Un aperçu : atteindre des structures imbriquées en une seule étape

Une fois que les listes commencent à contenir des records (des structures clé/valeur — introduites correctement juste après), tu voudras souvent une valeur qui se trouve à plusieurs niveaux de profondeur. Plutôt que de l'extraire étape par étape, tu peux passer à `get` un **chemin** — une liste d'index et de clés à suivre — et il résout le tout en une seule opération :

```
mogwai.reset

([id: 0 name: "DOE"] [id: 1 name: "SMITH"] [id: 2 name: "BLACK"]) (1 name:) get ?

# → SMITH
```

On verra dans un instant que les records gèrent une clé manquante plus en douceur qu'une liste ne gère un mauvais index — bon à garder en tête une fois que tu commences à combiner les deux.

C'est la boîte à outils du quotidien pour les listes. Il existe une plus longue liste de primitives liées aux listes dans la référence des fonctions — `insert`, `extract`, `sub`, la conversion d'une liste en tableau de bytes, et plus — pour quand tu en auras besoin.

---

*Suite : les records — les structures clé/valeur de MOGWAI, et comment elles s'associent naturellement aux listes.*

---

## 9. Records

Un record est la structure clé/valeur de MOGWAI — pense-y comme un dictionnaire, ou les champs d'un objet. Les records s'écrivent avec des crochets, et chaque entrée est une **clé** (un nom se terminant par `:`) suivie de sa valeur :

```
[x: 100 y: 50]              # a record with two keys, x: and y:
[]                           # an empty record
```

Une clé ne peut apparaître qu'une seule fois — si tu l'écris deux fois, la dernière valeur l'emporte : `[x: 10 x: 100]` équivaut à `[x: 100]`.

### Lire une valeur

`get` prend le record et la clé :

```
mogwai.reset

[x: 100 y: 200] y: get ?      # → 200
```

### Ajouter ou modifier une clé

`set` fonctionne aussi de la même façon pour les deux cas — ajouter une clé qui n'existe pas encore, ou en écraser une qui existe déjà :

```
mogwai.reset

[x: 100 y: 200] z: 300 set ?      # → [x: 100 y: 200 z: 300]
[x: 100 y: 200] y: 2000 set ?     # → [x: 100 y: 2000]
```

### Un `get` plus indulgent que celui des listes

C'est le seul endroit où records et listes divergent vraiment : demander à une liste un index hors limites lève une erreur, comme on l'a vu à l'instant — mais demander à un record une clé qui n'existe pas renvoie simplement `null`, sans lever d'erreur. Ça vaut le coup de bien le garder en tête, car il est facile de supposer que les deux conteneurs se comportent de la même façon.

### Atteindre des structures imbriquées en une seule étape

Même idée que l'aperçu du "chemin imbriqué" des listes de la section précédente — un record fait de records (ou de listes) imbriqués peut être parcouru en un seul `get`, en lui passant un chemin plutôt qu'une seule clé :

```
mogwai.reset

[id: 1 name: "DOE" gps: [latitude: 45 longitude: 5]] (gps: latitude:) get ?

# → 45
```

### Le reste de la boîte à outils du quotidien

```
mogwai.reset

[x: 100 y: 200] size ?                     # → 2                (number of keys)
[x: 100 y: 200] keys ?                     # → (x: y:)          (list of keys)

[x: 100 y: 200 z: 70 u: 10] (x: y:) extract ?    # → [x: 100 y: 200]
# Asking for a key that doesn't exist doesn't raise an error either —
# it's simply included in the result with the value null:
[x: 20 y: 20 z: 100] (x: t:) extract ?           # → [x: 20 t: null]

[x: 10 y: 20] y: contains ?                # → true
[x: 10 y: 20] x: purge ?                   # → [y: 20]
```

### Un raccourci à connaître tôt : `->key:` et `<-key:`

Parce que lire et écrire un champ de record par son nom est tellement courant, MOGWAI a des raccourcis compacts pour les deux — un nom de variable suivi directement de `->` ou `<-` et de la clé. Pour lire :

```
mogwai.reset

[x: 10 y: 20] -> 'R'
R->y: ?          # → 20, exactly equivalent to: R y: get ?
```

Écrire fonctionne de la même façon, avec la nouvelle valeur poussée en premier — mais remarque que tout seul, `<-` ne touche pas la variable d'origine. Il laisse la **copie modifiée** sur la pile, exactement comme le fait `set` :

```
mogwai.reset

[x: 10 y: 20] -> 'R'
1000 R<-y: ?        # → [x: 10 y: 1000] — R itself is still [x: 10 y: 20]
```

Si tu veux vraiment que `R` lui-même soit mis à jour, c'est là qu'intervient le sigil `&` — `&R<-y:` mute `R` en place au lieu de renvoyer une copie. On a reporté `&` volontairement ; il aura droit à une vraie introduction, aux côtés de tout ce qu'il fait par ailleurs, dans la section sur les sigils qui arrive.

---

*Suite : les fonctions — déclarer ton propre comportement, de la forme la plus simple jusqu'aux paramètres nommés et entièrement validés.*

---

## 10. Fonctions

Tout ce que tu as appelé jusqu'ici — `+`, `sort`, `str.upper`, `console.println`... — est une fonction que MOGWAI fournit déjà. Cette section parle d'écrire les tiennes.

### Déclarer une fonction basique

`to 'name' do { ... }` déclare une fonction. Le bloc prend ce dont il a besoin directement sur la pile, exactement comme le ferait n'importe quelle primitive native :

```
mogwai.reset

to 'square' do { dup * }

5 square ?          # → 25
```

`dup` duplique le sommet de la pile — donc `5 square` s'exécute ainsi : empile `5`, `dup` (la pile est maintenant `[5 5]`), puis `*` les multiplie. Une fonction est une valeur `.function` sous le capot — tu es libre d'en appeler une depuis l'intérieur d'une autre, comme le fait `cube` en réutilisant `square` ici :

```
mogwai.reset

to 'square' do { dup * }
to 'cube' do { dup square * }

5 cube ?          # → 125
```

### Deux façons d'appeler n'importe quelle fonction : RPN natif ou style classique

Tu as déjà vu ce schéma pour les primitives natives, et il s'applique à l'identique aux fonctions que tu déclares : les arguments d'abord en RPN natif, ou le nom de la fonction d'abord avec des parenthèses dans le style classique plus familier.

```
mogwai.reset

to 'square' do { dup * }

5 square ?         # native RPN
square(5) ?        # classic-style — strictly equivalent
```

Pour plusieurs arguments, ils sont simplement séparés par des espaces à l'intérieur des parenthèses — rappel, jamais de virgules :

```
mogwai.reset

to 'fx' with [a: .number b: .number x: .number] do { a x * b + }

5 9 156 fx ?           # native RPN
fx(5 9 156) ?           # classic-style
```

(`fx` utilise ici des paramètres typés — introduits en bonne et due forme juste en dessous. La convention d'appel est la même quelle que soit la façon dont la fonction a été déclarée.)

### Vérifier les types de paramètres

Une fonction basique fait confiance à ce qui se trouve sur la pile. Si tu préfères que MOGWAI vérifie les types pour toi — et donne des noms aux paramètres au lieu de les extraire à coup de jongleries `dup`/`swap` — déclare-la avec `with` suivi d'une liste de paramètres typés :

```
mogwai.reset

to 'square' with [x: .number] do { x dup * }

5 square ?          # → 25

"EEE" square ?
# raises an error:
#   bad argument type
#   .number expected but .string found for 'x' parameter
```

L'appeler avec trop peu de valeurs sur la pile lève à la place une erreur **trop peu d'arguments** — la vérification a lieu avant même que le corps ne s'exécute.

Plusieurs paramètres typés se déclarent de la même façon, dans l'ordre où ils sont attendus sur la pile :

```
mogwai.reset

# y = a*x + b, i.e. in RPN: a x * b +
to 'fx' with [a: .number b: .number x: .number] do { a x * b + }

5 9 156 fx ?          # → 789
```

Si tu as besoin d'un paramètre dont le type ne doit pas du tout être vérifié, utilise `.any` :

```
mogwai.reset

to 'nPrint' with [x: .any] do
{
    if (x ->type .number ==) then
    {
        "It is a number !" ?
    }
    else
    {
        "It is not a number !" ?
    }
}

234 nPrint      # → It is a number !
true nPrint     # → It is not a number !
```

### Paramètres nommés

Les paramètres positionnels deviennent moins lisibles à mesure que leur nombre augmente — le troisième argument, c'était `x` ou `b` ? Les paramètres nommés résolvent ça en faisant tout transiter par un seul record, avec les clés comme noms. Déclare avec `params` au lieu de `with` :

```
mogwai.reset

to 'fx' params [a: .number b: .number x: .number] do { a x * b + }

[a: 5 b: 9 x: 156] fx ?          # → 789
```

Les fonctions à paramètres nommés ont aussi droit à leur propre sucre syntaxique classique — des crochets au lieu de parenthèses, et le nom de la fonction peut se placer soit juste avant le record, soit comme première entrée du record :

```
mogwai.reset

to 'fx' params [a: .number b: .number x: .number] do { a x * b + }

[a: 5 b: 9 x: 156] fx ?       # native — record, then function
fx[a: 5 b: 9 x: 156] ?        # classic-style
[fx a: 5 b: 9 x: 156] ?       # function name as the record's first entry
```

Les trois sont strictement équivalentes — choisis celle qui se lit le mieux selon le contexte.

### Valeurs par défaut

Donne une valeur par défaut à un paramètre en associant son type à une valeur dans une petite liste, `(.type default)`. Si le record de l'appelant n'inclut pas cette clé, la valeur par défaut est utilisée à la place — et toute clé supplémentaire que l'appelant fournit au-delà de celles déclarées est simplement ignorée :

```
mogwai.reset

to 'foo' params [id: .number name: .string save: (.boolean true)] do
{
    "id   = {! id}" eval ?
    "name = {! name}" eval ?
    "save = {! save}" eval ?
}

[foo id: 10 name: "DOE John"]
# save = true (default)

[foo id: 30 name: "SMITH Mike" save: false]
# save = false (explicitly provided)
```

### Un piège à retenir : les fonctions attendant une seule liste

Celui-ci s'applique à tout appel écrit en style classique, natif ou déclaré par toi : les parenthèses se contentent de déplacer ce qu'il y a à l'intérieur sur la pile — elles ne groupent pas les valeurs en une liste. Une fonction dont le paramètre *entier* est une seule liste (`max`, `min`, `sum`, `sort`, ou une que tu écris toi-même de cette façon) a besoin que cette liste soit entourée de ses propres parenthèses à l'intérieur de l'appel :

```
mogwai.reset

max((1 2 3)) ?      # correct — inner (1 2 3) is the list, outer () is the call
max(1 2 3) ?         # wrong — pushes 3 separate values, not a list
```

### Vérifier ce que renvoie une fonction

Tout comme `with` vérifie les types qui entrent, `returns` vérifie le type qui sort — ajoute-le avant `do`, avec le(s) type(s) attendu(s) dans une liste :

```
mogwai.reset

to 'square' with [x: .number] returns (.number) do { x dup * }
```

`returns` fonctionne aux côtés de n'importe lequel des styles de déclaration ci-dessus — basique, `with`, ou `params`.

### Lister ce que tu as déclaré

```
mogwai.reset

to 'square' do { dup * }
to 'cube' do { dup square * }

funcs ?          # → ('square' 'cube')
```

Pratique pour vérifier qu'une fonction existe avant de s'appuyer dessus — utile dès que tu commences à écrire du code qui compose des fonctions dynamiquement.

---

*Suite : une poignée de notations avancées — `&`, `!`, `@`, et `-->` — qui rendent le travail avec les variables et les conteneurs plus rapide et plus expressif.*

---

## 11. Sigils avancés

Dans la section sur les variables, on a volontairement laissé de côté quelques notations pour plus tard — `A` tout simple suffisait à te rendre productif. Maintenant que les fonctions et les conteneurs (listes, records) sont sur la table, elles méritent vraiment leur place. Il y a quatre façons de lire une variable en MOGWAI :

| Notation | Comportement |
|----------|--------------|
| `A`  | Empile une **copie** de la valeur de A |
| `&A` | Empile une **référence** vers A, pour une mutation en place |
| `@A` | Une lecture résolue statiquement — plus rapide, même résultat que `A` |
| `!A` | Évalue directement le contenu de A |

### `&` — muter une variable en place

`A` tout simple te donne toujours une copie. Transforme-la, et tu dois explicitement la stocker en retour :

```
mogwai.reset

"bonjour" -> 'A'
A ->upper butfirst butlast -> 'A'
A ?          # → ONJOU
```

C'est très bien pour de petites valeurs, mais reconstruire et re-stocker une copie à chaque étape devient coûteux pour quelque chose de plus gros — une grande liste, par exemple. Préfixer une variable par `&` empile une référence directe à la place, donc une fonction qui la prend en charge la modifie **en place**, sans aucune copie impliquée :

```
mogwai.reset

"bonjour" -> 'A'
&A ->upper
A ?          # → BONJOUR — modified directly, no re-assignment needed
```

C'est exactement le mécanisme derrière le raccourci d'écriture de record/liste de la section précédente : `&$R<-y:` mute `$R` directement, là où `$R<-y:` tout seul aurait laissé une copie non assignée sur la pile. Toutes les fonctions ne supportent pas les références — passer `&A` à une qui ne le fait pas lève une erreur `bad argument type`.

La différence de performance est substantielle — en pratique, utiliser `&` plutôt que le schéma copie-et-réassigne peut être plus de mille fois plus rapide sur des données non triviales. Ça vaut le coup d'y penser à chaque fois que tu transformes la même variable de façon répétée.

### `-->` — enchaîner plusieurs transformations en place

Préfixer chaque étape individuelle par `&` devient verbeux dès que tu enchaînes plusieurs transformations :

```
mogwai.reset

"bonjour" -> 'A'
&A ->upper  &A butfirst  &A butlast
A ?          # → ONJOU
```

L'opérateur `-->` applique toute une liste de transformations à une variable en une seule expression à la place — chaque étape s'exécute en séquence, en se nourrissant de la valeur actuelle de la variable :

```
mogwai.reset

"bonjour" -> 'A'
(->upper butfirst butlast) --> &A
A ?          # → ONJOU
```

Les étapes peuvent aussi être des blocs de code complets (`{ ... }`) plutôt que de simples noms de fonction, quand une étape a besoin de faire plus qu'appeler une seule fonction :

```
mogwai.reset

"hello world" -> 'A'
(->upper { " !" + }) --> &A
A ?          # → HELLO WORLD !
```

`-->` est aussi **transactionnel** : si une étape du pipeline lève une erreur, la variable est ramenée à sa valeur d'avant le début du pipeline, et l'erreur se propage normalement.

```
mogwai.reset

"bonjour" -> 'A'
guard
{
    (->upper sqrt butlast) --> &A
}
else
{
    A ?     # → bonjour — untouched, because sqrt failed on a string
}
```

(`guard` / `else` ont droit à une vraie introduction dans la section sur la gestion d'erreurs — pour l'instant, remarque juste que le pipeline échoué a laissé `A` exactement comme au départ.)

### `!` — évaluer sur place

Certaines variables contiennent plus qu'une simple valeur — un bloc de code, une fonction, ou une chaîne contenant des blocs d'interpolation `{! ... }`. Par défaut, rien de tout ça n'est résolu automatiquement ; MOGWAI stocke ce que tu as écrit, pas son résultat, jusqu'à ce que tu le demandes explicitement. Le préfixe `!` fait ça en une étape, à la place d'écrire `A eval` :

```
mogwai.reset

100 -> 'A'
{ A 200 * } -> 'B'
"We are in {! now ->date year: get }" -> 'C'

!B    # → 20000
!C    # → We are in 2026
```

`!A` fonctionne uniformément sur les blocs, les fonctions, les chaînes, les listes et les records — et pour un simple scalaire (un nombre, un booléen...), c'est simplement une opération sans effet, identique à `A`. C'est ce qui rendait `!A` sûr à utiliser même avant qu'on ait introduit les conteneurs — il ne fait jamais la mauvaise chose.

Le même `!` apparaît comme **préfixe à l'intérieur d'un littéral de liste ou de record**, en raccourci pour appeler `eval` sur l'ensemble juste après sa construction :

```
mogwai.reset

100 -> 'A'

(A {! A 2 *} {! A 3 *}) eval ?     # → (100 200 300)
(! A {! A 2 *} {! A 3 *}) ?        # → (100 200 300) — same result, no separate eval
```

Une chose à savoir avant de t'appuyer là-dessus : les conteneurs sont **paresseux (lazy)** par conception — tout ce qu'il y a à l'intérieur est stocké comme une expression, pas pré-calculé, donc chaque évaluation `!A` reflète l'état du programme *au moment où tu le demandes*, pas au moment où le conteneur a été construit. C'est en général exactement ce que tu veux, et c'est aussi pourquoi MOGWAI détecte et rejette automatiquement les références circulaires (une variable dont l'évaluation dépend, directement ou par une chaîne, d'elle-même) plutôt que de boucler indéfiniment.

### `@` — une lecture plus rapide, même résultat

`@A` lit une variable de la même façon que `A` tout simple — même valeur, même comportement — juste résolue un peu plus vite, puisque MOGWAI peut déterminer où chercher à l'avance plutôt qu'au moment de l'exécution. C'est davantage une micro-optimisation qu'une nouvelle capacité : utilise-le dans les boucles chaudes ou le code sensible à la performance, et ne t'en préoccupe pas ailleurs.

---

*Suite : gérer les erreurs proprement — les intercepter, les inspecter, et en lever soi-même.*

---

## 12. Gestion d'erreurs

Par défaut, une erreur en MOGWAI arrête le programme — comme une exception non gérée dans la plupart des langages. Cette section parle d'éviter que ça arrive quand tu préfères t'en remettre.

### `trap` — protéger un bloc, silencieusement

`trap` exécute un bloc ; si quelque chose à l'intérieur lève une erreur, l'exécution de ce bloc s'arrête net et continue simplement avec ce qui vient après `trap` — aucune erreur ne se propage, rien n'est signalé. La pile est automatiquement restaurée à son état d'avant le `trap`, donc un bloc échoué ne la laisse jamais dans un état à moitié modifié :

```
mogwai.reset

trap
{
    "trap begins." ?
    10 a *                              # 'a' doesn't exist — this raises an error
    "This message will never be displayed." ?
}

"exit of the trap." ?
"the code continues..." ?
```

### `guard` / `else` — protéger, et réagir

`guard` est `trap` avec un bloc de récupération attaché — si le code protégé échoue, le bloc `else` s'exécute à la place :

```
mogwai.reset

guard
{
    "guard begins." ?
    10 a *
    "This message will never be displayed." ?
}
else
{
    "An error has occurred in the guard!" ?
}

"exit of the guard" ?
```

### Découvrir ce qui s'est mal passé

`error.last` renvoie le code de la dernière erreur, sous forme de chaîne — utile à l'intérieur d'un bloc `else` pour réagir différemment selon ce qui a réellement échoué. Il ne se réinitialise pas tout seul, donc une fois que tu as fini de le traiter, efface-le explicitement avec `error.reset` :

```
mogwai.reset

guard
{
    10 a *
}
else
{
    "The error " ?? error.last ?? " happened!" ?
    error.reset
}
```

### Lever tes propres erreurs

`error.throw` lève une erreur délibérément, en lui donnant son code sous forme de chaîne. Toutes les erreurs natives de MOGWAI suivent le schéma `MW.n` — quelques-unes de celles que tu croiseras le plus souvent :

| Code | Signification |
|------|---------|
| `MW.9` | erreur d'assertion |
| `MW.20` | trop peu d'arguments |
| `MW.21` | mauvais type d'argument |
| `MW.22` | mauvaise valeur d'argument |
| `MW.30` | division par zéro |
| `MW.40` | nom inconnu |

La liste complète — plusieurs dizaines de codes couvrant tout, des tâches aux fichiers en passant par l'OOP — se trouve dans la référence du langage ; pas besoin de la mémoriser, `error.last` est ce que tu liras réellement à l'exécution.

`error.throw` ne se limite pas non plus aux codes natifs `MW.n` — passe-lui n'importe quelle chaîne et, si elle n'est pas un code reconnu, MOGWAI la lève comme une **erreur utilisateur** portant cette chaîne :

```
mogwai.reset

"INVALID_LICENSE_KEY" error.throw
# → user error (INVALID_LICENSE_KEY)
```

Pratique pour signaler tes propres conditions d'erreur spécifiques à ton application, via le même mécanisme `guard` / `error.last` que les erreurs natives.

### Poser une précondition : `mogwai.assert`

Plutôt que d'écrire un `if` et de lever une erreur à la main, `mogwai.assert` vérifie une condition et arrête l'exécution avec `MW.9` si elle est fausse, accompagnée d'un message de ton choix. La condition peut être un booléen déjà sur la pile, ou une liste — auquel cas `mogwai.assert` l'évalue pour toi et vérifie que le résultat est un booléen unique :

```
mogwai.reset

to 'divide' with [x: .number y: .number] do
{
    (y 0 !=) "divisor must not be zero" mogwai.assert
    x y /
}

10 2 divide ?      # → 5
10 0 divide ?       # → error MW.9: "divisor must not be zero"
```

C'est la façon naturelle de valider les préconditions d'une fonction en amont, plutôt que de laisser une mauvaise entrée échouer plus loin et de façon moins évidente.

### Ce qui se passe quand un programme s'arrête

MOGWAI reconnaît deux noms de fonction spéciaux — les déclarer est entièrement **optionnel**. Si tu ne les définis pas, rien de spécial ne se passe à la sortie ; si tu le fais, MOGWAI appelle celui qui correspond automatiquement selon la façon dont le programme se termine :

- **`MOGWAI.onStop`** — s'exécute lors d'une sortie propre, que le script atteigne simplement sa fin ou appelle `mogwai.exit` explicitement.
- **`MOGWAI.onError`** — s'exécute quand le programme s'arrête **à cause d'une erreur non gérée**, y compris une levée délibérément avec `mogwai.halt` (qui se comporte exactement comme `mogwai.exit`, sauf qu'il lève `MW.2` au lieu de sortir tranquillement). Seul `error.last` est disponible comme contexte à ce moment-là.

Un seul des deux s'exécute jamais pour un arrêt donné, et seulement si tu l'as effectivement déclaré :

```
mogwai.reset

to 'MOGWAI.onStop' do { "The program has just ended." ? }
to 'MOGWAI.onError' do { "An error has occurred: " ?? error.last ? }

forever do
{
    rand 1000 * ->int -> 'r'
    r ? 250 wait

    if (r 50 <) then { exit }     # clean stop → MOGWAI.onStop runs
}
```

### Sortir plus tôt : `break` et `return`

`break` sort immédiatement de la boucle la plus imbriquée — on l'a déjà utilisé quelques fois dans la section sur le contrôle de flux. `return` en est l'équivalent pour les fonctions : il sort de la fonction courante sur-le-champ, en laissant ce qui se trouve déjà sur la pile comme résultat :

```
mogwai.reset

to 'displayValue' with [value: .number] do
{
    if (value 5 !=) then
    {
        value ?
        return
    }

    "Value 5 is not allowed!" ?
}

1 10 for 'i' do { i displayValue }
```

---

*Suite : MOGWAI orienté objet — classes, instances, et méthodes.*

---

## 13. Programmation orientée objet

Le système objet de MOGWAI est délibérément minimal — classes, instances, propriétés, méthodes, pas d'héritage, pas de ramasse-miettes. Tu crées des instances et les détruis explicitement ; rien ne se passe dans ton dos.

### Définir une classe

`class 'Name' do { ... }` déclare une classe, avec deux sections à l'intérieur : `private:` pour les membres accessibles uniquement depuis l'intérieur de la classe, et `public:` pour tout ce qui est appelable depuis l'extérieur.

À l'intérieur de l'une ou l'autre section, ce qui distingue une **propriété** d'une **méthode**, c'est simplement ce qui suit son nom : un type (`.number`, `.string`, ...) déclare une propriété ; un bloc de code `{ }` déclare une méthode.

```
mogwai.reset

class 'Counter' do
{
    private:
    {
        _step: .number
    }

    public:
    {
        value: .number

        onInit:
        {
            [step: (.number 1)] ->params
            self->reset:
            step self<-_step:
        }

        increment:
        {
            self->value: self->_step: + self<-value:
        }

        reset:
        {
            0 self<-value:
        }
    }
}
```

Une propriété est initialisée à `empty` quel que soit son type déclaré — tu peux vérifier si elle a réellement été définie avec `isEmpty`.

### `new` et `free` — le cycle de vie d'une instance

Deux noms de méthode spéciaux sont appelés automatiquement si tu les définis : `onInit:` à la création d'une instance, `onFree:` juste avant sa destruction. La création prend un record à paramètres nommés — exactement la syntaxe de record de la section sur les fonctions — suivi du nom de la classe et de `new` :

```
mogwai.reset

[id: 10 name: "SIBUE"] 'User' new -> '$U1'    # onInit: runs automatically

$U1 free                                       # onFree: runs automatically
```

Chaque instance obtient un identifiant unique, affiché sous forme de `§` suivi d'un numéro (`§453`) — jamais réutilisé pendant toute la durée de vie du moteur. Si plusieurs variables référencent la même instance et qu'elle est détruite, elles deviennent toutes invalides en même temps. Plutôt que de risquer d'utiliser une référence périmée, vérifie d'abord avec `isAlive` :

```
mogwai.reset

if ($U1 isAlive) then
{
    $U1->display:
}
```

### Accéder aux propriétés et méthodes

Les membres publics utilisent la même notation compacte `->` / `<-` que tu connais déjà pour les records — lire, écrire, et appeler une méthode ont tous la même forme :

```
mogwai.reset

$U1->name: ?                 # read a property — equivalent to: $U1 name: get ?
"DUPONT" &$U1<-name:         # write a property in place — equivalent to: "DUPONT" &$U1 name: set
$U1->display:                # call a method — equivalent to: $U1 display: get
```

Essayer d'atteindre un membre `private:` depuis l'extérieur lève une erreur — c'est tout l'intérêt des deux sections. Chaque instance a aussi une propriété `className:` en lecture seule, fournie automatiquement, indiquant de quelle classe elle a été construite.

### `self` — se référer à l'instance courante

À l'intérieur de n'importe quelle méthode, `self` est automatiquement disponible et fait référence à l'instance sur laquelle la méthode a été appelée — utilise-le pour lire ou écrire les propres propriétés de l'instance, ou appeler ses autres méthodes :

```
show:
{
    "USER={! self}" eval ?
    self->show2:            # calling another method on the same instance
}
```

### Valider ce que reçoit une méthode

Les trois mêmes niveaux de rigueur de la section sur les fonctions s'appliquent aussi aux méthodes — `->vars` (aucune vérification), `->safeVars` (vérifie le nombre et le type depuis la pile), `->params` (vérifie un record à paramètres nommés, l'ajustement naturel pour `onInit:`, puisque les instances sont toujours créées avec un) :

```
onInit:
{
    [id: .number name: .string index: (.number 0)] ->params

    id self<-id:
    name self<-name:
    index self<-index:
}
```

### Assembler le tout

```
mogwai.reset

class 'User' do
{
    private:
    {
        x: .number
        y: .number

        onInit:
        {
            [id: .number name: .string] ->params
            id self<-id:
            name self<-name:
            rand 100 * ->int self<-x:
            rand 100 * ->int self<-y:
        }

        onFree:
        {
            "FREE {! self}" eval ?
        }
    }

    public:
    {
        id: .number
        name: .string

        display:
        {
            "USER={! self} — {! self->name:} at ({! self->x:},{! self->y:})" eval ?
        }
    }
}

[id: 10 name: "SIBUE"] 'User' new -> '$U1'
$U1->display:
$U1 free
```

### Quelques outils d'introspection

`alive` liste chaque instance actuellement vivante, toutes classes confondues — pratique pour le nettoyage ou le débogage :

```
mogwai.reset

alive ?                                                          # → (§1 §2 §3 ...)
alive foreach 'item' filter { item->className: 'User' == } ?     # only the Users
```

`frame` décrit toute la structure d'une classe — propriétés, propriétés privées, méthodes, méthodes privées — sous forme de record :

```
mogwai.reset

'Counter' frame ?
# → [className: 'Counter' props: [value: .number] _props: [_step: .number] funcs: (onInit: increment: reset:) _funcs: ()]
```

---

*Suite : les tâches — exécuter des morceaux de code isolés de façon concurrente.*

---

## 14. Tâches

Une **tâche** est une unité d'exécution enfant — sa propre pile isolée, s'exécutant en parallèle du code qui l'a lancée (le **parent**). Le parent peut continuer à faire autre chose pendant qu'une tâche s'exécute ; une tâche peut elle-même lancer d'autres tâches enfants, sans autre limite que la mémoire disponible.

La règle unique qui façonne tout le reste ici : **les tâches ne se parlent jamais directement entre elles**. Une tâche enfant ne connaît que l'existence de son parent — pas celle de ses frères et sœurs — et toute communication, dans les deux sens, passe par des **événements**.

### Les événements, brièvement

Un événement est un nom plus un bloc de code à exécuter quand il est déclenché — déclaré avec `onEvent` :

```
mogwai.reset

onEvent 'MY_EVENT' do
{
    "Hello, event data was: {! eventData}" eval ?
}
```

Quelle que soit la valeur avec laquelle l'événement a été déclenché, elle est disponible à l'intérieur du bloc comme variable locale `eventData`. Les tâches utilisent exactement ce mécanisme pour rendre compte à leur parent — tu déclareras un gestionnaire `onEvent` pour chaque événement de cycle de vie de tâche qui t'intéresse.

### Les événements qu'une tâche envoie à son parent

| Événement | Contenu de `eventData` |
|-------|----------------------|
| `TASK_DID_START` | le nom de la tâche |
| `TASK_DID_END` | un record avec le nom de la tâche et son résultat (`task:` / `result:`) |
| `TASK_DID_FAIL` | un record avec le nom de la tâche, le code d'erreur, et le message |
| `TASK_DID_PUBLISH` | un record avec le nom de la tâche et ce que la tâche a choisi de publier (`task:` / `message:`) |

### Déclarer et démarrer une tâche

`task 'name' do { ... }` déclare une tâche, de la même façon que `to 'name' do { ... }` déclare une fonction. Démarre-la avec `task.start` (sans paramètre) ou `task 'name' start with object` (en lui passant un objet MOGWAI, placé sur la propre pile de la tâche juste avant qu'elle ne commence) :

```
mogwai.reset

onEvent 'TASK_DID_PUBLISH' do { "Progress: {! eventData->message:}" eval ? }
onEvent 'TASK_DID_END' do { "Task {! eventData->task:} finished — result: {! eventData->result:}" eval ? }

task 'sumTask' do
{
    ->vars                      # unpacks the record passed at launch — here, one key: limit

    0 -> 'total'

    1 limit for 'i' do
    {
        total i + -> 'total'
        if (i 25 mod 0 ==) then { "reached {! i}" eval task.publish }
    }

    total task.setResult
}

task 'sumTask' start with [limit: 100]

'sumTask' task.wait

"done" ?
```

À l'intérieur de la tâche, `task.publish` envoie une mise à jour de progression (elle arrive dans le parent comme `TASK_DID_PUBLISH`), et `task.setResult` enregistre la valeur que le parent verra une fois la tâche terminée (`TASK_DID_END`). `task.wait` bloque le parent jusqu'à ce que cette tâche précise se termine ; si tu en exécutes plusieurs en parallèle, `task.join` fait la même chose pour toute une liste de noms de tâches à la fois — `('T1' 'T2' 'T3') task.join`.

### Gérer les erreurs à l'intérieur d'une tâche

Si une tâche lève une erreur qu'elle ne rattrape pas elle-même, MOGWAI arrête cette tâche et déclenche `TASK_DID_FAIL` dans le parent — la tâche ne fait pas planter tout le programme. Malgré tout, l'habitude recommandée est d'envelopper le corps d'une tâche dans un `guard`, pour contrôler ce que signifie un "échec" et pouvoir le signaler proprement via `task.setResult` plutôt que de compter sur l'événement d'échec par défaut.

### Quelques limites pratiques

- Une tâche déjà en cours d'exécution ne peut pas être redémarrée — `task.start` / `task start with` lèvent une erreur si tu essaies. Vérifie d'abord `task.isRunning` en cas de doute.
- Une tâche terminée peut simplement être redémarrée, éventuellement avec un nouvel objet paramètre.
- La recommandation de MOGWAI lui-même est de rester autour de 50 à 100 tâches simultanées — largement suffisant pour la plupart des charges de travail réelles, mais pas illimité.

L'exemple de ce tutoriel est volontairement simple ; la référence du langage en présente un plus complet — plusieurs tâches téléchargeant des fichiers en parallèle et rendant compte de leur progression — qui montre les mêmes éléments à l'échelle réelle.

---

*Suite : un avant-goût de ce qui vient une fois à l'aise avec les bases — quelques-unes des familles de primitives natives les plus puissantes de MOGWAI.*

---

## 15. Un aperçu de ce qu'il reste à découvrir

Tout ce qui précède constitue le cœur du langage — de quoi écrire de vrais programmes. MOGWAI embarque aussi une grande bibliothèque standard (300+ primitives), et cette dernière étape est un rapide tour de trois familles qui ont tendance à surprendre par tout ce qu'elles couvrent. Les détails complets de tout ça — et de tout le reste — se trouvent dans la référence des fonctions.

### `calc` — les mathématiques infixes, revisitées

Dans la toute première section, on avait mentionné `calc` comme un pont pour ceux qui ne sont pas encore à l'aise en RPN. Ça vaut le coup d'y regarder à nouveau maintenant que tu as vu davantage du langage : il accepte une expression infixe complète sous forme de chaîne, parenthèses et priorité des opérateurs incluses, et l'évalue immédiatement en utilisant le classique algorithme de Shunting-yard sous le capot :

```
mogwai.reset

"5 * X + (7 + sin(Y))" calc ?
```

Vraiment utile chaque fois qu'une formule est plus facile à lire sous sa forme mathématique familière qu'épelée en RPN.

### `regex.*` — la reconnaissance de motifs

MOGWAI expose le moteur d'expressions régulières standard de .NET, donc tout motif que tu connais déjà depuis C#, .NET, ou la plupart des autres langages compatibles regex fonctionne sans changement. Cinq primitives couvrent les cas du quotidien :

```
mogwai.reset

"stephane@coding4phone.com" "^[\w.-]+@[\w.-]+\.\w+$" regex.isMatch ?
# → true

"2026-07-02" "(?<year>\d{4})-(?<month>\d{2})-(?<day>\d{2})" "${day}/${month}/${year}" regex.replace ?
# → 02/07/2026
```

`regex.isMatch` pour un test oui/non, `regex.match` / `regex.matches` pour extraire des données (y compris des groupes de capture nommés), `regex.replace` pour la recherche-remplacement, `regex.split` pour découper une chaîne selon un motif. Chacune accepte un délai d'expiration optionnel, de sorte qu'un motif qui s'emballe lève une erreur au lieu de figer ton programme.

### `http.*` — parler au web

Un ensemble complet de verbes HTTP — `http.get`, `http.head`, `http.post`, `http.put`, `http.patch`, `http.delete` — permet à un script MOGWAI d'appeler n'importe quelle API web. Les paramètres passent par un record, et la réponse aussi :

```
mogwai.reset

[
    uri: "https://api.github.com/orgs/dotnet/repos"
    requestHeaders: [User-Agent: "MyApp"]
] http.get -> 'result'

if (result->state:) then
{
    "OK - {! result->statusCode:}" eval ?
}
else
{
    "Failed - {! result->error:}" eval ?
}
```

`state:` te dit si l'appel a réussi, `statusCode:` et `response:` portent le résultat effectif, et `error:` explique ce qui s'est mal passé quand ce n'est pas le cas. La même forme record-en-entrée, record-en-sortie tient pour chaque verbe — une fois que tu connais `http.get`, les autres se lisent de la même façon.

---

## Et maintenant ?

Voilà pour la visite. Tu as maintenant un modèle mental fonctionnel pour chaque élément central de MOGWAI — la pile, les variables, les types, le contrôle de flux, les listes et records, les fonctions, les sigils, la gestion d'erreurs, l'OOP, et les tâches — plus une idée de l'ampleur de ce qui est disponible au-delà.

À partir d'ici :

- La **[référence du langage](https://github.com/Sydney680928/mogwai/tree/main/docs/FR/MOGWAI_FR.md)** couvre tout ce qui est dans ce tutoriel plus en profondeur, ainsi que des sujets qu'on n'a pas abordés — fichiers, dates, données binaires, timers, flags.
- La **[référence des fonctions](https://github.com/Sydney680928/mogwai/tree/main/docs/FR/MOGWAI_FUNCTIONS_FR.md)** documente les 300+ primitives natives.
- Le **[playground en ligne](https://sydney680928.github.io/MOGWAI/)** est le moyen le plus rapide d'essayer tout ce qui est dans ce tutoriel sans rien installer.
- Le **[dépôt GitHub](https://github.com/Sydney680928/mogwai)** contient les sources, des exemples, et une collection grandissante d'articles de blog pour aller plus loin sur des fonctionnalités spécifiques.

Bienvenue dans MOGWAI — profite bien de la pile.
