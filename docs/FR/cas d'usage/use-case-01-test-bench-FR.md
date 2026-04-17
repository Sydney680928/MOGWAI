# MOGWAI en Production — Cas d'usage n°1
## Banc de test de cartes électroniques

---

## Le défi

Un fabricant produit deux lignes de produits, chacune construite à partir de deux cartes électroniques :

- Une **carte d'alimentation** gérant l'alimentation électrique.
- Une **carte de traitement** intégrant le CPU principal.

![MOGWAI](../../../images/img13.png)

Le produit 1 utilise **LoRaWAN + Bluetooth Low Energy**. Le produit 2 utilise **4G + Bluetooth Low Energy**. Les deux types de cartes sont testés individuellement et en tant que produits assemblés, ce qui donne **cinq scénarios de test distincts** — chacun comprenant des dizaines d'étapes de vérification couvrant le firmware, l'horloge temps réel, les entrées/sorties, les LED, les mesures de tension, et bien d'autres.

L'exigence technique était stricte :

> Une seule application PC doit gérer **tous les tests — présents et futurs** — sans jamais être modifiée ni recompilée. Les nouveaux tests doivent pouvoir être déployés à distance, chez les sous-traitants, sans toucher au logiciel hôte.

En d'autres termes : une **application figée, avec des tests vivants**.

---

## La solution : intégrer le runtime MOGWAI

La réponse a été d'intégrer le **runtime MOGWAI** dans l'application de banc de test WinForm .NET.

Au lieu de coder en dur chaque séquence de test en C#, chaque séquence est écrite sous forme de script MOGWAI. L'application PC devient un **hôte** pur : elle gère la communication USB avec le banc de test, expose des fonctions étendues au runtime (affichage, sons, interaction utilisateur), et se contente d'*exécuter* le script chargé.

Ajouter un nouveau test revient à déposer un nouveau dossier contenant ses fichiers MOGWAI. Pas une seule ligne de C# n'a besoin de changer.

![MOGWAI](../../../images/img12.png)
---

## Architecture

```
PC Application (WinForm .NET)
│
├── MOGWAI Runtime (embedded)
│   ├── banc.mog              ← shared library for all tests
│   │
│   ├── Power board test/
│   │   ├── main.mog          ← test sequence
│   │   └── config.mog        ← fixture-specific parameters
│   │
│   ├── Processing board (LoRaWAN)/
│   │   ├── main.mog
│   │   └── config.mog
│   │
│   └── ... (3 more tests)
│
└── Host-exposed functions
    (USB serial, UI, logging, sounds, JLink...)
```

`banc.mog` est l'épine dorsale : une bibliothèque partagée contenant toutes les fonctions communes à tous les tests — communication avec le banc, programmation de firmware via JLink, interaction avec le serveur, vérifications de conformité et gestion des erreurs.

À noter que le **protocole de communication entre le PC et les bancs de test a été écrit entièrement en MOGWAI** (`banc.mog`), et non en C#. Cela signifie que si le protocole du banc évolue, seul le script est mis à jour — l'application hôte reste intacte.

---

## Ce que MOGWAI orchestre

### Attente de la mise sous tension du banc

![MOGWAI](../../../images/img11.png)

Plutôt que d'utiliser un délai arbitraire, MOGWAI effectue une vraie mesure et attend une réponse valide de la part du banc de test :

```mogwai
to 'WAIT_FOR_POWER_ON' params [signal: .string] do
«
    false -> 'showWaitForPower'

    forever do
    {
        "AT+MEASURE:{! signal}" eval COM.cwrite
        [COM.mread timeout: 1000 expected: ("*")] -> 'r'

        if (r->state "Success" !=) then
        {
            [UI.showModal icon: "error"
                message: "Communication error with the test fixture!"
                buttons: ("STOP")] drop
            mogwai.exit
        }
        else
        {
            if (r->answers "Command fail" contains) then
            {
                if (showWaitForPower not) then
                {
                    true -> 'showWaitForPower'
                    [UI.show icon: "Warning"
                        message: "Please power the fixture..."]
                }
                1000 wait
            }
            else
            {
                if (showWaitForPower) then { UI.hide }
                break
            }
        }
    }
»
```

La boucle interroge le banc via la liaison série USB, affiche un message à l'opérateur si nécessaire, et se termine proprement dès que la mise sous tension est détectée — sans approximation, sans délai codé en dur.

---

### Programmation du firmware via la sonde JLink

MOGWAI pilote directement la sonde JLink pour programmer le microcontrôleur STM32WB55. Le script génère dynamiquement le fichier de commandes JLink, lance le processus externe et attend son code de sortie :

```mogwai
to 'JLINK_FW_PROGRAMMING' do
«
    (! "HOME" $JLINK_FOLDER) dir.enter

    (
        !
        "Erase 0x08000000 0x080B9FFF"
        "LoadFile {! $JLINK_FW_MASTER_FILE}"
        "Exit"
    ) "\r\n" eval join ->ascii

    $JLINK_SCRIPT_FILE file.write

    [
        !
        PROCESS.start
        filename: $JLINK_PROGRAM
        arguments: "-AutoConnect 1 -ExitOnError 1 -device STM32WB55RG
                    -if swd -speed 2000 -CommandFile {! $JLINK_SCRIPT_FILE}"
        workingDirectory: $JLINK_FOLDER
        wait: true
    ]

    trap { $JLINK_SCRIPT_FILE file.purge }
»
```

