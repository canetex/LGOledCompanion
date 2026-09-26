// AppConfig.cs

namespace LGOledCompanion.Core;

public sealed class AppConfig
{
    public int Tempo_Inatividade { get; set; } = 5;
    public int Tempo_FadeOut_Overlay { get; set; } = 10;
    public int Tempo_FadeIn_Overlay { get; set; } = 10;
    public int Tempo_Transicao { get; set; } = 30;
    public int Opacidade_Overlay { get; set; } = 0;
    public string Cor_Overlay { get; set; } = "#000000";
    public string Cor_Destaque { get; set; } = "#FF3B7C";
    public string Pasta_Fotos { get; set; } = string.Empty;
    public string Horario_Noite_Inicio { get; set; } = "23:00";
    public string Horario_Noite_Fim { get; set; } = "07:00";
    public string Tv_Host { get; set; } = string.Empty;
    public string Tv_ClientKey { get; set; } = string.Empty;
    public string Tv_Mac { get; set; } = string.Empty;
    public bool Modo_Debug { get; set; }
}
