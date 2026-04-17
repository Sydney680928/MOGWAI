# MOGWAI — L'ESSENTIEL

## Table des matières

- [INTRODUCTION](#introduction)
- [BIEN DÉMARRER](#bien-démarrer)
- [AFFICHER DES VALEURS](#afficher-des-valeurs)
- [SAISIE À L'ÉCRAN](#saisie-à-lécran)
- [VARIABLES](#variables)
- [MUTATION DE VARIABLE EN PLACE](#mutation-de-variable-en-place)
- [CONSTANTES](#constantes)
- [TYPES](#types)
- [LA PILE](#la-pile)
- [TESTS](#tests)
- [BOUCLES](#boucles)
- [FONCTIONS MATHÉMATIQUES](#fonctions-mathématiques)
- [CHAÎNES DE CARACTÈRES](#chaînes-de-caractères)
- [FONCTIONS DE CONVERSION](#fonctions-de-conversion)
- [LISTES](#listes)
- [ENREGISTREMENTS](#enregistrements)
- [TABLEAUX D'OCTETS](#tableaux-doctets)
- [CONVERSION D'ENDIANNESS](#conversion-dendianness)
- [NOMBRES BINAIRES](#nombres-binaires)
- [GESTION DU TEMPS](#gestion-du-temps)
- [DÉCLARATION DE FONCTIONS](#déclaration-de-fonctions)
- [GESTION DES ERREURS](#gestion-des-erreurs)
- [FAIRE UNE PAUSE](#faire-une-pause)
- [SORTIR D'UNE FONCTION, D'UNE BOUCLE OU DU PROGRAMME](#sortir-dune-fonction-dune-boucle-ou-du-programme)
- [CRÉATION AUTOMATIQUE DE VARIABLES](#création-automatique-de-variables)
- [ÉVALUATION D'OBJETS](#évaluation-dobjets)
- [FLAGS](#flags)
- [GESTION DES FICHIERS](#gestion-des-fichiers)
- [TIMERS](#timers)
- [ÉVÉNEMENTS](#événements)
- [PROGRAMMATION ORIENTÉE OBJET](#programmation-orientée-objet)
- [TÂCHES](#tâches)


# INTRODUCTION

Je développe depuis très longtemps et j'ai eu l'occasion d'utiliser de nombreuses technologies et des langages très variés, mais je crois que le langage qui m'a le plus marqué est le RPL.

RPL signifie Reverse Polish Lisp, et c'est le nom d'un langage créé par HP pour ses calculatrices scientifiques et financières.
Calculatrice HP 48SX programmable en RPL.

Le RPL est très proche de FORTH. Comme FORTH, il utilise une pile pour recevoir les paramètres et stocker les résultats, et comme FORTH, il utilise la notation polonaise inverse (RPN — à ne pas confondre avec le langage RPL, bien sûr).

Ainsi en RPN on n'écrit pas `2+2` pour effectuer une addition, mais `2 2 +`, c'est un peu déroutant au début, mais avec cette notation il n'y a pas besoin de parenthèses ni de variables locales (en théorie).

Bien sûr, la saisie de programmes RPL se faisait directement sur la machine, et l'ergonomie n'était pas idéale, mais HP avait mis au point toute une batterie d'astuces et de fonctions pour rendre l'exercice supportable.

À première vue, le RPL n'a pas une syntaxe très simple, mais en creusant le sujet on réalise rapidement la puissance qu'il peut libérer.
 
## Une opportunité à saisir
En réalité, l'idée de se lancer dans le développement de **MOGWAI** est venue le jour où nous avions besoin, au travail, de pouvoir simuler un périphérique Bluetooth Low Energy.

Lors du développement d'une application mobile (c'est mon métier) qui utilise la communication Bluetooth Low Energy pour communiquer avec un appareil donné, l'appareil en question n'existe pas encore car il doit d'abord être physiquement conçu, et son logiciel interne doit ensuite être écrit, testé et validé.

Toute cette procédure prend du temps et, généralement, pour ne pas en perdre trop, on commence à développer l'application mobile bien avant que la carte électronique soit en mesure d'échanger la moindre information. Garder la partie communication BLE « pour la fin » n'est pas une bonne idée.

En effet, pour une intégration idéale et pour que le plus grand nombre possible de personnes puisse utiliser l'application les yeux fermés (communication comprise), la dimension BLE doit être intégrée dès le début.

Nous avons donc développé un outil qui permet de simuler le fonctionnement d'un appareil communicant en BLE avant même qu'il n'existe. Cela nous permet de valider les échanges à implémenter bien en avance et de réaliser très tôt toutes les petites choses qui n'avaient pas été correctement prévues.

C'est donc un outil très utile pour obtenir une application mobile robuste en matière de communication BLE. De plus, il permet à la partie électronique et embarquée de valider très tôt des choix cruciaux en matière de communication via Bluetooth Low Energy.

Avec ce type de moteur, les fonctions « profondes » du simulateur restent très génériques en effectuant toutes les opérations nécessaires sous la direction d'un code modifiable à volonté, en temps réel, sans recompilation, car c'est le code du moteur d'exécution qui se charge de toute la partie « logique » de la simulation. Il suffira de stocker pour chaque périphérique un ensemble de scripts adaptés à son mode de fonctionnement et aux tests à effectuer. La flexibilité obtenue était énorme !

Le simulateur doit pouvoir exécuter des instructions très variées. Il doit être capable de générer la structure du périphérique BLE à simuler, et aussi d'effectuer des tâches qui le feront réagir comme s'il était le vrai périphérique. Pour cela, il faut idéalement pouvoir « programmer » le simulateur. Et c'est pour cet usage fondamental que **MOGWAI** a été développé. C'est un moteur d'exécution qui peut être inclus dans une application ayant besoin d'être « motorisée ».

Le simulateur BLE était le projet idéal pour lancer le développement de **MOGWAI**.

## Une maturation lente

La première version de **MOGWAI** a été développée en .NET Standard avec le langage C#. La bibliothèque **MOGWAI** était incluse dans le simulateur développé en UWP. Comme le simulateur devait jouer le rôle d'un périphérique BLE, une machine équipée d'une puce BLE capable de supporter ce rôle était nécessaire (généralement, les puces BLE des PC de bureau ne savent supporter que le rôle Central BLE). Les Raspberry PI 3 sont équipés d'une puce BLE capable d'assumer les deux rôles. En installant Windows 10 IOT sur un Raspberry PI 3, nous avons pu faire fonctionner la première version du simulateur sans aucun problème, motorisé par la première version de **MOGWAI**. Cet outil nous a fait gagner beaucoup de temps à l'époque.

Au fur et à mesure que les besoins du simulateur BLE grandissaient, le moteur **MOGWAI** a été étendu, amélioré, et de nombreuses nouvelles fonctionnalités ont été ajoutées. Aujourd'hui **MOGWAI** sait gérer les connexions série, les requêtes HTTP, les bases de données SQLite et dispose de plus de 200 primitives.

Nous en sommes maintenant à la version 6, toujours développée en C# pour .NET. Cela lui permet d'être utilisé sous Windows, mais aussi sous Linux et Mac OSX avec des architectures X86, X64 et ARM. Par exemple, **MOGWAI** tourne nativement sur un Raspberry PI 3 sous Raspbian (Linux ARM).

## MOGWAI CLI pour utiliser le langage en mode interactif

Pour « jouer » avec **MOGWAI**, j'ai développé une application console interactive qui permet d'utiliser toutes les fonctionnalités du langage. Cette application s'appelle [**MOGWAI CLI**](https://github.com/Sydney680928/MOGWAI_CLI).

Il est tout à fait possible d'écrire des programmes **MOGWAI** avec un simple bloc-notes, mais il est quand même plus agréable de disposer d'outils de développement adaptés. [**MOGWAI Studio**](https://studio.mogwai.eu.com) est un IDE dédié à **MOGWAI**.

# BIEN DÉMARRER

Il y a un réflexe à adopter avec **MOGWAI**, qui est de placer la fonction `mogwai.reset` en première instruction de vos programmes.

Elle garantit que vous disposez d'un moteur d'exécution absolument propre : aucune variable, aucun timer, aucune tâche, rien du tout.

Par exemple, l'application **MOGWAI CLI** qui permet de « jouer » avec **MOGWAI** ne réinitialise jamais le contexte d'exécution, ce qui signifie que tout ce que vous créez au fur et à mesure que vous tapez des lignes est conservé, ce qui vous permet d'enchaîner des commandes pour effectuer des opérations pas à pas lors des tests.

Donc n'oubliez pas : pour tout remettre à zéro, utilisez la fonction `mogwai.reset`.
 
# AFFICHER DES VALEURS

Il existe principalement 2 fonctions pour afficher des valeurs à l'écran.

`console.println` affiche l'objet en position 1 sur la pile et effectue automatiquement un saut de ligne. Pour gagner en concision, il est possible d'utiliser `?` à la place.

`console.print` effectue la même opération sans saut de ligne automatique. Cette fonction peut être remplacée par `??`.

```
# We display the value 15 and the string "HELLO !" on 2 separate lines
15 ?
"HELLO !" ?

# We display the message "IT IS 2025" in 2 parts, a string and a number.
"IT IS " ??
2025 ?
```

Pour effacer l'écran, il faut utiliser la fonction `console.clear`.

# SAISIE À L'ÉCRAN

Pour saisir des données à l'écran, il existe 2 fonctions : `console.input` et `console.prompt`.

La plus simple est `console.input`, qui attend une saisie au clavier terminée par un retour chariot (touche `ENTRÉE`). L'information saisie est placée sur la pile sous forme de chaîne de caractères.

```
# We switch to input mode and store the result in the variable $X

console.input -> '$X'
```

La fonction `console.prompt` fonctionne exactement comme input, mais elle permet en plus d'afficher un message d'invite. Ce message est placé sur la pile avant d'appeler la fonction `console.prompt`.

```
# We ask for the name 
# And we store the information in the variable '$NOM"

"What is your name ? " console.prompt -> '$NOM'
```

# VARIABLES

Les variables sont définies par un nom. Si le nom commence par le symbole `$`, elle sera globale ; dans tous les autres cas, elle sera locale.

Par défaut, une variable n'a pas besoin d'être déclarée pour être utilisée. La première affectation la crée si elle n'existe pas déjà.

Par défaut, une variable n'a pas de type prédéfini : elle prend le type de la dernière valeur qui lui a été affectée. Il est toutefois possible de verrouiller le type d'une variable si une déclaration est effectuée préalablement à son utilisation. Elle prend alors le type déclaré et une erreur est levée si on tente de lui affecter une valeur d'un autre type.

Les variables typées sont déclarées avec la fonction `=>`.

```
# We assign the numeric value 50 to the local variable 'A'
50 -> 'A'

# We then assign a character string to this variable
"Hello !" -> 'A'

# We assign the numeric value 500 to the global variable '$R'
500 -> '$R'

# We declare the global variable '$Z' with the type of the default value 0 (which is a number).
# Which will allow it to store numbers
0 => '$Z'

# We can now only store numbers in '$Z'
1500 -> '$Z'

# Otherwise an error is raised
"Hello !" -> '$Z'

# If we want to declare a variable that accepts storing any type 
# Of values, we must use the type .any with the 'empty' value as default value
empty => '$X'
1500 -> '$X'
"Hello !" -> '$X'
```

Il est possible de rendre obligatoire la déclaration préalable des variables avant leur utilisation. Il suffit d'utiliser la fonction `mogwai.strict` avec `true` ou `false` pour activer ou désactiver cette exigence.

```
# We activate the mandatory declaration of variables before using them 
true mogwai.strict
```

Lorsqu'une variable n'a plus besoin d'exister, il est possible de la supprimer explicitement avec la fonction `purge`.

Une variable locale sera de toute façon automatiquement supprimée à la sortie de sa portée.

Si vous tentez de supprimer une variable qui n'existe pas, une erreur est levée.

```
# We delete the local variable A 
'A' purge
```

Pour placer la valeur d'une variable sur la pile, il suffit d'invoquer son nom sans apostrophes.

Pour accélérer l'exécution, vous pouvez utiliser le caractère `@` pour accéder explicitement à une variable.

```
# We assign 'A' and 'B' with numbers.
20 -> 'A'
30 -> 'B'

# We perform the sum of the 2 variables and store the result in the variable 'C'
A B + -> 'C'

# Or use the @ character to speed up execution
@A @B + -> 'C'
```

Pour évaluer immédiatement le contenu d'une variable, vous pouvez utiliser le sigil préfixe `!`. Ceci est utile lorsqu'une variable contient un objet qui intègre du code exécutable, comme un bloc, une fonction, une chaîne avec des blocs d'interpolation, une liste ou un enregistrement.

```
# A contains a number — !A behaves exactly like A
100 -> 'A'
!A    # → 100

# B contains a block — !B executes it immediately
{ A 10 * } -> 'B'
!B    # → 1000

# C contains a string with an interpolation block — !C resolves it
"The value of A is {! A}" -> 'C'
!C    # → "The value of A is 100"
```

Pour les types scalaires simples (nombres, booléens…), `!A` se comporte de manière identique à `A` — c'est un no-op silencieux, aucune erreur n'est levée.

Les quatre sigils préfixes disponibles pour une variable sont :

| Notation | Comportement |
|----------|-------------|
| `A`      | Lit A et place sa valeur sur la pile |
| `&A`     | Référence à A pour une mutation en place |
| `@A`     | Lecture à résolution statique (à la compilation) |
| `!A`     | Évalue directement le contenu de A |

Avec la fonction `rcl`, il est possible de placer la valeur d'une variable sur la pile en utilisant son nom.

```
# We retrieve the value of a variable via its name (with apostrophes).
100 -> 'A'
'A' rcl

# 100 is placed on the stack.
```

Pour stocker dans une variable numérique le résultat d'une opération mathématique sur elle-même (comme ajouter 1 à la valeur de la variable X), il existe 4 fonctions d'affectation supplémentaires :

`->+` Ajoute un nombre à une variable.

```
100 -> 'A'
10 ->+ 'A' 
# Now A equals 110.
```

`->-` Soustrait un nombre d'une variable.

```
100 -> 'A'
10 ->- 'A' 
# Now A equals 90.
```

`->*` Multiplie un nombre et une variable.

```
100 -> 'A'
10 ->* 'A' 
# Now A equals 1000.
```

`->/` Divise une variable par un nombre.

```
100 -> 'A'
10 ->/ 'A' 
# Now A equals 10.
```

Si la variable n'existe pas, elle est créée avec la valeur par défaut 0 ; l'opération sera alors effectuée à partir de cette valeur.

Si la variable n'est pas numérique, elle est initialisée comme si elle n'avait jamais existé auparavant.

Si la variable n'est pas de type numérique et a été déclarée (type verrouillé), une erreur est levée.

Pour gagner du temps, il existe également 2 fonctions pour incrémenter et décrémenter une variable numérique.

`++` Incrémente une variable.

```
100 -> 'A'
'A' ++
# Now A equals 101.
```

`--` Décrémente une variable.

```
100 -> 'A'
'A' --
# Now A equals 99.
```

La fonction `vars` retourne la liste de toutes les variables globales utilisées :

```
# We create 3 global variables $A, $B and $C

50 -> '$A'
100 -> '$B'
$A $B + -> '$C'

# We list the global variables used
vars

# Places the list ('$A' '$B' '$C') on the stack
```

La fonction `lvars` retourne la liste de toutes les variables locales utilisées :

```
# We create 3 local variables A, B and C

50 -> 'A'
100 -> 'B'
A B + -> 'C'

# We list the local variables used

lvars

# Places the list ('A' 'B' 'C') on the stack
```

Il est possible de vérifier l'existence d'une variable avec la fonction `exists`.

Cette fonction retourne `true` si le nom de variable passé en paramètre existe (variable locale ou globale).

```
# We create 1 local variable A

50 -> 'A'

'A' exists

# Places true on the stack
```

# MUTATION DE VARIABLE EN PLACE

Lorsque vous placez la valeur d'une variable sur la pile avec `A` ou `@A`, vous placez une **copie** de son contenu. Toute transformation que vous appliquez produit une nouvelle valeur qui doit être explicitement stockée dans la variable.

```
"bonjour" -> 'A'
A ->upper butfirst butlast -> 'A'
# A now contains "ONJOU"
```

Pour des cas simples, cela fonctionne bien, mais pour des objets complexes comme de grandes listes, pousser et reconstruire des copies à chaque opération peut devenir coûteux. **MOGWAI** fournit le préfixe `&` pour pousser la **référence directe** à une variable plutôt qu'une copie.

## Le préfixe de référence `&`

Préfixer un nom de variable avec `&` pousse le contenu réel de la variable — et non une copie — sur la pile. Toute fonction qui supporte les références modifiera alors la variable directement, sans créer de copies intermédiaires.

```
"bonjour" -> 'A'
&A ->upper
# A now contains "BONJOUR" — modified in place
```

Toutes les fonctions ne supportent pas les références. Si vous utilisez `&` avec une fonction qui ne le supporte pas, une erreur `bad argument type` est levée.

## L'opérateur de pipeline en place `-->`

Lorsque vous avez besoin d'appliquer une séquence de transformations à une variable en place, répéter `&` avant chaque étape devient verbeux :

```
"bonjour" -> 'A'
&A ->upper  &A butfirst  &A butlast
# A now contains "ONJOU"
```

L'opérateur `-->` résout cela en appliquant toute une liste de transformations à une variable en une seule expression :

```
"bonjour" -> 'A'
(->upper butfirst butlast) --> &A
# A now contains "ONJOU"
```

Chaque élément de la liste est appliqué en séquence, en utilisant la valeur courante de `A` comme entrée. La variable est mise à jour après chaque étape.

### Utiliser des quotations dans le pipeline

Les éléments de la liste peuvent être des fonctions ordinaires ou des quotations. Une quotation reçoit la valeur courante de la variable sur sa pile et peut effectuer n'importe quelle opération, du moment que le résultat final est laissé sur la pile :

```
"hello world" -> 'A'
(->upper { " !" + }) --> &A
# A now contains "HELLO WORLD !"
```

### Comportement transactionnel

L'opérateur `-->` est **transactionnel**. Avant le démarrage du pipeline, un instantané de la variable est pris. Si une étape lève une erreur, la variable est automatiquement restaurée à sa valeur d'origine et l'erreur est propagée.

```
"bonjour" -> 'A'
guard
{
    (->upper sqrt butlast) --> &A
}
else
{
    # An error was raised on ->sqrt (not applicable to a string)
    # A has been restored to its original value
    A ?  # displays "bonjour"
}
```

### Pipeline vide

Une liste vide `()` est un no-op : la variable est laissée inchangée.

```
"bonjour" -> 'A'
() --> &A
# A still contains "bonjour"
```

# TYPES

**MOGWAI** manipule des objets de différents types.

Chaque type a un nom qui commence par un point. Par exemple, le type correspondant à une chaîne de caractères s'appelle `.string`.

La fonction `->type` permet de récupérer le type de l'objet sur la pile.

```
# The type of a number is .number
1567 ->type ?

# We can test the type of a variable and make decisions accordingly
234 -> 'A'
if (A ->type .number ==) then {"A is a number" ?} else {"A is not a number" ?}
```

Les principaux types manipulés par **MOGWAI** sont les suivants :

| Nom | Type | Exemple |
|------|------|---------|
| `.number` | Nombre (réel double précision) | 154 ou -56.34 |
| `.string` | Chaîne de caractères | "Hello world" |
| `.boolean` | Valeur booléenne | true / false |
| `.list` | Liste d'objets | (5 "X1" 12.78) |
| `.code` | Bloc de code | {2 2 + ?} |
| `.function` | Fonction | «2 2 + ?» |
| `.name` | Nom symbolique | 'A' |
| `.key` | Clé utilisée dans un ENREGISTREMENT | latitude: |
| `.data` | Tableau d'octets | DATA:FF3456ED23 |
| `.binary` | Nombre binaire | BIN:110011110011 |
| `.record` | ENREGISTREMENT (dictionnaire) | [x: 50 y: 200] |
| `.null` | Valeur nulle | null -> 'A' |
| `.ref` | Référence à une variable | &A |
| `.objref` | Référence à une instance de classe | §56 |
| `.any` | Type libre (variant) | |


# LA PILE

**MOGWAI** est un langage qui utilise une pile LIFO pour fournir des paramètres aux fonctions et récupérer les résultats.
Vous pouvez placer sur la pile n'importe quel objet manipulé par **MOGWAI** (voir le chapitre TYPES).

Par exemple, lorsque vous écrivez `2 8 +` pour effectuer une addition, **MOGWAI** effectuera une série d'opérations lors de l'exécution :

1. Place 2 sur la pile (2 est en position 1).
2. Place 8 sur la pile (8 est en position 1, et 2 en position 2).
3. Exécute la fonction `+` qui prendra les 2 valeurs au sommet de la pile, les additionnera et placera le résultat sur la pile.

Au final, sur la pile, 2 et 8 ont disparu (on dit qu'ils ont été consommés par la fonction `+`), remplacés par le résultat de leur somme (la valeur 10).


## Fonctions de manipulation de la pile

La pile peut être manipulée car dans certains cas c'est très pratique. Cela permet souvent d'éviter des variables locales intermédiaires. Le code est au final plus rapide.

Par exemple, si vous voulez effectuer un calcul, afficher le résultat, puis effectuer un autre calcul à partir de ce résultat et l'afficher aussi, en théorie vous avez besoin d'une variable intermédiaire :

```
# We do a 1st calculation and display it.
# But we must keep a trace of the result for another calculation later.

# We do the 1st calculation and store the result in A.
2 7 + -> 'A' 

# We display the result of the 1st calculation.
A ?

# We do the second calculation from the result of the previous calculation which we display immediately.
A 200 * ?
```

En manipulant la pile, on peut éviter la variable intermédiaire et rendre le code plus compact et plus rapide. Pour cela, on utilisera la fonction `dup` qui duplique le 1er élément de la pile :

```
# We do the 1st calculation and duplicate the result to display it.
# Then we do the second calculation from the result of the previous calculation which we display.

2 7 + dup ?
200 * ?
```

## Fonctions de pile disponibles

| Fonction | Action                                                                 |
|:--------:|------------------------------------------------------------------------|
| `dup`    | Duplique le 1er élément de la pile.                                    |
| `swap`   | Échange le 1er et le 2e élément de la pile.                           |
| `clear`  | Vide la pile.                                                          |
| `depth`  | Place la taille de la pile au moment de la demande sur la pile.       |
| `drop`   | Supprime le 1er élément de la pile.                                   |

 
## La fonction `sign`

Il est possible de déterminer le type des éléments de la pile sans les en retirer. La fonction `sign`, qui prend en paramètre le nombre d'éléments à inspecter, retourne une liste contenant les types des éléments inspectés.

```
# We place 3 values of different types on the stack

10 "EE" (1 2)

# We inspect these 3 values

3 sign

# sign places the list (.list .string .number) on the stack
# Which correspond to the types of the elements present on the stack
# In position zero in the list the type of the last element placed on the stack
```

Si on tente d'inspecter plus d'éléments qu'il n'y en a réellement sur la pile, la fonction `sign` retourne une liste vide.

La fonction `sign` est très utile pour vérifier, sans modifier la pile, que les paramètres présents sont bien du type attendu.

# TESTS

## L'instruction `if`

`if` permet d'effectuer des tests et de prendre des décisions.

Lorsque le test est positif, un bloc de code est exécuté. Il est également possible de définir un bloc de code à exécuter lorsque le test est négatif.

```
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

Il est impératif que la clause de test (le code placé entre parenthèses) place une valeur booléenne sur la pile. Si ce n'est pas le cas, une erreur est levée.

```
# This expression will work 
if (true) then {"TRUE !" ?} else {"FALSE !" ?}

# This expression will raise an error
if ("TOTO") then {"TRUE !" ?} else {"FALSE !" ?}
```

## Opérations logiques booléennes (retournent `true` ou `false`)

| Test      | Signification             |
|-----------|---------------------------|
| `X Y ==`  | X égal à Y ?              |
| `X Y !=`  | X différent de Y ?        |
| `X Y >`   | X supérieur à Y ?         |
| `X Y <`   | X inférieur à Y ?         |
| `X Y >=`  | X supérieur ou égal à Y ? |
| `X Y <=`  | X inférieur ou égal à Y ? |
| `X not`   | Inversion logique de X    |
| `X Y or`  | X OU Y                    |
| `X Y and` | X ET Y                    |
| `X Y xor` | OU EXCLUSIF entre X et Y  |

 
## Opérations logiques binaires (retournent un nombre)


| Test      | Signification             |
|-----------|---------------------------|
| `X Y &`   | ET binaire                |
| `X Y |`   | OU binaire                |
| `X Y ^`   | OU EXCLUSIF binaire       |
| `X Y ~`   | NON binaire               |


## L'instruction `switch`

Pour éviter les `if .. else` en cascade, vous pouvez utiliser l'instruction `switch`.

Cette instruction est composée de plusieurs paires test / bloc de code.

Au 1er test rencontré qui retourne `true`, son bloc de code est exécuté et lui seul.

```
# We want to display a message according to the value of the variable 'a'

150 -> 'a'

switch 
{
    (a 100 <) then
    { 
        "100" ?
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
```

Si vous voulez absolument avoir un bloc de code qui s'exécute même si aucun autre n'est sélectionné (une sorte de bloc par défaut), il suffit de mettre un bloc en fin dont le test ne peut pas échouer (idéalement on met `true` directement dans le test).

# BOUCLES

## Boucle `repeat`

Pour exécuter un bloc de code un certain nombre de fois, il faut utiliser `repeat`.

```
# We will display the numbers from 1 to 10
# The variable 'I' serves as the loop counter

0 -> 'I'

10 repeat
{
    'I' ++
    I ?
}

# We will display the numbers from 1 to 10
# The variable 'I' serves as the loop counter
# We exit the loop when 'I' equals 5

0 -> 'I'

10 repeat
{
    'I' ++
    I ?

    if (I 5 ==) then {break}
}
```

## Boucle `during`

Pour exécuter un bloc de code pendant une certaine durée, il faut utiliser `during`.

La durée est exprimée en millisecondes (1000 = 1 seconde).

```
# We will execute the code for 10 seconds

0 -> 'I'

during 10000 do 
{
    'I' ++
    I ?
}
```

## Boucle `for`

Pour utiliser un compteur de boucle géré automatiquement, il faut utiliser `for`.

```
# We will display the numbers from 1 to 10
# The variable 'I' serves as the loop counter


1 10 for 'I' do
{
    I ?
}

# We will display the numbers from 10 to 1
# The variable 'I' serves as the loop counter


10 1 for 'I' step -1 do
{
    I ?
}

# We will display the numbers from 10 to 1
# The variable 'I' serves as the loop counter
# When we reach the value 5 we exit the loop


10 1 for 'I' step -1 do
{
    I ?

    if (I 5 ==) then {break}
}
```

## Boucle `foreach...do`

Pour itérer sur chaque élément d'une liste ou d'un tableau d'octets, il faut utiliser `foreach...do`.

Le bloc s'exécute sur la **pile principale** : il a accès à tout ce qui se trouve déjà sur la pile, et tout ce qu'il laisse sur la pile y demeure après la boucle.

```
# We display each element of the list

("L1" "L2" "L3" "L4" "L5" "L6" "L7") foreach 'item' do { item ? } 

# We display each element of the data

D:01020304 foreach 'item' do { item ? } 
```

## Boucle `foreach...transform`

Pour transformer chaque élément d'une liste, il faut utiliser `foreach...transform`.

Le bloc s'exécute sur sa **propre pile isolée**, distincte de la pile principale. Il a accès aux variables locales et globales, mais ne peut ni lire ni écrire sur la pile principale. La valeur laissée sur la pile du bloc à la fin de chaque itération devient l'élément transformé dans la liste résultante.

```
# We transform each element of the list

("L1" "L2" "L3" "L4" "L5" "L6" "L7") foreach 'item' transform { "-" item + } 
# Returns the list ("-L1" "-L2" "-L3" "-L4" "-L5" "-L6" "-L7")

(1 2 3 4 5) foreach 'item' transform { item 2 * } 
# Returns the list (2 4 6 8 10)
```

## Boucle `foreach...filter`

Pour filtrer les éléments d'une liste, il faut utiliser `foreach...filter`.

Le bloc s'exécute sur sa **propre pile isolée**, distincte de la pile principale. Il a accès aux variables locales et globales, mais ne peut ni lire ni écrire sur la pile principale. Le bloc doit laisser une valeur booléenne sur sa pile : seuls les éléments pour lesquels le bloc retourne `true` sont collectés dans une nouvelle liste, qui est poussée sur la pile principale.

```
# We keep only the even numbers

(1 2 3 4 5 6 7 8 9 10) foreach 'item' filter { item 2 mod 0 == }
# Returns the list (2 4 6 8 10)

# We keep only the elements between 5 and 8 inclusive

(1 2 3 4 5 6 7 8 9 10) foreach 'i' filter { i 5 >= i 8 <= and }
# Returns the list (5 6 7 8)
```

Le même résultat peut être obtenu avec `foreach...do` en gérant manuellement un accumulateur, mais `foreach...filter` exprime l'intention de manière plus directe et concise.

## Boucle `forever`

Pour exécuter une boucle indéfiniment, il faut utiliser `forever`.

```
# We execute the following code indefinitely

0 -> 'I'

forever do {'I' ++ ?}

# We execute the following code indefinitely
# But we exit when 'I' has the value 456

0 -> 'I'

forever do 
{
    'I' ++
    I ?

    if (I 456 ==) then {break}
}
```

## Boucle `while`

Pour exécuter un bloc de code tant qu'une condition est vraie, il faut utiliser `while`.

Avec cette notation (while en début de boucle), le test est effectué en premier :

```
# As long as I is less than 100 we display it

0 -> 'I'

while (I 100 <) do
{
    'I' ++
    I ?
}
```

## Boucle `do… while`

Pour exécuter un bloc de code tant qu'une condition est vraie, il faut utiliser `do … while`.

Avec cette notation, le code de la boucle est exécuté et le test est effectué en fin :

```
# As long as I is less than 100 we display it

0 -> 'I'

do
{
    'I' ++
    I ?
} while (I 100 <)
```
 
# FONCTIONS MATHÉMATIQUES

| Fonction | Usage                                                                                                                                                         | Exemple        |
|----------|---------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------|
| `->deg`  | Convertit un angle en radians en degrés.                                                                                                                      | `0.05 ->deg`   |
| `->rad`  | Convertit un angle en degrés en radians.                                                                                                                      | `3.14 ->rad`   |
| `+`      | Additionne 2 nombres.                                                                                                                                         | `5 7 +`        |
| `-`      | Soustrait 2 nombres.                                                                                                                                          | `5 7 -`        |
| `*`      | Multiplie 2 nombres.                                                                                                                                          | `5 7 *`        |
| `/`      | Divise 2 nombres.                                                                                                                                             | `5 7 /`        |
| `abs`    | Retourne la valeur absolue d'un nombre.                                                                                                                       | `-56 abs`      |
| `acos`   | Retourne l'arc cosinus d'un angle en radians.                                                                                                                 | `0.5 acos`     |
| `asin`   | Retourne l'arc sinus d'un angle en radians.                                                                                                                   | `0.5 asin`     |
| `atan`   | Retourne l'arc tangente d'un angle en radians.                                                                                                                | `0.5 atan`     |
| `ceil`   | Retourne la valeur du plus petit entier supérieur ou égal au nombre spécifié.                                                                                 | `56.89 ceil`   |
| `cos`    | Retourne le cosinus d'un angle en radians.                                                                                                                    | `0.5 cos`      |
| `max`    | Retourne la valeur maximale d'une liste.<br> Seuls les nombres sont autorisés.| `(1 2 3) max`  |
| `average`   | Retourne la moyenne d'une liste.<br> Seuls les nombres sont autorisés.| `(1 2 3) mean` |
| `min`    | Retourne la valeur minimale d'une liste.<br> Seuls les nombres sont autorisés.| `(1 2 3) min`  |
| `pow`    | Retourne un nombre donné élevé à la puissance spécifiée.                                                                                                      | `100 2 pow`    |
| `rand`   | Génère un nombre aléatoire entre 0 et 1.                                                                                                                      | `rand ->'A'`   |
| `>>`  | Effectue un décalage de bits sur un nombre donné.<br>Le décalage est effectué vers la droite.| `100 4 >>`  |
| `<<`  | Effectue un décalage de bits sur un nombre donné.<br>Le décalage est effectué vers la gauche.| `100 4 <<`  |
| `sin`    | Retourne le sinus d'un angle en radians.                                                                                                                      | `0.5 sin`      |
| `sqrt`   | Retourne la racine carrée d'un nombre.                                                                                                                        | `16 sqrt`      |
| `sum`    | Retourne la somme d'une liste.<br> Seuls les nombres sont pris en compte.<br> Retourne null si la liste ne contient aucun nombre.                             | `(1 2 3) sum`  |
| `tan`    | Retourne la tangente d'un angle en radians.                                                                                                                   | `0.5 tan`      |
| `PI`     | Retourne PI en degrés.                                                                                                                                        | `PI`           |
| `floor`  | Retourne la plus grande valeur entière inférieure ou égale au nombre spécifié.                                                                                | `45.8 floor`   |
| `mod`    | Retourne le reste de la division entière d'un nombre par un autre.                                                                                            | `100 3 mod`    |

 
# CHAÎNES DE CARACTÈRES

**MOGWAI** dispose de nombreuses fonctions de traitement des chaînes de caractères.

## Concaténation

La fonction `+` permet de concaténer 2 chaînes de caractères.

Cette fonction possède une certaine « intelligence » car selon le contexte elle sait s'adapter.


| Opération               | Résultat       |
|-------------------------|------------------|
| `"HELLO " "LE MONDE" +` | "HELLO LE MONDE" |
| `"HELLO" 3 +`           | "HELLO3"         |
| `3 "HELLO" +`           | "3HELLO"         |

## Extraction

Il existe plusieurs fonctions pour extraire une partie d'une chaîne de caractères.

| Opération                   | Résultat        |
|-----------------------------|-----------------|
| `"HELLO LE MONDE" 0 5 sub`  | "HELLO"         |
| `"HELLO LE MONDE" butfirst` | "ELLO LE MONDE" |
| `"HELLO LE MONDE" butlast`  | "HELLO LE MOND" |
| `"HELLO LE MONDE" first`    | "H"             |
| `"HELLO LE MONDE" last`     | "E"             |
| `"HELLO LE MONDE" 3 left`   | "HEL"           |
| `"HELLO LE MONDE" 3 right`  | "NDE"           |

## Taille

Pour récupérer la taille d'une chaîne de caractères, il faut utiliser la fonction `size`.

```
# We retrieve the size of a character string and display it

"HELLO LE MONDE" size ?
```

## Recherche d'éléments

Pour rechercher une sous-chaîne dans une chaîne de caractères, il faut utiliser la fonction `where` qui retourne une liste composée de toutes les positions correspondantes.

```
# We search for the location of all the letters "E"

"HELLO WORLD" "O" where

# The answer will be the list (4 7)
```

## Transformations

Pour transformer une chaîne de caractères, vous pouvez utiliser les fonctions suivantes :

| Opération                  | Résultat         |
|----------------------------|------------------|
| `"HELLO WORLD" ->lower`    | "hello world"    |
| `"hello world" ->upper`    | "HELLO WORLD"    |
| `("X" "Y" "Z") ";" join`   | "X;Y;Z"          |
| `"X;Y;Z" ";" split`        | ("X" "Y" "Z")    |

## Formater un nombre

Il est possible de formater un nombre avec la fonction `->format` qui prend en paramètres le nombre à formater et le format à appliquer.

Le format à appliquer est une chaîne de caractères décrivant la forme que doit prendre le nombre :

| Opération                | Résultat |
|--------------------------|----------|
| `50.678 "0.00" ->format` | "50.68"  |
| `34 "000" ->format`      | "034"    |

```
# We display the current date in dd/mm/yyyy format
# See the DATE MANAGEMENT chapter to fully understand the following code.

now ->date -> 'dt'

dt day: get "00" ->format ??
"/" ??
dt month: get "00" ->format ??
"/" ??
dt year: get "0000" ->format ?
```

## Inclure des valeurs dans une chaîne

Il est possible d'inclure directement dans une chaîne des éléments provenant de variables ou de fonctions.
Il est ainsi possible de composer une chaîne très facilement sans avoir à effectuer de fastidieuses opérations de construction élément par élément.
Pour indiquer l'emplacement d'un élément à incorporer dans une chaîne de caractères, il faut utiliser la notation de bloc de code auto-évalué.

Par exemple, pour incorporer le contenu de la variable `name`, il suffit d'écrire :

`"The name is {! name}" eval`

C'est la fonction `eval` qui se chargera de prendre la chaîne et de remplacer tous les éléments incorporés par leur vraie valeur.
Si l'évaluation d'un élément incorporé provoque une erreur (variable inexistante, code erroné), le remplacement de cet élément n'est pas effectué.

Dans notre exemple, si la variable `name` contient `"DOE John"` l'évaluation donnera :

`"The name is DOE John"`

Vous pouvez également y placer du code. Par exemple, vous pouvez afficher le nom en majuscules :

`"The name is {! name ->upper}" eval`

Ce qui donnera : `"Le nom est DOE JOHN"`

```
"DOE John" -> 'name'
50 -> 'age'

"{! name} is {! age} years old" eval ?

# This will display "DOE John is 50 years old"
```

# FONCTIONS DE CONVERSION

Pour convertir un objet en un autre (par exemple une chaîne de caractères en nombre ou vice versa), **MOGWAI** dispose de fonctions de conversion qui commencent ou se terminent par le symbole `->`.

| Opération                    | Résultat                                      |
|------------------------------|-----------------------------------------------|
| `D:4142434445 ->ascii`    | "ABCDE"                                       |
| `D:4142434445 ->ascii7`   | "ABCDE"                                       |
| `45 ->str`                   | "45"                                          |
| `"45" ->num`                 | 45                                            |
| `D:FF5612AE5678 ->base64` | "/1YSrlZ4"                                    |
| `"/1YSrlZ4" base64->`        | D:FF5612AE5678                             |
| `1968 ->bin`                 | B:11110110000                               |
| `(64 65 66) ->data`          | D:414243                                   |
| `64 65 66 3 ->data`          | D:414243                                   |
| `0.56 ->deg`                 | 32.08563652732611                             |
| `123.67432 "0.00" ->format`  | "123.67"                                      |
| `234 ->hex`                  | "EA"                                          |
| `20 ->i8`                    | D:14                                       |
| `20 ->i16`                   | D:0014                                     |
| `20 ->i32`                   | D:00000014                                 |
| `20 ->i64`                   | D:0000000000000014                         |
| `-30 ->u8`                   | D:E2                                       |
| `-30 ->u16`                  | D:FFE2                                     |
| `-30 ->u32`                  | D:FFFFFFE2                                 |
| `-30 ->u64`                  | D:FFFFFFFFFFFFFFE2                         |
| `56.9865 ->int`              | 56                                            |
| `"latitude" ->key`           | latitude:                                     |
| `"rand" ->keyword`           | rand                                          |
| `45 56 78 3 ->list`          | (45 56 78 3)                                  |
| `D:414243 ->list`         | (65 66 67)                                    |
| `"HELLO LE MONDE" ->lower`   | "hello le monde"                              |
| `"hello le monde" ->upper`   | "HELLO LE MONDE"                              |
| `D:2345E323 ->md5`        | D:0E9751A0F9AF52C737038B4F2108A907         |
| `"latitude" ->name`          | 'latitude'                                    |
| `(2 3 +) ->program`          | « 2 3 + »                                     |
| `35.3 ->rad`                 | 0.6161012259539983                            |
| `D:12ED45FE89 ->sha1`     | D:8B1FB372469A9B52DED84498FF26CEE06C07910B |
| `123 ->type`                 | .number                                       |
| `(1 2 3) ->type`             | .list                                         |
| `'latitude' ->type`          | .name                                         |
| `latitude: ->type`           | .key                                          |
| `"latitude" ->type`          | .string                                       |
| `"Hello !" ->utf8`           | D:48656C6C6F2021                           |
| `D:48656C6C6F2021 utf8->` | "Hello !"                                     |

# LISTES

Les listes **MOGWAI** ne sont pas typées : elles peuvent contenir une collection de n'importe quels objets.

Les listes sont notées avec des parenthèses. Les objets qu'elles contiennent sont simplement séparés par des espaces.

Par exemple `(1 2 7)` est une liste de nombres, `("X1" "X2" "X3")` est une liste de chaînes de caractères et `("X1" "X2" "X3" 45 67 (1 2 3) true)` est une liste d'objets très variés (une liste peut contenir des listes).

## Créer une liste

La méthode la plus simple pour créer une liste est de la saisir directement (comme ci-dessus).

Vous pouvez également placer sur la pile les éléments qui doivent la composer, indiquer combien en prendre et utiliser la fonction `->list`.

```
# We create a list from the objects that are on the stack.

10 20 30 40 50 5 ->list

# This instruction will place the list (10 20 30 40 50) on the stack
```

Vous pouvez aussi saisir la liste directement dans votre code :

```
# We create a list directly in the code

(10 20 30 40 50)

# This instruction will place the list (10 20 30 40 50) on the stack
```

## Ajouter des éléments à une liste

La fonction `+` permet d'ajouter un élément à une liste.

```
# We add 1 element to a list.

(10 20 30) 40 +

# This instruction will place the list (10 20 30 40) on the stack

# We add a list to a list.

(10 20 30) (100 200) +

# This instruction will place the list (10 20 30 (100 200)) on the stack
```
 
## Récupérer la taille (le nombre d'éléments) d'une liste

La fonction `size` retourne la taille d'une liste.

```
# We retrieve the size of a list to display it

(10 20 30 40) size ?

# Will display 4
````

## Modifier un élément d'une liste

La fonction `set` permet de modifier un élément particulier. Il faut fournir son index (de 0 à taille-1) et la nouvelle valeur :

```
# We modify the 3rd element of the list (we replace 55 with "Z")

(10 "E" 55 20 30) 2 "Z" set

# This instruction will place the list (10 "E" "Z" 20 30) on the stack
```

## Récupérer un élément d'une liste

La fonction `get` permet de récupérer un élément d'une liste. Comme pour `set`, il faut fournir son index (de 0 à taille-1) :

```
# We retrieve the 5th element of the list

(10 20 30 40 50 60 70) 5 get

# This instruction will place the value 60 on the stack
```

Si l'index spécifié n'est pas dans la plage possible (de 0 à taille-1), la fonction retourne `null` et ne lève pas d'erreur.
 
## Récupérer un élément « enfoui » dans une liste

Si une liste est composée de sous-listes et/ou de sous-enregistrements (voir plus loin la présentation des enregistrements qui sont des associations clé/valeur), il peut être intéressant de donner en une seule opération le « chemin » à suivre pour récupérer l'information :

```
# Method 1, basic, we retrieve information in multiple operations
# We will first retrieve the 2nd record, then the value of its key name:

([id: 0 name: "DOE"] [id: 1 name: "SMITH"] [id: 2 name: "BLACK"]) 1 get

# This operation places the record [id: 1 name: "SMITH"] on the stack
# Then we retrieve the value of the key name:

name: get

# Which places "SMITH" on the stack

# Method 2, we retrieve information in a single operation

([id: 0 name: "DOE"] [id: 1 name: "SMITH"] [id: 2 name: "BLACK"]) (1 name:) get

# This operation directly places "SMITH" on the stack
```

Si le chemin ne mène nulle part (mauvais chemin), la valeur retournée sera la valeur null.

```
# If the path is bad

([id: 0 name: "DOE"] [id: 1 name: "SMITH"] [id: 2 name: "BLACK"]) (5 name:) get

# This operation directly places null on the stack because element 5 of the list
# Does not exist.
```

## Extraire une partie d'une liste

La fonction `extract` permet d'extraire uniquement certains éléments d'une liste en une seule opération. Elle prend en paramètres la liste source et une liste d'index à extraire :

```
# We extract elements 1 2 4 from the list

(10 "E" 55 20 30) (1 2 4) extract

# This instruction will place the list ("E" 55 30) on the stack
```

Si vous demandez des index qui n'existent pas (hors des index de la liste source), des valeurs de type `null` seront ajoutées à leur place.
 

## Récupérer le 1er élément d'une liste

Il y a 2 façons, la 1ère est celle que nous venons de voir, en utilisant la fonction `get` avec un index égal à zéro.

La seconde façon est d'utiliser la fonction `first`, qui fait exactement la même chose. Si la liste est vide, elle retourne `null`.

```
# We retrieve the 1st element of the list in 2 ways

# With the get function

(10 20 30 40 50 60 70) 0 get

# This instruction will place the value 10 on the stack

# With the first function

(10 20 30 40 50 60 70) first

# This instruction will place the value 10 on the stack
```

## Récupérer le dernier élément d'une liste

Vous l'avez probablement deviné : la fonction `last` retourne le dernier élément d'une liste, et la valeur `null` si la liste est vide.

```
# We retrieve the last element of the list

(10 20 30 40 50 60 70) last

# This instruction will place the value 70 on the stack
```

## Supprimer un élément d'une liste

Pour supprimer un élément d'une liste, il faut utiliser la fonction `purge` avec la liste et l'index à supprimer en paramètres. Si l'index est < 0, une erreur est levée. Si l'index est >= taille, l'opération est simplement ignorée.

```
# We delete the 3rd element of the list, which is the value 40

(10 20 30 40 50 60 70) 3 purge

# This instruction will place (10 20 30 50 60 70) on the stack
```
 
## Extraire des éléments d'une liste à partir d'un index donné

Pour extraire une sous-liste, il faut utiliser la fonction `sub` avec l'index de départ et le nombre d'éléments à récupérer en paramètres. Si l'index de départ est hors de la liste, une erreur est levée.

Cette fonction retourne une liste composée des éléments sélectionnés.

Si vous demandez plus d'éléments que possible, la réponse sera composée du maximum d'éléments possible.

```
# We retrieve part of a list

(10 20 30 40 50 60 70) 2 3 sub

# This instruction will place (30 40 50) on the stack

# We retrieve part of a list by requesting too many elements

(10 20 30 40 50 60 70) 2 30 sub

# This instruction will place (30 40 50 60 70) on the stack

# We retrieve part of a list starting from an index that is too large

(10 20 30 40 50 60 70) 20 3 sub

# This instruction will raise the error "bad argument value"
```

## Récupérer toute une liste sauf le 1er élément ou le dernier élément

C'est la fonction `butfirst` qui permet de récupérer toute une liste sauf le 1er élément.

La fonction `butlast` permet de récupérer toute une liste sauf le dernier élément.

Si la liste est vide ou si elle ne comporte qu'un seul élément, ces fonctions retournent une liste vide.

```
# We retrieve a list without its 1st element

(10 20 30 40 50 60 70) butfirst

# This instruction will place (20 30 40 50 60 70) on the stack

# We retrieve a list without its last element

(10 20 30 40 50 60 70) butlast

# This instruction will place (10 20 30 40 50 60) on the stack
```

## Convertir une liste en tableau d'octets (data)

Vous pouvez créer un objet data (tableau d'octets) à partir d'une liste.

Seuls les nombres entre 0 et 255 sont autorisés.

```
# Example 1: We create a data object from a list of bytes expressed in hexadecimal

(0x10 0x20 0x30 0x40) ->data

# This instruction places the data object D:10203040 on the stack

# Example 2: We are not obliged to use hexadecimal notation

(100 200 120 10) ->data

# This instruction will place the data object D:64C8780A on the stack
```

## Trouver l'emplacement de valeurs

Pour rechercher l'emplacement de valeurs dans une liste, il faut utiliser la fonction `where`.

Cette fonction retourne tous les emplacements d'une valeur qui lui est passée en paramètre.

```
# We search for the indexes of the value "XX"

(10 20 "XX" "EA" 670 true "XX") "XX" where

# This instruction will place (2 6) on the stack
```

## Vérifier qu'une valeur est présente au moins une fois dans une liste

La fonction `contains` retourne une valeur booléenne indiquant si une valeur est présente (au moins une fois) dans une liste.

```
# We verify that the value "JEU" is present in the list

("L1" "L2" "L3" "L4" "L5" "L6" "L7") "L4" contains

# This instruction will place true on the stack
```

## Fonctions mathématiques

Certaines fonctions mathématiques utilisent des listes comme paramètres d'entrée. C'est le cas par exemple des fonctions `sum`, `average`, `min`, `max`.

Le paragraphe « Fonctions mathématiques » explique leur utilisation.

# ENREGISTREMENTS

Les enregistrements **MOGWAI** sont des objets qui permettent d'associer une valeur à une clé (similaires à un dictionnaire).

## L'objet KEY

La clé d'une association est affectée à un objet de type `.key` qui est un nom devant se terminer par le symbole `:` (deux-points).

## L'objet RECORD

Un objet de type `.record` est délimité par des crochets `[ ]` et contient une série de paires clé/valeur.
Un enregistrement peut être vide, auquel cas il s'écrit simplement `[]`.

Par exemple, un enregistrement contenant une valeur x et une valeur y aura une clé `x:` et une clé `y:` avec leurs valeurs, ce qui donnera : `[x: 100 y: 50]`.
La valeur peut être n'importe quel objet **MOGWAI**, et pourquoi pas une clé (qui est un objet **MOGWAI** donc autorisé), ou un autre enregistrement.

Une clé ne peut être présente qu'une seule fois dans un enregistrement. Si ce n'est pas le cas, seule la valeur de la dernière occurrence de la clé est prise en compte.

`[x: 10 y: 20 x: 100]` est équivalent à écrire `[x: 100 y: 20]`

## Ajouter ou modifier des clés

Pour ajouter une nouvelle clé ou modifier une clé existante, il faut utiliser la fonction `set` en spécifiant l'enregistrement à traiter, la clé à utiliser et la valeur associée.

```
# Example 1: We add the z: key with the value 300

[x: 100 y: 200] z: 300 set

# This instruction places [x: 100 y: 200 z: 300] on the stack

# Example 2: We modify the y: key by giving it the value 2000 instead of 200

[x: 100 y: 200] y: 2000 set

# This instruction places [x: 100 y: 2000] on the stack
```

## Récupérer la valeur d'une clé

Pour récupérer la valeur d'une clé, il faut utiliser la fonction `get` en indiquant l'enregistrement et la clé.

```
# We retrieve the value of the y: key

[x: 100 y: 200] y: get

# This instruction places 200 on the stack
```
 
## Récupérer une clé « enfouie » dans un enregistrement

Si un enregistrement est composé de sous-enregistrements et/ou de sous-listes, il peut être intéressant de donner en une seule opération le « chemin » à suivre pour récupérer l'information.

```
# Method 1, basic, we retrieve information in multiple operations
# We will first retrieve the value of the gps: key then the value of the latitude: key

[id: 1 name: "DOE" gps: [latitude: 45 longitude: 5]] gps:

# This operation places the record [latitude: 45 longitude: 5] on the stack
# Then we retrieve the value of the latitude: key

latitude: get

# Which places 45 on the stack

# Method 2, we retrieve the information in a single operation

[id: 1 name: "DOE" gps: [latitude: 45 longitude: 5]] (gps: latitude:) get

# This operation directly places 45 on the stack
```

## Récupérer la taille d'un enregistrement (nombre de clés)

La fonction `size` retourne le nombre de clés présentes dans un enregistrement.

```
# We retrieve the number of keys in the record

[x: 100 y: 200] size

# This instruction places 2 on the stack
```

## Récupérer la liste des clés d'un enregistrement

La fonction `keys` retourne la liste des clés d'un enregistrement.

```
# We retrieve the list of keys from a record

[x: 100 y: 200] keys

# This instruction places (x: y:) on the stack
```

## Extraire une partie d'un enregistrement

La fonction `extract` permet d'extraire uniquement certaines clés d'un enregistrement en une seule opération. Elle prend en paramètres l'enregistrement source et une liste de clés à extraire.

```
# We extract the x: y: keys from the record

[x: 100 y: 200 z: 70 u: 10] (x: y:) extract

# This instruction will place the record [x: 100 y: 200] on the stack
```

Si vous demandez une clé qui n'existe pas, une erreur est levée.

## Vérifier qu'une clé est présente dans un enregistrement

La fonction `contains` retourne une valeur booléenne indiquant si une clé est présente dans un enregistrement.

```
# We check that the x: key is present in a record

[x: 10 y: 20] y: contains

# This instruction will place true on the stack
```

## Supprimer une clé dans un enregistrement

La fonction `purge` permet de supprimer une clé. Elle prend en paramètres l'enregistrement et la clé à supprimer.

```
# We delete the x: key from the record

[x: 10 y: 20] x: purge

# This instruction will place [y: 20] on the stack
```

## Notation « compacte » pour get et set

**MOGWAI** fournit une notation compacte pour lire et écrire des valeurs dans n'importe quel conteneur — enregistrements, listes, tableaux d'octets et instances de classes — en utilisant les symboles `->` et `<-`.

Cette notation n'est acceptée qu'avec un nom de variable à gauche, pas directement avec une valeur littérale.

Le sélecteur placé à droite de `->` ou `<-` détermine à la fois l'opération et le type de conteneur :

| Sélecteur | Conteneur | Opération |
|----------|-----------|-----------| 
| `key:` | Enregistrement / Instance de classe | Lire ou écrire un champ nommé |
| `number` | Liste / Tableau d'octets | Lire ou écrire par index (base 0) |
| `$variable` | Tout | Lecture ou écriture dynamique en utilisant une clé ou un index stocké dans une variable |

### Lecture avec `->`

```
# Record: retrieve the value of y:
[x: 10 y: 20] -> '$R'
$R->y: ?
# Equivalent to: $R y: get ?
# Places 20 on the stack

# List: retrieve item at index 2
(10 20 30) -> '$L'
$L->2 ?
# Equivalent to: $L 2 get ?
# Places 30 on the stack

# Byte array: retrieve byte at index 1
D:FFAAEE -> '$D'
$D->1 ?
# Equivalent to: $D 1 get ?
# Places 0xAA on the stack

# Dynamic key stored in a variable
z: -> '$K'
[x: 10 y: 20 z: 30] -> '$R'
$R->$K ?
# Places 30 on the stack
```

### Écriture avec `<-`

La valeur à écrire doit être placée sur la pile avant l'expression `<-`. Pour les valeurs simples, c'est direct. Pour les valeurs calculées, utilisez un bloc `{! }`.

```
# Record: write a value to an existing key
[x: 10 y: 20] -> '$R'
1000 &$R<-y:
$R ?
# Places [x: 10 y: 1000] on the stack
# Equivalent to: 1000 &$R y: set

# Record: add a new key (upsert)
500 &$R<-z:
$R ?
# Places [x: 10 y: 1000 z: 500] on the stack

# List: write a value at index 1
(10 20 30) -> '$L'
2000 &$L<-1
$L ?
# Places (10 2000 30) on the stack

# Byte array: write a byte at index 1
D:FFFFFFFF -> '$D'
0xAA &$D<-1
$D ?
# Places D:FFAAFFFF on the stack

# Computed value: use a {! } block
{! rand 100 * ->int} &$R<-x:
```

> **Note :** Le sigil `&` avant le nom de la variable indique une mutation en place. Sans `&`, la copie modifiée est placée sur la pile et la variable d'origine n'est pas modifiée.

> **Changement non rétrocompatible (v8.6) :** L'ordre des paramètres de la fonction verbale `set` a été mis à jour pour être cohérent avec les conventions RPN. La valeur à écrire est maintenant le **premier** paramètre, avant le conteneur et la clé : `value container key: set`. Le code écrit pour **MOGWAI** 6 ou 7 utilisant l'ancien ordre (`container key: value set`) doit être mis à jour.

# TABLEAUX D'OCTETS

Dans le domaine industriel, il est très souvent nécessaire de manipuler des tableaux d'octets.

Les commandes sont envoyées sous forme de tableaux d'octets, les informations sont reçues sous la même forme. Il s'agit souvent de manipuler ces données de toutes sortes de façons.

**MOGWAI** ayant initialement été créé pour simuler un appareil utilisant le Bluetooth Low Energy, dispose naturellement de toute une batterie de fonctions pour manipuler les tableaux d'octets et les octets eux-mêmes aussi simplement que possible.

Un tableau d'octets est nommé DATA dans **MOGWAI** et le type est `.data`.

Il est possible de créer un DATA directement avec la notation `D:` suivie des octets qui le composent au format hexadécimal.

```
# We create a byte array composed of 4 bytes
# Which are AB 56 32 FF

D:AB5632FF

# Places the array of 4 bytes on the stack
```

Vous pouvez aussi créer un DATA vide avec la notation `D:`.

```
# We create an empty DATA and store it
# In the global variable $D

D: -> '$D'
```

Vous pouvez ajouter un octet au DATA avec la fonction `+` :

```
# We create a byte array composed of 4 bytes
# Which are 0xAB 0x56 0x32 0xFF

D:AB5632FF

# It is placed on the stack
# We now add a byte with value 0x56

0x56 +

# On the stack there is now D:AB5632FF56
```

La fonction `size` retourne la taille (le nombre d'octets) du DATA.

Vous pouvez concaténer 2 DATA avec la fonction `+` :

```
# On place 2 DATA dans 2 variables globales

D:FF56EB23 -> '$A'
D:89CD34 -> '$B'

# We concatenate the 2 DATA that we store in another global variable

$A $B + -> '$C'

# $C now contains D:FF56EB2389CD34
```

Pour récupérer un octet particulier d'un DATA, il faut utiliser la fonction `get` (le 1er octet a l'index zéro) :

```
# We create a DATA composed of 4 bytes and we
# Extract the byte placed in 3rd position

D:FF56EB23 2 get

# The value 0xEB (235 in decimal) is placed on the stack
```

Pour modifier la valeur d'un octet particulier, il faut utiliser la fonction `set` :

```
# We create a DATA composed of 4 bytes
# Then we modify the byte placed at position 1
# The value 0x56 will be replaced by 0x34

D:FF56EB23 1 0x34 set 

# D:FF34EB23 is placed on the stack
```

Pour modifier une partie d'un DATA avec un autre DATA, il faut aussi utiliser la fonction `set` :

```
# We will replace the first 2 bytes of a DATA

D:FFC0AB0146 0 D:AABB set ?

# There is now D:AABBAB0146 on the stack
````

Pour supprimer un octet particulier, il faut utiliser la fonction `purge` :

```
# We create a DATA composed of 4 bytes
# Then we delete the byte placed at position 1

D:FF56EB23 1 purge 

# D:FFEB23 is placed on the stack
```
 
Pour extraire une partie d'un DATA, il faut utiliser la fonction `sub` :

```
# We create a data composed of 6 bytes
# We extract 3 bytes starting from the 3rd byte

D:010203EB5634 2 3 sub

# D:03EB56 is placed on the stack
```

La fonction `extract` permet d'extraire uniquement certains éléments d'un data en une seule opération. Elle prend en paramètres le data source et une liste d'index à extraire :

```
# We extract elements 1 2 4 from the data

D:FF45AB23EA (1 2 4) extract

# This instruction will place the data D:45ABEA on the stack
```

Si vous demandez des index qui n'existent pas (hors des index du data source), une erreur est levée.

Il est possible de transformer un DATA en liste de nombres avec la fonction `->list` :

```
# We transform a DATA into a list

D:FF45EB12AD89 ->list

# The list (255 69 235 18 173 137) is placed on the stack
```

À partir d'une liste de nombres, vous pouvez créer un DATA avec la fonction `->data`.
Attention, seuls les nombres entre 0 et 255 seront pris en compte, les autres éléments de la liste seront ignorés :

```
# We transform a list into DATA

(50 25 45 36 0xFF) ->data

# D:32192D24FF is placed on the stack
```

Avec la fonction `->data` également, il est possible de créer un DATA directement à partir des éléments placés sur la pile.

Il suffit d'indiquer combien d'éléments utiliser. Attention, les éléments qui ne sont pas des nombres ou dont la valeur n'est pas comprise entre 0 et 255 ne sont pas autorisés :

```
# We transform the stack elements into DATA
# We must indicate how many elements to use
# Here 6

50 25 45 36 12 0xFF 6 ->data ?

# DATA:32192D240CFF is placed on the stack
```

Pour trouver toutes les occurrences d'un octet dans un DATA, il faut utiliser la fonction `where` :

```
# We will search for all occurrences of the value 0xC0 in a DATA

D:FFC005FA12C056EC 0xC0 where

# where will place the list (1 5) on the stack
# Because in this DATA, the value 0xC0 is present at position 1 and 5
```
 
Vous pouvez aussi trouver les emplacements d'un DATA dans un autre :

```
# We will search for all occurrences of 0xFFC0 in a DATA
# This is equivalent to searching for a DATA in another (here DATA:FFC0)

D:FFC005FA12C056EC DATA:FFC0 where

# where will place the list (0) on the stack
# Because in this DATA, the value 0xFFC0 is present at position 0 only
```

## Fonctions de conversion vers un DATA

Pour manipuler efficacement des tableaux d'octets, il faut pouvoir convertir des nombres dans différents formats.
Par exemple, prendre un nombre et le convertir en entier non signé sur 16 bits (2 octets), ou en entier signé sur 32 bits (4 octets) selon les besoins.

**MOGWAI** propose pour cela une série de fonctions de conversion qui prennent un nombre en paramètre et retournent le DATA correspondant après conversion.

Par exemple, après avoir converti un nombre en entier signé sur 32 bits, vous obtiendrez un DATA composé des 4 octets correspondant au résultat de la conversion demandée.

Une fois la conversion effectuée, il est assez simple d'insérer le résultat (qui est un DATA) dans un DATA avec la fonction `set`.

Fonctions de conversion de nombre retournant un DATA :

| Opération   | Usage                                                        | Résultat               |
|-------------|--------------------------------------------------------------|------------------------|
| `50 ->u8`   | Conversion en entier non signé sur 8 bits (1 octet)          | D:32                |
| `50 ->u16`  | Conversion en entier non signé sur 16 bits (2 octets)        | D:0032              |
| `50 ->u32`  | Conversion en entier non signé sur 32 bits (4 octets)        | D:00000032          |
| `50 ->u64`  | Conversion en entier non signé sur 64 bits (8 octets)        | D:0000000000000032  |
| `-50 ->i8`  | Conversion en entier signé sur 8 bits (1 octet)              | D:CE                |
| `-50 ->i16` | Conversion en entier signé sur 16 bits (2 octets)            | D:FFCE              |
| `-50 ->i32` | Conversion en entier signé sur 32 bits (4 octets)            | D:FFFFFFCE          |
| `-50 ->i64` | Conversion en entier signé sur 64 bits (8 octets)            | D:FFFFFFFFFFFFFFCE  |

Si un nombre trop grand ou trop petit est fourni en paramètre, il sera tronqué lors de la conversion sans lever d'erreur.
 
## Affichage avancé d'un DATA

Pour visualiser le contenu d'un DATA plus simplement, vous pouvez utiliser la fonction `?d` qui affichera le dump d'un DATA.

```
# We will download the main page of google.fr
# We store the response in the local variable R

[uri: "https://www.google.fr"] http.get -> 'R'

# The function returns a record composed of 2 keys
# state: of type .boolean which indicates if everything went well
# response: of type .data which contains the downloaded resource

if (R state: get) then
{
	# Everything is ok we can retrieve the response
	# Which we store in the local variable B
	
	R response: get -> 'B' 
	
	# We extract the first 1000 bytes and display them
	# In the form of a dump
	
	B 0 1000 sub ?d
}
```

Voici un exemple d'affichage de dump d'un DATA :

```
00000000  3C 21 64 6F 63 74 79 70 65 20 68 74 6D 6C 3E 3C  | <!doctype html><  |
00000010  68 74 6D 6C 20 69 74 65 6D 73 63 6F 70 65 3D 22  | html itemscope="  |
00000020  22 20 69 74 65 6D 74 79 70 65 3D 22 68 74 74 70  | " itemtype="http  |
00000030  3A 2F 2F 73 63 68 65 6D 61 2E 6F 72 67 2F 57 65  | ://schema.org/We  |
00000040  62 50 61 67 65 22 20 6C 61 6E 67 3D 22 66 72 22  | bPage" lang="fr"  |
00000050  3E 3C 68 65 61 64 3E 3C 6D 65 74 61 20 63 6F 6E  | ><head><meta con  |
00000060  74 65 6E 74 3D 22 74 65 78 74 2F 68 74 6D 6C 3B  | tent="text/html;  |
00000070  20 63 68 61 72 73 65 74 3D 55 54 46 2D 38 22 20  |  charset=UTF-8"   |
00000080  68 74 74 70 2D 65 71 75 69 76 3D 22 43 6F 6E 74  | http-equiv="Cont  |
00000090  65 6E 74 2D 54 79 70 65 22 3E 3C 6D 65 74 61 20  | ent-Type"><meta   |
000000A0  63 6F 6E 74 65 6E 74 3D 22 2F 69 6D 61 67 65 73  | content="/images  |
000000B0  2F 62 72 61 6E 64 69 6E 67 2F 67 6F 6F 67 6C 65  | /branding/google  |
000000C0  67 2F 31 78 2F 67 6F 6F 67 6C 65 67 5F 73 74 61  | g/1x/googleg_sta  |
000000D0  6E 64 61 72 64 5F 63 6F 6C 6F 72 5F 31 32 38 64  | ndard_color_128d  |
000000E0  70 2E 70 6E 67 22 20 69 74 65 6D 70 72 6F 70 3D  | p.png" itemprop=  |
000000F0  22 69 6D 61 67 65 22 3E 3C 74 69 74 6C 65 3E 47  | "image"><title>G  |
00000100  6F 6F 67 6C 65 3C 2F 74 69 74 6C 65 3E 3C 73 63  | oogle</title><sc  |
00000110  72 69 70 74 20 6E 6F 6E 63 65 3D 22 66 4C 6F 78  | ript nonce="fLox  |
00000120  59 71 79 59 4B 73 59 35 69 6E 59 78 79 4E 4F 4C  | YqyYKsY5inYxyNOL  |
00000130  6E 41 22 3E 28 66 75 6E 63 74 69 6F 6E 28 29 7B  | nA">(function(){  |
00000140  76 61 72 20 5F 67 3D 7B 6B 45 49 3A 27 47 72 39  | var _g={kEI:'Gr9  |
00000150  62 61 50 58 77 45 2D 79 59 6B 64 55 50 5F 39 79  | baPXwE-yYkdUP_9y  |
00000160  43 32 51 59 27 2C 6B 45 58 50 49 3A 27 30 2C 32  | C2QY',kEXPI:'0,2  |
00000170  30 32 37 39 32 2C 36 32 2C 32 2C 36 30 39 36 32  | 02792,62,2,60962  |
00000180  35 2C 33 38 38 2C 32 38 38 37 34 31 34 2C 31 31  | 5,388,2887414,11  |
00000190  30 31 2C 35 35 32 37 37 32 2C 34 32 35 36 30 33  | 01,552772,425603  |
000001A0  2C 32 34 37 33 31 39 2C 34 32 37 32 35 2C 35 32  | ,247319,42725,52  |
000001B0  33 30 32 38 30 2C 31 31 34 30 32 2C 33 32 37 36  | 30280,11402,3276  |
000001C0  38 39 33 33 2C 34 30 34 33 37 30 39 2C 32 35 32  | 8933,4043709,252  |
000001D0  32 38 36 38 31 2C 31 33 38 32 36 38 2C 31 34 31  | 28681,138268,141  |
000001E0  31 38 2C 31 31 39 34 30 2C 35 33 32 32 32 2C 36  | 18,11940,53222,6  |
```

## DATA et chaînes de caractères

Certaines fonctions de conversion liées aux chaînes de caractères prennent des DATA en paramètres ou retournent des DATA :

| Opération              | Usage                                                                                                     | Résultat    |
|------------------------|-----------------------------------------------------------------------------------------------------------|-------------|
| `D:414243 ->ascii`  | Retourne la chaîne de caractères ASCII (8 bits)<br> composée avec les octets du DATA passé en paramètre.  | "ABC"       |
| `D:414243 ->ascii7` | Retourne la chaîne de caractères ASCII (7 bits)<br> composée avec les octets du DATA passé en paramètre.  | "ABC"       |
| `D:414243 ->utf8`   | Retourne la chaîne de caractères UTF8<br> composée avec les octets du DATA passé en paramètre.            | "ABC"       |
| `D:414243 ->base64` | Retourne le tableau d'octets sous forme<br> de chaîne encodée en base 64.                                 | "QUJD"      |
| `"ABC" ascii->`        | Retourne le tableau d'octets correspondant à la conversion ASCII (8 bits)<br> d'une chaîne de caractères. | D:414243 |
| `"ABC" ascii7->`       | Retourne le tableau d'octets correspondant à la conversion ASCII (7 bits)<br> d'une chaîne de caractères. | D:414243 |
| `"QUJD" base64->`      | Retourne le tableau d'octets correspondant au décodage<br> d'une chaîne encodée en base 64.               | D:414243 |

## Autres fonctions disponibles

Fonctions de calcul de clé de hachage :

| Opération            | Usage                                     | Résultat                                      |
|----------------------|-------------------------------------------|-----------------------------------------------|
| `D:414243 ->md5`  | Retourne la clé de hachage MD5 d'un DATA  | D:902FBDD2B1DF0C4F70B4A5D23525E932         |
| `D:414243 ->sha1` | Retourne la clé de hachage SHA1 d'un DATA | D:3C01BDBB26F358BAB27F267924AA2C9A03FCFDB8 |

Il est possible, avec la fonction `->compress`, de compresser un DATA, et de le décompresser avec la fonction `->decompress` :

```
# We will download the main page of google.fr
# We store the response in the local variable R

[uri: "https://www.google.fr"] http.get -> 'R'

# The function returns a record composed of 2 keys
# state: of type .boolean which indicates if everything went well
# response: of type .data which contains the downloaded resource

if (R state: get) then
{
	# Everything is ok we can retrieve the response
	# Which we store in the local variable B
	# And we compress the response which we store in the local variable C
	
	R response: get -> 'B' 
	
	B ->compress -> 'C'
	
	# We can display the size difference
	
	B size ?
	C size ?
	
	# We decompress C and display its size
	
	C ->decompress size ?
}
```

# CONVERSION D'ENDIANNESS

Dans les contextes IoT et BLE, les payloads échangés avec des équipements matériels nécessitent un contrôle explicite de l'ordre des octets (endianness). **MOGWAI** fournit un ensemble complet de primitives pour convertir des nombres en `DATA` avec un ordre d'octets spécifique, et vice versa.

Deux ordres d'octets sont supportés :
- **Little Endian (LE)** : l'octet de poids faible est en premier. Utilisé par la plupart des profils BLE et les architectures x86/x64.
- **Big Endian (BE)** : l'octet de poids fort est en premier. Utilisé par certains protocoles matériels et standards réseau.

Tailles supportées : **8, 16, 24, 32, 48 et 64 bits**.

> Si la valeur est trop grande pour le nombre de bits demandé, les octets de poids fort sont silencieusement tronqués — cohérent avec le comportement du cast numérique C#.

---

## Conversion à taille fixe — Nombre vers DATA

Ces primitives prennent un nombre sur la pile et retournent le `DATA` correspondant dans l'ordre d'octets et la taille spécifiés.

### Little Endian

| Primitive | Exemple | Résultat |
|---|---|---|
| `->dataLE8` | `42 ->dataLE8` | `D:2A` |
| `->dataLE16` | `42 ->dataLE16` | `D:2A00` |
| `->dataLE24` | `42 ->dataLE24` | `D:2A0000` |
| `->dataLE32` | `42 ->dataLE32` | `D:2A000000` |
| `->dataLE48` | `42 ->dataLE48` | `D:2A0000000000` |
| `->dataLE64` | `42 ->dataLE64` | `D:2A00000000000000` |

### Big Endian

| Primitive | Exemple | Résultat |
|---|---|---|
| `->dataBE8` | `42 ->dataBE8` | `D:2A` |
| `->dataBE16` | `42 ->dataBE16` | `D:002A` |
| `->dataBE24` | `42 ->dataBE24` | `D:00002A` |
| `->dataBE32` | `42 ->dataBE32` | `D:0000002A` |
| `->dataBE48` | `42 ->dataBE48` | `D:0000000000002A` |
| `->dataBE64` | `42 ->dataBE64` | `D:000000000000002A` |

---

## Conversion à taille fixe — DATA vers Nombre

Ces primitives prennent un `DATA` sur la pile et retournent le nombre correspondant, en interprétant les octets dans l'ordre et la taille spécifiés.

La convention de nommage suit la règle directionnelle de **MOGWAI** : `->` en préfixe signifie *produire ce type*, `->` en suffixe signifie *consommer ce type*. Ainsi `dataLE32->` lit un `DATA` Little Endian 32 bits et retourne un nombre.

### Little Endian

| Primitive | Exemple | Résultat |
|---|---|---|
| `dataLE8->` | `D:2A dataLE8->` | `42` |
| `dataLE16->` | `D:2A00 dataLE16->` | `42` |
| `dataLE24->` | `D:2A0000 dataLE24->` | `42` |
| `dataLE32->` | `D:2A000000 dataLE32->` | `42` |
| `dataLE48->` | `D:2A0000000000 dataLE48->` | `42` |
| `dataLE64->` | `D:2A00000000000000 dataLE64->` | `42` |

### Big Endian

| Primitive | Exemple | Résultat |
|---|---|---|
| `dataBE8->` | `D:2A dataBE8->` | `42` |
| `dataBE16->` | `D:002A dataBE16->` | `42` |
| `dataBE24->` | `D:00002A dataBE24->` | `42` |
| `dataBE32->` | `D:0000002A dataBE32->` | `42` |
| `dataBE48->` | `D:0000000000002A dataBE48->` | `42` |
| `dataBE64->` | `D:000000000000002A dataBE64->` | `42` |

---

## Conversion à taille dynamique

Lorsque la taille n'est pas connue au moment de l'écriture du script, vous pouvez utiliser les variantes dynamiques. La taille (en bits) est prise sur la pile avec le nombre ou le `DATA`.

### Nombre vers DATA

| Primitive | Signature de pile | Exemple | Résultat |
|---|---|---|---|
| `->dataLE` | `number size →` | `42 32 ->dataLE` | `D:2A000000` |
| `->dataBE` | `number size →` | `42 32 ->dataBE` | `D:0000002A` |

### DATA vers Nombre

| Primitive | Signature de pile | Exemple | Résultat |
|---|---|---|---|
| `dataLE->` | `DATA size →` | `D:2A000000 32 dataLE->` | `42` |
| `dataBE->` | `DATA size →` | `D:0000002A 32 dataBE->` | `42` |

Si une taille autre que 8, 16, 24, 32, 48 ou 64 est fournie, une erreur `BadArgumentTypeError` est levée.

---

## Conversion flottante

Ces primitives convertissent entre `DATA` et nombres à virgule flottante selon la norme IEEE 754. Deux tailles sont supportées : **32 bits** (simple précision) et **64 bits** (double précision).

Le suffixe `F` dans le nom de la primitive indique un type flottant, par opposition aux primitives entières ci-dessus.

### Nombre vers DATA (flottant)

| Primitive | Exemple | Résultat |
|---|---|---|
| `->dataLE32F` | `1.0 ->dataLE32F` | `D:0000803F` |
| `->dataBE32F` | `1.0 ->dataBE32F` | `D:3F800000` |
| `->dataLE64F` | `1.0 ->dataLE64F` | `D:000000000000F03F` |
| `->dataBE64F` | `1.0 ->dataBE64F` | `D:3FF0000000000000` |

### DATA vers Nombre (flottant)

| Primitive | Exemple | Résultat |
|---|---|---|
| `dataLE32F->` | `D:0000803F dataLE32F->` | `1.0` |
| `dataBE32F->` | `D:3F800000 dataBE32F->` | `1.0` |
| `dataLE64F->` | `D:000000000000F03F dataLE64F->` | `1.0` |
| `dataBE64F->` | `D:3FF0000000000000 dataBE64F->` | `1.0` |

> Si le `DATA` passé à une primitive de conversion flottante est trop petit (moins de 4 octets pour 32 bits, moins de 8 octets pour 64 bits), une erreur `BadArgumentValueError` est levée.

---

## Exemples pratiques

```
# Round-trip verification (integer)
90000 ->dataBE32 dataBE32->
# → 90000 ✓

# Round-trip verification (float)
3.14 ->dataLE32F dataLE32F->
# → 3.14 ✓ (rounded to single precision)

# Building a BLE command payload
# Header (1 byte) + value (32-bit LE) + checksum (1 byte)
D:AA -> '$payload'
1234 ->dataLE32 -> '$value'
$payload $value + 0xFF + -> '$payload'
# → D:AAD2040000FF

# Reading a temperature sensor value (IEEE 754 float, Little Endian)
D:0000803F dataLE32F->
# → 1.0

# Reading a 48-bit MAC address
D:AABBCCDDEEFF dataLE48->
# → numeric value of the MAC address

# Dynamic size from a configuration variable
32 -> 'bits'
90000 bits ->dataLE
# → D:905F0100... (same as ->dataLE32)
```

# NOMBRES BINAIRES

Pour simplifier la manipulation des bits d'un nombre, il est possible d'utiliser un objet **MOGWAI** de type `.binary`.

Dans **MOGWAI**, un nombre binaire commence par `B:` suivi des bits utilisés. Par exemple le nombre binaire `11001101` en binaire s'écrit dans **MOGWAI** `B:11001101`.

Vous ne pouvez pas gérer un nombre binaire de plus de 64 bits.

La fonction `size` retourne la taille (en bits) du nombre binaire.

Il est possible d'assembler 2 nombres binaires avec la fonction `+` :

```
# We assemble 2 binary numbers
# The 1st is 1 bit, and the second 7 bits
# The total will therefore be 8 bits in the end

B:1 B:1111111 + 

# Places B:11111111 on the stack
```

Avec la fonction `->bin`, vous pouvez créer un nombre binaire à partir d'un nombre ordinaire. Le nombre de bits du nombre binaire créé sera limité à ceux nécessaires pour représenter le nombre d'origine.

Par exemple, le nombre 112 en binaire s'écrit `1110000`, donc le nombre binaire créé a une taille de 7 bits.

Vous pouvez également spécifier la taille du nombre binaire créé avec les fonctions `->bin..` comme `->bin8`, `->bin16`, `->bin32` et `->bin64`. Dans ce cas, le nombre binaire créé sera complété par des zéros à gauche pour atteindre la taille spécifiée.

La fonction `up` permet de lever un bit donné, et la fonction `down` permet l'opération inverse. Vous devez donner à ces fonctions le numéro du bit à modifier (le 1er bit a le numéro 0) :

```
# We create a 16-bit binary number having
# The value 112

112 ->bin16

# Places B:0000000001110000 on the stack

# We raise bit 15 and lower bit 5

15 up 
5 down

# Places B:1000000001010000 on the stack
````
 
Pour extraire une partie d'un nombre binaire, il faut utiliser la fonction `sub` en indiquant à partir de quel bit effectuer l'extraction et combien de bits extraire. La fonction retourne un nombre binaire composé des bits extraits :

```
# We create a 16-bit binary number having
# The value 112

112 ->bin16

# Places B:0000000001110000 on the stack

# We extract 8 bits starting from bit 3

3 8 sub

# Places B:00001110 on the stack
```
 
Il est aussi possible d'effectuer des décalages de bits avec les fonctions `>>` et `<<`. Il faut indiquer de combien de bits décaler (`<<` décale vers la gauche, `>>` vers la droite) :

```
# We shift the binary number B:00000001 by 2 bits to the left

B:00000001 2 <<

# Places B:00000100 on the stack

# We shift to the right by a single bit

1 >>

# Places B:00000010 on the stack
```

La fonction `not` permet d'appliquer un not binaire :

```
# We apply a binary not to B:11000111

B:11000111 not

# Places B:00111000 on the stack
```

La fonction `bit?` teste si un bit spécifique est à 1 dans un nombre binaire. Vous devez lui donner le numéro du bit à tester (le 1er bit a le numéro 0). Elle retourne `true` si le bit est à 1, `false` sinon :

```
# We test bit 1 of B:110011

B:110011 1 bit?

# Places true on the stack

# We test bit 2

B:110011 2 bit?

# Places false on the stack
```

Pour convertir un nombre binaire en nombre ordinaire, il faut utiliser la fonction `->num` :

```
# We retrieve the numeric value of B:10011011

B:10011011 ->num

# Places 155 on the stack
```
 
# GESTION DU TEMPS

**MOGWAI** sait manipuler les informations relatives aux dates et aux durées.

Une date est un nombre qui représente le nombre d'intervalles de 100 nanosecondes écoulées depuis minuit, le 1er janvier 0001. Par exemple, la valeur représentant la date du 05/03/2012 à 16h45 est 6.3466562759E+17.

Bien sûr sous cette forme ce n'est pas très pratique, c'est pourquoi **MOGWAI** dispose de toute une série de fonctions pour effectuer des opérations sur les dates et les durées.

## Récupérer la date courante

La fonction `now` retourne (place sur la pile) la date courante de votre machine.

## Récupérer les composantes d'une date

Pour récupérer toutes les composantes (jour, mois, année, heure, etc.) d'une date, il faut utiliser la fonction de conversion `->date` qui prend en paramètre une date (au format numérique) et retourne un enregistrement contenant toutes les composantes de cette date.

Les composantes retournées sont les suivantes (les clés de l'enregistrement retourné) :

| Clé          | Valeur                                                   |
|--------------|---------------------------------------------------------|
| `day:`       | Jour du mois.                                           |
| `month:`     | Mois.                                                   |
| `year:`      | Année.                                                  |
| `hour:`      | Heure.                                                  |
| `minute:`    | Minute.                                                 |
| `second:`    | Seconde.                                                |
| `dayOfYear:` | Numéro du jour dans l'année (ex. 244e jour).           |
| `dayOfWeek:` | Numéro du jour dans la semaine (Dimanche=0, Lundi=1, etc). |

Les composantes retournées sont toutes des nombres.

```
# We retrieve the components of the date provided by now

now ->date

# Will place on the stack for example
# [day: 23 month: 5 year: 2025 hour: 12 minute: 19 second: 51 dayOfYear: 143 dayOfWeek: 5]
```

Cette fonction retourne toutes les composantes d'une date.


## Créer une date de toutes pièces

Pour créer une date à partir de ses composantes, il suffit de fournir un enregistrement contenant les composantes de la date et d'utiliser à nouveau la fonction `date->` qui retournera cette date au format numérique.

Il n'est pas nécessaire de fournir toutes les composantes, seuls le jour, le mois et l'année sont obligatoires. Ceux qui sont omis sont considérés comme étant à zéro.

`[day: 15 month: 4 year: 2015] ->date` crée la date `15/04/2015 à 00:00:00`

`[day: 15 month: 4 year: 2015 hour : 15] ->date` crée la date `15/04/2015 à 15:00:00`

## Calculer des durées

Il est également possible de calculer des durées. Une durée étant une différence entre 2 dates, vous pouvez très facilement effectuer de tels calculs avec **MOGWAI**.

```
# We calculate the real time actually elapsed during a 2450 ms pause with de wait function

now -> 'begin'
2450 wait
now -> 'end'
end begin - ->duration ?

# Result = [days: 0 hours: 0 minutes: 0 seconds: 2 milliseconds: 461]
# That is 2 seconds and 461 milliseconds
```
 
Pour récupérer le temps écoulé entre 2 moments (2 dates), il suffit de soustraire la date d'arrivée de la date de départ et d'utiliser la fonction `->duration` pour extraire les composantes de cette durée.

La valeur retournée est un enregistrement composé de 5 clés :

| Clé              | Valeur                            |
|------------------|-----------------------------------|
| `days:`          | Nombre de jours de la durée.      |
| `hours:`         | Nombre d'heures de la durée.      |
| `minutes:`       | Nombre de minutes de la durée.    |
| `secondes:`      | Nombre de secondes de la durée.   |
| `ms:`            | Nombre de millisecondes de la durée. |

Il est également possible de récupérer ces composantes directement. Dans ce cas, vous obtenez la durée totale dans l'unité demandée.

```
# We calculate the time elapsed during a 2450 ms pause

now -> 'begin'
2450 wait
now -> 'end'
end begin - ->duration seconds: get ?

# Result = 2.4551168 seconds

# We calculate the time elapsed during a 2450 ms pause
# With a more compact writing.

now -> 'begin' 2450 sleep now wait - ->duration seconds: get ?
```
 
# DÉCLARATION DE FONCTIONS

En plus de toutes les fonctions fournies en standard par **MOGWAI**, vous pouvez créer vos propres fonctions (de type `.function`).

Il existe différentes façons de déclarer des fonctions, nous les verrons les unes après les autres. Les différences résident dans le niveau de sécurité sur les paramètres passés (vérification plus ou moins avancée des types de paramètres) et le type des valeurs retournées.

Une fonction doit être déclarée avant de pouvoir être utilisée.


## Déclarer une fonction basique

Une fonction basique prend tous ses paramètres depuis la pile. Lors de sa déclaration, rien n'est précisé concernant les paramètres attendus. Bien entendu, une fonction peut n'avoir aucun paramètre.

```
# We create a function carre that takes a number as parameter and returns its square
# We take the parameter from the stack, duplicate it then perform their multiplication
# The result remains on the stack, the function is finished.

to 'carre' do { dup * }

# To use it:

5 carre

# Places the value 25 (the square of 5) on the stack

# You can also use a more traditional notation by passing parameters as a list attached directly to the function name.

carre(5)
````
 
Une fonction peut, en plus de toutes celles fournies par **MOGWAI**, utiliser celles que vous définissez. Par exemple pour créer la fonction 'cube' qui calculera le cube d'un nombre, on utilisera la fonction 'carre' définie ci-dessus :

```
# We create a function cube that takes a number as parameter and returns its cube
# We take the parameter from the stack, duplicate it then calculate its square
# Then we multiply the 2 values to obtain the cube.
# The result remains on the stack, the function is finished.

to 'cube' do { dup carre * }

# To use it:

5 cube

# or cube(5)

# Places the value 125 (the cube of 5) on the stack
```
 
## Déclarer une fonction avec vérification des types de paramètres

Il est possible de créer une fonction avec des paramètres vérifiés au moment de l'appel. Cela évite d'avoir à effectuer toutes les vérifications dans le corps de la fonction. Ce sont des opérations qui peuvent être fastidieuses et coûteuses en temps.

Par exemple dans la fonction précédente 'carre', rien n'est vérifié : si vous passez une chaîne de caractères en paramètre au lieu d'un nombre, une erreur sera levée au moment d'effectuer la multiplication. Idéalement vous devriez vérifier que le type du paramètre est bien `.number` avant de faire quoi que ce soit.

Pour éviter cela, vous pouvez indiquer les paramètres attendus et leur type dès la déclaration de la fonction :

```
# We create a function carre with verification of the input parameter type.

to 'carre' with [x: .number] do { x dup * }

# The expected parameter will be placed in the local variable 'x' and its type will be verified.
# If the number of parameters passed is insufficient or the type of one of the parameters is
# Wrong, an error is raised.

5 carre 

# Will place 25 on the stack

"EEE" carre

# The type is incorrect!
# An error is raised with an explanatory message:
# bad argument type
# ->safeVars
# .number expected but .string found for 'x' parameter

clear carre

# If we empty the stack and call the function without any parameters
# An error is raised:
# too few arguments
# ->safeVars

You can define if needed an unlimited number of verified parameters:
# We create a function that calculates a point on a line with the formula
# y=a*x+b which is in RPN a x * b +

to 'fx' with [a: .number b: .number x: .number] do { a x * b + }

# For the values y=5x+9 with x=156 we call

5 9 156 fx

# Which will place 5*156+9 or 789 on the stack
```

Si vous avez besoin de passer un paramètre sans vérifier son type, vous devez utiliser le type `.any` à la place d'un type spécifique :

```
# We create a function that displays a particular message if the type is a number.

to 'nPrint' with [x: .any] do
«
    if (x ->type .number ==) then
    {
        "It is a number !" ?
    }
    else
    {
        "It is not a number !" ?
    }
»

# We call with a number as parameter…

234 nPrint

# Will display the message "It is a number !"

# If we call with a boolean…

true nPrint

# Will display the message "It is not a number !"
```

## Déclarer une fonction avec des paramètres nommés

Il est également possible de déclarer une fonction dont les paramètres sont explicitement nommés et les types vérifiés (ceinture et bretelles). Vous pouvez même définir des valeurs par défaut.
Pour la lisibilité du code, c'est bien plus clair et la sécurité est maximale avec cette façon de faire.

Les paramètres sont passés via un enregistrement dont les clés sont les noms des paramètres et les valeurs sont celles des paramètres.

Si on déclare notre précédente fonction 'fx' avec cette méthode, cela ressemblerait à ceci :

```
# We create a function that calculates a point on a line with the formula
# y=a*x+b which is in RPN a x * b +

to 'fx' params [a: .number b: .number x: .number] do { a x * b + }

# For the values y=5x+9 with x=156 we call

[a: 5 b: 9 x: 156] fx

# Which will place 5*156+9 or 789 on the stack
```
 
Il est possible d'appeler ce type de fonction d'une façon moins RPN (paramètres puis fonction) en incluant le nom de la fonction en 1ère position de l'enregistrement de paramètres (cette notation n'est possible qu'avec les appels de fonction, ce type de notation pour un enregistrement n'existe pas ailleurs) :

```
# We create a function that calculates a point on a line with the formula
# y=a*x+b which is in RPN a x * b +

to 'fx' params [a: .number b: .number x: .number] do { a x * b + }

# For the values y=5x+9 with x=156 we call

[fx a: 5 b: 9 x: 156]

# Which will place 5*156+9 or 789 on the stack
```

Il est également possible d'appeler ce type de fonction d'une autre façon moins RPN, en incluant le nom de la fonction juste avant l'enregistrement de paramètres, sans espace entre le nom de la fonction et l'enregistrement de paramètres :

```
# We create a function that calculates a point on a line with the formula
# y=a*x+b which is in RPN a x * b +

to 'fx' params [a: .number b: .number x: .number] do { a x * b + }

# For the values y=5x+9 with x=156 we call

fx[a: 5 b: 9 x: 156]

# Which will place 5*156+9 or 789 on the stack
```

Pour déclarer des valeurs par défaut, il suffit de stipuler le type et la valeur par défaut dans une liste. Ainsi, si le paramètre n'est pas fourni, la valeur par défaut sera utilisée :

```
# We create a function 'foo' that has an optional boolean parameter save:
# If we don't specify it, it will have true as default value

to 'foo' params [id: .number name: .string save: (.boolean true)] do 
{
    " " ?
    "id   = {! id}" eval ?
    "name = {! name}" eval ?
    "save = {! save}" eval ?
}

[foo id: 10 name: "DOE John"]
[foo id: 30 name: "SMITH Mike" save: false]

# During the 1st call, the save: parameter will have the value true (default value)
# During the 2nd call, it will have the value false (parameter provided explicitly)
```

## Vérifier le type des valeurs retournées

Vous pouvez également vérifier le type de la valeur retournée par une fonction. Pour cela, il suffit d'indiquer les types attendus avant le mot-clé `do` avec le mot-clé `returns` et la liste des types attendus lors de la déclaration de la fonction :

```
# We create a function carre with verification of the input parameter type and verification of returned value type.

to 'carre' with [x: .number] returns (.number) do { x dup * }

```

Le mot-clé `returns` peut être utilisé avec tous les types de déclaration de fonction vus ci-dessus (basique, avec paramètres vérifiés, avec paramètres nommés).

## Récupérer la liste des fonctions déclarées

La fonction `funcs` retourne la liste des fonctions déclarées sous forme de liste de noms. Il est possible par exemple, en cours de programme, de vérifier qu'une fonction existe avant de tenter de l'utiliser.

```
# We create the functions carre and cube

to 'carre' do { dup * }

to 'cube' do { dup carre * }

# We list the existing functions

funcs 

# Places the list ('carre' 'cube') on the stack
```

# GESTION DES ERREURS

En cas de problème, **MOGWAI**, comme la plupart des langages de programmation, lève une erreur et arrête le programme.
Il est possible de gérer le déclenchement d'une erreur et de s'assurer que le programme ne plante pas bêtement.

## L'instruction trap

Pour éviter d'arrêter le programme en cas d'erreur, l'instruction `trap` permet de « protéger » un bloc de code. Si une erreur survient, le code protégé s'arrête et le code continue juste après l'instruction `trap`. La pile est restaurée dans l'état où elle était avant l'exécution du code protégé, il n'y a donc pas à se préoccuper de l'état de la pile après une erreur, elle est automatiquement restaurée à l'état antérieur à l'instruction `trap`.

```
# We will generate an error by using a variable that does not exist yet.
# The code will be protected by the trap instruction.

trap 
{ 
    "trap begins." ?
	
    10 a *
	
    "This message will never be displayed." ?
}

"exit of the trap." ?
"the code continue…" ?
```

## L'instruction guard

L'instruction `guard` est un peu plus évoluée que `trap`. Elle permet d'exécuter du code si une erreur survient. La pile est également restaurée dans l'état où elle était avant l'exécution du code protégé.

```
# We will generate an error by using a variable that does not exist yet.
# The code will be protected by the guard instruction.

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
"the code continues…" ?
```
 
## Connaître la dernière erreur levée

Savoir qu'une erreur s'est produite sans tuer le code c'est bien, mais savoir quelle erreur a été levée c'est mieux pour pouvoir réagir.

La fonction `error.last` retourne le code de la dernière erreur générée. Le code est une chaîne qui donne des informations sur l'erreur.

La fonction `error.reset` permet de réinitialiser (pas d'erreur) le code de la dernière erreur. Il est donc conseillé de réinitialiser cette information une fois que vous avez fini de gérer la dernière erreur, car elle ne se réinitialise pas d'elle-même.

```
# We will generate an error by using a variable that does not exist yet.
# The code will be protected by the guard instruction.

guard 
{ 
    "guard begins." ?
	
    10 a *
	
    "This message will never be displayed." ?
}
else
{
    "The error " ?? error.last ?? " happened!" ?
    error.reset
}

"exit of the guard" ?
```

## Lever artificiellement une erreur

Il est possible de lever une erreur en utilisant la fonction `error.throw` qui prend en paramètre le code chaîne de l'erreur à lever.

Liste des principales erreurs :

| Code   | Libellé                  |
|--------|--------------------------|
| MW.0   | no error.                                         |
| MW.1   | parse error.                                      |
| MW.2   | halt encounted error.                             |
| MW.3   | empty code error.                                 |
| MW.4   | internal error.                                   |
| MW.5   | platform not supported error.                     |
| MW.6   | unabled to fire event error.                      |
| MW.7   | operation not supported error.                    |
| MW.8   | circular reference error.                         |
| MW.9   | assert error.                                     |
| MW.10  | generic error.                                    |
| MW.11  | primitive not found error.                        |
| MW.20  | too few arguments error.                          |
| MW.21  | bad argument type error.                          |
| MW.22  | bad argument value error.                         |
| MW.23  | stack size error.                                 |
| MW.24  | stack corruption error.                           |
| MW.30  | division by zero error.                           |
| MW.31  | mathematical error.                               |
| MW.32  | convert error.                                    |
| MW.40  | unknown name error.                               |
| MW.41  | name already exits error.                         |
| MW.42  | function already exists error.                    |
| MW.43  | name already used by function error.              |
| MW.44  | name already used by var error.                   |
| MW.45  | unknown key error.                                |
| MW.46  | invalid name error.                               |
| MW.47  | unabled to write value in var.                    |
| MW.48  | unabled to write value in undeclared var.         |
| MW.50  | unknown word error.                               |
| MW.60  | task creation error.                              |
| MW.61  | unabled to start task error.                      |
| MW.62  | invalid outside of a task error.                  |
| MW.70  | invalid path error.                               |
| MW.71  | path does not exists error.                       |
| MW.72  | file operation error.                             |
| MW.73  | unknown file error.                               |
| MW.80  | using error.                                      |
| MW.81  | using already exists error.                       |
| MW.90  | class definition error.                           |
| MW.91  | unknown class error.                              |
| MW.92  | instance creation error.                          |
| MW.93  | unknown instance error.                           |
| MW.94  | unknown property error.                           |
| MW.95  | reserved property error.                          |
| MW.!!! | fatal error.                                      |

# FAIRE UNE PAUSE

Il est parfois nécessaire de faire une pause dans un programme ou une fonction.

## La fonction `wait`

Avec la fonction `wait`, le programme est suspendu pendant le nombre de millisecondes passé en paramètre, et les événements et timers continuent de fonctionner.

```
# We will display the numbers from 1 to 100
# With a pause of 250 milliseconds between each

1 100 for 'i' do
{
    i ?
    250 wait
}
```
 
# SORTIR D'UNE FONCTION, D'UNE BOUCLE OU DU PROGRAMME

Le flux d'un programme peut être « interrompu » par les 5 fonctions `mogwai.exit`, `mogwai.halt`, `mogwai.assert`, `break` et `return`.
 
## La fonction `mogwai.exit`

Il est possible à tout moment d'arrêter le programme, d'en sortir.
La fonction `mogwai.exit` s'en charge.

Quand un programme se termine sans erreur (arrêt normal ou provoqué par l'instruction `mogwai.exit`), la fonction réservée `MOGWAI.onStop` est automatiquement exécutée par **MOGWAI**. Si elle est définie dans votre code, elle sera appelée automatiquement :

```
# We define the function that will be executed at the end
# Normal program

to 'MOGWAI.onStop' do 
{
    "The program has just ended." ?
}

# We perform an infinite task
# But if a value < 50 comes out we stop the program

forever do
{
    # We draw a random number and store it
    # In the local variable 'r'
    rand 1000 * ->int -> 'r'
	
    # We display it and take a short break
    r ? 250 wait
	
    # If the number is < 50 then we stop the program
    if (r 50 <) then {exit}
}

# So the code that follows will never be executed
# But the MOGWAI.onStop function will be automatically executed

"Death code!" ?
```
 
## La fonction `mogwai.halt`

La fonction `mogwai.halt` se comporte exactement comme la fonction `mogwai.exit`, mais elle lève l'erreur "MW.2", "halt encounted error" au lieu de ne rien signaler. C'est donc un arrêt sur erreur.

Lorsqu'un programme se termine sur une erreur (`mogwai.halt` lève une erreur), la fonction réservée `MOGWAI.onError` est automatiquement exécutée par **MOGWAI**. Si elle est définie dans votre code, elle sera appelée automatiquement. À l'intérieur de `MOGWAI.onError`, `error.last` retourne le code de l'erreur qui a déclenché l'arrêt — c'est la seule information d'exécution disponible à ce stade :

```
# We define the function that will be executed
# If an error is raised in the program

to 'MOGWAI.onError' do 
{
    "An error has occurred: " ?? error.last ?
}

# We perform an infinite task
# But if a value < 50 comes out we stop the program
# With halt which causes an error stop

forever do
{
    # We draw a random number and store it
    # In the local variable 'r'
    rand 1000 * ->int -> 'r'
	
    # We display it and take a short break
    r ? 250 wait
	
    # If the number is < 50 then we stop the program
    if (r 50 <) then {halt}
}

# So the code that follows will never be executed
# But the MOGWAI.onError function will be automatically executed

"Death code!" ?
```

## La fonction `mogwai.assert`

`mogwai.assert` vérifie qu'une condition est vraie. Si elle est fausse, elle lève l'erreur `MW.9` (`assert error`) et arrête l'exécution. Si `MOGWAI.onError` est défini, il sera appelé automatiquement.

`mogwai.assert` prend deux paramètres : une condition et un message.

La condition peut être :
- Une **liste** — elle est automatiquement évaluée. Après exécution, `mogwai.assert` vérifie qu'exactement une valeur a été poussée sur la pile par le code de test (erreur `MW.24` stack corruption sinon), et que cette valeur est un booléen (erreur `MW.21` bad argument type sinon).
- Un **booléen** déjà sur la pile — utilisé directement.

Tout autre type lève `MW.21` (bad argument type).

Le message est une chaîne affichée à côté de l'erreur. Il n'est pas accessible par programme — `error.last` retourne `MW.9`.

```
# Using a list — the condition is evaluated by mogwai.assert
(a 10 ==) "a must equal 10" mogwai.assert

# Using a boolean already on the stack
a 0 >  "a must be positive" mogwai.assert
a islist "a must be a list" mogwai.assert
```

`mogwai.assert` est particulièrement utile pour valider les préconditions dans les fonctions, ou pour écrire des tests directement dans les scripts :

```
to 'divide' with [x: .number y: .number] do
{
    (y 0 !=) "divisor must not be zero" mogwai.assert
    x y /
}
```

## La fonction `break`

Quand vous êtes dans une boucle (voir le chapitre BOUCLES), il est possible d'en sortir « de force » avec la fonction `break` qui peut être utilisée dans les boucles `while`, `do ... while`, `for`, `foreach`, `during`, `repeat` et `forever`.

```
# We display the numbers from 1 to 100 with a for loop
# If the current number is > 10 we exit the loop

1 100 for 'i' do
{
    i ?
	
    if (i 10 >) then {break}
}

# The code continues here

"Continuing the program…" ?
```
 
## La fonction `return`

Elle permet de sortir prématurément d'une fonction.

```
to 'displayValue' with [value: .number] do 
{
    # We display the value as is
    # Unless it's the value 5 which we replace with a message
    # We could have done it with an else but it's just for the example
	
    if (value 5 !=) then
    {
        value ?
        return
    }

    "Valeur 5 interdite !" ?
}

1 10 for 'i' do
{
    i displayValue
}
```
 
# CRÉATION AUTOMATIQUE DE VARIABLES

## La fonction `->vars`

La fonction `->vars` évite de nombreuses opérations quand on veut créer des variables locales depuis une source telle qu'un enregistrement, ou depuis la pile.

### `->vars` depuis un enregistrement

Si vous avez un enregistrement et que vous devez récupérer les valeurs qu'il contient pour les manipuler, la solution de base consiste à récupérer les valeurs pour les affecter manuellement à des variables locales avant de les traiter.

```
# We need to process the values carried by the x: and y: keys
# Of a record that is stored in the local variable 'r'

[x: 50 y: 30] -> 'r'

# Basic method, we manually retrieve the values
# To store them in local variables with the same name as the keys

r x: get -> 'x'
r y: get -> 'y'

# Now, we can process the values
# Through the local variables
```

Vous pouvez simplifier le code en utilisant la fonction `->vars`

```
# We need to process the values carried by the x: and y: keys
# Of a record that is stored in the local variable 'r'

[x: 50 y: 30] -> 'r'

# Faster and automatic method
# To store them in local variables with the same name as the keys

r ->vars

# Now, we can process the values with the local variables x and y
# Which were automatically created by ->vars and which carry the values
# that the corresponding keys have in the RECORD.
```

### `->vars` depuis la pile

Il est possible d'extraire automatiquement des éléments de la pile et de les stocker dans des variables locales avec `->vars`.

Il suffit de spécifier la liste des variables à créer en paramètre. Le nombre d'éléments correspondant au nombre de variables dans la liste sera pris sur la pile et stocké dans les variables locales correspondantes. S'il n'y a pas assez d'éléments sur la pile pour remplir toutes les variables listées, la fonction lève une erreur sans modifier la pile.

```
# We place 5 elements on the stack for the test

56 
"HELLO" 
12.34 
(1 2 3) 
true

# We store the stack elements in the local variables a b and c

('a' 'b' 'c') ->vars

# The variables a b and c have been created with the values
# a=12.34, b=(1 2 3) and c=true
# The elements "HELLO" and 56 have not been taken
```

## La fonction `->safeVars`

Avec la fonction `->safeVars`, il est possible de vérifier que les valeurs présentes sur la pile sont bien celles attendues. Vous pouvez vérifier leur nombre et leur type, et affecter automatiquement des variables locales avec les valeurs de la pile. En cas de non-conformité, une erreur est levée.

```
# We place 5 elements on the stack for the test

56 
"HELLO" 
12.34 
(1 2 3) 
true

# We store the stack elements in the local variables a b and c
# We determine which types are expected

[a: .number b: .list c: .boolean] ->safeVars

# The variables a b and c have been created with the values
# a=12.34, b=(1 2 3) and c=true
# The elements "HELLO" and 56 have not been taken
# By the way, ->safeVars verified that the value taken from the stack for the variable
# 'a' is of type .number, for 'b' of type .list and for 'c' of type .boolean.
```
 
Cette fonction est utilisée automatiquement quand vous déclarez une fonction avec le mot-clé `with` :

```
to 'carre' with [x: .number] do « x x * »

5 carre 

# Will place 25 on the stack
```

## La fonction `->params`

La fonction `->params` permet de passer des paramètres nommés (paires clé/valeur dans un enregistrement) et de vérifier que les paramètres attendus sont bien présents et que leur type correspond. Si tout est correct, les variables locales correspondant aux paramètres attendus sont automatiquement créées avec les valeurs correspondantes.

Cette fonction prend 2 enregistrements en paramètres. Le 1er contient les valeurs à récupérer, le second décrit les paramètres attendus et leur type.

Par exemple, pour récupérer 2 paramètres, nommés nom et age, nom étant une chaîne de caractères et age un nombre, on aura comme enregistrement de définition des paramètres :

`[nom: .string age: .number]`

Donc pour passer "STEPHANE" pour le nom et 55 pour l'âge on aura :

`[nom: "STEPHANE" age: 55] [nom: .string age: .number] ->params`

Comme tout correspond, **MOGWAI** créera les variables locales `'nom'` avec la valeur "STEPHANE" et `'age'` avec la valeur 55.

```
# We pass as parameter a name of type character string
# And an age which is a number

[nom: "STEPHANE" age: 55] [nom: .string age: .number] ->params

nom ?
age ?
```

Si vous passez des valeurs avec un type incorrect, une erreur est levée :

```
[nom: "STEPHANE" age: "TOO OLD"] [nom: .string age: .number] ->params

# age does not have the right type
# An error is raised
```

Si vous passez plus de paramètres qu'attendu, ils seront simplement ignorés. En revanche, si vous ne passez pas tous les paramètres attendus, une erreur est levée.
 
Pour passer un paramètre de n'importe quel type, il faut utiliser le type .any

```
[nom: "STEPHANE" age: 55 libre: true] [nom: .string age: .number unrestricted: .any] ->params

nom ?
age ?
unrestricted ?

# Will display:
# STEPHANE
# 55
# true
```

Cette fonction est utilisée automatiquement quand vous déclarez une fonction avec le mot-clé `params` :

```
to 'fx' params [a: .number b: .number x: .number] do « a x * b + »

[a: 5 b: 9 x: 156] fx

# Which will place 5*156+9 or 789 on the stack
```

# Vérifier la conformité de la pile en fin de fonction

## La fonction `check`

Si vous voulez vous assurer qu'une fonction laisse la pile dans un certain état, vous pouvez utiliser la fonction `check` en fin de corps de fonction ou ailleurs selon vos besoins. Elle prend en paramètre une liste décrivant l'état attendu de la pile (type de chaque élément à vérifier). Si la pile ne correspond pas à l'état attendu, une erreur est levée.

```
# We place a number and a character string on the stack

56 "HELLO"
    
# We check that the stack is composed of a character string on top of a number
# If not, an error is raised

(.string .number) check

# The first element is the last placed on the stack, the second element is the one before, etc.
```
 
# ÉVALUATION D'OBJETS

**MOGWAI** permet de placer des références directes à des variables, des fonctions et même du code exécutable dans certains objets.

Les objets qui peuvent supporter cette possibilité sont les enregistrements, les listes et les chaînes de caractères.

Lorsque vous utilisez des références directes, elles ne seront pas automatiquement remplacées par leur valeur au moment où vous les utilisez.

## Évaluer une liste

Si vous avez une variable `A` avec la valeur 100, et que vous placez la liste `(4 5 A 50)` sur la pile, vous aurez `(4 5 A 50)` sur la pile et non `(4 5 100 50)`.

Pour que la liste utilise la vraie valeur de `A`, vous devez l'évaluer à l'aide de la fonction `eval`.

Donc si vous placez `(4 5 A 50)` sur la pile et utilisez `eval` juste après, vous aurez finalement `(4 5 100 50)` sur la pile.

## Évaluer un enregistrement

La même chose est possible avec un enregistrement :

`[x: 10 y: 50 z: A] eval` donnera `[x: 10 y: 50 z: 100]`

## Évaluer une chaîne de caractères

Pour les chaînes de caractères, il faut utiliser la notation de bloc de code dans laquelle vous affichez simplement le nom de la variable à remplacer.

Si vous avez besoin d'inclure la valeur de `A` dans une chaîne de caractères, vous pouvez par exemple écrire :

`"The value of A is {! A}" eval` ce qui donnera `"The value of A is 100"`

Le symbole `!` doit être collé à l'accolade ouvrante du bloc de code, sinon la séquence ne sera pas reconnue.

## Utiliser du code directement dans des objets

Il est possible d'utiliser du code dans les objets vus précédemment :

```
# We will display the multiplication table of 7

0 9 for 'i' do
{
  "7 x {! i} = {! i 7 *}" eval ?
}

# Which will display
# 7 x 0 = 0
# 7 x 1 = 7
# 7 x 2 = 14
# ...
# 7 x 6 = 42
# 7 x 7 = 49
# 7 x 8 = 56
# 7 x 9 = 63
```

Vous pouvez faire de même avec une liste, avec `A` ayant la valeur 100 :

`(A {! A 2 *} {! A 3 *}) eval` donnera `(100 200 300)`

Ou avec un enregistrement :

`[x: A y: {! A 2 *} z: {! A 3 *}] eval` donnera `[x: 100 y: 200 z: 300]`

## Notation plus rapide pour l'évaluation

La fonction `eval` peut être remplacée dans les listes et les enregistrements par le symbole `!` en première position.

Si on reprend nos exemples précédents :

`(! A {! A 2 *} {! A 3 *})` donnera `(100 200 300)`

`[! x: A y: {! A 2 *} z: {! A 3 *}]` donnera `[x: 100 y: 200 z: 300]`

Il n'est plus nécessaire d'appeler la fonction `eval`, l'évaluation est effectuée directement avant que la valeur soit placée sur la pile.

## Évaluer une variable avec `!`

Lorsqu'une variable contient un objet qui intègre du code exécutable, utiliser `!` comme sigil préfixe l'évalue directement — sans la pousser d'abord sur la pile. C'est plus efficace que la séquence équivalente `A eval` et exprime l'intention plus clairement au point d'appel.

```
100 -> 'A'
{ A 200 * } -> 'B'
"We are in {! now ->date year: get }" -> 'C'

!B    # → 20000
!C    # → "We are in 2026"
```

`!A` est universel : il fonctionne sur les blocs, fonctions, chaînes, listes et enregistrements. Pour les types scalaires simples (nombres, booléens…), c'est un no-op silencieux.

## Les conteneurs sont paresseux

Tout ce qui se trouve dans un conteneur — bloc, fonction, chaîne, liste ou enregistrement — est différé jusqu'à ce que l'évaluation soit déclenchée. Le conteneur stocke des expressions, pas des valeurs. Cela signifie que `!A` sur un objet composite évalue toujours avec l'**état actuel** du programme au moment de l'appel.

```
10 -> 'A'
{ A 200 * } -> 'B'
[ x: { A 10 * }
  y: "We are in {! now ->date year: get }"
  z: !B ] -> 'R'

!R    # → [ x: 100   y: "We are in 2026"   z: 2000 ]

20 -> 'A'
!R    # → [ x: 200   y: "We are in 2026"   z: 4000 ]
```

L'enregistrement `R` se comporte comme un **modèle vivant** : il capture l'intention, pas l'état. Chaque `!R` est une évaluation fraîche.

## Détection des références circulaires

Comme les conteneurs sont paresseux, il est possible d'écrire du code où l'évaluation d'une variable déclenche l'évaluation d'elle-même, directement ou via une chaîne de variables. **MOGWAI** détecte ces situations automatiquement et lève une erreur plutôt que de boucler indéfiniment.

```
{ !B } -> 'A'
{ !A } -> 'B'
!A    # → error: circular reference detected (A → B → A)
```

Le message d'erreur contient la chaîne complète des noms de variables impliquées dans le cycle, ce qui facilite l'identification du problème.
 
# FLAGS

Les flags servent à indiquer un état. Un flag a un nom et un état qui peut être soit activé soit désactivé.

## Activer un flag

C'est la fonction `flag.set` qui active un flag. Elle prend en paramètre le nom du flag à activer : `'MY_FLAG' flag.set`

## Désactiver un flag

C'est la fonction `flag.clear` qui désactive un flag. Elle prend également en paramètre le nom du flag à désactiver : `'MY_FLAG' flag.clear`

## Vérifier qu'un flag est activé

Pour vérifier si un flag est activé, il faut utiliser la fonction `flag.isSet` qui retourne `true` si le flag est activé, et `false` sinon.

Elle prend en paramètre le nom du flag à vérifier : `if ('MY_FLAG' flag.isSet) then { ... }`

## Vérifier qu'un flag est désactivé

Pour vérifier si un flag est désactivé, il faut utiliser la fonction `flag.isClear` qui retourne `true` si le flag est désactivé, et `false` sinon.

Elle prend en paramètre le nom du flag à vérifier : `if ('MY_FLAG' flag.isClear) then { ... }`

## Lister les flags activés

La fonction `flags` retourne la liste de tous les flags activés. Les flags désactivés sont considérés comme inexistants et n'apparaissent donc pas dans cette liste.

# GESTION DES FICHIERS

La version 8 de **MOGWAI** introduit un système de gestion de fichiers entièrement repensé. Contrairement aux versions précédentes qui utilisaient une approche par nœuds inspirée du RPL des calculatrices HP, la V8 adopte un système conventionnel basé sur les chemins, plus facile à utiliser et mieux aligné avec les systèmes d'exploitation modernes.

Le runtime **MOGWAI** peut fonctionner de deux façons : soit il utilise la structure de dossiers prédéfinie, soit il s'appuie sur son application hôte ou le code du script pour fournir les chemins de fichiers.

## Chemins par défaut

Par défaut, **MOGWAI** utilise une structure de dossiers spécifique dont la racine se trouve dans le dossier `documents` de l'utilisateur courant.

Ainsi sous Windows, dans le dossier `documents` de l'utilisateur courant, vous trouverez la structure suivante :
```
MOGWAI.8/
  ├─ Programs/
  ├─ Usings/
  └─ Files/
```

Le dossier `Programs` contient les programmes, le dossier `Usings` contient les bibliothèques d'extension (appelées « usings » dans la terminologie MOGWAI, comme MOGWAI_SERIAL par exemple), et le dossier `Files` contient les fichiers de données utilisés et créés par les programmes.

Les fonctions suivantes retournent directement les chemins vers ces dossiers :

| Fonction        | Usage                                                       |
|-----------------|-------------------------------------------------------------|
| `path.programs` | Retourne le dossier standard des programmes.                |
| `path.files`    | Retourne le dossier standard des fichiers.                  |
| `path.usings`   | Retourne le dossier standard des bibliothèques d'extension. |

Certaines fonctions de gestion de fichiers utiliseront ces chemins par défaut si aucun chemin n'est spécifié.

Il est possible de personnaliser ces chemins par défaut à l'aide des fonctions `path.setPrograms`, `path.setFiles` et `path.setUsings`. Par exemple, si vous voulez que vos programmes soient stockés dans un dossier différent, vous pouvez utiliser `path.setPrograms` pour définir le nouveau chemin :

```mogwai
"C:\MyPrograms" path.setPrograms
```

Si l'application hôte **MOGWAI** fournit des chemins spécifiques, ces chemins seront utilisés à la place des chemins par défaut. Ces dossiers alternatifs ne seront pas automatiquement créés par **MOGWAI** ; il incombe à l'application hôte ou au code du script de s'assurer que ces dossiers existent et sont accessibles.

## Chemins des dossiers système

Certains dossiers importants du système d'exploitation sont accessibles via des fonctions spécifiques. Par exemple, la fonction `path.desktop` retourne le chemin vers le bureau de l'utilisateur, tandis que `path.documents` retourne le chemin vers le dossier documents de l'utilisateur.

| Fonction             | Usage                                                                   |
|----------------------|-------------------------------------------------------------------------|
| `path.desktop`       | Retourne le dossier bureau de l'utilisateur courant.                    |
| `path.documents`     | Retourne le dossier documents de l'utilisateur courant.                 |
| `path.music`         | Retourne le dossier où sont stockés les fichiers musicaux de l'utilisateur courant. |
| `path.videos`        | Retourne le dossier où sont stockées les vidéos de l'utilisateur courant. |
| `path.pictures`      | Retourne le dossier où sont stockées les images de l'utilisateur courant. |
| `path.programData`   | Retourne le dossier système 'ProgramData'.                              |
| `path.tempDirectory` | Retourne le dossier des fichiers temporaires.                           |
| `path.tempFilename`  | Retourne un chemin complet vers un nouveau fichier temporaire créé par le système. |

## Construction de chemins

Pour générer un chemin de fichier ou de dossier, vous pouvez utiliser la fonction `path.make`. Cette fonction prend une liste de segments de chemin en argument et les combine pour créer un chemin complet.

Par exemple, pour créer un chemin vers un fichier nommé `data.txt` dans le dossier `Files` de la structure par défaut, vous pouvez utiliser la fonction `path.make` comme suit :

```mogwai
# Version with auto-evaluation of the segment list via the ! character at the start of the list
(! path.files "data.txt") path.make

# Result on Windows: "C:\Users\Username\Documents\MOGWAI.8\Files\data.txt"

# Version with manual evaluation of the segment list
(path.files "data.txt") eval path.make
```

## Gestion des dossiers

**MOGWAI** fournit des fonctions pour manipuler les dossiers du système de fichiers :

| Fonction | Exemple | Usage |
|----------|---------|-------|
| `dir.exists` | `"C:\Temp" dir.exists` | Retourne `true` si le dossier existe. |
| `dir.create` | `"C:\Temp\MonDossier" dir.create` | Crée un nouveau dossier. |
| `dir.purge` | `"C:\Temp\MonDossier" dir.purge` | Supprime un dossier et tout son contenu. |
| `dir.rename` | `"AncienNom" "NouveauNom" dir.rename` | Renomme un dossier. |
| `dir.current` | `dir.current` | Retourne le dossier de travail courant. |
| `dir.setCurrent` | `"C:\Projets" dir.setCurrent` | Définit le dossier de travail courant. |
| `dir.directories` | `"C:\Temp" dir.directories` | Retourne la liste des sous-dossiers d'un dossier. |
| `dir.files` | `"C:\Temp" dir.files` | Retourne la liste des fichiers contenus dans un dossier. |

### Exemples

```mogwai
# Create a working folder
(! path.files "MyProject") path.make -> 'projectDir'

if (projectDir dir.exists not) then
{
    projectDir dir.create
    "Folder created" ?
}

# List files in a folder
path.files dir.files -> 'fileList'
fileList ?

# List subfolders
path.files dir.directories -> 'dirList'
dirList ?
```

## Gestion des fichiers

**MOGWAI** fournit deux approches pour manipuler les fichiers :

### Lecture/écriture complète (binaire)

Pour lire ou écrire un fichier complet en une seule opération :

| Fonction | Exemple | Usage |
|----------|---------|-------|
| `file.data.read` | `"data.bin" file.data.read` | Lit tout le contenu binaire d'un fichier en une seule fois. |
| `file.data.write` | `bytearray "data.bin" file.data.write` | Écrit des données binaires complètes dans un fichier. |

### Lecture/écriture séquentielle avec handles

Pour les opérations séquentielles (lecture ligne par ligne, écriture progressive, gros fichiers), utilisez des handles de fichiers.

**Un handle est une chaîne** représentant l'identifiant hexadécimal unique du flux de fichier ouvert (filestream). Ce handle doit être conservé pour toutes les opérations ultérieures sur le fichier.

| Fonction | Exemple | Usage |
|----------|---------|-------|
| `file.open` | `"data.txt" file.open` | Ouvre un fichier en lecture et retourne un handle. |
| `file.create` | `"data.txt" file.create` | Ouvre un fichier en écriture (efface le fichier s'il existe) et retourne un handle. |
| `file.append` | `"log.txt" file.append` | Ouvre un fichier en écriture à la fin (conserve le contenu existant) et retourne un handle. |
| `file.read` | `handle size file.read` | Lit jusqu'à `size` octets depuis un fichier ouvert et retourne un DATA. |
| `file.readLine` | `handle file.readLine` | Lit une ligne complète (terminée par `\n` ou `\r\n`) et retourne un DATA. |
| `file.write` | `data handle file.write` | Écrit des données dans un fichier ouvert. **N'ajoute pas** automatiquement de saut de ligne. |
| `file.size` | `handle file.size` | Retourne la taille totale (en octets) d'un fichier ouvert en lecture. |
| `file.eof` | `handle file.eof` | Retourne `true` si la fin du fichier ouvert en lecture est atteinte. |
| `file.close` | `handle file.close` | Ferme un fichier ouvert. Fermez toujours les fichiers après utilisation ! |

### Conversion entre DATA et chaîne

Les fonctions de lecture de fichiers texte (`file.readLine`, `file.read`) retournent des DATA (tableaux d'octets) qui doivent être convertis en chaînes selon l'encodage du fichier. De même, pour écrire du texte dans un fichier, les chaînes doivent d'abord être converties en DATA.

**MOGWAI** fournit des fonctions de conversion dans les deux sens :

#### DATA vers chaîne (lecture)

| Fonction | Exemple | Usage |
|----------|---------|-------|
| `utf8->` | `data utf8->` | Convertit un DATA en chaîne avec l'encodage UTF-8. |
| `ascii->` | `data ascii->` | Convertit un DATA en chaîne avec l'encodage ASCII. |
| `ascii7->` | `data ascii7->` | Convertit un DATA en chaîne avec l'encodage ASCII 7 bits. |

#### Chaîne vers DATA (écriture)

| Fonction | Exemple | Usage |
|----------|---------|-------|
| `->utf8` | `string ->utf8` | Convertit une chaîne en DATA avec l'encodage UTF-8. |
| `->ascii` | `string ->ascii` | Convertit une chaîne en DATA avec l'encodage ASCII. |
| `->ascii7` | `string ->ascii7` | Convertit une chaîne en DATA avec l'encodage ASCII 7 bits. |

#### Sauts de ligne

`file.write` **n'ajoute pas** automatiquement de saut de ligne. Pour écrire des lignes, vous devez ajouter manuellement les octets de saut de ligne au DATA :

| Notation | Usage |
|----------|-------|
| `D:0D0A` | Saut de ligne Windows (CR LF : Retour chariot + Saut de ligne) |
| `D:0A` | Saut de ligne Unix/Linux/Mac (LF : Saut de ligne uniquement) |

**Exemple** : `"Ma ligne" ->utf8 D:0D0A + handle file.write`

L'opérateur `+` concatène les DATA pour créer un seul tableau d'octets.

### Manipulation de fichiers

| Fonction | Exemple | Usage |
|----------|---------|-------|
| `file.exists` | `"data.txt" file.exists` | Retourne `true` si le fichier existe, `false` sinon. |
| `file.info` | `"data.txt" file.info` | Retourne un enregistrement contenant toutes les métadonnées du fichier. |
| `file.copy` | `"source.txt" "dest.txt" file.copy` | Copie un fichier. |
| `file.rename` | `"ancien.txt" "nouveau.txt" file.rename` | Renomme un fichier. |
| `file.purge` | `"data.txt" file.purge` | Supprime un fichier. |

#### Métadonnées retournées par file.info

La fonction `file.info` retourne un enregistrement contenant les informations suivantes :

| Clé | Type | Description | Exemple |
|-----|------|-------------|---------|
| `name` | String | Nom du fichier avec extension | `"FIND NUMBER.mog"` |
| `fullName` | String | Chemin absolu complet du fichier | `"C:\Users\...\FIND NUMBER.mog"` |
| `directoryName` | String | Chemin du dossier contenant le fichier | `"C:\Users\...\Progs"` |
| `extension` | String | Extension du fichier | `".mog"` |
| `modifiedTime` | Number | Date de dernière modification (ticks .NET) | `6.390445690514954E+17` |
| `lastAccessTime` | Number | Date de dernier accès (ticks .NET) | `6.390643650826527E+17` |
| `length` | Number | Taille du fichier en octets | `992` |
| `isReadOnly` | Boolean | Fichier en lecture seule | `false` |
| `isArchive` | Boolean | Attribut archive (Windows) | `true` |
| `isHidden` | Boolean | Fichier caché | `false` |
| `isSystem` | Boolean | Fichier système | `false` |

**Note** : Les horodatages sont en ticks .NET (nombre d'intervalles de 100 nanosecondes depuis le 01/01/0001). Utilisez la fonction `->date` pour convertir ces valeurs en un enregistrement avec `day:`, `month:`, `year:`, etc.

**⚠️ Important** : Si le fichier n'existe pas, `file.info` lève une erreur. Utilisez `file.exists` pour vérifier l'existence avant d'appeler `file.info`.

### Exemples

**Lecture/écriture binaire complète :**

```mogwai
# Read an entire binary file
"image.png" file.data.read -> 'imageData'

# Write binary data
imageData "copy.png" file.data.write
```

**Lecture séquentielle avec handle :**

```mogwai
# Open a text file for reading
(! path.files "data.txt") path.make file.open -> 'fileHandle'

# The handle is a hexadecimal string, for example: "A3F5B2C8"
fileHandle ?

# Get the file size
fileHandle file.size -> 'fileSize'
"File size: {! fileSize} bytes" eval ?

# Read line by line until the end
while (fileHandle file.eof not) do
{
    # Read a line (returns a DATA)
    fileHandle file.readLine -> 'lineData'
    
    # Convert the DATA to a UTF-8 string
    lineData utf8-> -> 'line'
    
    # Display the line
    line ?
}

# Always close the file
fileHandle file.close
```

**Lecture par blocs d'octets :**

```mogwai
# Read a file in 1024-byte blocks
"bigfile.dat" file.open -> 'h'

while (h file.eof not) do
{
    # Read up to 1024 bytes
    h 1024 file.read -> 'chunk'
    
    # Process the chunk
    chunk process
}

h file.close
```

**Écriture séquentielle avec handle :**

```mogwai
# Open a file for writing (overwrites existing content)
(! path.files "report.txt") path.make file.create -> 'h'

# Write multiple lines with UTF-8 conversion and line breaks
"=== REPORT ===" ->utf8 D:0D0A + h file.write
"Important data with accents: éàç" ->utf8 D:0D0A + h file.write
"End of report" ->utf8 D:0D0A + h file.write

# Close the file
h file.close
```

**Note** : `D:0D0A` représente les octets CR LF (Retour chariot + Saut de ligne, saut de ligne Windows). Pour un saut de ligne Unix/Linux, utilisez `D:0A` uniquement. L'opérateur `+` concatène les DATA.

**Écriture avec différents encodages :**

```mogwai
"test.txt" file.create -> 'h'

# Line in UTF-8 (supports all characters)
"Français: éèêë" ->utf8 D:0D0A + h file.write

# Line in ASCII (basic characters)
"English: Hello" ->ascii D:0D0A + h file.write

# Line in ASCII 7-bit (strictly 7 bits)
"Basic: ABC123" ->ascii7 D:0D0A + h file.write

h file.close
```

**Mode ajout (ajouter à la fin) :**

```mogwai
# Add to an existing file (log mode)
(! path.files "debug.log") path.make file.append -> 'logHandle'

"[2025-02-10 14:30] New log entry" ->utf8 D:0D0A + logHandle file.write

logHandle file.close
```

**Gestion de plusieurs fichiers simultanément :**

```mogwai
# Open one file for reading and another for writing
(! path.files "input.txt") path.make file.open -> 'handleIn'
(! path.files "output.txt") path.make file.create -> 'handleOut'

# Read line by line from input and write to output
while (handleIn file.eof not) do
{
    # Read a line and convert to UTF-8
    handleIn file.readLine utf8-> -> 'line'
    
    # Process the line (example: convert to uppercase)
    line upper -> 'processedLine'
    
    # Convert back to DATA, add line break and write
    processedLine ->utf8 D:0D0A + handleOut file.write
}

# Close both files
handleIn file.close
handleOut file.close
```

**Copier et manipuler des fichiers :**

```mogwai
# Copy a file
(! path.files "original.txt") path.make 
(! path.files "copy.txt") path.make 
file.copy

# Rename a file
(! path.files "copy.txt") path.make
(! path.files "backup.txt") path.make
file.rename

# Delete a file
(! path.files "temp.txt") path.make file.purge
```

**Obtenir les métadonnées d'un fichier :**

```mogwai
# Retrieve all file information
"data.txt" file.info -> 'info'

# Display the complete record
info ?

# Access specific fields
info length: get -> 'size'
"File size: {! size} bytes" eval ?

info extension: get -> 'ext'
"Extension: {! ext}" eval ?

# Convert timestamp to readable date
info modifiedTime: get ->date -> 'dateModif'
"Last modified: {! dateModif day: get}/{! dateModif month: get}/{! dateModif year: get}" eval ?

# Check attributes
info isReadOnly: get -> 'readonly'
if (readonly) then
{
    "File is read-only, cannot modify it" ?
}
```

**Vérifier la taille avant de charger un fichier :**

```mogwai
"bigfile.dat" file.info -> 'info'
info length: get -> 'size'

if (size 10000000 >) then
{
    "File too large ({! size} bytes), block processing recommended" eval ?
    
    # Process by blocks
    "bigfile.dat" file.open -> 'h'
    while (h file.eof not) do
    {
        h 1024 file.read process
    }
    h file.close
}
else
{
    "File of reasonable size, complete read" ?
    "bigfile.dat" file.data.read process
}
```

**Vérifier si un fichier a été modifié récemment :**

```mogwai
"config.txt" file.info -> 'info'
info modifiedTime: get -> 'mtime'

# Get the current timestamp
now ->timestamp -> 'current'

# Calculate the difference (1 day = 864000000000 ticks = 86400 * 10000000)
current mtime - -> 'diff'

if (diff 864000000000 >) then
{
    "Outdated configuration (more than 24h), update recommended" ?
}
else
{
    "Configuration up to date" ?
}
```

**Gérer les fichiers absents avec file.exists :**

```mogwai
# Check existence before getting information
if ("config.txt" file.exists) then
{
    "config.txt" file.info -> 'info'
    info length: get ?
}
else
{
    "File config.txt not found!" ?
}

# Alternative: use guard/else to catch the error
guard
{
    "config.txt" file.info -> 'info'
    info length: get ?
}
else
{
    "File config.txt not found!" ?
}
```

# TIMERS

**MOGWAI** permet d'exécuter du code à intervalles réguliers. Les timers gèrent cela.

Vous pouvez créer autant de timers que vous voulez. Les timers utilisent leur propre pile et ne peuvent donc pas perturber celle de votre programme principal. L'inconvénient de cela est que le code d'un timer n'a pas accès à ce que vous avez éventuellement placé sur la pile et ne peut donc pas l'utiliser pour passer des paramètres par exemple. Ce cloisonnement est nécessaire car, comme le code d'un timer peut être déclenché à tout moment, il ne doit pas perturber le bon fonctionnement de votre programme.

Le code d'un timer a accès aux variables globales du programme en cours d'exécution.

Pour utiliser un timer, il faut le déclarer puis l'activer. À tout moment vous pouvez l'arrêter et le supprimer.

> **Note** : Pour l'exécution parallèle de code dans des processus séparés, voir [TÂCHES](#tâches). Les tâches offrent un meilleur isolement et une gestion des erreurs plus robuste que les timers.

## Timer de type `after`

Un timer de type `after` ne se déclenchera qu'une seule fois, après une période définie. La période est définie en millisecondes.

Quand il se déclenche, son code est exécuté (avec sa propre pile) et il s'arrête. Pour le réutiliser, il suffit de le redémarrer.

## Timer de type `every`

Un timer de type `every` se déclenchera à intervalles réguliers. La période est également définie en millisecondes.

Quand il se déclenche, son code est exécuté (avec sa propre pile) puis il est reprogrammé pour se déclencher à nouveau après que la période définie se soit écoulée.

## Déclarer un timer

La déclaration d'un timer se fait avec une syntaxe très simple.

Pour un timer de type after :

```
# We declare a timer of type after
# After 5 seconds it will display a message

timer 'timer1' after 5000 do 
{
    "Hello !" ? 
}

# We activate the timer

'timer1' timer.start

# We wait without doing anything for it to trigger

forever do
{

}
```

Au bout de 5 secondes, le message "Hello !" sera affiché, puis plus rien ne se passera.

Pour un timer de type every :

```
# We declare a timer of type every
# After 5 seconds it will display a message

timer 'timer1' every 5000 do 
«
    "Hello !" ? 
»

# We activate the timer

'timer1' timer.start

# We wait without doing anything for it to trigger every 5 seconds

forever do
{

}
```

Toutes les 5 secondes, le message "Hello !" sera affiché.

Voici les fonctions disponibles pour gérer les timers :

`timer.start` active le timer dont le nom est passé en paramètre : `'timer1' timer.start`

`timer.stop` arrête le timer dont le nom est passé en paramètre : `'timer1' timer.stop`

`timer.purge` supprime le timer dont le nom est passé en paramètre : `'timer1' timer.purge`

`timer.state` retourne true si le timer dont le nom est passé en paramètre est actif : `'timer1' timer.state`

`timer.list` retourne la liste des timers déclarés.

## Suspendre le déclenchement des timers

Il peut être nécessaire de s'assurer que les timers ne se déclenchent pas pendant un certain temps.

La fonction `DI` (disable interrupts) permet de bloquer le déclenchement des timers.

Pour réactiver les timers, utilisez la fonction `EI` (enable interrupts).

La fonction `DI` empêche l'exécution du code du timer mais ne suspend pas le timer lui-même, la fonction du timer est mise en file d'attente.

## Lancer du code avec un délai d'exécution

Il existe des cas où vous avez besoin de lancer du code après un certain délai. **MOGWAI** dispose d'un mécanisme basé sur les timers de type `after` pour réaliser cette fonctionnalité :

```
# We launch a function in 2 seconds

after 2000 do
{
    "Hello world !" ?
}

# We wait without doing anything for it to trigger

forever do
{

}
```

Vous n'avez aucun contrôle sur l'exécution de la fonction, impossible de la supprimer avant son exécution.
 
# ÉVÉNEMENTS

**MOGWAI** peut déclencher des événements et y répondre. Un événement est défini par un nom (par exemple 'MY_EVENT') et par du code à exécuter quand il est déclenché.

Un événement peut être déclenché par le code **MOGWAI** en cours d'exécution ou par l'application qui héberge le moteur (**MOGWAI CLI** héberge le runtime **MOGWAI**, et à ce titre peut déclencher des événements dans votre code).

Votre code **MOGWAI** peut également générer des événements à destination de l'application qui héberge le runtime. C'est une façon de communiquer avec elle.

> Les interactions entre le runtime et l'application hôte ne seront pas couvertes dans cette documentation mais dans celle qui explique comment intégrer **MOGWAI** dans une application hôte.

## Déclarer un événement

Pour répondre à un événement, vous devez le déclarer. Par exemple, pour déclarer l'événement 'MY_EVENT' qui affichera simplement "Hello !" quand il sera déclenché, il suffit de saisir :

```
onEvent 'MY_EVENT' do
{
    "Hello !" ?
}
```

Quand l'événement 'MY_EVENT' est déclenché, **MOGWAI** exécutera le code associé.

Le code d'un événement dispose systématiquement de la variable locale `eventData` qui porte le paramètre de l'événement (par exemple un nom ou un nombre). Cette valeur est fournie par celui qui déclenche l'événement. Si aucune valeur n'est associée, `eventData` porte la valeur `null`.

## Déclencher un événement

Depuis votre code **MOGWAI**, vous pouvez déclencher des événements à tout moment.

C'est la fonction `event.fire` qui permet de déclencher un événement. Elle prend en paramètres le nom de l'événement et le paramètre associé (si pas de paramètre, utilisez `null`) : `'MY_EVENT' null event.fire`

## Lister les événements supportés

Il est possible à tout moment de lister tous les événements auxquels vous pouvez répondre. C'est la fonction `event.list` qui s'en charge. Elle retourne la liste des noms des événements déclarés.

## Supprimer la prise en charge d'un événement

Pour que votre application **MOGWAI** cesse de répondre à un événement, vous devez le supprimer avec la fonction `event.purge` qui prend en paramètre le nom de l'événement à supprimer.

## Événements et Tâches

Les événements sont également utilisés par **MOGWAI** pour gérer la communication entre les tâches parentes et enfants. Voir la section [TÂCHES](#tâches) pour des informations détaillées sur la façon dont les tâches utilisent les événements pour la communication inter-processus.

## Mettre les événements en attente

Comme pour les timers, les événements peuvent être bloqués par la fonction `DI` (disable interrupts). Attention, si vous utilisez la fonction `DI`, les événements ne sont pas perdus, ils sont mis en file d'attente et quand les interruptions sont réactivées par la fonction `EI` (enable interrupts), ils seront tous exécutés les uns après les autres.
 
Vous pouvez tester ce comportement avec le code suivant :

```
mogwai.reset cls

event 'MY_EVENT' do
{
    "Hello num {! eventData}" eval ?
}

1 100 for 'i' do
{
    'MY_EVENT' i event.fire
    1000 wait
	
    if (i 10 ==) then { DI }
	
    if (i 20 ==) then { EI }
}
```

# PROGRAMMATION ORIENTÉE OBJET

**MOGWAI** fournit un système de programmation orientée objet simple mais complet. Il permet de définir des classes qui regroupent données et comportements, de créer des instances à partir de ces classes, et de gérer leur cycle de vie explicitement.

Ce système est intentionnellement maintenu simple : pas d'héritage, pas de ramasse-miettes. Vous avez le contrôle total de la création et de la destruction des instances.

## Définir une classe

Une classe est définie avec le mot-clé `class`, suivi de son nom sous forme de chaîne, du mot-clé `do`, et d'un bloc contenant deux sections :

- `private:` — propriétés et méthodes privées, accessibles uniquement depuis l'intérieur de la classe
- `public:` — propriétés et méthodes publiques, accessibles depuis l'extérieur de la classe

Les propriétés sont déclarées avec un nom suivi d'un type (`.number`, `.string`, `.bool`, `.any`, etc.). Les méthodes sont déclarées avec un nom suivi d'un bloc de code `{ }`.

```
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

## Propriétés et méthodes

Dans une section, **MOGWAI** distingue les propriétés des méthodes par leur valeur déclarée :

- Un **sigil de type** (`.number`, `.string`, etc.) déclare une propriété. Elle sera initialisée à `empty` quel que soit son type. L'annotation de type est utilisée pour la validation lors de l'affectation d'une valeur. Vous pouvez vérifier si une propriété a été initialisée avec `isEmpty`.
- Un **bloc de code** `{ }` déclare une méthode.

Le nom `className:` est réservé et ne peut pas être utilisé comme nom de propriété ou de méthode dans une définition de classe. Toute tentative de le déclarer lève l'erreur MW.95 (propriété réservée).

## Hooks de cycle de vie

Deux méthodes spéciales sont appelées automatiquement par le moteur si elles sont définies. Elles peuvent être placées dans `private:` ou `public:` :

- `onInit:` est appelée automatiquement lorsqu'une nouvelle instance est créée avec `new`. Elle reçoit les paramètres nommés passés à la création.
- `onFree:` est appelée automatiquement juste avant qu'une instance soit détruite avec `free`.

## Créer et détruire des instances

Utilisez `new` pour créer une instance et `free` pour la détruire.

```
# Create an instance, onInit: is called automatically
[id: 10 name: "SIBUE"] 'User' new -> '$U1'

# Destroy the instance, onFree: is called automatically
$U1 free
```

Chaque instance se voit attribuer un handle interne unique (noté `§453` pour l'instance numéro 453). Ce numéro n'est jamais réutilisé pendant la durée de vie du moteur — un handle d'instance détruite est définitivement invalide.

Plusieurs variables peuvent contenir une référence à la même instance. Si l'instance est détruite, toutes les variables qui pointent vers elle deviennent invalides. Toute tentative de les utiliser lèvera une erreur.

## Accéder aux propriétés et méthodes

Les propriétés et méthodes publiques sont accessibles avec la notation compacte `->` et `<-`, ou avec les formes verbales `get` et `set` :

```
# Read a public property
$U1->name: ?
# Equivalent to: $U1 name: get ?

# Write a public property
"DUPONT" &$U1<-name:
# Equivalent to: "DUPONT" &$U1 name: set

# Call a public method
$U1->display:
# Equivalent to: $U1 display: get
```

Tenter d'accéder à un membre `private:` depuis l'extérieur de la classe lève une erreur.

La propriété `className:` est une propriété publique réservée en lecture seule, automatiquement disponible sur chaque instance. Elle retourne le nom de la classe à laquelle appartient l'instance :

```
$U1->className: ?   # → 'User'
```

Tenter d'écrire dans `className:` ou de la déclarer dans une définition de classe lève l'erreur MW.95 (propriété réservée).

## La variable `self`

À l'intérieur de toute méthode, la variable `self` est automatiquement disponible et contient une référence à l'instance courante. Elle peut être utilisée pour lire ou écrire les propres propriétés de l'instance et pour appeler ses autres méthodes :

```
display:
{
    "USER={! self}" eval ?
    self->show:         # calls a private method
}
```

Utiliser `self` en dehors d'une méthode lève une erreur.

## Valider les paramètres d'une méthode

Toute méthode peut valider ses entrées de trois façons selon le niveau de sécurité requis.

**`->vars`** est l'option la plus simple. Elle extrait des valeurs de la pile ou d'un enregistrement et les affecte automatiquement à des variables locales, sans aucune validation de type :

```
setCoords:
{
    ('x' 'y') ->vars

    x self<-x:
    y self<-y:
}
```

S'il n'y a pas assez d'éléments sur la pile pour remplir toutes les variables listées, `->vars` lève une erreur.

**`->safeVars`** fonctionne comme `->vars` mais valide également le nombre et le type des valeurs de la pile. Une erreur est levée immédiatement si les valeurs ne correspondent pas :

```
setCoords:
{
    [.number .number] ->safeVars 'x' 'y'

    x self<-x:
    y self<-y:
}
```

**`->params`** attend un enregistrement de paramètres nommés sur la pile. Il valide les noms, les types, et les valeurs par défaut optionnelles. C'est le choix naturel pour `onInit:` puisque les instances sont créées avec un enregistrement nommé :

```
onInit:
{
    [id: .number name: .string index: (.number 0)] ->params

    id self<-id:
    name self<-name:
    index self<-index:
}
```

Si l'enregistrement ne correspond pas aux noms et types de paramètres déclarés, `->params` lève une erreur immédiatement.

## Exemple complet

```
mogwai.reset
console.clear

class 'User' do
{
    private:
    {
        x: .number
        y: .number
        z: .number

        onInit:
        {
            [id: .number name: .string] ->params

            id self<-id:
            name self<-name:

            rand 100 * ->int self<-x:
            rand 100 * ->int self<-y:
            rand 100 * ->int self<-z:
        }

        onFree:
        {
            "FREE {! self}" eval ?
        }

        show:
        {
            "ID={! self->id:}" eval ?
            "NAME={! self->name:}" eval ?
            self->show2:
        }

        show2:
        {
            "X={! self->x:}" eval ?
            "Y={! self->y:}" eval ?
            "Z={! self->z:}" eval ?
        }
    }

    public:
    {
        id: .number
        name: .string

        display:
        {
            "USER={! self}" eval ?
            self->show:
        }
    }
}

[id: 10 name: "SIBUE"] 'User' new -> '$U1'
[id: 20 name: "DUPONT"] 'User' new -> '$U2'

$U1->display:
" " ?
$U2->display:
" " ?

$U1 free
$U2 free
```

La sortie de ce programme ressemblera à ceci :

```
USER §1
ID=10
NAME=SIBUE
X=42
Y=67
Z=13

USER §2
ID=20
NAME=DUPONT
X=88
Y=5
Z=71

FREE §1
FREE §2
```

# TÂCHES

**MOGWAI** facilite grandement la création de tâches parallèles.
Ces tâches sont appelées tâches enfants.

Les tâches enfants communiquent avec leur tâche parente via des événements (voir [ÉVÉNEMENTS](#événements)). Comme les [TIMERS](#timers), les tâches s'exécutent avec leur propre pile isolée et peuvent être gérées indépendamment.

Une tâche enfant peut elle-même créer des tâches enfants. Il n'y a pas de limite autre que la mémoire disponible. Il est recommandé de ne pas lancer trop de tâches en parallèle pour éviter de saturer la mémoire et de dégrader les performances.

Le code d'une tâche enfant est défini dans la tâche parente, mais s'exécute en parallèle avec elle. La tâche parente peut continuer à faire d'autres choses pendant que les tâches enfants s'exécutent.

Pour illustrer l'utilisation des tâches, nous allons utiliser un exemple qui télécharge des pages HTML en arrière-plan et les sauvegarde sur le disque. Nous lancerons autant de tâches parallèles qu'il y a de pages à télécharger. Cela permettra de voir le cycle de vie de chaque tâche.

## Fonctionnement d'une tâche

### Événements pour la communication

Une tâche enfant ne peut pas communiquer directement avec sa tâche parente — elle doit utiliser des événements qui seront déclenchés dans le code de la tâche parente.

La tâche parente ne peut communiquer avec ses tâches enfants que via des événements qui seront déclenchés dans le code de la tâche enfant concernée.

Les tâches enfants n'ont aucun moyen de se parler directement.
Elles ne se connaissent pas et de leur point de vue seule la tâche parente existe.

Les événements qui peuvent être déclenchés par une tâche enfant vers sa tâche parente sont :

| Événement | Usage |
|-------|-------|
| `TASK_DID_START` | Événement déclenché quand la tâche a démarré.<br>La variable locale `eventData` contient le nom de la tâche concernée (ex. 'T1'). |
| `TASK_DID_END` | Événement déclenché quand la tâche est terminée.<br>La variable locale `eventData` contient un enregistrement composé du nom de la tâche (clé task:) et de la valeur retournée par task.result (clé result:) depuis la tâche enfant. |
| `TASK_DID_FAIL` | Événement déclenché quand une erreur a été levée dans le code de la tâche enfant.<br>La variable locale `eventData` contient un enregistrement composé de 3 clés : la clé `task:` portant le nom de la tâche concernée, la clé `error:` portant le code d'erreur, et la clé `message:` portant le message d'erreur. |
| `TASK_DID_PUBLISH` | Événement déclenché quand une tâche enfant envoie des données à sa tâche parente.<br>La variable locale `eventData` contient un enregistrement avec une clé `task:` qui porte le nom de la tâche concernée, et la clé `message:` qui contient le message. Le message peut être de n'importe quel type supporté par **MOGWAI**. |
| `TASK_DID_RECEIVE` | Événement déclenché ==dans le code d'une tâche enfant== quand la tâche parente lui envoie des données.<br>La variable locale `eventData` contient les données qui peuvent être de n'importe quel type supporté par **MOGWAI**. |

### Fonctions de la tâche parente

Pour gérer les tâches enfants, une tâche parente dispose des fonctions suivantes :

| Fonction | Exemple | Usage |
|----------|---------|-------|
| `task.isRunning` | `'T1' task.isRunning` | Retourne `true` si la tâche passée en paramètre est en cours d'exécution. |
| `task.join` | `('T1' 'T2' 'T3') task.join` | Suspend le programme jusqu'à ce que toutes les tâches listées soient terminées. |
| `task.list` | `task.list` | Retourne la liste de toutes les tâches définies, quel que soit leur état. |
| `task.purge` | `'T1' task.purge` | Supprime la tâche passée en paramètre. Si la tâche était en cours d'exécution, elle est arrêtée avant d'être supprimée. |
| `task.result` | `'T1' task.result` | Retourne le résultat de la tâche passée en paramètre. Le résultat peut être de n'importe quel type supporté par MOGWAI. |
| `task start with` | `task 'T1' start with object` | Exécute la tâche passée en paramètre en lui passant un objet MOGWAI. L'objet est placé sur la pile de la tâche enfant juste avant le lancement. Cette fonction retourne immédiatement. |
| `task send` | `task 'T1' send object` | Envoie l'objet passé à la tâche 'T1'. La tâche enfant reçoit l'objet via l'événement `TASK_DID_RECEIVE`. |
| `task.wait` | `'T1' task.wait` | Exécute la tâche passée en paramètre et attend qu'elle se termine avant de retourner. |
| `task.stop` | `'T1' task.stop` | Arrête la tâche passée en paramètre. L'arrêt d'une tâche enfant déclenche l'événement `TASK_DID_END` dans la tâche parente avec la valeur résultat au moment de l'arrêt. |
 
### Fonctions de la tâche enfant

Une tâche enfant peut utiliser les fonctions suivantes :

| Fonction | Exemple | Usage |
|----------|---------|-------|
| `task.name` | | Retourne le nom de la tâche enfant. |
| `task.publish` | `object task.publish` | Envoie un objet à la tâche parente via l'événement `TASK_DID_PUBLISH`.<br>L'objet peut être de n'importe quel type supporté par **MOGWAI**. |
| `task.setResult` | `object task.setResult` | Définit le résultat de la tâche. Il peut être de n'importe quel type supporté par **MOGWAI**. |


### Passer des paramètres à une tâche enfant

Quand une tâche parente lance une tâche enfant, elle peut lui passer un objet **MOGWAI** en paramètre. Cet objet est placé sur la pile de la tâche enfant juste avant le lancement. C'est à la tâche enfant de récupérer cet objet.

Pour passer des paramètres à une tâche enfant, il suffit de les placer dans un objet **MOGWAI** et de le passer à la fonction `task 'T1' start with object` de la tâche parente. La tâche enfant récupère cet objet car il est automatiquement placé sur la pile au début de son code.

Attention : si vous essayez de lancer une tâche enfant qui est déjà en cours d'exécution, la fonction `task start with` lèvera une erreur. Il est recommandé de vérifier que la tâche n'est pas déjà en cours d'exécution avant de la lancer.

## Comportement en cas d'erreur non gérée

Si une tâche enfant lève une erreur qui n'est pas gérée par un `guard` ou `trap` dans son code, la tâche enfant est automatiquement arrêtée et l'événement `TASK_DID_FAIL` est déclenché dans la tâche parente avec les informations d'erreur comme décrit ci-dessus.

## Attendre la fin d'une tâche enfant

Si vous voulez attendre la fin d'une tâche enfant avant de continuer l'exécution du programme, il suffit d'utiliser la fonction `task.wait` de la tâche parente en lui passant le nom de la tâche concernée.

La tâche enfant doit avoir été lancée avec `task start with` au préalable pour que `task.wait` fonctionne. Si la tâche enfant n'a pas été lancée, `task.wait` retournera immédiatement.

## Attendre la fin de plusieurs tâches enfants

Si vous voulez attendre la fin de plusieurs tâches enfants avant de continuer l'exécution du programme, il suffit d'utiliser la fonction `task.join` de la tâche parente en lui passant la liste des tâches concernées.

## Relancer une tâche enfant terminée

Une tâche enfant qui a été lancée et s'est terminée peut être relancée. Il suffit de l'appeler à nouveau avec `task start with`, en lui passant optionnellement un nouvel objet en paramètre.

## Bonnes pratiques

- Toujours utiliser `guard` dans les tâches pour capturer les erreurs.
- Limiter à 50-100 tâches simultanées au maximum.
- Utiliser `task.setResult` pour retourner un statut de succès/échec ou d'autres informations à la tâche parente.
- Préférer `task.join` aux boucles d'attente avec `task.isRunning`.
- Les tâches enfants ne connaissent pas les chemins standard de MOGWAI 8 (voir exemple).

## Exemple complet

Dans cet exemple, nous allons télécharger plusieurs pages HTML en parallèle et les sauvegarder sur le disque.

```
# TASK DEMO

mogwai.reset

console.clear

# Prepare the TASKDEMO download folder for the demo
# We use MOGWAI 8's standard paths for files (path.files)

(! path.files "TASKDEMO") path.make dir.create

# Declare all child task monitoring events

onEvent 'TASK_DID_START' do 
{ 
	"TASK DID START {! eventData}" eval ?
}

onEvent 'TASK_DID_END' do 
{ 
	"TASK DID END {! eventData}" eval ?
}

onEvent 'TASK_DID_FAIL' do 
{ 
	"TASK DID FAIL {! eventData}" eval ?
}

onEvent 'TASK_DID_PUBLISH' do 
{ 
	"TASK DID PUBLISH {! eventData}" eval ?
}

# Prepare the information that will be provided to each download task
# The download url
# The file to use to save the downloaded data

(
	[name: 'T1' url: "https://www.google.fr" filename: "google.bin"]
	
	[name: 'T2' url: "https://www.coding4phone.com" filename: "c4p.bin"]
	
	[name: 'T3' url: "https://www.mogwai.eu.com" filename: "mogwai.bin"]
)

foreach 'item' do
{
	item ->vars
	
	task name do
	{
		# The startup parameter (placed by task start with)
		# is in this example a record of type [url: "..." filename: "..."]
		# which allows it to retrieve which url to download and in which file to save the information
		
		# ->vars will take the record and create a corresponding local variable for each key
		# The local variables url and filename will be automatically created with the values carried by the record
		
		->vars
		
		now -> 'begin'
		
		[! http.get uri: url] -> 'r'
		
		now begin - ->duration -> 'd'
		
		if (r->state) then
		{
			"Download duration: ({! d->ms} ms)" eval task.publish
			
			guard
			{			
				(! path.files filename) path.make r->response file.data.write
				true task.setResult
			}
			else
			{
				false task.setResult
			}
		}
		else
		{
			"Download error!" task.publish
			
			false task.setResult
		}
	}
	
	# We must provide the child task with the complete path
	# A child task doesn't know MOGWAI 8's standard folders
	
	(! path.files "TASKDEMO" filename) path.make -> 'filename'
	
	# We launch the task by providing it with the record containing the information
	# it needs
	
	task name start with [! url: url filename: filename]
}

('T1' 'T2' 'T3') task.join

"PROGRAM COMPLETED" ?
```

Ce programme crée trois tâches de téléchargement en parallèle. Chaque tâche télécharge une page HTML et la sauvegarde sur le disque. Les événements de surveillance des tâches sont déclenchés pour afficher des messages dans la console à chaque étape du cycle de vie des tâches enfants. Enfin, le programme attend que toutes les tâches soient terminées avant d'afficher "PROGRAM COMPLETED".

La sortie console de ce programme ressemblera à ceci :

```
TASK DID START 'T3'
TASK DID START 'T2'
TASK DID START 'T1'
TASK DID PUBLISH [task: 'T2' message: "Download duration: (475 ms)"]
TASK DID END [task: 'T2' result: true]
TASK DID PUBLISH [task: 'T1' message: "Download duration: (579 ms)"]
TASK DID END [task: 'T1' result: true]
TASK DID PUBLISH [task: 'T3' message: "Download duration: (807 ms)"]
TASK DID END [task: 'T3' result: true]
PROGRAM COMPLETED
```


