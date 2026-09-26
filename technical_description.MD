# LGOledCompanion — descrição técnica

App pessoal de desktop Windows para TV OLED LG: de dia mostra um slideshow de fotos em tela cheia; de noite apaga o painel WebOS. Os dois trabalhos são first-class. Este app é a única política de idle/tela da máquina — o screensaver e o “apagar tela” do Windows ficam desligados. Sleep/hibernate continua com o plano de energia do SO.

Nome único em exe, bandeja, título da janela e mutex: `LGOledCompanion`.

Repositório: `https://github.com/canetex/LGOledCompanion` (privado).

---

## 1. O que o app faz

### Dia

Depois de `Tempo_Inatividade` minutos sem mouse/teclado, abre uma janela fullscreen em **todas** as telas ligadas ao Windows e mostra a **mesma** foto ao mesmo tempo. A TV permanece ligada. Um ciclo de overlay anima a opacidade sobre a imagem e, ao terminar, avança para a próxima foto.

### Noite

Sem slideshow e sem janela preta. Só envia `-screenoff` para o device WebOS configurado no LGTVCompanion. Os outros monitores ficam como estão.

### Wake e resume

Qualquer input encerra o slideshow na hora. De noite, manda `-screenon`. No resume do Windows, fecha o slideshow se houver e manda `-screenon`.

---

## 2. Features

- Instância única (mutex). Um segundo start só abre/foca o Settings.
- Ícone na bandeja: Settings, Pausar/Retomar, Testar TV, Sair.
- Pausa em memória: bloqueia slideshow e `screenoff` até Retomar. Não sobrevive a reinício.
- Detecção de idle via `GetLastInputInfo`.
- Bloqueio quando um app pede `Display Required` (YouTube/Netflix/player em fullscreen).
- Bloqueio com a janela de Settings aberta.
- Janela noturna com wrap de meia-noite (ex.: 23:00–07:00). Início == fim desliga o modo noite.
- Troca imediata se o idle atravessar a fronteira dia/noite.
- Slideshow em todas as telas, mesma foto, Cover (pixel bounds + DPI).
- Pasta de fotos recursiva: `jpg`, `jpeg`, `png`, `webp`, `bmp`.
- Ordem aleatória a cada ciclo completo; arquivo corrompido é pulado.
- Orientação EXIF respeitada.
- WebP via ImageSharp 3.x (WPF não decodifica WebP nativo).
- Overlay colorido com color picker, opacidade 0–90%.
- Ciclo de overlay em 3 fases (fade-out, hold da imagem, fade-in), sliders 0–120 s cada.
- Pré-visualização ao vivo no Settings (foto + overlay + ciclo).
- Color picker do overlay e swatch com hex.
- Autostart no login (atalho `.lnk` na pasta Startup).
- `config.json` ao lado do exe, gravado **somente** em Salvar.
- Log em disco só com `Modo_Debug`.
- Integração com `LGTVcli.exe`: `-screenoff` / `-screenon`, 3 retries com backoff.
- “Testar TV”: `-screenoff` ~3 s e `-screenon`. Ignora `Display Required` e Settings aberto. Desabilitado se o app estiver pausado.
- Fullscreen 4K/high-DPI via conversão pixel → DIP e `SetWindowPos`.
- Decode de imagem limitado a 3840 px de largura (evita 8K estourar memória).

---

## 3. Tecnologias

| Peça | Uso |
| --- | --- |
| C# / .NET 8 LTS | Runtime. Host WPF em `net8.0-windows`; Core e testes em `net8.0`. |
| WPF | Janelas de slideshow e Settings, overlay, pré-visualização. |
| Windows Forms | `NotifyIcon` na bandeja, `FolderBrowserDialog`, `ColorDialog`. |
| SixLabors.ImageSharp 3.1.11 (Apache 2.0) | Identificar arquivos, decodificar WebP → PNG, `AutoOrient`. **Não** usar 4.x (licença paga). |
| xUnit 2.9 | Testes do Core. |
| Win32 `GetLastInputInfo` | Tempo desde o último input. |
| `NtPowerInformation` (`SystemExecutionState`) | Flag `ES_DISPLAY_REQUIRED`. |
| `SetWindowPos` + `HWND_TOPMOST` | Posicionar a janela nos pixels reais da tela (4K / DPI). |
| `WScript.Shell` | Criar atalho de autostart. |
| System.Text.Json | Ler/gravar `config.json`. |
| LGTVCompanion / `LGTVcli.exe` | Pairing e websocket WebOS ficam no LGTVCompanion. Este app só chama a CLI. |

