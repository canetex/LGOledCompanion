// WebOsProtocol.cs
// Top 5: Register O(p), Parse O(bytes), Request O(1), ExtractMac O(1), WritePermissions O(p)
// p = permissões

using System.Text;
using System.Text.Json;

namespace LGOledCompanion.Core;

public enum WebOsMessageType
{
    Unknown,
    PairingPrompt,
    Registered,
    Success,
    Error
}

public sealed class WebOsMessage
{
    public WebOsMessageType Type { get; init; }
    public string? ClientKey { get; init; }
    public string? Mac { get; init; }
}

public static class WebOsProtocol
{
    public const string TurnOffScreen = "ssap://com.webos.service.tvpower/power/turnOffScreen";
    public const string TurnOnScreen = "ssap://com.webos.service.tvpower/power/turnOnScreen";
    public const string GetInfo = "ssap://com.webos.service.connectionmanager/getinfo";

    private static readonly string[] Permissions =
    [
        "LAUNCH",
        "CONTROL_POWER",
        "CONTROL_DISPLAY",
        "CONTROL_TV_SCREEN",
        "CONTROL_TV_STANDBY",
        "READ_POWER_STATE",
        "READ_NETWORK_STATE",
        "WRITE_NOTIFICATION_TOAST"
    ];

    public static string Register(string? client_key)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("type", "register");
            writer.WriteString("id", "register_0");
            writer.WriteStartObject("payload");
            writer.WriteBoolean("forcePairing", false);
            writer.WriteString("pairingType", "PROMPT");
            if (!string.IsNullOrWhiteSpace(client_key))
            {
                writer.WriteString("client-key", client_key);
            }

            writer.WriteStartObject("manifest");
            writer.WriteNumber("manifestVersion", 1);
            writer.WriteString("appVersion", "1.1");
            writer.WriteStartArray("permissions");
            // O(p) p = permissões
            foreach (var permission in Permissions)
            {
                writer.WriteStringValue(permission);
            }

            writer.WriteEndArray();
            writer.WriteEndObject();
            writer.WriteEndObject();
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static string Request(string id, string uri)
    {
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            writer.WriteString("id", id);
            writer.WriteString("type", "request");
            writer.WriteString("uri", uri);
            writer.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray());
    }

    public static WebOsMessage Parse(string json)
    {
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;
            var type = root.TryGetProperty("type", out var type_el) ? type_el.GetString() : null;
            var payload = root.TryGetProperty("payload", out var payload_el) ? payload_el : default;

            if (string.Equals(type, "registered", StringComparison.OrdinalIgnoreCase))
            {
                return new WebOsMessage
                {
                    Type = WebOsMessageType.Registered,
                    ClientKey = ReadClientKey(payload)
                };
            }

            if (string.Equals(type, "error", StringComparison.OrdinalIgnoreCase))
            {
                return new WebOsMessage { Type = WebOsMessageType.Error };
            }

            if (string.Equals(type, "response", StringComparison.OrdinalIgnoreCase) && payload.ValueKind == JsonValueKind.Object)
            {
                if (payload.TryGetProperty("pairingType", out var pairing) && pairing.GetString() == "PROMPT")
                {
                    return new WebOsMessage { Type = WebOsMessageType.PairingPrompt };
                }

                if (payload.TryGetProperty("returnValue", out var ok) && ok.ValueKind == JsonValueKind.True)
                {
                    return new WebOsMessage
                    {
                        Type = WebOsMessageType.Success,
                        ClientKey = ReadClientKey(payload),
                        Mac = ExtractMac(payload)
                    };
                }

                if (payload.TryGetProperty("returnValue", out ok) && ok.ValueKind == JsonValueKind.False)
                {
                    return new WebOsMessage { Type = WebOsMessageType.Error };
                }
            }
        }
        catch
        {
        }

        return new WebOsMessage { Type = WebOsMessageType.Unknown };
    }

    private static string? ReadClientKey(JsonElement payload)
    {
        if (payload.ValueKind != JsonValueKind.Object)
        {
            return null;
        }

        return payload.TryGetProperty("client-key", out var key) ? key.GetString() : null;
    }

    private static string? ExtractMac(JsonElement payload)
    {
        foreach (var name in new[] { "wiredInfo", "wifiInfo", "wired", "wifi" })
        {
            if (payload.TryGetProperty(name, out var info) &&
                info.ValueKind == JsonValueKind.Object &&
                info.TryGetProperty("macAddress", out var mac))
            {
                var value = mac.GetString();
                if (!string.IsNullOrWhiteSpace(value))
                {
                    return value;
                }
            }
        }

        return payload.TryGetProperty("macAddress", out var direct) ? direct.GetString() : null;
    }
}