Le même mécanisme gère la programmation de la pile BLE et des octets d'option — trois fonctions indépendantes, chacune générant à la volée son propre script JLink à partir des variables du runtime.

---

### Vérification colorimétrique des LED

Le banc vérifie que les LED de signalisation émettent les bonnes couleurs. MOGWAI commence par calibrer le capteur (10 mesures moyennées), puis contrôle chaque mesure par rapport aux plages RGB attendues :

```mogwai
to 'GET_LED' params [name: .string timeout: .number save: .boolean
                     rMin: .number rMax: .number
                     gMin: .number gMax: .number
                     bMin: .number bMax: .number] do
«
    [COM.mwrite command: "AT+LED:?" timeout: 5000
        expected: ("*,*,*,*")] -> 'result'

    result (answers: 0) get "," split -> 'result'

    result 0 get hex-> $LED_INIT_R - -> 'r'
    result 1 get hex-> $LED_INIT_G - -> 'g'
    result 2 get hex-> $LED_INIT_B - -> 'b'

    r rMin >= r rMax <= and -> 'rTest'
    g gMin >= g gMax <= and -> 'gTest'
    b bMin >= b bMax <= and -> 'bTest'

    if (rTest gTest bTest and and not) then
    {
        "Non-conformant!" LOG.write
        EXECUTE_ON_ERROR_FUNCTION
    }
»
```

---

### Résilience serveur avec file d'attente locale

Lorsque le serveur est inaccessible, les résultats de test ne sont pas perdus. MOGWAI les sérialise dans des fichiers horodatés et les resoumet automatiquement dès que la connexion est rétablie :

```mogwai
# Server unavailable → queue locally
content "LB-{! year}{! month}{! day}-{! hour}{! minute}{! second}.mog"
    eval file.pack

false -> '$SERVER_LAST_KNOWN_STATE'
'EVENT_SERVER_ERROR' null event.host.fire
```

Un processus de synchronisation en arrière-plan vide la file d'attente dès que le serveur redevient disponible.

---

### Gestion des clés LoRaWAN

Pour les tests des produits LoRaWAN, MOGWAI gère un stock local de clés de provisionnement (DevEUI, AppEUI, AppKey), récupérées par lots depuis le serveur de l'entreprise et consommées une par une pendant les tests :

```mogwai
to 'SERVER_GET_NEXT_KEY' do
«
    # Load or refresh the local key file
    # Extract the first available key
    # Refill the pool automatically when running low (< 10 keys)
    
    r 0 get -> 'k'
    r 0 purge -> 'r'
    r "KEYS.DAT" file.pack

    if (r size 10 <) then
    {
        SERVER_FILL_KEY_FILE drop
    }

    k   # return the key
»
```

---

## Gestion flexible des erreurs

Chaque test peut définir son propre comportement en cas d'erreur grâce à un mécanisme de callback :

```mogwai
# Register a custom error handler
ON_ERROR
«
    false UI.progress.setVisible
    [UI.showModal icon: "Error"
        message: "Board rejected — please remove and retry."
        buttons: ("OK")] drop
    mogwai.reset
»

# Later in the test: any failing check calls EXECUTE_ON_ERROR_FUNCTION,
# which dispatches to the registered handler.
```

Si aucun gestionnaire personnalisé n'est enregistré, MOGWAI revient à un comportement sécurisé par défaut : afficher une boîte de dialogue d'erreur et terminer proprement.

---

## Résultats

| Indicateur | Valeur |
|---|---|
| Scénarios de test | 5 (carte d'alimentation, carte de traitement ×2, produit assemblé ×2) |
| Vérifications par test | Des dizaines (firmware, RTC, E/S, LED, tensions, BLE, LoRaWAN…) |
| Durée de test | 2 à 10 minutes par carte |
| En production depuis | Octobre 2025 |
| Volume projeté | Plusieurs milliers de cartes par an |
| Modifications de l'application hôte nécessaires pour ajouter un test | **Zéro** |

![MOGWAI](../../../images/img14.png)
---

## Points clés

**Séparation des responsabilités.** L'application hôte est une infrastructure générique. Toute la logique de test vit dans les scripts. Ce n'est pas qu'une préférence de conception — c'était une exigence absolue, et MOGWAI l'a rendue atteignable.

**La couche protocole est aussi un script.** Déplacer le protocole de communication USB du C# vers `banc.mog` était un choix délibéré. Si les bancs évoluent, seul le script change.

**Le déploiement est trivial.** Les tests nouveaux ou mis à jour sont distribués sous forme d'un dossier contenant des fichiers `.mog`. Pas d'installeur, pas de recompilation, pas d'intervention informatique chez les sous-traitants.

**MOGWAI STUDIO accélère le développement.** Le débogueur intégré permet aux ingénieurs d'avancer pas à pas dans des séquences de test de plusieurs minutes à vitesse maximale, de poser des points d'arrêt et d'inspecter la pile — réduisant considérablement le temps de mise au point des nouveaux tests.

---

*→ [MOGWAI sur GitHub](https://github.com/Sydney680928/MOGWAI)*
*→ [Essayer le Playground en ligne](https://sydney680928.github.io/MOGWAI/)*
