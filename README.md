# LGOledCompanion

App para Windows: slideshow de fotos de dia e proteção da OLED de noite (apaga o painel WebOS da TV LG).

**Licença:** MIT — pode clonar, fazer fork e modificar.

## Requisitos

- Windows 10/11 64 bits
- TV LG WebOS na mesma rede (só para o modo noite / Testar TV)

O slideshow de dia funciona sem a TV. Não precisa ter o [LGTV Companion](https://github.com/JPersson77/LGTVCompanion) instalado.

## Créditos

O controle WebOS nativo (pareamento SSAP, `screenoff`/`screenon` e Wake-on-LAN) foi incorporado a partir do [LGTV Companion](https://github.com/JPersson77/LGTVCompanion), de [JPersson77](https://github.com/JPersson77).

## Instalar

1. Baixe o instalador `LGOledCompanion-Setup-x.y.z.exe` na [página de Releases](https://github.com/canetex/LGOledCompanion/releases).
2. Execute o setup (instalação por usuário, sem admin).
3. Abra o app (ícone na bandeja).
4. Clique com o botão direito → **Settings**.

Alternativa sem instalador: baixe `LGOledCompanion.exe` da mesma Release e coloque numa pasta. O `config.json` nasce ao lado do exe no primeiro **Salvar**.

## Primeiro uso

1. Pasta de fotos (jpg, jpeg, png, webp, bmp).
2. Em **TV WebOS**: IP da TV → **Parear TV** → aceite o pedido na tela da TV.
3. Opcional: MAC para Wake-on-LAN se a TV não responder ao `screenon`.
4. Em **Windows (tela e energia)**:
   - **Abrir configurações do screensaver** → escolha **Nenhum**.
   - **Abrir configurações de desligar o monitor** → **Nunca** (na tomada e, se quiser, na bateria).
5. **Salvar Configurações**.
6. Opcional: **Iniciar com o Windows** e **Testar TV**.

Este app deve ser a única política de idle da máquina. Sleep/hibernate do Windows continua no plano de energia.

## Uso

| Bandeja | Função |
| --- | --- |
| Settings | Configurar e pré-visualizar o overlay |
| Pausar / Retomar | Bloqueia slideshow e screenoff até retomar |
| Testar TV | `screenoff` ~3 s e `screenon` |
| Sair | Encerra o app |

De dia, após o idle: slideshow em todas as telas. De noite: só `screenoff` na TV pareada. Qualquer input encerra o slideshow; de noite manda `screenon`.

## Compilar

```
dotnet test
dotnet publish src/LGOledCompanion -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -o publish
```

Documentação técnica: [technical_description.MD](technical_description.MD).