SDK pinado em `global.json` (SDK 9.x compilando alvo net8.0). DPI: `PerMonitorV2`.

Publish previsto: self-contained, `win-x64`, single-file.

```
dotnet test
dotnet run --project src/LGOledCompanion
dotnet publish src/LGOledCompanion -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
```

---

## 4. Arquitetura

```
LGOledCompanion.sln
├── src/LGOledCompanion.Core          regras sem WPF (testável)
├── src/LGOledCompanion               host WPF + bandeja + Win32
└── tests/LGOledCompanion.Core.Tests  xUnit
```

### Core (`LGOledCompanion.Core`)

| Tipo | Responsabilidade |
| --- | --- |
| `AppConfig` | Modelo do JSON e defaults. |
| `ConfigStore` | Load/Save. Sem arquivo → defaults em memória, **não** cria JSON. |
| `SessionPolicy` | Decide a próxima ação a partir do estado + idle + noite + bloqueios. |
| `NightWindow` | Intervalo HH:mm, inclusive wrap de meia-noite. |
| `OverlayCycle` | Timeline 3 fases → opacidade + `Completed`. Durações clamp 0–120 s. |
| `OverlayTint` | Parse/format de `#RRGGBB` (aceita `#RGB`). Inválido → preto. |
| `PhotoCatalog` | Walk recursivo, shuffle Fisher-Yates, `CanOpen` via ImageSharp Identify. |
| `PhotoPlayback` | Próximo índice carregável (pula arquivo ruim). |
| `ScreenPlacement` | Pixel bounds → DIP. |
| `SettingsPreview` | Anterior/próxima na pré-visualização. |
| `LgtvClient` | `-screenoff` / `-screenon` com 3 tentativas e backoff 200 ms / 400 ms. |
| `WebpDecoder` | WebP → PNG bytes + AutoOrient. |
| `FileLogger` | Append em `log.txt` só se debug estiver on. |

### Host WPF (`LGOledCompanion`)

| Tipo | Responsabilidade |
| --- | --- |
| `App` | Mutex `LGOledCompanion`; segundo processo sinaliza `LGOledCompanion.OpenSettings`. |
| `SessionHost` | Timer 50 ms, idle, política, slideshow, CLI, bandeja. |
| `SlideshowWindow` | Fullscreen por tela, foto + overlay. |
| `SettingsWindow` | Formulário, sliders, color picker, preview animado. |
| `PhotoImageLoader` | Decode WPF/`FileStream` ou WebP; `DecodePixelWidth = 3840`. |
| `NativeIdle` | `GetLastInputInfo` + `NtPowerInformation`. |
| `AutostartService` | `.lnk` em `%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup`. |

A lógica de negócio fica no Core. O host só aplica: abrir/fechar janelas, chamar a CLI, desenhar overlay.

---

## 5. Política de sessão

Estados: `Desktop`, `Slideshow`, `NightScreenOff`.

Bloqueios (`blocked`): pausa **ou** Settings aberto **ou** `Display Required`.

| Estado | Condição | Ação |
| --- | --- | --- |
| Desktop | blocked ou ativo | nenhuma |
| Desktop | idle + dia | `StartSlideshow` |
| Desktop | idle + noite | `ScreenOff` |
| Slideshow | blocked ou ativo | `StopSlideshow` |
| Slideshow | idle + noite | `SwitchToNight` (fecha janelas + `screenoff`) |
| NightScreenOff | blocked ou ativo | `ScreenOn` |
| NightScreenOff | idle + dia | `SwitchToDay` (`screenon` + slideshow) |

