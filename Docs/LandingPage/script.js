const carousel = document.querySelector("[data-carousel]");
const languageStorageKey = "local-knights-landing-language";
const languageMetadata = {
  en: { flag: "🇺🇸", htmlLang: "en" },
  "pt-BR": { flag: "🇧🇷", htmlLang: "pt-BR" },
  es: { flag: "🇪🇸", htmlLang: "es" },
  fr: { flag: "🇫🇷", htmlLang: "fr" },
  de: { flag: "🇩🇪", htmlLang: "de" },
  ru: { flag: "🇷🇺", htmlLang: "ru" }
};

const translations = {
  en: {
    pageTitle: "Local Knights",
    pageDescription: "Local Knights is the open source community chess club management software for players, ratings, games, and competitions.",
    brandName: "Local Knights",
    navGithub: "GitHub",
    navLicense: "License",
    navThirdParty: "Third-Party Notices",
    langEnglish: "English",
    langPortugueseBrazil: "Portuguese (Brazil)",
    langSpanish: "Spanish",
    langFrench: "French",
    langGerman: "German",
    langRussian: "Russian",
    selectLanguage: "Select language",
    heroEyebrow: "Open source. Local-first. Club-ready.",
    heroTitle: "The open source community chess club management software",
    heroText: "Run your chess club from one modern desktop app. Register players, record games, organize competitions, maintain internal ratings and keep full ownership of your data without server overhead and completely free.",
    downloadButton: "Download",
    carouselPrev: "Previous screenshot",
    carouselNext: "Next screenshot",
    carouselGoTo: "Go to screenshot {0}",
    altBrandLogo: "Local Knights logo",
    altDemo: "Local Knights demo animation",
    altDashboard: "Dashboard page screenshot",
    altPlayers: "Players page screenshot",
    altPlayerProfile: "Player profile screenshot",
    altGame: "Game page screenshot",
    altCompetitionInfo: "Competition information screenshot",
    altCompetitionRounds: "Competition rounds screenshot",
    altCompetitionStandings: "Competition standings screenshot",
    captionDashboard: "Dashboard overview with club activity, quick actions, and latest results.",
    captionPlayers: "Players list with searchable records and club member management.",
    captionPlayerProfile: "Detailed player profile with ratings, identities, and match history.",
    captionGame: "Game record view for saving results, opponents, and time controls.",
    captionCompetitionInfo: "Competition overview with event setup details and tournament metadata.",
    captionCompetitionRounds: "Round management with pairings, boards, and result tracking.",
    captionCompetitionStandings: "Standings table with rankings, scores, and tournament progress.",
    whyEyebrow: "Why Local Knights",
    whyTitle: "Packed with the features all clubs need, without the complexity.",
    featureOwnDataTitle: "Own your data",
    featureOwnDataText: "Everything runs locally on the club owner’s computer. No mandatory cloud services, no hosted backend, no subscription dependency.",
    featureBackupTitle: "Simple backup model",
    featureBackupText: "Your club lives in a single password-protected SQLite database file. Backup and restore is as straightforward as copying one file.",
    featureCompetitionTitle: "Serious competition support",
    featureCompetitionText: "Manage Swiss, Round-Robin, Knockout, and two-player Match events with pairings, rounds, standings, and exportable results in one workflow.",
    featureRatingsTitle: "Meaningful ratings",
    featureRatingsText: "Track internal club ratings with Glicko-2 or Elo system and keep player strength data organized across multiple time formats.",
    featureProfilesTitle: "Connected player profiles",
    featureProfilesText: "Link profiles to Chess.com, Lichess, FIDE, and and other organizations to enrich player records with existing data.",
    featureUxTitle: "Polished day-to-day UX",
    featureUxText: "Modern navigation, responsive layouts, dark and light themes, and multilingual support for language localization.",
    whoEyebrow: "Who It's For",
    whoTitle: "Made for groups that want to organize chess without overhead.",
    audienceClubsTitle: "Local chess clubs",
    audienceClubsText: "Ideal for community clubs that want to manage players, games, pairings, and standings in one desktop app without relying on online services.",
    audienceGroupsTitle: "Friend groups and small communities",
    audienceGroupsText: "A practical fit for enthusiastic groups who want something more structured than spreadsheets, but still simple enough to run on one machine.",
    audienceSchoolsTitle: "Schools and learning environments",
    audienceSchoolsText: "Useful for school clubs, classrooms, and scholastic programs that need a free, ad-free, open source tool with no paid licenses or subscriptions.",
    demoEyebrow: "Demo",
    demoTitle: "See Local Knights in motion.",
    footerLicense: "© 2026 Local Knights. Licensed under the Mozilla Public License 2.0",
    footerAuthor: "Created by Anderson Urbano"
  },
  "pt-BR": {
    pageTitle: "Local Knights",
    pageDescription: "Local Knights é o software de código aberto para gerenciamento de clubes de xadrez, jogadores, ratings, partidas e competições.",
    brandName: "Local Knights",
    navGithub: "GitHub",
    navLicense: "Licença",
    navThirdParty: "Avisos de Terceiros",
    langEnglish: "Inglês",
    langPortugueseBrazil: "Português (Brasil)",
    langSpanish: "Espanhol",
    langFrench: "Francês",
    langGerman: "Alemão",
    langRussian: "Russo",
    selectLanguage: "Selecionar idioma",
    heroEyebrow: "Código aberto. Controle total. Pronto para clubes.",
    heroTitle: "Seu gestor de código aberto para clubes de xadrez",
    heroText: "Gerencie seu clube de xadrez em um só aplicativo desktop moderno. Cadastre jogadores, registre partidas, organize competições, mantenha ratings internos e preserve total controle dos seus dados sem dependência de nuvem ou servidores e de forma gratuita.",
    downloadButton: "Baixar",
    carouselPrev: "Imagem anterior",
    carouselNext: "Próxima imagem",
    carouselGoTo: "Ir para a imagem {0}",
    altBrandLogo: "Logotipo do Local Knights",
    altDemo: "Animação de demonstração do Local Knights",
    altDashboard: "Captura de tela do painel",
    altPlayers: "Captura de tela da página de jogadores",
    altPlayerProfile: "Captura de tela do perfil do jogador",
    altGame: "Captura de tela da página da partida",
    altCompetitionInfo: "Captura de tela das informações da competição",
    altCompetitionRounds: "Captura de tela das rodadas da competição",
    altCompetitionStandings: "Captura de tela da classificação da competição",
    captionDashboard: "Visão geral do painel com atividade do clube, ações rápidas e resultados recentes.",
    captionPlayers: "Lista de jogadores com pesquisa, cadastro e gerenciamento dos membros do clube.",
    captionPlayerProfile: "Perfil detalhado do jogador com ratings, identificadores e histórico de partidas.",
    captionGame: "Tela de partida para registrar resultados, adversários e controles de tempo.",
    captionCompetitionInfo: "Visão geral da competição com detalhes de configuração e metadados do torneio.",
    captionCompetitionRounds: "Gerenciamento de rodadas com emparceiramentos, mesas e acompanhamento de resultados.",
    captionCompetitionStandings: "Tabela de classificação com posições, pontuações e progresso do torneio.",
    whyEyebrow: "Por que usar Local Knights",
    whyTitle: "Cheio dos recursos de que todo clube precisa, sem a complexidade.",
    featureOwnDataTitle: "Seus dados sob seu controle",
    featureOwnDataText: "Tudo roda localmente no computador do responsável pelo clube. Sem serviços em nuvem obrigatórios, sem backend hospedado e sem dependência de assinaturas.",
    featureBackupTitle: "Backup simples",
    featureBackupText: "Seu clube fica em um único arquivo de banco de dados SQLite protegido por senha. Fazer backup e restaurar é tão simples quanto copiar um arquivo.",
    featureCompetitionTitle: "Suporte robusto a competições",
    featureCompetitionText: "Crie e gerencie competições do tipo Suíço, Round-Robin, Eliminação e Match com emparceiramentos, rodadas, classificações e resultados exportáveis em um único fluxo.",
    featureRatingsTitle: "Ratings consistentes",
    featureRatingsText: "Mantenha ratings internos com sistema Glicko-2 ou Elo para acompanhar a força dos jogadores em diferentes ritmos de tempo.",
    featureProfilesTitle: "Perfis conectados",
    featureProfilesText: "Associe perfis do Chess.com, Lichess, FIDE e outras organizações para enriquecer o cadastro dos jogadores com dados já existentes.",
    featureUxTitle: "UX simples e polida",
    featureUxText: "Navegação moderna, layouts responsivos, temas claro e escuro, e suporte a localização de idiomas.",
    whoEyebrow: "Para quem é",
    whoTitle: "Feito para grupos que querem organizar xadrez sem dor de cabeça.",
    audienceClubsTitle: "Clubes locais de xadrez",
    audienceClubsText: "Ideal para clubes comunitários que querem gerenciar jogadores, partidas, emparceiramentos e classificações em um único aplicativo desktop, sem depender de serviços online.",
    audienceGroupsTitle: "Grupos de amigos e pequenas comunidades",
    audienceGroupsText: "Uma opção prática para grupos entusiastas que querem algo mais estruturado do que planilhas, mas ainda simples o suficiente para rodar em uma única máquina.",
    audienceSchoolsTitle: "Escolas e ambientes de aprendizagem",
    audienceSchoolsText: "Útil para clubes escolares, salas de aula e projetos educacionais que precisam de uma ferramenta gratuita, sem anúncios, de código aberto e sem licenças pagas nem assinaturas.",
    demoEyebrow: "Demo",
    demoTitle: "Veja o Local Knights em ação.",
    footerLicense: "© 2026 Local Knights. Licenciado sob a Mozilla Public License 2.0",
    footerAuthor: "Criado por Anderson Urbano"
  },
  es: {
    pageTitle: "Local Knights",
    pageDescription: "Local Knights es el software de código abierto para gestionar clubes de ajedrez, jugadores, ratings, partidas y competiciones.",
    brandName: "Local Knights",
    navGithub: "GitHub",
    navLicense: "Licencia",
    navThirdParty: "Avisos de terceros",
    langEnglish: "Inglés",
    langPortugueseBrazil: "Portugués (Brasil)",
    langSpanish: "Español",
    langFrench: "Francés",
    langGerman: "Alemán",
    langRussian: "Ruso",
    selectLanguage: "Seleccionar idioma",
    heroEyebrow: "Código abierto. Datos locales. Listo para tu club.",
    heroTitle: "El software de código abierto para gestionar clubes de ajedrez",
    heroText: "Gestiona tu club de ajedrez desde una única aplicación de escritorio moderna. Registra jugadores y partidas, organiza competiciones, mantén ratings internos y conserva el control total de tus datos sin servidores ni costes.",
    downloadButton: "Descargar",
    carouselPrev: "Captura anterior",
    carouselNext: "Captura siguiente",
    carouselGoTo: "Ir a la captura {0}",
    altBrandLogo: "Logotipo de Local Knights",
    altDemo: "Animación de demostración de Local Knights",
    altDashboard: "Captura de pantalla del panel",
    altPlayers: "Captura de pantalla de la página de jugadores",
    altPlayerProfile: "Captura de pantalla del perfil del jugador",
    altGame: "Captura de pantalla de la página de la partida",
    altCompetitionInfo: "Captura de pantalla de la información de la competición",
    altCompetitionRounds: "Captura de pantalla de las rondas de la competición",
    altCompetitionStandings: "Captura de pantalla de la clasificación de la competición",
    captionDashboard: "Panel general con la actividad del club, acciones rápidas y resultados recientes.",
    captionPlayers: "Lista de jugadores con búsqueda de registros y gestión de socios del club.",
    captionPlayerProfile: "Perfil detallado del jugador con ratings, identificadores e historial de partidas.",
    captionGame: "Ficha de partida para guardar resultados, rivales y ritmos de juego.",
    captionCompetitionInfo: "Resumen de la competición con su configuración y los datos del torneo.",
    captionCompetitionRounds: "Gestión de rondas con emparejamientos, tableros y seguimiento de resultados.",
    captionCompetitionStandings: "Clasificación con puestos, puntuaciones y progreso del torneo.",
    whyEyebrow: "Por qué Local Knights",
    whyTitle: "Todo lo que necesita un club, sin complicaciones.",
    featureOwnDataTitle: "Tus datos son tuyos",
    featureOwnDataText: "Todo se ejecuta localmente en el ordenador del responsable del club. Sin servicios obligatorios en la nube, servidores externos ni suscripciones.",
    featureBackupTitle: "Copias de seguridad sencillas",
    featureBackupText: "Todos los datos del club se guardan en un único archivo de base de datos SQLite protegido con contraseña. Crear o restaurar una copia es tan sencillo como copiar un archivo.",
    featureCompetitionTitle: "Gestión completa de competiciones",
    featureCompetitionText: "Gestiona torneos por sistema suizo, todos contra todos y eliminación directa, además de encuentros entre dos jugadores, con emparejamientos, rondas, clasificaciones y resultados exportables.",
    featureRatingsTitle: "Ratings que importan",
    featureRatingsText: "Controla los ratings internos del club con Glicko-2 o Elo y organiza la fuerza de juego de cada jugador en distintos ritmos.",
    featureProfilesTitle: "Perfiles conectados",
    featureProfilesText: "Vincula perfiles de Chess.com, Lichess, FIDE y otras organizaciones para completar las fichas de los jugadores con datos existentes.",
    featureUxTitle: "Una experiencia cuidada",
    featureUxText: "Navegación moderna, diseño adaptable, temas claro y oscuro, y compatibilidad con varios idiomas.",
    whoEyebrow: "Para quién es",
    whoTitle: "Creado para grupos que quieren organizar el ajedrez sin cargas innecesarias.",
    audienceClubsTitle: "Clubes locales de ajedrez",
    audienceClubsText: "Ideal para clubes comunitarios que quieren gestionar jugadores, partidas, emparejamientos y clasificaciones desde una sola aplicación de escritorio, sin depender de servicios en línea.",
    audienceGroupsTitle: "Grupos de amigos y pequeñas comunidades",
    audienceGroupsText: "Una opción práctica para grupos aficionados que buscan algo más organizado que una hoja de cálculo, pero lo bastante sencillo para funcionar en un solo equipo.",
    audienceSchoolsTitle: "Escuelas y entornos educativos",
    audienceSchoolsText: "Útil para clubes escolares, aulas y programas educativos que necesitan una herramienta gratuita, sin anuncios, de código abierto y sin licencias ni suscripciones de pago.",
    demoEyebrow: "Demostración",
    demoTitle: "Descubre Local Knights en acción.",
    footerLicense: "© 2026 Local Knights. Publicado bajo la Licencia Pública de Mozilla 2.0",
    footerAuthor: "Creado por Anderson Urbano"
  },
  fr: {
    pageTitle: "Local Knights",
    pageDescription: "Local Knights est le logiciel libre de gestion de clubs d’échecs, de joueurs, de classements Elo, de parties et de compétitions.",
    brandName: "Local Knights",
    navGithub: "GitHub",
    navLicense: "Licence",
    navThirdParty: "Mentions relatives aux tiers",
    langEnglish: "Anglais",
    langPortugueseBrazil: "Portugais (Brésil)",
    langSpanish: "Espagnol",
    langFrench: "Français",
    langGerman: "Allemand",
    langRussian: "Russe",
    selectLanguage: "Choisir la langue",
    heroEyebrow: "Logiciel libre. Données locales. Prêt pour votre club.",
    heroTitle: "Le logiciel libre de gestion des clubs d’échecs",
    heroText: "Gérez votre club d’échecs depuis une seule application de bureau moderne. Inscrivez les joueurs, enregistrez les parties, organisez les compétitions, gérez les classements Elo internes et gardez la pleine maîtrise de vos données, sans serveur ni frais.",
    downloadButton: "Télécharger",
    carouselPrev: "Capture précédente",
    carouselNext: "Capture suivante",
    carouselGoTo: "Afficher la capture {0}",
    altBrandLogo: "Logo de Local Knights",
    altDemo: "Animation de démonstration de Local Knights",
    altDashboard: "Capture d’écran du tableau de bord",
    altPlayers: "Capture d’écran de la page des joueurs",
    altPlayerProfile: "Capture d’écran du profil d’un joueur",
    altGame: "Capture d’écran de la page d’une partie",
    altCompetitionInfo: "Capture d’écran des informations de la compétition",
    altCompetitionRounds: "Capture d’écran des rondes de la compétition",
    altCompetitionStandings: "Capture d’écran du classement de la compétition",
    captionDashboard: "Vue d’ensemble de l’activité du club, des actions rapides et des derniers résultats.",
    captionPlayers: "Liste consultable des joueurs et gestion des membres du club.",
    captionPlayerProfile: "Profil détaillé d’un joueur avec ses classements Elo, ses identifiants et son historique de parties.",
    captionGame: "Fiche de partie pour enregistrer le résultat, les adversaires et la cadence.",
    captionCompetitionInfo: "Vue d’ensemble de la compétition avec sa configuration et les informations du tournoi.",
    captionCompetitionRounds: "Gestion des rondes avec les appariements, les échiquiers et le suivi des résultats.",
    captionCompetitionStandings: "Classement présentant les places, les scores et l’avancement du tournoi.",
    whyEyebrow: "Pourquoi Local Knights",
    whyTitle: "Toutes les fonctions indispensables à un club, sans la complexité.",
    featureOwnDataTitle: "Gardez la maîtrise de vos données",
    featureOwnDataText: "Tout s’exécute localement sur l’ordinateur du responsable du club. Aucun service infonuagique obligatoire, serveur externe ou abonnement requis.",
    featureBackupTitle: "Sauvegardes simplifiées",
    featureBackupText: "Toutes les données du club sont réunies dans un seul fichier de base de données SQLite protégé par mot de passe. Sauvegarder ou restaurer vos données revient simplement à copier ce fichier.",
    featureCompetitionTitle: "Gestion complète des compétitions",
    featureCompetitionText: "Gérez les tournois au système suisse, toutes rondes et à élimination directe, ainsi que les matchs entre deux joueurs, avec les appariements, les rondes, les classements et l’exportation des résultats.",
    featureRatingsTitle: "Des classements Elo pertinents",
    featureRatingsText: "Suivez les classements internes du club avec les systèmes Glicko-2 ou Elo et organisez le niveau des joueurs selon plusieurs cadences.",
    featureProfilesTitle: "Profils connectés",
    featureProfilesText: "Associez les profils Chess.com, Lichess, FIDE et d’autres organismes afin d’enrichir les fiches des joueurs avec leurs données existantes.",
    featureUxTitle: "Une expérience soignée au quotidien",
    featureUxText: "Navigation moderne, mise en page adaptative, thèmes clair et sombre, et prise en charge de plusieurs langues.",
    whoEyebrow: "À qui s’adresse-t-il ?",
    whoTitle: "Conçu pour les groupes qui veulent organiser les échecs sans contraintes inutiles.",
    audienceClubsTitle: "Clubs d’échecs locaux",
    audienceClubsText: "Idéal pour les clubs associatifs qui souhaitent gérer joueurs, parties, appariements et classements depuis une seule application de bureau, sans dépendre de services en ligne.",
    audienceGroupsTitle: "Groupes d’amis et petites communautés",
    audienceGroupsText: "Une solution pratique pour les groupes passionnés qui recherchent un outil plus structuré qu’un tableur, tout en restant assez simple pour fonctionner sur un seul ordinateur.",
    audienceSchoolsTitle: "Écoles et établissements d’enseignement",
    audienceSchoolsText: "Adapté aux clubs scolaires, aux classes et aux programmes éducatifs qui ont besoin d’un outil gratuit, sans publicité, libre et sans licence ni abonnement payant.",
    demoEyebrow: "Démonstration",
    demoTitle: "Découvrez Local Knights en action.",
    footerLicense: "© 2026 Local Knights. Distribué sous la Mozilla Public License 2.0",
    footerAuthor: "Créé par Anderson Urbano"
  },
  de: {
    pageTitle: "Local Knights",
    pageDescription: "Local Knights ist die quelloffene Software zur Verwaltung von Schachvereinen, Spielern, Wertungen, Partien und Turnieren.",
    brandName: "Local Knights",
    navGithub: "GitHub",
    navLicense: "Lizenz",
    navThirdParty: "Hinweise zu Drittanbietern",
    langEnglish: "Englisch",
    langPortugueseBrazil: "Portugiesisch (Brasilien)",
    langSpanish: "Spanisch",
    langFrench: "Französisch",
    langGerman: "Deutsch",
    langRussian: "Russisch",
    selectLanguage: "Sprache auswählen",
    heroEyebrow: "Quelloffen. Lokal gespeichert. Bereit für den Verein.",
    heroTitle: "Die quelloffene Software zur Verwaltung von Schachvereinen",
    heroText: "Verwalten Sie Ihren Schachverein mit einer modernen Desktop-App. Erfassen Sie Spieler und Partien, organisieren Sie Turniere, pflegen Sie interne Wertungen und behalten Sie ohne Serveraufwand und völlig kostenlos die volle Kontrolle über Ihre Daten.",
    downloadButton: "Herunterladen",
    carouselPrev: "Vorheriger Screenshot",
    carouselNext: "Nächster Screenshot",
    carouselGoTo: "Zu Screenshot {0}",
    altBrandLogo: "Logo von Local Knights",
    altDemo: "Demoanimation von Local Knights",
    altDashboard: "Screenshot der Übersichtsseite",
    altPlayers: "Screenshot der Spielerseite",
    altPlayerProfile: "Screenshot des Spielerprofils",
    altGame: "Screenshot der Partieseite",
    altCompetitionInfo: "Screenshot der Turnierinformationen",
    altCompetitionRounds: "Screenshot der Turnierrunden",
    altCompetitionStandings: "Screenshot der Turniertabelle",
    captionDashboard: "Übersicht mit Vereinsaktivitäten, Schnellaktionen und aktuellen Ergebnissen.",
    captionPlayers: "Durchsuchbare Spielerliste zur Verwaltung der Vereinsmitglieder.",
    captionPlayerProfile: "Ausführliches Spielerprofil mit Wertungen, Kennungen und Partieverlauf.",
    captionGame: "Partieansicht zum Erfassen von Ergebnissen, Gegnern und Bedenkzeiten.",
    captionCompetitionInfo: "Turnierübersicht mit Einstellungen und Turnierdaten.",
    captionCompetitionRounds: "Rundenverwaltung mit Paarungen, Brettern und Ergebniserfassung.",
    captionCompetitionStandings: "Turniertabelle mit Platzierungen, Punkten und Turnierverlauf.",
    whyEyebrow: "Warum Local Knights",
    whyTitle: "Alle Funktionen, die ein Verein braucht, ohne unnötige Komplexität.",
    featureOwnDataTitle: "Ihre Daten bleiben bei Ihnen",
    featureOwnDataText: "Alles läuft lokal auf dem Computer des Vereinsverantwortlichen. Keine verpflichtenden Cloud-Dienste, kein gehostetes Backend und keine Abhängigkeit von Abonnements.",
    featureBackupTitle: "Einfache Datensicherung",
    featureBackupText: "Ihre Vereinsdaten befinden sich in einer einzigen passwortgeschützten SQLite-Datenbankdatei. Sichern und Wiederherstellen ist so einfach wie das Kopieren einer Datei.",
    featureCompetitionTitle: "Umfassende Turnierverwaltung",
    featureCompetitionText: "Verwalten Sie Turniere im Schweizer System, Rundenturniere, K.-o.-Turniere und Zweikämpfe mit Paarungen, Runden, Tabellen und exportierbaren Ergebnissen in einem Arbeitsablauf.",
    featureRatingsTitle: "Aussagekräftige Wertungen",
    featureRatingsText: "Verfolgen Sie interne Vereinswertungen mit Glicko-2 oder Elo und verwalten Sie die Spielstärke über verschiedene Bedenkzeiten hinweg.",
    featureProfilesTitle: "Verknüpfte Spielerprofile",
    featureProfilesText: "Verknüpfen Sie Profile von Chess.com, Lichess, FIDE und anderen Organisationen, um Spielerdatensätze mit vorhandenen Daten zu ergänzen.",
    featureUxTitle: "Durchdachte Bedienung im Alltag",
    featureUxText: "Moderne Navigation, responsive Layouts, helle und dunkle Designs sowie mehrsprachige Unterstützung.",
    whoEyebrow: "Für wen ist es gedacht?",
    whoTitle: "Für Gruppen, die Schach ohne unnötigen Aufwand organisieren möchten.",
    audienceClubsTitle: "Lokale Schachvereine",
    audienceClubsText: "Ideal für Vereine, die Spieler, Partien, Paarungen und Tabellen in einer Desktop-App verwalten möchten, ohne von Onlinediensten abhängig zu sein.",
    audienceGroupsTitle: "Freundeskreise und kleine Gemeinschaften",
    audienceGroupsText: "Eine praktische Lösung für engagierte Gruppen, die mehr Struktur als eine Tabelle wünschen, aber weiterhin alles auf einem einzigen Computer verwalten möchten.",
    audienceSchoolsTitle: "Schulen und Lernumgebungen",
    audienceSchoolsText: "Geeignet für Schulschachgruppen, Klassen und Bildungsprogramme, die ein kostenloses, werbefreies und quelloffenes Werkzeug ohne kostenpflichtige Lizenzen oder Abonnements benötigen.",
    demoEyebrow: "Demo",
    demoTitle: "Erleben Sie Local Knights in Aktion.",
    footerLicense: "© 2026 Local Knights. Lizenziert unter der Mozilla Public License 2.0",
    footerAuthor: "Erstellt von Anderson Urbano"
  },
  ru: {
    pageTitle: "Local Knights",
    pageDescription: "Local Knights — программа с открытым исходным кодом для управления шахматными клубами, игроками, рейтингами, партиями и турнирами.",
    brandName: "Local Knights",
    navGithub: "GitHub",
    navLicense: "Лицензия",
    navThirdParty: "Уведомления о сторонних компонентах",
    langEnglish: "Английский",
    langPortugueseBrazil: "Португальский (Бразилия)",
    langSpanish: "Испанский",
    langFrench: "Французский",
    langGerman: "Немецкий",
    langRussian: "Русский",
    selectLanguage: "Выбрать язык",
    heroEyebrow: "Открытый код. Локальное хранение. Всё для клуба.",
    heroTitle: "Программа с открытым исходным кодом для управления шахматным клубом",
    heroText: "Управляйте шахматным клубом в одном современном настольном приложении. Регистрируйте игроков и партии, проводите турниры, ведите внутренние рейтинги и полностью контролируйте свои данные без затрат на серверы и совершенно бесплатно.",
    downloadButton: "Скачать",
    carouselPrev: "Предыдущий снимок экрана",
    carouselNext: "Следующий снимок экрана",
    carouselGoTo: "Перейти к снимку экрана {0}",
    altBrandLogo: "Логотип Local Knights",
    altDemo: "Демонстрация Local Knights",
    altDashboard: "Снимок экрана панели управления",
    altPlayers: "Снимок экрана страницы игроков",
    altPlayerProfile: "Снимок экрана профиля игрока",
    altGame: "Снимок экрана страницы партии",
    altCompetitionInfo: "Снимок экрана с информацией о турнире",
    altCompetitionRounds: "Снимок экрана с турами турнира",
    altCompetitionStandings: "Снимок экрана турнирной таблицы",
    captionDashboard: "Обзор работы клуба, быстрые действия и последние результаты.",
    captionPlayers: "Список игроков с поиском и управлением участниками клуба.",
    captionPlayerProfile: "Подробный профиль игрока с рейтингами, идентификаторами и историей партий.",
    captionGame: "Карточка партии для сохранения результата, соперников и контроля времени.",
    captionCompetitionInfo: "Обзор турнира с параметрами проведения и сведениями о соревновании.",
    captionCompetitionRounds: "Управление турами: жеребьёвка, пары, доски и учёт результатов.",
    captionCompetitionStandings: "Турнирная таблица с местами, очками и ходом соревнования.",
    whyEyebrow: "Почему Local Knights",
    whyTitle: "Все необходимые клубу возможности без лишней сложности.",
    featureOwnDataTitle: "Ваши данные принадлежат вам",
    featureOwnDataText: "Всё работает локально на компьютере руководителя клуба. Никаких обязательных облачных сервисов, внешних серверов или подписок.",
    featureBackupTitle: "Простое резервное копирование",
    featureBackupText: "Все данные клуба хранятся в одном защищённом паролем файле базы данных SQLite. Для резервного копирования и восстановления достаточно скопировать один файл.",
    featureCompetitionTitle: "Полноценное проведение турниров",
    featureCompetitionText: "Проводите турниры по швейцарской системе, круговые турниры, турниры на выбывание и матчи между двумя игроками: жеребьёвка и пары, туры, таблицы и экспорт результатов доступны в едином рабочем процессе.",
    featureRatingsTitle: "Информативные рейтинги",
    featureRatingsText: "Ведите внутренние рейтинги клуба по системе Glicko-2 или Elo и отслеживайте силу игроков в разных контролях времени.",
    featureProfilesTitle: "Связанные профили игроков",
    featureProfilesText: "Связывайте профили Chess.com, Lichess, FIDE и других организаций, чтобы дополнять карточки игроков уже имеющимися данными.",
    featureUxTitle: "Удобство в повседневной работе",
    featureUxText: "Современная навигация, адаптивная компоновка, светлая и тёмная темы, а также поддержка нескольких языков.",
    whoEyebrow: "Для кого",
    whoTitle: "Для тех, кто хочет организовать шахматную жизнь без лишних хлопот.",
    audienceClubsTitle: "Местные шахматные клубы",
    audienceClubsText: "Подходит общественным клубам, которые хотят управлять игроками, партиями, жеребьёвкой, парами и турнирными таблицами в одном настольном приложении без зависимости от интернет-сервисов.",
    audienceGroupsTitle: "Дружеские компании и небольшие сообщества",
    audienceGroupsText: "Практичный выбор для увлечённых групп, которым нужно больше порядка, чем дают электронные таблицы, но важно сохранить простоту работы на одном компьютере.",
    audienceSchoolsTitle: "Школы и образовательные учреждения",
    audienceSchoolsText: "Подходит школьным клубам, классам и учебным программам, которым нужен бесплатный инструмент без рекламы с открытым исходным кодом, платных лицензий и подписок.",
    demoEyebrow: "Демонстрация",
    demoTitle: "Посмотрите Local Knights в действии.",
    footerLicense: "© 2026 Local Knights. Распространяется по лицензии Mozilla Public License 2.0",
    footerAuthor: "Автор — Anderson Urbano"
  }
};

