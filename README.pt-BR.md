[English](README.md) | **Português (Brasil)** | [Español](README.es.md) | [Français](README.fr.md) | [Deutsch](README.de.md) | [Русский](README.ru.md)

# Local Knights

[![License: MPL 2.0](https://img.shields.io/badge/License-MPL%202.0-brightgreen.svg)](LICENSE)
[![Download Latest Release](https://img.shields.io/github/v/release/urbanoanderson/local-knights?label=Latest%20Release)](https://github.com/urbanoanderson/local-knights/releases)
[![Patrocine](https://img.shields.io/github/sponsors/urbanoanderson?logo=githubsponsors&label=Sponsor)](https://github.com/sponsors/urbanoanderson)
![Platform: Windows](https://img.shields.io/badge/Platform-Windows%2010%2B-0078D6)
![Built with .NET 10](https://img.shields.io/badge/.NET-10-512BD4)

<p align="center">
	<img src="LocalKnights.WindowsDesktop/Resources/Icon/logo.png" alt="Logotipo do Local Knights" width="180">
</p>

<p align="center"><strong>O software de código aberto para gerenciamento de clubes de xadrez.</strong></p>

<p align="center">
	Local Knights é um aplicativo desktop moderno para administrar clubes de xadrez. Gerencie jogadores, registre partidas, organize competições, gere emparceiramentos e mantenha ratings internos do clube em uma interface moderna e elegante.
</p>

## Por que usar Local Knights ♟️

- Totalmente gratuito e de código aberto sob a licença MPL-2.0. Sem assinaturas, sem anúncios
- Experiência desktop totalmente local, sem dependência de nuvem. Navegação simples, layouts responsivos, temas claro e escuro e suporte multilíngue
- Dados do clube armazenados em um único arquivo de banco de dados protegido por senha. Criação e restauração simples de backups
- Perfeito para clubes comunitários de xadrez, grupos de amigos e programas escolares que precisam de algo mais estruturado que planilhas e querem um fluxo de trabalho gratuito e de código aberto

## Principais recursos 🚀

### Gerencie os membros do clube 👥

- Cadastre jogadores e mantenha perfis completos dos membros do clube
- Vincule perfis de jogadores ao [Chess.com](https://www.chess.com/), [Lichess](https://lichess.org/), [FIDE](https://www.fide.com/) e a outras organizações de xadrez
- Insira manualmente ou busque ratings nas plataformas externas compatíveis durante a criação do jogador
- Mantenha ratings separados para os membros em diferentes ritmos de jogo (Bullet, Blitz, Rápido e Clássico)

### Organize jogos competitivos 🏆

- Suporte aos sistemas de rating [Glicko-2](https://en.wikipedia.org/wiki/Glicko_rating_system#Glicko-2_algorithm) e [Elo](https://en.wikipedia.org/wiki/Elo_rating_system). Atualize os ratings à medida que partidas e resultados de torneios forem registrados
- Registre partidas individuais (válidas ou não para rating) e, se necessário, armazene suas notações em [PNG](https://en.wikipedia.org/wiki/Portable_Game_Notation)
- Organize competições [Suíças](https://en.wikipedia.org/wiki/Swiss-system_tournament), [Round-Robin](https://en.wikipedia.org/wiki/Round-robin_tournament), de [Eliminação](https://en.wikipedia.org/wiki/Single-elimination_tournament) e Matches entre dois jogadores. Acompanhe rodadas, emparceiramentos, classificações e resultados em um só lugar
- Gere emparceiramentos suíços com lógica aprovada pela FIDE por meio da biblioteca [Chesspairing](https://github.com/gnutterts/chesspairing/)
- Exporte competições em formato Excel de fácil utilização ou no formato TRF oficial da FIDE

### Experiência moderna de aplicativo desktop 💻

- Desenvolvido com tecnologias modernas e telas responsivas que continuam fáceis de usar em diferentes tamanhos de janela
- Feito para o uso prático diário, com navegação direta focada em jogadores, partidas, competições e configurações do clube
- Temas claro e escuro para diferentes ambientes e preferências
- Suporte à localização em diferentes idiomas

### Feito para o controle local 🔒

Ao contrário de ferramentas para clubes voltadas primeiro à Web, Local Knights foi projetado com foco no controle local.

- O banco de dados do seu clube fica na sua própria máquina. Nenhum backend hospedado é necessário
- Fazer backup é tão simples quanto copiar um arquivo `.db` criptografado
- A restauração é simples, portátil e independente de infraestrutura de terceiros

## Primeiros passos ▶️

Colocar um clube em funcionamento é intencionalmente simples.

1. Baixe a versão mais recente do aplicativo como arquivo zip na nossa página de [releases](https://github.com/urbanoanderson/local-knights/releases).
1. Extraia o conteúdo do zip para qualquer pasta desejada. Opcionalmente, crie um atalho para o executável "Local Knights.exe".
1. Inicie o aplicativo por meio de "Local Knights.exe".
1. Na primeira execução, clique no botão "+" e configure o nome do clube, as preferências de rating, a senha etc.
1. Entre no clube usando a senha criada e acesse o painel do clube.
1. Cadastre os jogadores individualmente pelo botão Criar na página Jogadores. Como alternativa, é possível importar em massa arquivos de jogadores no formato CSV ([arquivo de importação de exemplo](Scripts/Seed/player-bulk-import-sample.csv)).
1. Crie partidas e competições com os jogadores cadastrados nas respectivas páginas e formulários de criação.
1. Divirta-se e mantenha o xadrez vivo em sua comunidade! Os dados do clube serão armazenados em "C:\Users\<USERNAME>\AppData\Local\LocalKnights\Clubs".

![Demonstração do Local Knights](Docs/Screenshots/demo.gif)

## Privacidade e tratamento de dados ⚠️

Este aplicativo foi projetado com a privacidade em mente:

- **Somente armazenamento local**
  Todos os dados do clube (jogadores, partidas e competições) são armazenados localmente no computador do usuário em um banco de dados SQLite. O aplicativo não transmite, sincroniza nem compartilha dados com servidores externos ou terceiros.

- **Nenhuma coleta de dados pelo desenvolvedor**
  O desenvolvedor deste software não coleta, acessa nem processa qualquer informação pessoal inserida no aplicativo.

- **Responsabilidade dos responsáveis pelo clube**
  Os responsáveis e administradores de clubes que usam este software devem garantir a conformidade com as leis de proteção de dados aplicáveis, incluindo o Regulamento Geral sobre a Proteção de Dados (GDPR) da UE e a Lei Geral de Proteção de Dados (LGPD) do Brasil. Isso inclui informar os jogadores sobre o uso de seus dados, obter consentimento quando necessário e respeitar os direitos de acesso, correção e exclusão.

- **Recomendações de segurança**
  - Proteja o acesso ao seu computador e aos arquivos do banco de dados.
  - Faça backups seguros dos seus dados regularmente.
  - Considere criar senhas fortes para proteger os bancos de dados de clubes reais. As senhas não são armazenadas em backup pelo Local Knights nem por serviços de terceiros. Recomenda-se usar um gerenciador de senhas para evitar a perda de acesso aos dados do clube.

**Aviso:** Este software é fornecido “no estado em que se encontra”. Embora facilite o gerenciamento local de dados, a responsabilidade final pelo uso legal e em conformidade dos dados pessoais cabe aos responsáveis e administradores do clube.

## Informações para desenvolvedores 🛠️

### Tecnologias utilizadas ⚙️

- Aplicativo desktop .NET 10 desenvolvido com WPF e o padrão MVVM
- Armazenamento SQLite com SQLCipher para proteção por senha
- Biblioteca [Chesspairing](https://github.com/gnutterts/chesspairing/) para gerar emparceiramentos de acordo com as regras da FIDE

### Versionamento 🔢

- Local Knights usa o formato de versão `MAJOR.MINOR.REVISION`
- Versões estáveis são baseadas na branch `main`. Pré-lançamentos experimentais são baseados na branch `dev`
- Os fluxos de CI/CD compilam, testam, criam artefatos e geram pré-lançamentos e lançamentos automaticamente

### Compilar, testar e executar a partir do código-fonte 🧪

O SDK do .NET 10 é obrigatório

```bash
dotnet restore

dotnet build ProjectSolution.slnx

dotnet test ProjectSolution.slnx

dotnet run --project LocalKnights.WindowsDesktop/LocalKnights.WindowsDesktop.csproj
```

### Criar um pacote para a Microsoft Store 📦

No Windows, com os SDKs do .NET 10 e do Windows instalados, execute:

```powershell
.\Scripts\create-msix.ps1
```

Isso cria um MSIX x64 não assinado em
`LocalKnights.WindowsDesktop\bin\Packages\LocalKnights_<version>_x64.msix`, usando a
identidade de pacote associada ao Local Knights no Partner Center. Envie esse arquivo
na seção **Pacotes** da submissão; a Microsoft Store o assina após o envio.
Aumente a versão do projeto antes de enviar uma atualização.

## Licença 📄

Local Knights é licenciado sob a Mozilla Public License 2.0.

Consulte [LICENSE](LICENSE) para ler o texto completo da licença.

Para componentes de terceiros, bibliotecas incluídas e referências externas de implementação, consulte [THIRD-PARTY-NOTICES.md](THIRD-PARTY-NOTICES.md).

## Apoie o projeto 💖

Se o Local Knights ajudou você ou seu clube a passar menos tempo lutando com planilhas e mais tempo dando xeque-mate nos amigos, considere [patrocinar o projeto](https://github.com/sponsors/urbanoanderson). Até uma pequena contribuição ajuda a manter os cavalos em movimento — e cavalos são surpreendentemente caros. 🐴♟️
