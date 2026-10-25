[English](README.md) | [Português (Brasil)](README.pt-BR.md) | [Español](README.es.md) | [Français](README.fr.md) | **Deutsch** | [Русский](README.ru.md)

# Local Knights

[![License: MPL 2.0](https://img.shields.io/badge/License-MPL%202.0-brightgreen.svg)](LICENSE)
[![Download Latest Release](https://img.shields.io/github/v/release/urbanoanderson/local-knights?label=Latest%20Release)](https://github.com/urbanoanderson/local-knights/releases)
[![Microsoft Store](https://img.shields.io/badge/Microsoft_Store-Download-0078D4?logo=microsoft)](https://apps.microsoft.com/detail/9p78z94q4nrt)
[![Sponsern](https://img.shields.io/github/sponsors/urbanoanderson?logo=githubsponsors&label=Sponsor)](https://github.com/sponsors/urbanoanderson)
![Platform: Windows](https://img.shields.io/badge/Platform-Windows%2010%2B-0078D6)
![Built with .NET 10](https://img.shields.io/badge/.NET-10-512BD4)

<p align="center">
	<img src="LocalKnights.WindowsDesktop/Resources/Icon/logo.png" alt="Logo von Local Knights" width="180">
</p>

<p align="center"><strong>Die quelloffene Verwaltungssoftware für gemeinschaftliche Schachvereine.</strong></p>

<p align="center">
	Local Knights ist eine moderne Desktop-App zur Führung von Schachvereinen. Verwalten Sie Spieler, erfassen Sie Partien, organisieren Sie Turniere, erstellen Sie Paarungen und pflegen Sie interne Vereinswertungen über eine durchdachte Oberfläche.
</p>

## Warum Local Knights ♟️

- Vollständig kostenlos und quelloffen unter der MPL-2.0-Lizenz. Keine Abonnements, keine Werbung
- Komplett lokale Desktop-Nutzung ohne Cloud-Abhängigkeit. Übersichtliche Navigation, responsive Layouts, helle und dunkle Designs sowie mehrsprachige Unterstützung
- Vereinsdaten werden in einer einzigen passwortgeschützten Datenbankdatei gespeichert. Sicherung und Wiederherstellung sind unkompliziert
- Ideal für gemeinschaftliche Schachvereine, Freundeskreise und Schulprogramme, die mehr Struktur als Tabellen benötigen und einen kostenlosen, quelloffenen Arbeitsablauf wünschen

## Hauptfunktionen 🚀

### Vereinsmitglieder verwalten 👥

- Spieler registrieren und vollständige Profile für Vereinsmitglieder pflegen
- Spielerprofile mit [Chess.com](https://www.chess.com/), [Lichess](https://lichess.org/), [FIDE](https://www.fide.com/) und anderen Schachorganisationen verknüpfen
- Wertungen bei der Spielerstellung manuell eingeben oder von unterstützten externen Plattformen abrufen
- Getrennte Mitgliederwertungen für verschiedene Bedenkzeiten führen (Bullet, Blitz, Schnell- und Standardschach)

### Wettkampfschach organisieren 🏆

- Unterstützung der Wertungssysteme [Glicko-2](https://en.wikipedia.org/wiki/Glicko_rating_system#Glicko-2_algorithm) und [Elo](https://en.wikipedia.org/wiki/Elo_rating_system). Wertungen beim Erfassen von Partien und Turnierergebnissen aktualisieren
- Einzelpartien (gewertet oder ungewertet) erfassen und bei Bedarf sogar ihre [PNG](https://en.wikipedia.org/wiki/Portable_Game_Notation)-Notation speichern
- Turniere im [Schweizer System](https://en.wikipedia.org/wiki/Swiss-system_tournament), [Rundenturniere](https://en.wikipedia.org/wiki/Round-robin_tournament), [K.-o.-Turniere](https://en.wikipedia.org/wiki/Single-elimination_tournament) und Zweikämpfe organisieren. Runden, Paarungen, Tabellen und Ergebnisse an einem Ort verfolgen
- Schweizer Paarungen mithilfe der Bibliothek [Chesspairing](https://github.com/gnutterts/chesspairing/) nach einer von der FIDE genehmigten Logik erstellen
- Turniere in einem benutzerfreundlichen Excel-Format oder im offiziellen TRF-Format der FIDE exportieren

### Moderne Desktop-App 💻

- Mit modernen Technologien und responsiven Ansichten entwickelt, die bei unterschiedlichen Fenstergrößen bedienbar bleiben
- Für den praktischen Alltag mit direkter Navigation zu Spielern, Partien, Turnieren und Vereinseinstellungen konzipiert
- Helle und dunkle Designs für verschiedene Umgebungen und Vorlieben
- Lokalisierungsunterstützung für verschiedene Sprachen

### Für lokale Datenhoheit entwickelt 🔒

Anders als weborientierte Vereinswerkzeuge ist Local Knights auf lokale Kontrolle ausgelegt.

- Die Datenbank Ihres Vereins bleibt auf Ihrem eigenen Computer. Kein gehostetes Backend erforderlich
- Zur Sicherung genügt das Kopieren einer verschlüsselten `.db`-Datei
- Die Wiederherstellung ist einfach, portabel und unabhängig von der Infrastruktur Dritter

## Erste Schritte ▶️

Einen Verein einzurichten ist bewusst einfach gehalten.

1. Wählen Sie eine Installationsmethode:
   - **Empfohlen:** Installieren Sie Local Knights über den [Microsoft Store](https://apps.microsoft.com/detail/9p78z94q4nrt).
   - **Alternative:** Laden Sie die neueste ZIP-Datei von [GitHub Releases](https://github.com/urbanoanderson/local-knights/releases) herunter, entpacken Sie sie in den gewünschten Ordner, erstellen Sie optional eine Verknüpfung und starten Sie „Local Knights.exe“.
1. Klicken Sie beim ersten Start auf die Schaltfläche „+“ und richten Sie Vereinsname, Wertungseinstellungen, Passwort usw. ein.
1. Melden Sie sich mit dem erstellten Passwort bei Ihrem Verein an und öffnen Sie die Vereinsübersicht.
1. Registrieren Sie Spieler einzeln über die Schaltfläche Erstellen auf der Spielerseite. Alternativ wird der Massenimport von Spielerdateien im CSV-Format unterstützt ([Beispielimportdatei](Scripts/Seed/player-bulk-import-sample.csv)).
1. Erstellen Sie über die jeweiligen Seiten und Formulare Partien und Turniere mit den registrierten Spielern.
1. Viel Spaß – halten Sie das Schach in Ihrer Gemeinschaft lebendig! Die Vereinsdaten werden unter „C:\Users\<USERNAME>\AppData\Local\LocalKnights\Clubs“ gespeichert.

![Demo von Local Knights](Docs/Screenshots/demo.gif)

## Datenschutz und Datenverarbeitung ⚠️

Diese Anwendung wurde mit Blick auf den Datenschutz entwickelt:

- **Ausschließlich lokale Speicherung**
  Alle Vereinsdaten (Spieler, Partien und Turniere) werden lokal auf dem Computer des Benutzers in einer SQLite-Datenbank gespeichert. Die Anwendung überträgt, synchronisiert oder teilt keine Daten mit externen Servern oder Dritten.

- **Keine Datenerhebung durch den Entwickler**
  Der Entwickler dieser Software erhebt, liest oder verarbeitet keine in die Anwendung eingegebenen personenbezogenen Daten.

- **Verantwortung der Vereinsleitung**
  Vereinsverantwortliche und Administratoren, die diese Software verwenden, müssen die Einhaltung der geltenden Datenschutzgesetze sicherstellen, darunter die Datenschutz-Grundverordnung (DSGVO) der EU und die brasilianische Lei Geral de Proteção de Dados (LGPD). Dazu gehören die Information der Spieler über die Verwendung ihrer Daten, das Einholen einer erforderlichen Einwilligung sowie die Wahrung der Rechte auf Auskunft, Berichtigung und Löschung.

- **Sicherheitsempfehlungen**
  - Schützen Sie den Zugriff auf Ihren Computer und Ihre Datenbankdateien.
  - Sichern Sie Ihre Daten regelmäßig und sicher.
  - Verwenden Sie starke Passwörter zum Schutz echter Vereinsdatenbanken. Passwörter werden weder von Local Knights noch von Drittanbietern gesichert. Ein Passwortmanager wird empfohlen, damit der Zugriff auf die Vereinsdaten nicht verloren geht.

**Haftungsausschluss:** Diese Software wird ohne Mängelgewähr bereitgestellt. Sie erleichtert zwar die lokale Datenverwaltung, die letztendliche Verantwortung für die rechtmäßige und vorschriftsmäßige Verwendung personenbezogener Daten liegt jedoch bei den Vereinsverantwortlichen und Administratoren.

## Informationen für Entwickler 🛠️

### Verwendete Technologien ⚙️

- .NET-10-Desktopanwendung auf Basis von WPF und dem MVVM-Muster
- SQLite-Speicherung mit SQLCipher zum Passwortschutz
- Bibliothek [Chesspairing](https://github.com/gnutterts/chesspairing/) zur Erstellung von Paarungen nach FIDE-Regeln

### Versionierung 🔢

- Local Knights verwendet das Versionsformat `MAJOR.MINOR.REVISION`
- Stabile Releases basieren auf dem Branch `main`. Experimentelle Vorabversionen basieren auf dem Branch `dev`
- CI/CD-Workflows kompilieren und testen automatisch, erstellen Artefakte und erzeugen Vorabversionen und Releases

### Aus dem Quellcode kompilieren, testen und ausführen 🧪

Das .NET 10 SDK ist erforderlich

```bash
dotnet restore

dotnet build ProjectSolution.slnx

dotnet test ProjectSolution.slnx

dotnet run --project LocalKnights.WindowsDesktop/LocalKnights.WindowsDesktop.csproj
```

### Microsoft-Store-Paket erstellen 📦

Führen Sie unter Windows mit installiertem .NET 10 SDK und Windows SDK Folgendes aus:

```powershell
.\Scripts\create-msix.ps1
```

Dadurch wird ein unsigniertes x64-MSIX unter
`LocalKnights.WindowsDesktop\bin\Packages\LocalKnights_<version>_x64.msix` erstellt. Dabei wird
die Local Knights in Partner Center zugeordnete Paketidentität verwendet. Laden Sie diese Datei
im Abschnitt **Pakete** der Einreichung hoch; Microsoft Store signiert sie nach der Einreichung.
Erhöhen Sie vor dem Einreichen eines Updates die Projektversion.

## Lizenz 📄

Local Knights ist unter der Mozilla Public License 2.0 lizenziert.

Den vollständigen Lizenztext finden Sie unter [LICENSE](LICENSE).

Informationen zu Drittanbieterkomponenten, eingebundenen Bibliotheken und externen Implementierungsreferenzen finden Sie unter [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

## Das Projekt unterstützen 💖

Wenn Local Knights Ihnen oder Ihrem Verein geholfen hat, weniger Zeit mit widerspenstigen Tabellen und mehr Zeit mit dem Mattsetzen von Freunden zu verbringen, können Sie [das Projekt sponsern](https://github.com/sponsors/urbanoanderson). Selbst ein kleiner Beitrag hält die Springer in Bewegung – Pferde sind schließlich überraschend teuer. 🐴♟️
