#ifndef MyAppVersion
  #define MyAppVersion "1.0.0"
#endif
#ifndef AppPublishDir
  #define AppPublishDir "..\publish"
#endif

[Setup]
AppId={{8C3E6A21-4B7D-4F19-9E2A-7B1C0D4E5F60}}
AppName=LGOledCompanion
AppVersion={#MyAppVersion}
AppPublisher=canetex
AppPublisherURL=https://github.com/canetex/LGOledCompanion
AppSupportURL=https://github.com/canetex/LGOledCompanion/issues
DefaultDirName={localappdata}\LGOledCompanion
DefaultGroupName=LGOledCompanion
DisableProgramGroupPage=yes
LicenseFile=..\LICENSE
OutputDir=..\dist
OutputBaseFilename=LGOledCompanion-Setup-{#MyAppVersion}
SetupIconFile=..\src\LGOledCompanion\Assets\app.ico
UninstallDisplayIcon={app}\LGOledCompanion.exe
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible

[Languages]
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: "english"; MessagesFile: "compiler:Default.isl"

[Tasks]
Name: "desktopicon"; Description: "Criar atalho na área de trabalho"; GroupDescription: "Atalhos:"; Flags: unchecked

[Files]
Source: "{#AppPublishDir}\LGOledCompanion.exe"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\LGOledCompanion"; Filename: "{app}\LGOledCompanion.exe"
Name: "{autodesktop}\LGOledCompanion"; Filename: "{app}\LGOledCompanion.exe"; Tasks: desktopicon

[Run]
Filename: "{app}\LGOledCompanion.exe"; Description: "Iniciar LGOledCompanion"; Flags: nowait postinstall skipifsilent
