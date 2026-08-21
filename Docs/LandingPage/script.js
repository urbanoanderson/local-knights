const carousel = document.querySelector("[data-carousel]");
const languageStorageKey = "local-knights-landing-language";

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
    heroEyebrow: "Open source. Local-first. Club-ready.",
    heroTitle: "The open source community chess club management software",
    heroText: "Run your chess club from one modern desktop app. Register players, record games, organize competitions, maintain internal ratings and keep full ownership of your data without server overhead and completely free.",
    downloadButton: "Download",
    carouselPrev: "Previous screenshot",
    carouselNext: "Next screenshot",
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
    heroEyebrow: "Código aberto. Controle total. Pronto para clubes.",
    heroTitle: "Seu gestor de código aberto para clubes de xadrez",
    heroText: "Gerencie seu clube de xadrez em um só aplicativo desktop moderno. Cadastre jogadores, registre partidas, organize competições, mantenha ratings internos e preserve total controle dos seus dados sem dependência de nuvem ou servidores e de forma gratuita.",
    downloadButton: "Baixar",
    carouselPrev: "Imagem anterior",
    carouselNext: "Próxima imagem",
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
  }
};

function applyLanguage(language) {
  const copy = translations[language] || translations.en;

  document.documentElement.lang = language === "pt-BR" ? "pt-BR" : "en";

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

  const currentFlag = document.querySelector("[data-current-flag]");

  if (currentFlag) {
    currentFlag.textContent = language === "pt-BR" ? "🇧🇷" : "🇺🇸";
  }

  document.querySelectorAll("[data-language-option]").forEach((button) => {
    button.classList.toggle("is-active", button.dataset.languageOption === language);
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

    if (browserLanguage === "pt-br" || browserLanguage.startsWith("pt")) {
      preferredLanguage = "pt-BR";
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
      dot.setAttribute("aria-label", `Go to screenshot ${index + 1}`);

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