Falha da CLI no `screenoff`: balloon na bandeja, estado volta para Desktop, `_hold_cli_until_activity` impede novo `screenoff` até o usuário voltar a usar o PC. Slideshow de dia **não** depende da CLI.

“Testar TV” ignora bloqueios de Settings/`Display Required`, mas não roda se o app estiver pausado.

---

## 6. Ciclo de overlay e transição de imagem

Três durações independentes, cada uma **0–120 segundos** (slider e clamp no Core):

1. **Fade-out** (`Tempo_FadeOut_Overlay`, default 10 s)  
   Overlay começa em **100%** e desce até a opacidade configurada (`Opacidade_Overlay`).
2. **Hold / transição de imagem** (`Tempo_Transicao`, default 30 s)  
   Overlay fica em **0%** (foto limpa).
3. **Fade-in** (`Tempo_FadeIn_Overlay`, default 10 s)  
   Overlay sobe de **0%** até a opacidade configurada.

Ao completar o ciclo: próxima foto (shuffle quando a lista acaba) e reinício em 100%.

Exemplo canônico: **10 + 30 + 10 = 50 s**.

Se as três durações forem 0, o total vira **1 s** (segurança para não travar o avanço).

Há um corte de opacidade no fim do fade-out: o overlay chega no valor configurado e, no hold, vai a 0%. No fim do fade-in chega de novo no configurado e a foto troca.

O Settings e o slideshow usam o mesmo `OverlayCycle.At(...)`. Tick de 50 ms.

---

## 7. Configurações

Arquivo: `config.json` **ao lado do exe**. Só existe depois do primeiro **Salvar**. Sem arquivo, o app sobe com defaults em memória.

Autostart **não** vai no JSON: é presença do atalho `LGOledCompanion.lnk` na pasta Startup.

### Chaves do `config.json`

| Chave | Tipo | Default | Faixa / formato | Significado |
| --- | --- | --- | --- | --- |
| `Tempo_Inatividade` | int | `5` | ≥ 1 (Settings; mínimo efetivo 1) | Minutos sem input até o app agir. |
| `Tempo_FadeOut_Overlay` | int | `10` | 0–120 s | Fade de overlay 100% → opacidade configurada. |
| `Tempo_FadeIn_Overlay` | int | `10` | 0–120 s | Fade de overlay 0% → opacidade configurada. |
| `Tempo_Transicao` | int | `30` | 0–120 s | Hold com overlay 0% (tempo da foto “limpa”). |
| `Opacidade_Overlay` | int | `0` | 0–90, passo 10 no slider | Opacidade alvo do overlay (%). |
| `Cor_Overlay` | string | `#000000` | `#RRGGBB` ou `#RGB` | Cor do overlay (color picker). Inválido → preto. |
| `Pasta_Fotos` | string | `""` | path local | Raiz recursiva das imagens. Vazio/inexistente → fullscreen preto de dia. |
| `Horario_Noite_Inicio` | string | `23:00` | `HH:mm` | Início da janela noturna. |
| `Horario_Noite_Fim` | string | `07:00` | `HH:mm` | Fim da janela noturna. |
| `Device_WebOS` | string | `Device1` | nome do LGTVCompanion | Alvo da CLI, **não** o índice de display do Windows. |
| `Caminho_LGTVcli` | string | `C:\Program Files\LGTV Companion\LGTVcli.exe` | path do exe | CLI do LGTVCompanion. |
| `Modo_Debug` | bool | `false` | — | Se `true`, grava `log.txt` ao lado do exe. |

Validação no Salvar:

- Idle inválido → 5.
- Overlay clamp 0–90.
- Horário inválido → MessageBox, não grava.
- Device em branco → `Device1`.
- Durações do ciclo vêm dos sliders (0–120). No runtime, valores fora da faixa no JSON são clampados a 0–120.

### Controles da UI (Settings)

