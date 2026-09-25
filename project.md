# Smart Photo Screensaver (Protetor de Tela Inteligente)

## 📌 Visão Geral
Uma aplicação de desktop leve e inteligente que funciona como um protetor de tela personalizado. Ao invés de apenas exibir fotos, a aplicação roda silenciosamente em segundo plano, monitora a inatividade do sistema com inteligência (evitando interrupções durante o consumo de mídia) e oferece recursos avançados de economia de energia, incluindo o desligamento de monitores baseado em horários.

## ✨ Funcionalidades Principais

*   **Execução em Segundo Plano:** O aplicativo não polui a barra de tarefas principal. Ele é minimizado para a Bandeja do Sistema (System Tray / Taskbar) e roda silenciosamente como um serviço de background.
*   **Apresentação de Fotos em Tela Cheia:** Após um período configurável de inatividade (idle), o sistema inicia um slideshow em tela cheia (modo protetor de tela).
*   **Diretório Customizável:** As imagens exibidas no slideshow são carregadas a partir de uma pasta local definida pelo usuário nas configurações.
*   **Controle de Brilho via Overlay:** Possibilidade de configurar uma camada preta (overlay) translúcida por cima das fotos. Isso permite reduzir o brilho geral da tela sem alterar as configurações físicas do monitor, ideal para ambientes escuros.
*   **Modo de Supressão Inteligente (Media-Aware):** A aplicação detecta se o usuário está assistindo a vídeos (YouTube, filmes, Netflix, etc.) no navegador ou em players de mídia, impedindo que o protetor de tela seja ativado de forma indevida.
*   **Despertar Instantâneo (Instant Wake):** Ao menor movimento do mouse, clique ou pressionamento de qualquer tecla, o protetor de tela é imediatamente encerrado, retornando o computador ao estado normal de uso sem engasgos.
*   **Desligamento Agendado do Monitor:** Em vez de exibir o protetor de tela, é possível configurar um intervalo de horário específico (ex: madrugada) onde a inatividade acionará o desligamento real do monitor. 
    *   *Nota de Integração:* Este recurso pode ser utilizado em conjunto com ferramentas como o [LGTVCompanion](https://github.com/JPersson77/LGTVCompanion) para controlar telas OLED e TVs, evitando burn-in e economizando energia.

## ⚙️ Configurações Disponíveis

A aplicação possui um menu de configurações acessível pelo ícone na bandeja do sistema, permitindo ajustar os seguintes parâmetros:

- `Tempo_Inatividade`: Tempo em minutos até a ativação do protetor (ex: 5 minutos).
- `Pasta_Fotos`: Caminho do diretório local contendo as imagens (ex: `C:\Imagens\Screensaver`).
- `Tempo_Transicao`: Tempo de exibição de cada foto no slideshow (ex: 10 segundos).
- `Opacidade_Overlay`: Nível de escurecimento da tela de 0% (desativado) a 90% (muito escuro).
- `Horario_Desligar_Monitor_Inicio`: Horário de início para a regra de desligamento do monitor (ex: 23:00).
- `Horario_Desligar_Monitor_Fim`: Horário de fim para a regra de desligamento do monitor (ex: 07:00).

## 🛠️ Sugestões de Arquitetura e Tecnologias (Para o Desenvolvedor)

Caso esteja decidindo como construir a aplicação, aqui estão algumas sugestões:

*   **Linguagem/Framework:** 
    *   *C# (.NET / WPF):* Excelente para integração profunda com as APIs do Windows (detectar inatividade real, modo tela cheia, system tray).
    *   *Python (PyQt/Tkinter + pynput):* Rápido para prototipar e possui bibliotecas fáceis para capturar inatividade e desenhar janelas fullscreen.
    *   *Electron:* Bom se você quiser usar tecnologias web (HTML/CSS/JS) para desenhar a interface de configurações e o slideshow, mas pode consumir mais memória RAM.
*   **Detecção de Mídia (Não interromper vídeos):** No Windows, pode ser feito monitorando a API `SystemParametersInfo` ou verificando o status de "Display Required" do sistema operativo (que o navegador/player de vídeo ativa quando está em tela cheia/reproduzindo).

## 🚀 Como Executar
*(Esta seção será preenchida após o desenvolvimento da aplicação, contendo os passos de instalação e execução).*