function applyLanguage(language) {
  const copy = translations[language] || translations.en;
  const metadata = languageMetadata[language] || languageMetadata.en;

  document.documentElement.lang = metadata.htmlLang;

  document.querySelectorAll("[data-i18n]").forEach((element) => {
    const key = element.dataset.i18n;

    if (copy[key]) {
      element.textContent = copy[key];
    }
  });

  document.querySelectorAll("[data-i18n-content]").forEach((element) => {
    const key = element.dataset.i18nContent;

    if (copy[key]) {
      element.setAttribute("content", copy[key]);
    }
  });

  document.querySelectorAll("[data-i18n-aria-label]").forEach((element) => {
    const key = element.dataset.i18nAriaLabel;

    if (copy[key]) {
      element.setAttribute("aria-label", copy[key]);
    }
  });

  document.querySelectorAll("[data-i18n-alt]").forEach((element) => {
    const key = element.dataset.i18nAlt;

    if (copy[key]) {
      element.setAttribute("alt", copy[key]);
    }
  });

  const currentFlag = document.querySelector("[data-current-flag]");

  if (currentFlag) {
    currentFlag.textContent = metadata.flag;
  }

  document.querySelectorAll("[data-language-option]").forEach((button) => {
    button.classList.toggle("is-active", button.dataset.languageOption === language);
  });

  document.querySelectorAll("[data-carousel-dot]").forEach((dot, index) => {
    dot.setAttribute("aria-label", copy.carouselGoTo.replace("{0}", index + 1));
  });

  try {
    window.localStorage.setItem(languageStorageKey, language);
  } catch {
    // Ignore storage failures in restricted environments.
  }
}

