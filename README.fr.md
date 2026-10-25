[English](README.md) | [Português (Brasil)](README.pt-BR.md) | [Español](README.es.md) | **Français** | [Deutsch](README.de.md) | [Русский](README.ru.md)

# Local Knights

[![License: MPL 2.0](https://img.shields.io/badge/License-MPL%202.0-brightgreen.svg)](LICENSE)
[![Download Latest Release](https://img.shields.io/github/v/release/urbanoanderson/local-knights?label=Latest%20Release)](https://github.com/urbanoanderson/local-knights/releases)
[![Parrainer](https://img.shields.io/github/sponsors/urbanoanderson?logo=githubsponsors&label=Sponsor)](https://github.com/sponsors/urbanoanderson)
![Platform: Windows](https://img.shields.io/badge/Platform-Windows%2010%2B-0078D6)
![Built with .NET 10](https://img.shields.io/badge/.NET-10-512BD4)

<p align="center">
	<img src="LocalKnights.WindowsDesktop/Resources/Icon/logo.png" alt="Logo de Local Knights" width="180">
</p>

<p align="center"><strong>Le logiciel libre de gestion des clubs d’échecs associatifs.</strong></p>

<p align="center">
	Local Knights est une application de bureau moderne conçue pour gérer les clubs d’échecs. Gérez les joueurs, enregistrez les parties, organisez les compétitions, générez les appariements et tenez les classements internes du club depuis une interface soignée.
</p>

## Pourquoi Local Knights ♟️

- Entièrement gratuit et libre sous licence MPL-2.0. Aucun abonnement, aucune publicité
- Une expérience de bureau entièrement locale, sans dépendance au nuage. Navigation claire, mises en page adaptatives, thèmes clair et sombre et prise en charge de plusieurs langues
- Les données du club sont conservées dans un seul fichier de base de données protégé par mot de passe. La sauvegarde et la restauration sont simples
- Idéal pour les clubs d’échecs associatifs, les groupes d’amis et les programmes scolaires qui ont besoin d’un outil plus structuré qu’un tableur et souhaitent un flux de travail gratuit et libre

## Fonctionnalités principales 🚀

### Gérez les membres du club 👥

- Inscrivez les joueurs et tenez des profils complets pour les membres du club
- Associez les profils des joueurs à [Chess.com](https://www.chess.com/), [Lichess](https://lichess.org/), la [FIDE](https://www.fide.com/) et d’autres organismes d’échecs
- Saisissez manuellement les classements ou récupérez-les auprès des plateformes externes compatibles lors de la création d’un joueur
- Gérez des classements distincts pour plusieurs cadences (Bullet, Blitz, Rapide et Classique)

### Organisez des compétitions 🏆

- Prise en charge des systèmes de classement [Glicko-2](https://en.wikipedia.org/wiki/Glicko_rating_system#Glicko-2_algorithm) et [Elo](https://en.wikipedia.org/wiki/Elo_rating_system). Mettez les classements à jour à mesure que les parties et les résultats des tournois sont enregistrés
- Enregistrez des parties individuelles (classées ou amicales) et conservez même leur notation [PNG](https://en.wikipedia.org/wiki/Portable_Game_Notation) si nécessaire
- Organisez des compétitions au [système suisse](https://en.wikipedia.org/wiki/Swiss-system_tournament), [toutes rondes](https://en.wikipedia.org/wiki/Round-robin_tournament), à [élimination directe](https://en.wikipedia.org/wiki/Single-elimination_tournament) et des matchs entre deux joueurs. Suivez les rondes, les appariements, les classements et les résultats au même endroit
- Générez des appariements suisses selon une logique approuvée par la FIDE grâce à la bibliothèque [Chesspairing](https://github.com/gnutterts/chesspairing/)
- Exportez les compétitions dans un format Excel facile à utiliser ou au format TRF officiel de la FIDE

### Une application de bureau moderne 💻

- Créée avec des technologies modernes et des écrans adaptatifs qui restent pratiques quelle que soit la taille de la fenêtre
- Conçue pour un usage quotidien, avec une navigation directe centrée sur les joueurs, les parties, les compétitions et les paramètres du club
- Thèmes clair et sombre adaptés aux différents environnements et préférences
- Prise en charge de la localisation dans plusieurs langues

### Conçue pour une maîtrise locale 🔒

Contrairement aux outils de club privilégiant le Web, Local Knights est conçu autour de la maîtrise locale des données.

- La base de données de votre club reste sur votre propre ordinateur. Aucun serveur hébergé n’est nécessaire
- La sauvegarde consiste simplement à copier un fichier `.db` chiffré
- La restauration est simple, portable et indépendante de toute infrastructure tierce

## Bien démarrer ▶️

La mise en service d’un club est volontairement simple.

1. Téléchargez la dernière version de l’application sous forme d’archive zip depuis notre page des [versions](https://github.com/urbanoanderson/local-knights/releases).
1. Extrayez le contenu de l’archive dans le dossier de votre choix. Vous pouvez également créer un raccourci vers l’exécutable « Local Knights.exe ».
1. Lancez l’application avec « Local Knights.exe ».
1. Au premier lancement, cliquez sur le bouton « + » et configurez le nom du club, les préférences de classement, le mot de passe, etc.
1. Connectez-vous à votre club avec le mot de passe créé et accédez au tableau de bord.
1. Inscrivez les joueurs individuellement avec le bouton Créer de la page Joueurs. Vous pouvez aussi importer en bloc des fichiers de joueurs au format CSV ([fichier d’importation d’exemple](Scripts/Seed/player-bulk-import-sample.csv)).
1. Créez des parties et des compétitions avec les joueurs inscrits depuis les pages et formulaires correspondants.
1. Amusez-vous et faites vivre les échecs dans votre communauté ! Les données du club seront stockées dans « C:\Users\<USERNAME>\AppData\Local\LocalKnights\Clubs ».

![Démonstration de Local Knights](Docs/Screenshots/demo.gif)

## Confidentialité et traitement des données ⚠️

Cette application est conçue dans le respect de la vie privée :

- **Stockage local uniquement**
  Toutes les données du club (joueurs, parties et compétitions) sont stockées localement sur l’ordinateur de l’utilisateur dans une base de données SQLite. L’application ne transmet, ne synchronise et ne partage aucune donnée avec des serveurs externes ou des tiers.

- **Aucune collecte de données par le développeur**
  Le développeur de ce logiciel ne collecte, ne consulte et ne traite aucune information personnelle saisie dans l’application.

- **Responsabilité des responsables de club**
  Les responsables et administrateurs de club qui utilisent ce logiciel doivent veiller au respect des lois applicables en matière de protection des données, notamment le Règlement général sur la protection des données (RGPD) de l’UE et la Lei Geral de Proteção de Dados (LGPD) du Brésil. Ils doivent notamment informer les joueurs de l’utilisation de leurs données, recueillir leur consentement lorsque cela est nécessaire et respecter leurs droits d’accès, de rectification et d’effacement.

- **Recommandations de sécurité**
  - Protégez l’accès à votre ordinateur et aux fichiers de base de données.
  - Sauvegardez régulièrement vos données de manière sécurisée.
  - Choisissez des mots de passe robustes pour protéger les bases de données réelles des clubs. Les mots de passe ne sont sauvegardés ni par Local Knights ni par des services tiers. Il est recommandé d’utiliser un gestionnaire de mots de passe pour ne pas perdre l’accès aux données du club.

**Avertissement :** Ce logiciel est fourni « en l’état ». Bien qu’il facilite la gestion locale des données, la responsabilité finale d’une utilisation licite et conforme des données personnelles incombe aux responsables et administrateurs du club.

## Informations pour les développeurs 🛠️

### Technologies utilisées ⚙️

- Application de bureau .NET 10 créée avec WPF et le modèle MVVM
- Stockage SQLite avec SQLCipher pour la protection par mot de passe
- Bibliothèque [Chesspairing](https://github.com/gnutterts/chesspairing/) pour générer les appariements conformément aux règles de la FIDE

### Gestion des versions 🔢

- Local Knights utilise le format de version `MAJOR.MINOR.REVISION`
- Les versions stables sont basées sur la branche `main`. Les préversions expérimentales sont basées sur la branche `dev`
- Les flux CI/CD compilent, testent, créent les artefacts et génèrent automatiquement les préversions et les versions

### Compiler, tester et exécuter depuis les sources 🧪

Le SDK .NET 10 est requis

```bash
dotnet restore

dotnet build ProjectSolution.slnx

dotnet test ProjectSolution.slnx

dotnet run --project LocalKnights.WindowsDesktop/LocalKnights.WindowsDesktop.csproj
```

### Créer un paquet Microsoft Store 📦

Sous Windows, avec les SDK .NET 10 et Windows installés, exécutez :

```powershell
.\Scripts\create-msix.ps1
```

Cette commande crée un paquet MSIX x64 non signé dans
`LocalKnights.WindowsDesktop\bin\Packages\LocalKnights_<version>_x64.msix` en utilisant
l’identité de paquet associée à Local Knights dans Partner Center. Téléversez ce fichier
dans la section **Packages** de la soumission ; Microsoft Store le signe après la soumission.
Incrémentez la version du projet avant de soumettre une mise à jour.

## Licence 📄

Local Knights est distribué sous la Mozilla Public License 2.0.

Consultez [LICENSE](LICENSE) pour lire le texte intégral de la licence.

Pour les composants tiers, les bibliothèques intégrées et les références d’implémentation externes, consultez [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

## Soutenir le projet 💖

Si Local Knights vous a permis, à vous ou à votre club, de passer moins de temps à vous battre avec des tableurs et plus de temps à mater vos amis, pensez à [parrainer le projet](https://github.com/sponsors/urbanoanderson). Même une petite contribution aide les cavaliers à rester en mouvement : les chevaux coûtent étonnamment cher. 🐴♟️
