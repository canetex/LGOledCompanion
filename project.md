# LGOledCompanion

App pessoal de desktop Windows: slideshow de fotos de dia e proteção da OLED de noite. Os dois jobs são first-class.

Este app é a única política de idle/tela desta máquina. O screensaver e o “apagar tela” do Windows ficam desligados. Sleep/hibernate continua com o plano de energia do SO.

Nome único em exe, bandeja, título da janela e mutex: `LGOledCompanion`.

## Fora do v1

- Reimplementar pairing, WOL ou websocket WebOS
- Apagar displays pelo Windows
- Heurística de processo/janela para detectar mídia
- Overlay em cima de fullscreen preto
- Foto diferente por tela
- Impedir sleep do PC
- Persistir pausa entre reinícios
- Instalador / distribuição pública

## Comportamento

### Idle

- Atividade = último mouse/teclado via `GetLastInputInfo`.
- Após `Tempo_Inatividade` sem input, o app age (slideshow de dia ou `screenoff` de noite), salvo bloqueio.
- Bloqueios (nenhum slideshow, nenhum `screenoff` automático):
  - Power Request `Display Required` (YouTube/Netflix/player em fullscreen)
  - Janela de settings aberta
  - Pausa na bandeja
- Wake: qualquer input encerra o slideshow na hora. De noite, manda `screenon`.
- Sleep do Windows: o app não mexe. No resume, fecha slideshow se houver e manda `screenon`.

### Dia vs noite

- **Dia:** slideshow em todas as telas ligadas ao Windows, a mesma foto ao mesmo tempo. TV ligada.
- **Noite:** sem slideshow e sem janela preta. Só `screenoff` na TV pareada. Os outros monitores ficam como estão.
- Se o idle atravessar a fronteira do horário, troca na hora. Se ainda estiver idle às 07:00: `screenon` + slideshow.
- Se `Horario_Noite_Inicio` > `Horario_Noite_Fim`, o intervalo atravessa meia-noite (ex.: 23:00–07:00).
- Se início == fim, o modo noturno está desligado.

### TV (WebOS)

- Pareamento nativo (SSAP) pelo Settings: IP da TV → **Parear TV** → aceite o pedido na tela.
- Noite / idle noturno: `turnOffScreen`. Wake e resume: `turnOnScreen`.
- O alvo é o IP da TV na LAN (`Tv_Host`), não o índice de display do Windows.
- MAC opcional (`Tv_Mac`) para WOL no `screenon`.
- Falha SSAP: 3 retries com backoff, balloon na bandeja, nova tentativa no próximo ciclo de idle. O slideshow de dia não depende da TV.
- **Testar TV** (bandeja): `screenoff` ~3 s e depois `screenon`. Ignora `Display Required` e settings aberto. Fica desabilitado se o app estiver pausado.

### Slideshow e fotos

- Pasta local, busca recursiva.
- Extensões: `jpg`, `jpeg`, `png`, `webp`, `bmp`.
- Ordem aleatória a cada ciclo. Respeita orientação EXIF.
- WebP via ImageSharp (WPF não decodifica WebP nativo).
- Overlay preto 0–90% só em cima de foto. Fullscreen preto não leva overlay.
- Pasta vazia, path inexistente ou todas as imagens ilegíveis: de dia, fullscreen preto em todas as telas. Arquivo isolado corrompido é pulado.
- Plug/unplug de monitor no meio do slideshow é ignorado. Janela órfã some só no wake.

## Bandeja

Menu: Settings, Pausar/Retomar, Testar TV, Sair.

- **Pausa:** bloqueia slideshow e `screenoff` até Retomar. Só em memória; o próximo start (login ou Sair+abrir) volta ativo.
- Instância única (mutex). Um segundo start só foca o settings.

## Configuração

Arquivo `config.json` **ao lado do exe**. Só é gravado quando o usuário clica Salvar. Sem arquivo, o app sobe com defaults em memória e não cria o JSON sozinho.

| Chave | Default | Significado |
| --- | --- | --- |
| `Tempo_Inatividade` | 5 minutos | Idle até agir |
| `Tempo_Transicao` | 10 segundos | Tempo de tela de cada foto (corte seco, sem fade) |
| `Opacidade_Overlay` | 0% | Escurecimento 0–90% só sobre foto |
| `Pasta_Fotos` | vazio | Pasta recursiva de imagens |
| `Horario_Noite_Inicio` | 23:00 | Início da janela noturna |
| `Horario_Noite_Fim` | 07:00 | Fim da janela noturna |
| `Tv_Host` | vazio | IP da TV na LAN |
| `Tv_ClientKey` | vazio | Chave SSAP após parear |
| `Tv_Mac` | vazio | MAC para WOL (opcional) |
| `Modo_Debug` | off | Se on, grava log em disco |

Autostart no login do usuário (atalho na pasta Startup).

## Stack

- C# / WPF / **.NET 8 LTS**
- Publish: self-contained, `win-x64`, single-file
- Pasta portátil: `LGOledCompanion.exe` + `config.json` (depois do primeiro Salvar)
- ImageSharp só para decodificar WebP
- Log em disco apenas com `Modo_Debug` ligado
- ImageSharp 3.x (Apache) só para WebP

## Como executar

Na pasta do repositório:

```
dotnet test
dotnet run --project src/LGOledCompanion
```

Publicar o exe único self-contained:

```
dotnet publish src/LGOledCompanion -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
```

O `config.json` só aparece ao lado do exe depois de Salvar no settings. No primeiro uso, abra o ícone da bandeja → Settings, aponte a pasta de fotos, informe o IP da TV e pareie.
