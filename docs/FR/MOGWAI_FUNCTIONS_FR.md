# GLOSSAIRE

## FONCTIONS DU LANGAGE

### `mogwai.reset`

Force **MOGWAI** à effectuer une réinitialisation du runtime.

***

### `mogwai.info`

Retourne un enregistrement contenant diverses informations sur le runtime et le système sur lequel il tourne.

```
mogwai.info ?d
```

Affichera :

```
name:                "MOGWAI CLI"
version:             "8.0.0.0"
platform:            "WINDOWS"
architecture:        "X64"
OSdescription:       "Microsoft Windows 10.0.26200"
framework:           ".NET 9.0.13"
runtimeID:           "win-x64"
prompt:              "MOGWAI RUNTIME 8.0.0.0...
primitives:          ('+' '-' '*' '/' 'sin' 'cos' 'tan' 'asin' 'acos' '...
externalKeywords:    ()
hostKeywords:        ('?s' 'run' 'edit' 'file.edit' 'file.select')
debug:               true
keepAlive:           true
isTask:              false
```

| Clé                 | Signification                                                                                        |
| ------------------- | ---------------------------------------------------------------------------------------------------- |
| `name:`             | Nom du runtime.                                                                                      |
| `version:`          | Version du runtime.                                                                                  |
| `platform:`         | Nom de la plateforme sur laquelle tourne le runtime.                                                 |
| `architecture:`     | Architecture de la plateforme.                                                                       |
| `OSdescription:`    | Description complète de la plateforme.                                                               |
| `framework:`        | Version du runtime .NET.                                                                             |
| `runtimeID:`        | Identifiant de runtime de la plateforme.                                                             |
| `prompt:`           | Invite du runtime **MOGWAI**.                                                                        |
| `primitives:`       | Liste des primitives disponibles.                                                                    |
| `externalKeywords:` | Liste des mots-clés externes disponibles (fonctions fournies par les extensions).                    |
| `hostKeywords:`     | Liste des mots-clés hôtes disponibles (fonctions fournies par l'hôte).                               |
| `debug:`            | true si le runtime est en mode debug.                                                                |
| `extensions:`       | Liste des extensions chargées.                                                                       |
| `keepAlive:`        | true si le runtime **MOGWAI** conserve son contexte d'exécution d'une session à l'autre.            |
| `isTask:`           | true si le runtime **MOGWAI** est une tâche enfant.                                                 |

***

### `mogwai.exit`

Force le runtime à arrêter l'exécution en cours sans lever d'erreur.

***

### `mogwai.halt`

Force le runtime à arrêter l'exécution en cours et lève l'erreur MW.2 « halt encountered ».

***

### `mogwai.assert`

Vérifie qu'une condition est vraie. Si la condition est fausse, lève l'erreur MW.9 (`assert error`) et arrête l'exécution. Si `MOGWAI.onError` est défini, il est appelé automatiquement.

Prend deux paramètres : une condition et une chaîne de message.

La condition peut être une **liste** (automatiquement évaluée ; après exécution, exactement une valeur doit avoir été poussée sur la pile — `MW.24` sinon — et doit être un booléen — `MW.21` sinon) ou un **booléen** déjà sur la pile. Tout autre type lève `MW.21` (bad argument type).

Le message est utilisé dans l'affichage de l'erreur. Il n'est pas accessible par programme — `error.last` retourne `MW.9`.

```
# Condition as a list — evaluated by mogwai.assert
(a 10 ==) "a must equal 10" mogwai.assert

# Condition as a boolean already on the stack
a 0 >  "a must be positive" mogwai.assert
```

***

### `mogwai.cclear`

Vide le cache des procédures incluses via la commande `include`.

Garantit que le code inclus est bien la dernière version.

***

### `mogwai.strict`

Si `true` est passé en paramètre, toutes les variables doivent être déclarées avant d'être utilisées.

Les variables sont déclarées avec la fonction `=>` :

```
100 'A' => # Declares variable A as .number type and assigns it the value 100.
```

> Par défaut, `mogwai.strict` est désactivé.

***

### `mogwai.isTask`

Retourne `true` si le runtime est une tâche enfant (voir la gestion des tâches).

***

### `mogwai.sendMessage`

Envoie un message à l'hôte. Le message est un enregistrement contenant au minimum la clé `type:`.

```
"MY_EVENT" 567 mogwai.sendMessage
# Sends the following message to the host by the MessageReceivedFromRuntime delegate function.
# The number 567 is passed as parameter to the host.
# Task<EvalResult> MessageReceivedFromRuntime(Engine engine, string message, MOGObject parameter);
```

***

### `env.machineName`

Retourne le nom de la machine sur laquelle tourne le runtime sous forme de chaîne.

***

### `funcs`

Retourne la liste des fonctions utilisateur définies.

***

### `->`

Stocke une valeur dans une variable.

```
50 -> 'A'
```

***

### `->+`

Ajoute une valeur à une variable.

```
50 ->+ 'A'
```

***

### `->-`

Soustrait une valeur d'une variable.

```
50 ->- 'A'
```

***

### `->*`

Multiplie une variable par une valeur :

```
50 ->* 'A'
```

***

### `->/`

Divise une variable par une valeur :

```
50 ->/ 'A'
```

***

### `++`

Incrémente une variable.

```
'A' ++
```

***

### `--`

Décrémente une variable.

```
'A' --
```

***

### `&`

Pousse une référence directe à une variable sur la pile, au lieu d'une copie de sa valeur. Les fonctions qui supportent les références modifient la variable directement, sans créer de copies intermédiaires.

```
"bonjour" -> 'A'
&A ->upper
# A now contains "BONJOUR" — modified in place
```

> Toutes les fonctions ne supportent pas les références. Si vous utilisez `&` avec une fonction qui ne le supporte pas, une erreur `bad argument type` est levée.

***

### `-->`

Applique une liste de transformations à une variable en place. Chaque élément de la liste est appliqué en séquence en utilisant la valeur courante de la variable comme entrée.

```
"bonjour" -> 'A'
(->upper butfirst butlast) --> &A
# A now contains "ONJOU"
```

Les éléments de la liste peuvent être des fonctions ordinaires ou des quotations :

```
"hello world" -> 'A'
(->upper { " !" + }) --> &A
# A now contains "HELLO WORLD !"
```

L'opération est **transactionnelle** : un instantané de la variable est pris avant le démarrage du pipeline. Si une étape lève une erreur, la variable est automatiquement restaurée à sa valeur d'origine et l'erreur est propagée.

Une liste vide `()` est un no-op : la variable est laissée inchangée.

***

### `rcl`

Pousse la valeur d'une variable dont le nom est passé en paramètre sur la pile.

```
100 -> '$A'
'$A' rcl ?

# Displays 100
```

***

### `purge`

Supprime une variable dont le nom est passé en paramètre.

```
'$A' purge
```

***

### `exists`

Retourne `true` si la variable dont le nom est passé en paramètre existe.

```
'$A' exists
```

***

### `eval`

Évalue un objet sur la pile.

Le comportement diffère selon le type d'objet évalué :

- Les fonctions et blocs de code sont exécutés.
- Les chaînes sont mises à jour avec les caractères de contrôle et les blocs de remplacement.
- Les éléments dynamiques d'une liste sont remplacés par leur valeur courante.
- Les éléments dynamiques d'un enregistrement sont remplacés par leur valeur courante.

```
"Mr. X" -> 'name'
"The name is {! Name}" eval ?

# Displays "The name is Mr. X"

[x: 50 name: name] eval

# Pushes [x: 50 name: "Mr. X"] onto the stack
```

***

### `include`

Inclut et exécute immédiatement le code d'un fichier.

```
"my code.mog" include
```

***

### `mogwai.using`

Importe une bibliothèque d'extension au format ***MOGWAI***.

Si l'extension se trouve dans le répertoire `path.usings`, il suffit de spécifier son nom (avec un objet nom) sans le chemin ni l'extension.

```
'MOGWA_SERIAL' mogwai.using
```

Si l'extension ne se trouve pas dans le répertoire `path.usings`, il faut spécifier son nom complet avec chemin et extension (avec un objet chaîne).

```
"my extensions/MOGWA_SERIAL.dll" mogwai.using
```

***

### `mogwai.usings`

Liste les usings effectués et disponibles.

***

### `get`

Retourne la valeur d'une clé dans un enregistrement ou une instance de classe, un élément d'une liste ou d'un tableau d'octets.

| Action                          | Résultat               |
| ------------------------------- | ---------------------- |
| `(1 2 3 4) 1 get`               | retournera 2           |
| `[x: 10 y: 20] x: get`          | retournera 10          |
| `[x: 10 l: (1 2 3)] (l: 1) get` | retournera 2           |
| `D:FFEA10 1 get`                | retournera 234 (0xEA)  |
| `$U1 name: get`                 | retournera la valeur de la propriété `name:` de l'instance `$U1` |

Lorsqu'elle est appelée sur une instance de classe, `get` exécute également la méthode si la clé désigne une méthode plutôt qu'une propriété.

Voir aussi la notation compacte `->` dans [ENREGISTREMENTS](#records).

***

### `set`

Modifie la valeur d'une clé dans un enregistrement ou une instance de classe, un élément d'une liste ou d'un tableau d'octets.

> **Changement non rétrocompatible (v8.6) :** L'ordre des paramètres a été mis à jour pour la cohérence RPN. La valeur à écrire est maintenant le **premier** paramètre : `value container key: set`. Le code utilisant l'ancien ordre (`container key: value set`) doit être mis à jour.

| Action                      | Résultat                     |
| --------------------------- | ---------------------------- |
| `10 (1 2 3 4) 0 set`        | retournera `(10 2 3 4)`      |
| `100 [x: 10 y: 20] x: set`  | retournera `[x: 100 y: 20]`  |
| `0xAA D:FFEA10 0 set`       | retournera `D:AAEA10`        |
| `"DUPONT" &$U1 name: set`   | écrit `"DUPONT"` dans la propriété `name:` de l'instance `$U1` |

Lors de l'écriture dans une instance de classe, `set` n'accepte que les clés déclarées dans les sections `public:` ou `private:` de la classe. Toute tentative d'écriture sur une clé non déclarée lève une erreur.

Voir aussi la notation compacte `<-` dans [ENREGISTREMENTS](#records).

***

### `size`

Retourne la taille d'un enregistrement, d'une liste, d'un data, d'un binaire ou d'une chaîne.

***

### `keys`

Retourne une liste composée des clés d'un enregistrement.

```
[x: 10 y: 50 z: 100] keys 

# Will push (x: y: z:) onto the stack
```

***

### `first`

Retourne le premier élément d'une chaîne, d'une liste ou d'un data.

***

### `last`

Retourne le dernier élément d'une chaîne, d'une liste ou d'un data.

***

### `butfirst`

Retourne tous les éléments sauf le premier d'une chaîne, d'une liste ou d'un data.

***

### `butlast`

Retourne tous les éléments sauf le dernier d'une chaîne, d'une liste ou d'un data.

***

### `contains`

Retourne `true` si un élément est présent dans une chaîne, un enregistrement, une liste ou un data.

| Action                         | Résultat             |
| ------------------------------ | -------------------- |
| `"TOTO" "T" contains`          | retournera `true`.   |
| `[x: 50 y: 100] x: contains`   | retournera `true`.   |
| `(10 "EEE" 20 50) 20 contains` | retournera `true`.   |
| `D:FF00FFAB 0xFF contains`     | retournera `true`.   |

***

### `where`

Retourne une liste de tous les emplacements d'un élément dans une chaîne, une liste ou un data.

| Action                       | Résultat                  |
| ---------------------------- | ------------------------- |
| `"HELLO WORLD" "O" where`    | retournera `(4 7)`        |
| `(10 100 40 10 24) 10 where` | retournera `(0 3)`        |
| `D:45ED23FF0645DD 0x45`      | where retournera `(0 5)`  |

***

### `split`

Retourne une liste composée des éléments d'une chaîne séparés par une chaîne contenant le séparateur (qui peut être composé de plusieurs caractères).

| Action                  | Résultat                                       |
| ----------------------- | ---------------------------------------------- |
| `"X1;X45;Z34;12" split` | retournera `("X1" "X45" "Z34" "12")` |

***

### `join`

Recompose une chaîne à partir des éléments d'une liste et d'un séparateur.

> Fonction inverse de split.

| Action                             | Résultat                       |
| ---------------------------------- | ------------------------------ |
| `("X1" "X45" "Z34" "12") ";" join` | retournera `"X1;X45;Z34;12"` |

### `like`

Retourne `true` si une chaîne correspond à un motif particulier.

```
?           = Any single character
*           = Zero or more characters
#           = Any digit (0 to 9)
[charlist]  = Any single character in charlist
[!charlist] = Any single character not in charlist
```

```
"MR SMITH 62" "M? SM*H ??" like

# Pushes true onto the stack
```

***

### `right`

Retourne les n derniers caractères d'une chaîne ou octets d'un data.

```
"Hello world!" 6 right

# Pushes "world!" onto the stack

D:56231245 3 right # Pushes D:231245 onto the stack
```

***

### `left`

Retourne les n premiers caractères d'une chaîne ou octets d'un data.

```
"Hello world!" 6 left

# Pushes "Hello " onto the stack

D:56231245 3 left # Pushes D:562312 onto the stack
```

***

### `extract`

Extrait plusieurs éléments d'une liste ou d'un data en spécifiant les éléments à extraire dans une liste.

```
(10 20 30 40) (0 1 3) extract 

# will return (10 20 40)

D:FF45AB23 (0 1 3) extract

# will return DATA:FF4523

[x: 10 y: 20 z: 100] (x: z:) extract 

# will return [x: 10 z: 100]
```

***

### `wait`

Suspend le runtime pendant une durée exprimée en millisecondes sans bloquer le traitement des messages de type événement et timer.

***

### `rand`

Retourne un nombre aléatoire entre 0 et 1.

***

### `sub`

Extrait une partie d'une liste, d'un data ou d'un nombre binaire en spécifiant le début et l'étendue.

```
(10 20 30 40 50) 1 3 sub
# Pushes (20 30 40) onto the stack

D:05FFEDAB2312 2 3 sub 
# Pushes DATA:EDAB23 onto the stack

B:1001111001111 2 4 sub 
# Pushes B:0011 onto the stack
```

***

### `break`

Force la sortie d'une boucle for, while, foreach, forever et during.

***

### `foreach...transform`

Itère chaque élément d'une liste, lui applique un bloc de transformation, et retourne une nouvelle liste des éléments transformés.

Le bloc s'exécute sur sa **propre pile isolée**, distincte de la pile principale. Il a accès aux variables locales et globales, mais ne peut ni lire ni écrire sur la pile principale. La valeur laissée sur la pile du bloc à la fin de chaque itération devient l'élément correspondant dans la liste résultante.

Le nom de la variable de boucle est spécifié entre les mots-clés `foreach` et `transform`.

```
(1 2 3 4 5) foreach 'item' transform { item 2 * }
# Returns (2 4 6 8 10)

("L1" "L2" "L3") foreach 'item' transform { "-" item + }
# Returns ("-L1" "-L2" "-L3")
```

***

### `foreach...filter`

Itère chaque élément d'une liste, lui applique un bloc prédicat, et retourne une nouvelle liste contenant uniquement les éléments pour lesquels le bloc s'évalue à `true`.

Le bloc s'exécute sur sa **propre pile isolée**, distincte de la pile principale. Il a accès aux variables locales et globales, mais ne peut ni lire ni écrire sur la pile principale.

Le nom de la variable de boucle est spécifié entre les mots-clés `foreach` et `filter`.

```
(1 2 3 4 5 6 7 8 9 10) foreach 'i' filter { i 5 >= i 8 <= and }
# Returns (5 6 7 8)

(1 2 3 4 5 6 7 8 9 10) foreach 'item' filter { item 2 mod 0 == }
# Returns (2 4 6 8 10)
```

***

### `return`

Force la sortie d'une fonction.

***

### `flags`

Retourne la liste de tous les flags actifs.

***

### `flag.set`

Active le flag dont le nom est passé en paramètre.

***

### `flag.clear`

Désactive le flag dont le nom est passé en paramètre.

***

### `flag.isSet`

Retourne true si le flag dont le nom est passé en paramètre est actif.

***

### `flag.isClear`

Retourne true si le flag dont le nom est passé en paramètre est inactif.

***

### `unique`

Retourne un code unique sous forme de chaîne.

Ex : « DEC378AF69F246B6A1688799F70A987A »

***

### `guid`

Retourne un code unique au format UUID (ou GUID) sous forme de chaîne.

Ex : « 392BDA7A-9BEB-43B2-ACC7-05C8A06B0F44 »

***

### `json->`

Crée une liste ou un enregistrement à partir d'une chaîne au format json.

***

### `->json`

Crée une chaîne au format json à partir d'une liste ou d'un enregistrement.

***

### `->escape`

Échappe une chaîne passée en paramètre.

Les guillemets sont remplacés par `"`, les sauts de ligne par `
` et/ou `
`, etc…

***

### `->unescape`

Déséchappe une chaîne passée en paramètre (voir ->escape).

***

### `error.last`

Retourne le code de la dernière erreur levée.

***

### `error.reset`

Réinitialise le code de la dernière erreur levée à « MW.0 » (pas d'erreur).

***

### `error.throw`

Lève artificiellement l'erreur dont le code est passé en paramètre.

***

### `+`

Additionne 2 objets.

Les combinaisons possibles sont :

- 2 nombres
- 1 liste et un objet
- 2 chaînes
- 1 data et un octet
- 2 data
- 2 listes

***

### `-`

Soustrait 2 nombres.

***

### `*`

Multiplie 2 nombres.

***

### `/`

Divise 2 nombres.

***

### `<`

Retourne `true` si le premier paramètre est inférieur au second.

***

### `>`

Retourne `true` si le premier paramètre est supérieur au second.

***

### `<=`

Retourne `true` si le premier paramètre est inférieur ou égal au second.

***

### `>=`

Retourne `true` si le premier paramètre est supérieur ou égal au second.

***

### `==`

Retourne `true` si le premier paramètre est égal au second.

***

### `!=`

Retourne `true` si le premier paramètre est différent du second.

***

### `and`

Effectue l'opération ET logique entre le premier et le second paramètre.

***

### `or`

Effectue l'opération OU logique entre le premier et le second paramètre.

***

### `xor`

Effectue l'opération XOR logique entre le premier et le second paramètre.

***

### `not`

Effectue l'opération NOT logique entre le premier et le second paramètre.

***

### `isnull`

Retourne `true` si l'objet passé en paramètre est `null`.

***

### `isEmpty`

Retourne `true` si l'objet sur la pile est `empty`.

### `drop`

Supprime le premier élément de la pile.

***

### `swap`

Échange les 2 premiers éléments de la pile.

***

### `dup`

Duplique le premier élément de la pile.

***

### `depth`

Retourne le nombre d'éléments dans la pile.

***

### `clear`

Vide la pile.

***

### `sign`

Retourne une liste contenant le type des n éléments de la pile sans modifier la pile.

```
# Place elements on the stack
10 "EEE"

# Request the type of these 2 elements
2 sign

# The list (.string .number) is pushed onto the stack
```

***

### `->type`

Retourne le type de l'objet passé en paramètre.

***

### `->compress`

Retourne un data résultant de la compression d'un data passé en paramètre.

***

### `->decompress`

Retourne un data résultant de la décompression d'un data passé en paramètre. Le data passé en paramètre est normalement le résultat de la fonction `compress`.

***

### `->pack`

Sérialise un objet passé en paramètre et retourne le résultat sous forme de data.

***

### `->unpack`

Désérialise un data passé en paramètre et retourne le résultat sous forme d'objet. Le data passé en paramètre est normalement le résultat de la fonction `->pack`.

***

### `vars`

Retourne la liste de toutes les variables globales existantes.

***

### `lvars`

Retourne la liste de toutes les variables locales existantes.

***

### `console.print` ou `??`

Affiche une chaîne à l'écran sans saut de ligne.

```
"Hello " console.print "world!" console.println

# Will display:
# Hello world!
```

***

### `console.println` ou `?`

Affiche une chaîne à l'écran avec un saut de ligne.

***

### `?d`

Affiche les listes, enregistrements et data à l'écran dans une version « plus lisible ».

```
(10 20 30 40 50) ?d
```

Affichera :

```
0 : 10
1 : 20
2 : 30
3 : 40
4 : 50
```

```
[x: 100 y: 50 z: "HELLO"] ?d
```

Affichera :

```
x:  100
y:  50
z:  "HELLO"
```

```
D:5612FFEA1789AD34C5FAFEFF01021020ABACA0 ?d
```

Affichera :

```
00000000  56 12 FF EA 17 89 AD 34 C5 FA FE FF 01 02 10 20  | V.ÿê.?­4Åúþÿ...   |
00000010  AB AC A0                                         | «¬               |
```

***

### `console.clear`

Efface l'écran.

***

### `console.input`

Attend une saisie clavier (terminée par validation avec la touche `ENTRÉE`) et retourne la chaîne correspondante.

***

### `console.prompt`

Comme la fonction `input` mais affiche un message d'invite passé en paramètre.

```
"What is your first name? " console.prompt
"Your first name is: " swap + ?
```

Affichera l'invite, puis vous pourrez saisir (par exemple STEPHANE)

```
What is your first name? STEPHANE
```

Puis une fois la saisie (STEPHANE) validée :

```
Your first name is: STEPHANE
```

***

### `console.show`

Affiche la console de sortie (si gérée par l'hôte).

> N'a aucun effet dans **MOGWAI CLI**.

***

### `console.hide`

Masque la console de sortie (si gérée par l'hôte).

> N'a aucun effet dans **MOGWAI CLI**.

***

### `->list`

Construit une liste à partir des éléments présents sur la pile. Vous devez passer le nombre d'éléments à prendre en paramètre. Une erreur est levée si la pile ne contient pas assez d'éléments.

```
10 20 30 40 50 5 ->list ?

# Pushes (10 20 30 40 50) onto the stack
```

***

### `->int`

Convertit un nombre passé en paramètre en entier.

***

### `->str`

Convertit un objet passé en paramètre en chaîne.

***

### `->format`

Convertit un nombre en chaîne en utilisant un format.

```
50 "000" ->format ?
# Will display 050

50.8 "000.000" ->format ?
# Will display 050.800
```

***

### `->vars`

Extrait des valeurs et les affecte à des variables créées localement.

Avec un enregistrement, extrait les valeurs de toutes les clés et crée les variables locales correspondantes pour les clés extraites :

```
[x: 10 y: 20 z: 50] ->vars 
"x={! x}" eval ? 
"y={! y}" eval ? 
"z={! z}" eval ?
```

Affichera :

```
x=10
y=20
z=50
```

***

Avec la pile, extrait des valeurs et crée les variables locales correspondantes :

```
20 30 40 ('a' 'b' 'c') ->vars 
"a={! a}" eval ? 
"b={! b}" eval ? 
"c={! c}" eval ?
```

Affichera :

```
a=20
b=30
c=40
```

***

### `->safeVars`

Vérifie que les valeurs présentes sur la pile sont conformes aux attentes.
Vous pouvez vérifier leur nombre et leur type, et affecter automatiquement des variables locales avec les valeurs de la pile. Une erreur est levée en cas de non-conformité.

```
"EEE" 50 [x: .string y: .number] ->safeVars 
"x={! x}" eval ? 
"y={! y}" eval ?
```

Affichera :

```
x=EEE
y=50
```

***

### `->params`

Permet de passer des paramètres nommés (paires clé/valeur dans un enregistrement) et de vérifier que les paramètres attendus sont présents et que leurs types correspondent.
Si tout est correct, les variables locales correspondant aux paramètres attendus sont automatiquement créées avec leurs valeurs correspondantes.

```
[x: 100 y: "HELLO"] [x: .number y: .string] ->params 
"x={! x}" eval ? 
"y={! y}" eval ?
```

Affichera :

```
x=100
y=HELLO
```

***

### `check`

Vérifie que les n premiers éléments de la pile sont du type attendu.

```
10 "EEE" 20 4 (.number .number .string .number) check
# No error is raised because the 4 first elements of the stack are of the expected type.

10 "EEE" 20 4 (.string .number .string .number) check
# An error is raised :
# stack corruption error (MW.24)
# stack types expected (.string .number .string .number) but actually (.number .number .string .number)
```

***

### `->num`

Convertit une chaîne en nombre lorsque c'est possible.
Si impossible, une erreur est levée.

***

### `->char`

Convertit un nombre en caractère selon la norme Unicode.

***

### `char->`

Convertit un caractère de chaîne unique en son code selon la norme Unicode.

***

### `->name`

Convertit une chaîne ou une clé en nom.

***

### `->key`

Convertit une chaîne ou un nom en clé.

***

### `->data`

Prend n éléments de la pile et crée un data.
Le nombre d'éléments à prendre est passé en paramètre.

```
0xFF 0xAB 0x45 3 ->data

# Pushes D:FFAB45 onto the stack
```

***

### `->hex`

Convertit un nombre en chaîne au format hexadécimal.

```
255 ->hex 

# Pushes "FF" onto the stack
```

***

### `hex->`

Convertit une chaîne au format hexadécimal en nombre.

```
"FF" hex->

# Pushes the number 255 onto the stack
```

***

### `->bin`

Convertit un nombre en objet binaire.

```
278 ->bin

# Pushes B:100010110 onto the stack
```

***

### `->bin8`

Convertit un nombre en objet binaire de 8 bits.

```
278 ->bin8 # Pushes B:100010110 onto the stack
```

***

### `->bin16`

Convertit un nombre en objet binaire de 16 bits.

```
278 ->bin16 # Pushes B:0000000100010110 onto the stack
```

***

### `->bin32`

Convertit un nombre en objet binaire de 32 bits.

```
278 ->bin32 # Pushes B:00000000000000000000000100010110 onto the stack
```

***

### `->bin48`

Convertit un nombre en objet binaire de 48 bits.

```
278 ->bin48 # Pushes B:000000000000000000000000000000000000000100010110 onto the stack
```

***

### `->bin64`

Convertit un nombre en objet binaire de 64 bits.

```
278 ->bin64 # Pushes B:0000000000000000000000000000000000000000000000000000000100010110 onto the stack
```

***

### `->upper`

Convertit une chaîne en majuscules.

***

### `->lower`

Convertit une chaîne en minuscules.

***

### `->function`

Convertit une liste ou une chaîne en fonction.

```
( 2 2 + ) ->function
# Pushes « 2 2 + » onto the stack

" 2 2 + " ->function
# Pushes « 2 2 + » onto the stack
```

***

### `->primitive`

Convertit une chaîne en primitive **MOGWAI**.

> Attention, la primitive est placée sur la pile et n'est pas automatiquement exécutée. Pour l'exécuter, vous devez utiliser la fonction eval.

***

### `->code`

Convertit une liste ou une chaîne en bloc de code. Le bloc de code n'est pas exécuté, il est simplement poussé sur la pile. Pour l'exécuter, vous devez utiliser la fonction eval.

```
( 2 2 + ) ->code
# Pushes { 2 2 + } onto the stack

" 2 2 + " ->code
# Pushes { 2 2 + } onto the stack
```

***

### `->u8`

Convertit un nombre en entier non signé 8 bits.
Le résultat est retourné sous forme de data.

***

### `->i8`

Convertit un nombre en entier signé 8 bits.
Le résultat est retourné sous forme de data.

***

### `->u16`

Convertit un nombre en entier non signé 16 bits.
Le résultat est retourné sous forme de data.

***

### `->i16`

Convertit un nombre en entier signé 16 bits.
Le résultat est retourné sous forme de data.

***

### `->u32`

Convertit un nombre en entier non signé 32 bits.
Le résultat est retourné sous forme de data.

***

### `->i32`

Convertit un nombre en entier signé 32 bits.
Le résultat est retourné sous forme de data.

***

### `->u64`

Convertit un nombre en entier non signé 64 bits.
Le résultat est retourné sous forme de data.

***

### `->i64`

Convertit un nombre en entier signé 64 bits.
Le résultat est retourné sous forme de data.

***

### `->dataLE8` / `->dataLE16` / `->dataLE24` / `->dataLE32` / `->dataLE48` / `->dataLE64`

Convertit un nombre en DATA en ordre d'octets **Little Endian**, avec la taille spécifiée en bits.

```
42 ->dataLE32   # → D:2A000000
42 ->dataLE16   # → D:2A00
42 ->dataLE48   # → D:2A0000000000
```

Si la valeur est trop grande pour la taille demandée, les octets de poids fort sont silencieusement tronqués.

***

### `->dataBE8` / `->dataBE16` / `->dataBE24` / `->dataBE32` / `->dataBE48` / `->dataBE64`

Convertit un nombre en DATA en ordre d'octets **Big Endian**, avec la taille spécifiée en bits.

```
42 ->dataBE32   # → D:0000002A
42 ->dataBE16   # → D:002A
42 ->dataBE48   # → D:0000000000002A
```

Si la valeur est trop grande pour la taille demandée, les octets de poids fort sont silencieusement tronqués.

***

### `dataLE8->` / `dataLE16->` / `dataLE24->` / `dataLE32->` / `dataLE48->` / `dataLE64->`

Convertit un DATA en nombre, en interprétant les octets en ordre **Little Endian**, avec la taille spécifiée en bits.

```
D:2A000000 dataLE32->   # -> 42
D:2A00 dataLE16->       # -> 42
```

***

### `dataBE8->` / `dataBE16->` / `dataBE24->` / `dataBE32->` / `dataBE48->` / `dataBE64->`

Convertit un DATA en nombre, en interprétant les octets en ordre **Big Endian**, avec la taille spécifiée en bits.

```
D:0000002A dataBE32->   # -> 42
D:002A dataBE16->       # -> 42
```

***

### `->dataLE` / `->dataBE`

Variantes à taille dynamique de `->dataLEx` / `->dataBEx`. La taille (en bits) est prise sur la pile avec le nombre.

Tailles supportées : 8, 16, 24, 32, 48, 64. Toute autre valeur lève une `BadArgumentTypeError`.

```
42 32 ->dataLE   # -> D:2A000000
42 32 ->dataBE   # -> D:0000002A
```

***

### `dataLE->` / `dataBE->`

Variantes à taille dynamique de `dataLEx->` / `dataBEx->`. La taille (en bits) est prise sur la pile avec le DATA.

Tailles supportées : 8, 16, 24, 32, 48, 64. Toute autre valeur lève une `BadArgumentTypeError`.

```
D:2A000000 32 dataLE->   # -> 42
D:0000002A 32 dataBE->   # -> 42
```

***

### `->dataLE32F` / `->dataBE32F` / `->dataLE64F` / `->dataBE64F`

Convertit un nombre à virgule flottante en DATA selon la norme **IEEE 754**, dans l'ordre d'octets et la taille spécifiés.

- Les variantes `32F` utilisent la simple précision (4 octets).
- Les variantes `64F` utilisent la double précision (8 octets).

```
1.0 ->dataLE32F   # -> D:0000803F
1.0 ->dataBE32F   # -> D:3F800000
1.0 ->dataLE64F   # -> D:000000000000F03F
1.0 ->dataBE64F   # -> D:3FF0000000000000
```

***

### `dataLE32F->` / `dataBE32F->` / `dataLE64F->` / `dataBE64F->`

Convertit un DATA en nombre à virgule flottante selon la norme **IEEE 754**, en interprétant les octets dans l'ordre et la taille spécifiés.

- Les variantes `32F` attendent au moins 4 octets.
- Les variantes `64F` attendent au moins 8 octets.

Si le DATA est trop petit, une `BadArgumentValueError` est levée.

```
D:0000803F dataLE32F->   # -> 1.0
D:3F800000 dataBE32F->   # -> 1.0
D:000000000000F03F dataLE64F->   # -> 1.0
D:3FF0000000000000 dataBE64F->   # -> 1.0
```

***

### `utf8->`

Convertit un data en chaîne encodée en UTF-8.

***

### `->utf8`

Convertit une chaîne en data encodé en UTF-8.

***

### `ascii7->`

Convertit un data en chaîne encodée en ASCII 7 bits.

***

### `->ascii7`

Convertit une chaîne en data encodé en ASCII 7 bits.

***

### `ascii->`

Convertit un data en chaîne encodée en ASCII 8 bits.

### `->ascii`

Convertit une chaîne en data encodé en ASCII 8 bits.

***

### `->base64`

Convertit un data en chaîne encodée en base 64.

***

### `base64->`

Convertit une chaîne encodée en base 64 en data.

***

### `->md5`

Retourne le hash md5 d'un data.
Le hash est fourni sous forme de data.

***

### `->sha1`

Retourne le hash sha1 d'un data.
Le hash est fourni sous forme de data.

***

### `->sha256`

Retourne le hash sha256 d'un data.
Le hash est fourni sous forme de data.

***

### `->sha512`

Retourne le hash sha512 d'un data.
Le hash est fourni sous forme de data.

***

### `>>` et `<<`

Effectue un décalage de bits sur un nombre ou un objet binaire.
Le décalage est passé en paramètre.
`>>` décale les bits vers la droite, `<<` vers la gauche.

```
500 2 >>
# Pushes 125 onto the stack

B:01101111 2 >>
# Pushes B:00011011 onto the stack

B:01101111 2 <<
# Pushes B:10111100 onto the stack
```

***

### `~`

Inverse chaque bit d'un nombre passé en paramètre.

***

### `&`

ET binaire entre 2 nombres passés en paramètres.

***

### `|`

OU binaire entre 2 nombres passés en paramètres.

***

### `^`

XOR binaire entre 2 nombres passés en paramètres.

***

### `up`

Met à 1 un bit particulier d'un objet binaire.

```
B:110001 2 up
# Pushes BIN:110101 onto the stack
```

***

### `down`

Met à 0 un bit particulier d'un objet binaire.

```
B:110101 2 down
# Pushes BIN:110001 onto the stack
```

***

### `bit?`

Retourne `true` si le bit à la position spécifiée d'un objet binaire est à 1, `false` sinon. La position est basée sur zéro, en partant du bit le plus à droite.

```
B:110011 0 bit?
# Pushes true onto the stack (rightmost bit is 1)

B:110011 1 bit?
# Pushes true onto the stack

B:110011 2 bit?
# Pushes false onto the stack
```

***

### `sin`

Retourne le sinus d'un angle passé en paramètre. L'angle est en radians.

***

### `cos`

Retourne le cosinus d'un angle passé en paramètre. L'angle est en radians.

***

### `tan`

Retourne la tangente d'un angle passé en paramètre. L'angle est en radians.

***

### `asin`

Retourne l'angle en radians dont le sinus est passé en paramètre.

***

### `acos`

Retourne l'angle en radians dont le cosinus est passé en paramètre.

***

### `atan`

Retourne l'angle en radians dont la tangente est passée en paramètre.

### `PI`

Retourne le nombre PI.

***

### `->deg`

Retourne l'angle en degrés d'un angle en radians passé en paramètre.

```
PI 3 / ->deg
# Pushes 60 onto the stack
```

***

### `->rad`

Retourne l'angle en radians d'un angle en degrés passé en paramètre.

```
60 ->rad
# Pushes 1.0471975511965976 onto the stack
```

***

### `abs`

Retourne la valeur absolue du nombre passé en paramètre.

***

### `sqrt`

Retourne la racine carrée du nombre passé en paramètre.

***

### `floor`

Retourne la plus grande valeur entière inférieure ou égale au nombre passé en paramètre.

***

### `ceil`

Retourne la plus petite valeur entière supérieure ou égale au nombre passé en paramètre.

***

### `pow`

Retourne un nombre passé en paramètre élevé à la puissance passée en paramètre.

```
50 3 pow
# Pushes 125000 onto the stack
```

***

### `mod`

Retourne le reste de la division entière d'un nombre par un autre.

```
65 3 mod ?
# Pushes 2 onto the stack
```

***

### `min`

Retourne le plus petit nombre présent dans une liste.

> Seuls les nombres sont autorisés.

```
(56 34 9 27) min
# Pushes 9 onto the stack
```

***

### `max`

Retourne le plus grand nombre présent dans une liste.

> Seuls les nombres sont autorisés.

```
(1 56 34 9 27) max
# Pushes 56 onto the stack
```

***

### `sum`

Retourne la somme de tous les nombres présents dans une liste.

> Seuls les nombres sont autorisés.

```
(1 56 34 9 27) sum
# Pushes 127 onto the stack
```

***

### `average`

Retourne la moyenne de tous les nombres présents dans une liste.

> Seuls les nombres sont autorisés.

```
(1 56 34 9 27) average ?
# Pushes 25.4 onto the stack
```

***

### `console.locate`

Demande à l'hôte du runtime de positionner le curseur aux coordonnées passées en paramètre.
L'hôte n'est pas obligé de répondre.

> MOGWAI CLI gère cette fonction.

```
5 7 console.locate
```

***

### `console.cursor`

Retourne les coordonnées courantes du curseur sur l'écran hôte.
Si l'hôte ne gère pas cette information, les coordonnées 0 0 sont retournées.

> **MOGWAI CLI** gère cette fonction.

***

### `console.setForgroundColor`

Demande à l'hôte de changer la couleur d'affichage des caractères en passant le nom de la couleur à utiliser en paramètre.

Les couleurs définies dans **MOGWAI CLI** sont :

- 'black'
- 'blue'
- 'cyan'
- 'gray'
- 'green'
- 'magenta'
- 'red'
- 'white'
- 'yellow'

```
'red' console.setForegroundColor
```

***

### `console.setBackgroundColor`

Demande à l'hôte de changer la couleur de fond de l'écran.

> Dans **MOGWAI CLI**, utilise les mêmes couleurs que pour `console.setForegroundColor`.

***

### `console.getInputKey`

Demande à l'hôte de fournir le code de la touche actuellement pressée. -1 si aucune touche n'est actuellement pressée.

***

### `http.get`

Effectue un http get sur une uri en spécifiant les valeurs d'en-tête nécessaires.

Les paramètres sont passés via un enregistrement :

```
[
    uri: "https://api.github.com/orgs/dotnet/repos" 
    requestHeaders: [User-Agent: ".NET Foundation Repository Reporter" token: "XXXXX"]
] http.get
```

La réponse est un enregistrement contenant les clés suivantes :

| Clé           | Usage                                                                                             |
| ------------- | ------------------------------------------------------------------------------------------------- |
| `state:`      | `true` si tout s'est bien passé.                                                                  |
| `statusCode:` | Le code de statut réellement retourné (ex. 200).                                                  |
| `response:`   | Un data contenant la réponse.
En cas d'erreur, cette clé n'est pas présente dans la réponse.   |

### `http.post`

Effectue un http post sur une uri en spécifiant les en-têtes de requête, les en-têtes de contenu et le contenu.

Tous les paramètres sont définis dans un enregistrement passé en paramètre :

```
[
    uri: "https://api.github.com/orgs/dotnet/repos" 
    requestHeaders: [ ]
    contentHeaders: [ ]
    content: DATA
]
```

Le contenu est de type data.

La réponse, un enregistrement, est formatée exactement comme celle de la fonction `http.get`.

***

### `->uri`

Compose une uri à partir d'un enregistrement dont les clés correspondent aux différentes parties d'une uri :

```
[
    url: "https://www.google.com" 
    path: "api/v0/login" 
    query: [id: "50" name: "DOE"]
] ->uri 

# Pushes "https://www.google.com:443/api/v0/login?id=50&name=DOE" onto the stack
```

***

### `->urlEncode`

Encode une chaîne URL passée en paramètre.

Cette fonction peut être utilisée pour encoder l'URL entière, y compris les valeurs de la chaîne de requête.
L'encodage URL convertit les caractères non autorisés dans une URL en équivalents entité-caractère.
Par exemple, lorsque les caractères < et > sont intégrés dans un bloc de texte à transmettre dans une URL, ils sont encodés en %3c et %3e.

***

### `process.start`

Démarre un processus.

Les informations du processus sont fournies via un enregistrement composé des clés suivantes :

| Clé                 | Usage                                                                      |
| ------------------- | -------------------------------------------------------------------------- |
| `filename:`         | Fichier à exécuter (ex. notepad.exe)                                       |
| `arguments:`        | Arguments à utiliser pour démarrer le processus.                           |
| `workingDirectory:` | Définit le répertoire courant du processus.                                |
| `wait:`             | Si `true`, attend la fin de l'exécution du processus avant de retourner.  |

> Seule la clé `filename:` est obligatoire.

```
[
    filename: "toto.exe" 
    arguments: "/u -K" 
    workingDirectory: "C:\...." 
    wait: true ] process.start
```

***

### `process.exec`

Lance un processus, capture sa sortie et retourne un record résultat.
Contrairement à `process.start`, `process.exec` attend toujours la fin du processus et capture `stdout` et `stderr`.

Les informations du processus sont fournies via un record composé des clés suivantes :

| Clé                 | Usage                                                                         |
| ------------------- | ----------------------------------------------------------------------------- |
| `filename:`         | Fichier à exécuter (ex. myservice.exe)                                        |
| `arguments:`        | Arguments à passer au processus.                                              |
| `workingDirectory:` | Définit le répertoire courant du processus.                                   |
| `input:`            | Chaîne optionnelle envoyée au processus via `stdin`. Omise si non nécessaire. |

> Seule la clé `filename:` est obligatoire.

Pousse un record résultat sur la pile :

| Clé       | Type   | Description                                          |
| --------- | ------ | ---------------------------------------------------- |
| `status:` | Nombre | Code de retour du processus (0 = succès)             |
| `output:` | Chaîne | Contenu écrit sur `stdout` par le processus          |
| `error:`  | Chaîne | Contenu écrit sur `stderr` par le processus          |

```
[filename: "myservice.exe" arguments: "--mode calc" input: "42"] process.exec -> 'r'

r status: get -> 'code'
r output: get -> 'resultat'
r error:  get -> 'err'

if (code 0 ==) then
{
    "Résultat : {! resultat}" eval ?
}
else
{
    "Erreur : {! err}" eval ?
}
```

***

## FONCTIONS DE DÉBOGAGE (utilisées avec MOGWAI STUDIO)

### `debug.write`

Demande à l'hôte et à **MOGWAI STUDIO** (si connecté) d'afficher un message dans la console de débogage.

```
"Debug message" debug.write
```

***

### `debug.clear`

Demande à l'hôte et à **MOGWAI STUDIO** (si connecté) d'effacer l'écran de débogage.

***

### `debug.halt` ou `¤`

Effectue une pause. Correspond à un point d'arrêt.

Le programme doit être démarré en mode debug pour que le point d'arrêt soit pris en compte.
Quand l'exécution atteint cette instruction, le runtime se met en pause.
Il est alors possible d'avancer pas à pas si nécessaire.

```
1 10 for 'i' do
{
    i ?
    100 wait

    # Place a breakpoint here
    debug.halt
}
```

***

### `debug.tron`

Active le traçage. La durée entre chaque instruction est définie en paramètre en millisecondes.
Si **MOGWAI STUDIO** est connecté, il affiche l'instruction en cours d'exécution en temps réel.

```
250 debug.tron
```

***

### `debug.troff`

Désactive le traçage.

***

## FONCTIONS DE GESTION DU TEMPS

### `now`

Retourne la date courante de votre machine sous forme d'un nombre représentant le nombre d'intervalles de 100 nanosecondes écoulées depuis minuit, le 1er janvier 0001.

Par exemple, le nombre 6.389664359647076E+17 correspond à la date 21/10/2025 à 11:39:56

***

### `->date`

Convertit une date numérique en composantes de date et d'heure.

Cette fonction retourne un enregistrement composé des clés suivantes :

| Clé          | Usage                                                          |
| ------------ | -------------------------------------------------------------- |
| `day:`       | Jour.                                                          |
| `month:`     | Mois.                                                          |
| `year:`      | Année.                                                         |
| `hour:`      | Heures.                                                        |
| `minute:`    | Minutes.                                                       |
| `second:`    | Secondes.                                                      |
| `dayOfYear:` | Numéro du jour dans l'année.                                   |
| `dayOfWeek:` | Numéro du jour dans la semaine.
(Dimanche=0, Lundi=1, …, Samedi=6) |

```
now ->date

# If today is 21/10/2025 at 11:51:29
# Pushes [day: 21 month: 10 year: 2025 hour: 11 minute: 51 second: 29 dayOfYear: 294 dayOfWeek: 2] onto the stack
```

***

### `date->`

Convertit des composantes de date et d'heure en date numérique. L'enregistrement passé en paramètre contient les mêmes clés que l'enregistrement retourné par la fonction `->date`.

```
[day: 21 month: 10 year: 2025 hour: 11 minute: 51 second: 29] date->
# Pushes 6.38966438969E+17 onto the stack
```

***

### `->duration`

Retourne une durée sous forme d'un enregistrement composé des clés suivantes :

| Clé        | Usage                            |
| ---------- | -------------------------------- |
| `days:`    | Nombre de jours écoulés.         |
| `hours:`   | Nombre d'heures écoulées.        |
| `minutes:` | Nombre de minutes écoulées.      |
| `seconds:` | Nombre de secondes écoulées.     |
| `ms:`      | Nombre de millisecondes écoulées.|

En général, pour calculer le temps écoulé entre 2 moments, vous pouvez stocker le `now` au début, puis à la fin soustraire le `now` de départ du `now` courant, puis utiliser la fonction `->duration` pour obtenir le temps écoulé entre ces 2 moments.

```
now 2500 wait now - abs ->duration

# For a total duration of 2 seconds and 507 milliseconds
# Pushes [days: 0 hours: 0 minutes: 0 seconds: 2 ms: 507] onto the stack
```

***

### `duration->`

Convertit un enregistrement de durée (voir `->duration`) en nombre de millisecondes.

```
[days: 0 hours: 0 minutes: 0 seconds: 2 ms: 507] duration->
# Pushes 25070000 onto the stack
```

***

### `->durations`

Convertit un nombre de millisecondes en liste de durées dans différentes unités (ms, secondes, minutes, heures, jours).

```
25070000 ->durations
# Pushes [totalDays: 2.9016203703703704E-05 totalHours: 0.0006963888888888889 totalMinutes: 0.04178333333333333 totalSeconds: 2.507 totalMs: 2507]) onto the stack
```

***

## FONCTIONS DE GESTION DES TÂCHES

### `task.wait`

Attend la fin de l'exécution de la tâche dont le nom est passé en paramètre avant de retourner.

***

### `task.isRunning`

Retourne true si la tâche enfant dont le nom est passé en paramètre est en cours d'exécution.

***

### `task.stop`

Arrête la tâche dont le nom est passé en paramètre. La tâche est arrêtée dès que possible, mais ce n'est pas un arrêt immédiat.

***

### `task.purge`

Supprime la tâche dont le nom est passé en paramètre.

***

### `task.list`

Retourne la liste des noms de toutes les tâches enfants existantes.

***

### `task.setResult`

Permet à une tâche enfant de stocker son résultat. Cette fonction ne peut être utilisée que depuis le code d'une tâche enfant. Le résultat peut être de n'importe quel type géré par **MOGWAI**.

```
"MyResult" task.setResult
54 task.setResult
```

***

### `task.result`

Retourne le résultat de la tâche enfant dont le nom est passé en paramètre. Par défaut, le résultat a la valeur `null`.

***

### `task.name`

Retourne le nom de la tâche enfant.

> Cette fonction ne peut être utilisée que depuis le code d'une tâche enfant.

***

### `task.join`

Attend la fin de toutes les tâches listées en paramètre avant de retourner.

```
('T1' 'T2' 'T3') task.join
```

***

### `task.publish`

Permet à une tâche enfant de publier (envoyer) une valeur à sa tâche parente.
La valeur publiée peut être de n'importe quel type géré par **MOGWAI**.

> Cette fonction ne peut être utilisée que depuis le code d'une tâche enfant.

```
"MyValue" task.publish

2345 task.publish
```

***

## FONCTIONS DE GESTION DES ÉVÉNEMENTS

### `event.purge`

Supprime la gestion d'un événement dont le nom est passé en paramètre.

***

### `event.list`

Retourne la liste de tous les événements déclarés en cours de traitement.

***

### `event.fire`

Déclenche un événement vers le runtime.
Passer en paramètres le nom de l'événement, un objet qui accompagne l'événement et sera récupéré via la variable locale `eventData` dans le code de l'événement.

```
'MyEvent' "Hello" event.fire
```

***

## FONCTIONS DE GESTION DES TIMERS

### `timer.start`

Démarre le timer dont le nom est passé en paramètre.

***

### `timer.stop`

Arrête le timer dont le nom est passé en paramètre.

***

### `timer.purge`

Supprime le timer dont le nom est passé en paramètre.

***

#### `timer.state`

Retourne `true` si le timer est en cours d'exécution.

***

### `timer.list`

Retourne la liste de tous les timers déclarés quel que soit leur état (en cours d'exécution ou arrêté).

***

### `DI`

Suspend le déclenchement de tous les timers et événements.

> Attention, ils sont mis en attente et seront exécutés lorsque les interruptions seront réactivées.

***

### `EI`

Autorise le déclenchement des timers et événements.

***

## FONCTIONS DE GESTION DES FICHIERS

La version 8 de **MOGWAI** introduit un système de gestion de fichiers entièrement repensé utilisant une approche conventionnelle par chemins au lieu du système par nœuds des versions précédentes.

### Gestion des chemins

### `path.programs`

Retourne le chemin du dossier standard des programmes.

```
path.programs ?
# Returns: "C:\Users\Username\Documents\MOGWAI.8\Programs"
```

***

### `path.files`

Retourne le chemin du dossier standard des fichiers.

```
path.files ?
# Returns: "C:\Users\Username\Documents\MOGWAI.8\Files"
```

***

### `path.usings`

Retourne le chemin du dossier standard des bibliothèques d'extension.

```
path.usings ?
# Returns: "C:\Users\Username\Documents\MOGWAI.8\Usings"
```

***

### `path.desktop`

Retourne le dossier bureau de l'utilisateur courant.

***

### `path.documents`

Retourne le dossier documents de l'utilisateur courant.

***

### `path.music`

Retourne le dossier où sont stockés les fichiers musicaux de l'utilisateur courant.

***

### `path.videos`

Retourne le dossier où sont stockées les vidéos de l'utilisateur courant.

***

### `path.pictures`

Retourne le dossier où sont stockées les images de l'utilisateur courant.

***

### `path.programData`

Retourne le dossier système 'ProgramData'.

***

### `path.tempDirectory`

Retourne le dossier des fichiers temporaires.

***

### `path.tempFilename`

Retourne un chemin complet vers un nouveau fichier temporaire créé par le système.

***

### `path.make`

Génère un chemin de fichier ou de dossier à partir d'une liste de segments.

Passer une liste de segments de chemin en paramètre. La liste peut utiliser l'auto-évaluation avec le caractère `!`.

```
(! path.files "data.txt") path.make
# Returns: "C:\Users\Username\Documents\MOGWAI.8\Files\data.txt"

(path.files "MyFolder" "report.txt") eval path.make
```

***

### `path.setPrograms`

Personnalise le chemin du dossier par défaut des programmes.

```
"C:\MyPrograms" path.setPrograms
```

***

### `path.setFiles`

Personnalise le chemin du dossier par défaut des fichiers.

```
"D:\MyData" path.setFiles
```

***

### `path.setUsings`

Personnalise le chemin du dossier par défaut des bibliothèques d'extension.

```
"C:\MyLibraries" path.setUsings
```

***

### Gestion des dossiers

### `dir.exists`

Retourne `true` si le dossier existe au chemin spécifié.

```
"C:\Temp" dir.exists
```

***

### `dir.create`

Crée un nouveau dossier au chemin spécifié. Crée les répertoires parents de manière récursive si nécessaire.

```
"C:\Temp\MyFolder\SubFolder" dir.create
```

***

### `dir.purge`

Supprime un dossier et tout son contenu au chemin spécifié.

```
"C:\Temp\MyFolder" dir.purge
```

***

### `dir.rename`

Renomme un dossier. Passer l'ancien chemin et le nouveau chemin en paramètres.

```
"C:\Temp\OldName" "C:\Temp\NewName" dir.rename
```

***

### `dir.current`

Retourne le chemin du dossier de travail courant.

```
dir.current ?
# Returns: "C:\Projects"
```

***

### `dir.setCurrent`

Définit le dossier de travail courant.

```
"C:\Projects" dir.setCurrent
```

***

### `dir.directories`

Retourne la liste des sous-dossiers du dossier spécifié en chemin.

```
"C:\Temp" dir.directories
# Returns: ("Folder1" "Folder2" "Folder3")

path.files dir.directories
```

***

### `dir.files`

Retourne la liste des fichiers du dossier spécifié en chemin.

```
"C:\Temp" dir.files
# Returns: ("file1.txt" "file2.dat" "report.pdf")

path.files dir.files
```

***

### Gestion des fichiers — Lecture/Écriture complète

### `file.data.read`

Lit tout le contenu binaire d'un fichier en une seule fois et le retourne sous forme de DATA.

Passer le chemin complet du fichier en paramètre.

```
"C:\data.bin" file.data.read
(! path.files "image.png") path.make file.data.read
```

***

### `file.data.write`

Écrit des données binaires complètes dans un fichier.

Passer le chemin complet du fichier et le DATA en paramètres.

```
"C:\MyFile.bin" DATA:FF45ABEA23 file.data.write
# Writes bytes 0xFF, 0x45, 0xAB, 0xEA and 0x23 to the file.

imageData (! path.files "copy.png") path.make file.data.write
```

***

### Gestion des fichiers — Opérations séquentielles avec handles

**Un handle est une chaîne** représentant l'identifiant hexadécimal unique du flux de fichier ouvert (ex. « A3F5B2C8 »). Ce handle doit être conservé pour toutes les opérations ultérieures sur le fichier.

### `file.open`

Ouvre un fichier en lecture et retourne un handle.

```
"data.txt" file.open -> 'handle'
(! path.files "report.txt") path.make file.open -> 'h'
```

***

### `file.create`

Ouvre un fichier en écriture (efface le fichier s'il existe) et retourne un handle.

```
"report.txt" file.create -> 'handle'
(! path.files "output.txt") path.make file.create -> 'h'
```

***

### `file.append`

Ouvre un fichier en écriture à la fin (conserve le contenu existant) et retourne un handle.

Utilisé pour les fichiers journaux ou pour ajouter du contenu à des fichiers existants.

```
"log.txt" file.append -> 'handle'
(! path.files "debug.log") path.make file.append -> 'h'
```

***

### `file.read`

Lit jusqu'à `size` octets depuis un fichier ouvert et retourne un DATA.

Passer le handle et la taille en paramètres.

```
handle 1024 file.read
# Reads up to 1024 bytes from the file
```

***

### `file.readLine`

Lit une ligne complète (terminée par `
` ou `
`) depuis un fichier ouvert et retourne un DATA.

Passer le handle en paramètre.

```
handle file.readLine
# Returns the line as DATA (must be converted to string with utf8->, ascii->, etc.)

handle file.readLine utf8-> -> 'line'
```

***

### `file.write`

Écrit des données dans un fichier ouvert. **N'ajoute pas** automatiquement de saut de ligne.

Passer le DATA et le handle en paramètres. Pour écrire des lignes, ajouter manuellement les octets de saut de ligne (`D:0D0A` pour Windows, `D:0A` pour Unix/Linux).

```
"Hello" ->utf8 D:0D0A + handle file.write
# Writes "Hello" with a Windows line break

"Line without break" ->utf8 handle file.write
```

***

### `file.size`

Retourne la taille totale (en octets) d'un fichier ouvert en lecture.

Passer le handle en paramètre.

```
handle file.size -> 'fileSize'
"File size: {! fileSize} bytes" eval ?
```

***

### `file.eof`

Retourne `true` si la fin du fichier ouvert en lecture est atteinte.

Passer le handle en paramètre. Utilisé dans les boucles pour lire les fichiers de manière séquentielle.

```
while (handle file.eof not) do
{
    handle file.readLine utf8-> ?
}
```

***

### `file.close`

Ferme un fichier ouvert. **Fermez toujours les fichiers après utilisation !**

Passer le handle en paramètre.

```
handle file.close
```

***

### Manipulation de fichiers

### `file.exists`

Retourne `true` si le fichier existe au chemin spécifié, `false` sinon.

```
"data.txt" file.exists
(! path.files "config.txt") path.make file.exists
```

***

### `file.info`

Retourne un enregistrement contenant toutes les métadonnées du fichier.

Passer le chemin du fichier en paramètre.

L'enregistrement contient les clés suivantes :

| Clé               | Type    | Description                                   |
| ----------------- | ------- | --------------------------------------------- |
| `name:`           | String  | Nom du fichier avec extension                 |
| `fullName:`       | String  | Chemin absolu complet du fichier              |
| `directoryName:`  | String  | Chemin du dossier contenant le fichier        |
| `extension:`      | String  | Extension du fichier                          |
| `modifiedTime:`   | Number  | Date de dernière modification (ticks .NET)    |
| `lastAccessTime:` | Number  | Date de dernier accès (ticks .NET)            |
| `length:`         | Number  | Taille du fichier en octets                   |
| `isReadOnly:`     | Boolean | Fichier en lecture seule                      |
| `isArchive:`      | Boolean | Attribut archive (Windows)                    |
| `isHidden:`       | Boolean | Fichier caché                                 |
| `isSystem:`       | Boolean | Fichier système                               |

```
"data.txt" file.info -> 'info'
info length: get -> 'size'
"File size: {! size} bytes" eval ?

# Convert timestamp to readable date
info modifiedTime: get ->date -> 'dateModif'
```

**Note** : Les horodatages sont en ticks .NET (nombre d'intervalles de 100 nanosecondes depuis le 01/01/0001). Utilisez la fonction `->date` pour convertir en un enregistrement de date avec `day:`, `month:`, `year:`, etc.

**Important** : Si le fichier n'existe pas, `file.info` lève une erreur. Utilisez `file.exists` pour vérifier l'existence avant d'appeler `file.info`.

***

### `file.copy`

Copie un fichier. Passer le chemin source et le chemin destination en paramètres.

```
"source.txt" "dest.txt" file.copy
(! path.files "original.txt") path.make 
(! path.files "copy.txt") path.make 
file.copy
```

***

### `file.rename`

Renomme un fichier. Passer l'ancien chemin et le nouveau chemin en paramètres.

```
"old.txt" "new.txt" file.rename
(! path.files "temp.txt") path.make
(! path.files "backup.txt") path.make
file.rename
```

***

### `file.purge`

Supprime un fichier au chemin spécifié.

```
"temp.txt" file.purge
(! path.files "old_data.bin") path.make file.purge
```

***

### Fonctions de conversion de données

Les fonctions de lecture de fichiers texte (`file.readLine`, `file.read`) retournent des DATA (tableaux d'octets) qui doivent être convertis en chaînes selon l'encodage du fichier. De même, pour écrire du texte dans un fichier, les chaînes doivent d'abord être converties en DATA.

### `utf8->`

Convertit un DATA en chaîne avec l'encodage UTF-8.

```
data utf8->
handle file.readLine utf8-> -> 'line'
```

***

### `ascii->`

Convertit un DATA en chaîne avec l'encodage ASCII.

```
data ascii->
handle file.readLine ascii-> -> 'line'
```

***

### `ascii7->`

Convertit un DATA en chaîne avec l'encodage ASCII 7 bits.

```
data ascii7->
```

***

### `->utf8`

Convertit une chaîne en DATA avec l'encodage UTF-8.

Utilisé avant l'écriture de texte dans un fichier.

```
"Hello" ->utf8
"Français: éèêë" ->utf8 D:0D0A + handle file.write
```

***

### `->ascii`

Convertit une chaîne en DATA avec l'encodage ASCII.

```
"Hello" ->ascii
"English: Hello" ->ascii D:0D0A + handle file.write
```

***

### `->ascii7`

Convertit une chaîne en DATA avec l'encodage ASCII 7 bits.

```
"Basic" ->ascii7
"ABC123" ->ascii7 D:0D0A + handle file.write
```

***

### Constantes de saut de ligne

Lors de l'écriture de fichiers texte, les sauts de ligne doivent être ajoutés manuellement :

- `D:0D0A` - Saut de ligne Windows (CR LF : Retour chariot + Saut de ligne)
- `D:0A` - Saut de ligne Unix/Linux/Mac (LF : Saut de ligne uniquement)

```
"My line" ->utf8 D:0D0A + handle file.write
```

L'opérateur `+` concatène les DATA pour créer un seul tableau d'octets.

***

## FONCTIONS DE GESTION DES CLASSES

### `class`

Mot-clé syntaxique utilisé pour définir une classe. Doit être suivi du nom de la classe sous forme de chaîne, du mot-clé `do`, et d'un bloc contenant les sections `private:` et `public:`.

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

Dans une section, un nom suivi d'un sigil de type déclare une **propriété** (initialisée à `empty`). Un nom suivi d'un bloc de code déclare une **méthode**.

La section `private:` n'est accessible que depuis l'intérieur de la classe. La section `public:` est accessible depuis l'extérieur.

Deux noms de méthodes spéciaux sont réservés comme hooks de cycle de vie optionnels : `onInit:` (appelé automatiquement lors de `new` s'il est défini) et `onFree:` (appelé automatiquement lors de `free` s'il est défini). Ils peuvent être placés dans l'une ou l'autre section.

***

### `new`

Crée une nouvelle instance d'une classe. Si la classe définit une méthode `onInit:`, elle est appelée automatiquement avec toute valeur présente sur la pile. `onInit:` est optionnel.

```
# Without parameters
'Counter' new -> '$C'

# With named parameters (when onInit: uses ->params)
[step: 10] 'Counter' new -> '$C'
```

Chaque instance se voit attribuer un handle interne unique noté `§N` (ex. `§1`, `§2`). Ce numéro n'est jamais réutilisé pendant la durée de vie du moteur.

***

### `free`

Détruit une instance de classe. Si la classe définit une méthode `onFree:`, elle est appelée automatiquement avant la destruction.

```
$C free
```

Après `free`, toute variable contenant encore une référence à l'instance détruite devient invalide. Toute tentative de l'utiliser lève une erreur.

***

### `isAlive`

Retourne `true` si la référence d'instance présente sur la pile est toujours valide (c'est-à-dire que l'instance n'a pas été libérée), `false` sinon.

```
$U1 isAlive   # → true ou false

if ($U1 isAlive) then
{
    $U1->display:
}
```

`isAlive` effectue une recherche O(1) dans le registre des instances. Elle ne lève jamais d'erreur lorsqu'elle est appelée sur une référence d'instance — en revanche, lui passer une valeur qui n'est pas une référence d'instance lève MW.21 (bad argument type).

***

### `self`

Disponible à l'intérieur de toute méthode de classe. Pousse la référence à l'instance courante sur la pile.

```
display:
{
    "USER={! self}" eval ?
    self->name: ?
}
```

Utiliser `self` en dehors d'une méthode de classe lève une erreur.

***

### `className:` (propriété réservée)

Propriété publique réservée en lecture seule, automatiquement disponible sur chaque instance de classe. Retourne le nom de la classe à laquelle appartient l'instance.

```
$U1->className: ?   # → 'User'
```

Tenter d'écrire dans `className:`, ou de la déclarer dans une définition de classe, lève l'erreur MW.95 (propriété réservée).

***

### `alive`

Retourne une liste de toutes les références d'instances actuellement vivantes (`.objref`). Utile pour l'itération, le débogage ou le nettoyage.

```
alive ?
# → (§1 §2 §3 ...)
```

On peut filtrer par classe avec `foreach...filter` :

```
alive foreach 'item' filter { item->className: 'User' == } -> '$users'
```

Si aucune instance n'est en vie, retourne une liste vide `()`.

***

### `frame`

Retourne un record décrivant la structure complète d'une classe nommée — ses propriétés et méthodes publiques et privées.

```
'Counter' frame ?
# → [className: 'Counter' props: [value: .number] _props: [_step: .number] funcs: (onInit: increment: reset:) _funcs: ()]
```

Le record retourné contient les clés suivantes :

| Clé | Contenu |
|-----|---------|
| `className:` | Nom de la classe |
| `props:` | Propriétés publiques avec leur type déclaré |
| `_props:` | Propriétés privées avec leur type déclaré |
| `funcs:` | Noms des méthodes publiques |
| `_funcs:` | Noms des méthodes privées |

```
'Counter' frame -> '$F'
$F->className: ?
$F->props: ?
$F->_props: ?
$F->funcs: ?
$F->_funcs: ?
```

***