function resolvePreferredLanguage() {
  let preferredLanguage = "en";

  try {
    preferredLanguage = window.localStorage.getItem(languageStorageKey) || preferredLanguage;
  } catch {
    preferredLanguage = "en";
  }

  if (!translations[preferredLanguage]) {
    const browserLanguage = (navigator.language || navigator.userLanguage || "en").toLowerCase();

    if (browserLanguage.startsWith("pt")) {
      preferredLanguage = "pt-BR";
    } else if (browserLanguage.startsWith("es")) {
      preferredLanguage = "es";
    } else if (browserLanguage.startsWith("fr")) {
      preferredLanguage = "fr";
    } else if (browserLanguage.startsWith("de")) {
      preferredLanguage = "de";
    } else if (browserLanguage.startsWith("ru")) {
      preferredLanguage = "ru";
    } else {
      preferredLanguage = "en";
    }
  }

  return translations[preferredLanguage] ? preferredLanguage : "en";
}

function setupLanguageSwitcher() {
  const switcher = document.querySelector("[data-language-switcher]");

  if (!switcher) {
    return;
  }

  const toggle = switcher.querySelector("[data-language-toggle]");
  const menu = switcher.querySelector("[data-language-menu]");
  const options = Array.from(switcher.querySelectorAll("[data-language-option]"));

  function closeMenu() {
    menu.hidden = true;
    toggle.setAttribute("aria-expanded", "false");
  }

  function openMenu() {
    menu.hidden = false;
    toggle.setAttribute("aria-expanded", "true");
  }

  toggle.addEventListener("click", () => {
    if (menu.hidden) {
      openMenu();
      return;
    }

    closeMenu();
  });

  options.forEach((option) => {
    option.addEventListener("click", () => {
      applyLanguage(option.dataset.languageOption);
      closeMenu();
    });
  });

  document.addEventListener("click", (event) => {
    if (!switcher.contains(event.target)) {
      closeMenu();
    }
  });

  document.addEventListener("keydown", (event) => {
    if (event.key === "Escape") {
      closeMenu();
    }
  });

  applyLanguage(resolvePreferredLanguage());
}

