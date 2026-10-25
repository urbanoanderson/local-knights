[English](README.md) | [Português (Brasil)](README.pt-BR.md) | **Español** | [Français](README.fr.md) | [Deutsch](README.de.md) | [Русский](README.ru.md)

# Local Knights

[![License: MPL 2.0](https://img.shields.io/badge/License-MPL%202.0-brightgreen.svg)](LICENSE)
[![Download Latest Release](https://img.shields.io/github/v/release/urbanoanderson/local-knights?label=Latest%20Release)](https://github.com/urbanoanderson/local-knights/releases)
[![Patrocina](https://img.shields.io/github/sponsors/urbanoanderson?logo=githubsponsors&label=Sponsor)](https://github.com/sponsors/urbanoanderson)
![Platform: Windows](https://img.shields.io/badge/Platform-Windows%2010%2B-0078D6)
![Built with .NET 10](https://img.shields.io/badge/.NET-10-512BD4)

<p align="center">
	<img src="LocalKnights.WindowsDesktop/Resources/Icon/logo.png" alt="Logotipo de Local Knights" width="180">
</p>

<p align="center"><strong>El software de código abierto para gestionar clubes comunitarios de ajedrez.</strong></p>

<p align="center">
	Local Knights es una aplicación de escritorio moderna para dirigir clubes de ajedrez. Gestiona jugadores, registra partidas, organiza competiciones, genera emparejamientos y mantiene ratings internos del club desde una interfaz cuidada.
</p>

## Por qué Local Knights ♟️

- Totalmente gratuito y de código abierto bajo la licencia MPL-2.0. Sin suscripciones ni anuncios
- Experiencia de escritorio completamente local, sin depender de la nube. Navegación clara, diseños adaptables, temas claro y oscuro y compatibilidad con varios idiomas
- Los datos del club se guardan en un único archivo de base de datos protegido con contraseña. Crear y restaurar copias de seguridad es sencillo
- Perfecto para clubes comunitarios de ajedrez, grupos de amigos y programas escolares que necesitan algo más estructurado que las hojas de cálculo y quieren un flujo de trabajo gratuito y de código abierto

## Funciones principales 🚀

### Gestiona a los socios del club 👥

- Registra jugadores y mantén perfiles completos de los socios del club
- Vincula los perfiles con [Chess.com](https://www.chess.com/), [Lichess](https://lichess.org/), [FIDE](https://www.fide.com/) y otras organizaciones de ajedrez
- Introduce manualmente u obtén ratings de las plataformas externas compatibles al crear un jugador
- Mantén ratings distintos para cada socio según el ritmo de juego (Bullet, Blitz, Rápido y Clásico)

### Organiza el juego competitivo 🏆

- Compatibilidad con los sistemas de rating [Glicko-2](https://en.wikipedia.org/wiki/Glicko_rating_system#Glicko-2_algorithm) y [Elo](https://en.wikipedia.org/wiki/Elo_rating_system). Actualiza los ratings al registrar partidas y resultados de torneos
- Registra partidas individuales (con o sin rating) e incluso guarda sus notaciones [PNG](https://en.wikipedia.org/wiki/Portable_Game_Notation) cuando sea necesario
- Organiza competiciones por [sistema suizo](https://en.wikipedia.org/wiki/Swiss-system_tournament), [todos contra todos](https://en.wikipedia.org/wiki/Round-robin_tournament), [eliminación directa](https://en.wikipedia.org/wiki/Single-elimination_tournament) y encuentros entre dos jugadores. Controla rondas, emparejamientos, clasificaciones y resultados en un solo lugar
- Genera emparejamientos suizos con lógica aprobada por la FIDE mediante la biblioteca [Chesspairing](https://github.com/gnutterts/chesspairing/)
- Exporta las competiciones en un formato Excel fácil de usar o en el formato TRF oficial de la FIDE

### Experiencia moderna de escritorio 💻

- Creado con tecnologías modernas y pantallas adaptables que siguen siendo prácticas con distintos tamaños de ventana
- Diseñado para el uso diario, con una navegación directa centrada en jugadores, partidas, competiciones y ajustes del club
- Temas claro y oscuro para distintos entornos y preferencias
- Compatibilidad con la localización en varios idiomas

### Creado para el control local 🔒

A diferencia de las herramientas para clubes centradas en la Web, Local Knights está diseñado para dar prioridad al control local.

- La base de datos del club permanece en tu propio equipo. No hace falta ningún backend alojado
- Crear una copia de seguridad es tan sencillo como copiar un archivo `.db` cifrado
- La restauración es sencilla, portátil e independiente de infraestructuras de terceros

## Primeros pasos ▶️

Poner un club en marcha es deliberadamente sencillo.

1. Descarga la última versión de la aplicación como archivo zip desde nuestra página de [versiones](https://github.com/urbanoanderson/local-knights/releases).
1. Extrae el contenido del zip en la carpeta que prefieras. Si quieres, crea un acceso directo al ejecutable "Local Knights.exe".
1. Inicia la aplicación mediante "Local Knights.exe".
1. En el primer inicio, pulsa el botón "+" y configura el nombre del club, las preferencias de rating, la contraseña, etc.
1. Accede al club con la contraseña creada y abre su panel.
1. Registra jugadores individualmente con el botón Crear de la página Jugadores. También puedes importar en bloque archivos de jugadores en formato CSV ([archivo de importación de ejemplo](Scripts/Seed/player-bulk-import-sample.csv)).
1. Crea partidas y competiciones con los jugadores registrados mediante sus páginas y formularios correspondientes.
1. ¡Disfruta y mantén vivo el ajedrez en tu comunidad! Los datos del club se guardarán en "C:\Users\<USERNAME>\AppData\Local\LocalKnights\Clubs".

![Demostración de Local Knights](Docs/Screenshots/demo.gif)

## Privacidad y tratamiento de datos ⚠️

Esta aplicación está diseñada pensando en la privacidad:

- **Solo almacenamiento local**
  Todos los datos del club (jugadores, partidas y competiciones) se guardan localmente en el equipo del usuario en una base de datos SQLite. La aplicación no transmite, sincroniza ni comparte datos con servidores externos o terceros.

- **El desarrollador no recopila datos**
  El desarrollador de este software no recopila, consulta ni procesa información personal introducida en la aplicación.

- **Responsabilidad de los responsables del club**
  Los responsables y administradores del club que utilicen este software deben garantizar el cumplimiento de las leyes de protección de datos aplicables, incluido el Reglamento General de Protección de Datos (RGPD) de la UE y la Lei Geral de Proteção de Dados (LGPD) de Brasil. Esto incluye informar a los jugadores sobre el uso de sus datos, obtener su consentimiento cuando sea necesario y respetar sus derechos de acceso, rectificación y supresión.

- **Recomendaciones de seguridad**
  - Protege el acceso a tu equipo y a los archivos de la base de datos.
  - Haz copias de seguridad de tus datos periódicamente.
  - Crea contraseñas seguras para proteger las bases de datos reales del club. Local Knights y los servicios de terceros no guardan copias de las contraseñas. Se recomienda usar un gestor de contraseñas para evitar perder el acceso a los datos del club.

**Aviso:** Este software se proporciona «tal cual». Aunque facilita la gestión local de datos, la responsabilidad última de utilizar los datos personales de forma legal y conforme a la normativa recae en los responsables y administradores del club.

## Información para desarrolladores 🛠️

### Tecnologías utilizadas ⚙️

- Aplicación de escritorio .NET 10 creada con WPF y el patrón MVVM
- Almacenamiento SQLite con SQLCipher para protección mediante contraseña
- Biblioteca [Chesspairing](https://github.com/gnutterts/chesspairing/) para generar emparejamientos según las reglas de la FIDE

### Control de versiones 🔢

- Local Knights utiliza el formato de versión `MAJOR.MINOR.REVISION`
- Las versiones estables se basan en la rama `main`. Las versiones preliminares experimentales se basan en la rama `dev`
- Los flujos de CI/CD compilan, prueban, crean artefactos y generan versiones preliminares y definitivas automáticamente

### Compilar, probar y ejecutar desde el código fuente 🧪

Se requiere el SDK de .NET 10

```bash
dotnet restore

dotnet build ProjectSolution.slnx

dotnet test ProjectSolution.slnx

dotnet run --project LocalKnights.WindowsDesktop/LocalKnights.WindowsDesktop.csproj
```

### Crear un paquete para Microsoft Store 📦

En Windows, con los SDK de .NET 10 y Windows instalados, ejecuta:

```powershell
.\Scripts\create-msix.ps1
```

Esto crea un MSIX x64 sin firmar en
`LocalKnights.WindowsDesktop\bin\Packages\LocalKnights_<version>_x64.msix` con la
identidad de paquete asociada a Local Knights en Partner Center. Sube ese archivo a
la sección **Paquetes** del envío; Microsoft Store lo firma después del envío.
Aumenta la versión del proyecto antes de enviar una actualización.

## Licencia 📄

Local Knights se distribuye bajo la Mozilla Public License 2.0.

Consulta [LICENSE](LICENSE) para leer el texto completo de la licencia.

Para conocer los componentes de terceros, las bibliotecas incluidas y las referencias externas de implementación, consulta [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

## Apoya el proyecto 💖

Si Local Knights te ha ayudado a ti o a tu club a pasar menos tiempo peleando con hojas de cálculo y más tiempo dando jaque mate a tus amigos, considera [patrocinar el proyecto](https://github.com/sponsors/urbanoanderson). Incluso una pequeña aportación ayuda a mantener los caballos en movimiento; resulta que los caballos son sorprendentemente caros. 🐴♟️