| Controle | Liga em |
| --- | --- |
| Idle (minutos) | `Tempo_Inatividade` |
| Slider Fade-out overlay (0–120 s) | `Tempo_FadeOut_Overlay` |
| Slider Fade-in overlay (0–120 s) | `Tempo_FadeIn_Overlay` |
| Slider Transição imagem (0–120 s) | `Tempo_Transicao` |
| Label “Ciclo total” | soma das 3 fases (mínimo 1 s se todas 0) |
| Slider Overlay (0–90%) | `Opacidade_Overlay` |
| Botão cor + hex | `Cor_Overlay` |
| Pasta de fotos + “...” | `Pasta_Fotos` |
| Noite início / fim | `Horario_Noite_*` |
| Device WebOS | `Device_WebOS` |
| LGTVcli.exe + “...” | `Caminho_LGTVcli` |
| Modo debug | `Modo_Debug` |
| Iniciar com o Windows | atalho Startup |
| Pré-visualização + Anterior/Próxima | mesma pasta e mesmo ciclo, sem gravar |
| Salvar | escreve JSON, aplica autostart, recarrega config no host |

A pré-visualização anima o ciclo em tempo real e avança a foto ao completar. Mudar um slider de tempo reinicia o ciclo no preview.

---

## 8. Fotos e tela

- Extensões: `.jpg` `.jpeg` `.png` `.webp` `.bmp` (case-insensitive).
- Walk recursivo; pastas sem permissão são ignoradas.
- Shuffle a cada volta na lista.
- `PhotoCatalog.CanOpen` / `PhotoImageLoader.Load` pulam arquivo ilegível.
- Pasta vazia ou tudo ilegível: de dia, fullscreen preto em todas as telas; overlay ainda anima.
- Plug/unplug de monitor no meio do slideshow é ignorado. Janela órfã some no wake.
- Posicionamento: bounds em pixels da `Screen` → DIP (`ScreenPlacement`) + `SetWindowPos` no HWND, topmost. Evita a caixa 200×200 em TVs 4K high-DPI.
- Stretch da foto: `Uniform` (letterbox preto, sem crop).

---

## 9. TV WebOS / LGTVCompanion

Este app **não** faz pairing, WOL nem websocket. Exige [LGTVCompanion](https://github.com/JPersson77/LGTVCompanion) instalado e o **gerenciamento automático desligado**, para não competir no idle.

Comandos:

```
LGTVcli.exe -screenoff "Device1"
LGTVcli.exe -screenon "Device1"
```

Timeout 15 s por tentativa; 3 tentativas; backoff 200 ms e 400 ms. Exit code ≠ 0 ou exceção → falha.

O device é o nome cadastrado no LGTVCompanion (`Device1` ou nome amigável), não o índice do monitor no Windows.

---

## 10. Logging e debug

Com `Modo_Debug = true`, append em `log.txt` ao lado do exe:

```
yyyy-MM-dd HH:mm:ss mensagem
```

Eventos típicos: `started`, `config saved`, `paused` / `resumed`, `slideshow start photos=N screens=S`, `screenoff ok` / `failed`, `screenon ok` / `failed`.

Com debug off, nada é escrito em disco.

---

## 11. Fora do escopo (v1)

- Reimplementar pairing, WOL ou websocket WebOS.
- Apagar displays pelo Windows.
- Heurística de processo/janela para detectar mídia (só `Display Required`).
- Foto diferente por tela.
- Impedir sleep do PC.
- Persistir pausa entre reinícios.
- Instalador / distribuição pública.
- Vídeo no slideshow (arquivos não-imagem na pasta são ignorados).

---

## 12. Como usar na primeira vez

1. `dotnet run --project src/LGOledCompanion` (ou o exe publicado).
2. Bandeja → Settings.
3. Pasta de fotos, `LGTVcli.exe`, device WebOS.
4. Ajustar idle, overlay, cor e os 3 sliders do ciclo (ex.: 10 / 10 / 30).
5. Desligar o auto-manage do LGTVCompanion.
6. Salvar. O `config.json` aparece ao lado do exe.
7. Opcional: “Iniciar com o Windows” e “Testar TV”.

O horário noturno default no código é 23:00–07:00. Se a TV for usada de madrugada como tela principal, ajuste `Horario_Noite_*` (ex.: 03:00–08:00) para o idle de dia ainda abrir o slideshow.