setupLanguageSwitcher();

if (carousel) {
  const track = carousel.querySelector("[data-carousel-track]");
  const slides = Array.from(track.children);
  const prevButton = carousel.querySelector("[data-carousel-prev]");
  const nextButton = carousel.querySelector("[data-carousel-next]");
  const dotsContainer = document.querySelector("[data-carousel-dots]");

  let activeIndex = 0;

  function renderDots() {
    dotsContainer.innerHTML = "";

    slides.forEach((_, index) => {
      const dot = document.createElement("button");
      dot.type = "button";
      dot.className = "carousel-dot";
      dot.dataset.carouselDot = "";
      const language = resolvePreferredLanguage();
      dot.setAttribute("aria-label", translations[language].carouselGoTo.replace("{0}", index + 1));

      if (index === activeIndex) {
        dot.classList.add("is-active");
      }

      dot.addEventListener("click", () => updateCarousel(index));
      dotsContainer.appendChild(dot);
    });
  }

  function updateCarousel(index) {
    activeIndex = (index + slides.length) % slides.length;
    track.style.transform = `translateX(-${activeIndex * 100}%)`;

    slides.forEach((slide, slideIndex) => {
      slide.classList.toggle("is-active", slideIndex === activeIndex);
    });

    renderDots();
  }

  prevButton.addEventListener("click", () => updateCarousel(activeIndex - 1));
  nextButton.addEventListener("click", () => updateCarousel(activeIndex + 1));

  updateCarousel(0);
}