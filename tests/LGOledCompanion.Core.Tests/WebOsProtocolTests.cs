using System.Text.Json;
using LGOledCompanion.Core;

namespace LGOledCompanion.Core.Tests;

public sealed class WebOsProtocolTests
{
    [Fact]
    public void Register_asks_for_prompt_and_power_permission()
    {
        var json = WebOsProtocol.Register(null);
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;
        Assert.Equal("register", root.GetProperty("type").GetString());
        var payload = root.GetProperty("payload");
        Assert.Equal("PROMPT", payload.GetProperty("pairingType").GetString());
        Assert.False(payload.TryGetProperty("client-key", out _));
        var permissions = payload.GetProperty("manifest").GetProperty("permissions");
        Assert.Contains(permissions.EnumerateArray(), item => item.GetString() == "CONTROL_POWER");
        Assert.Contains(permissions.EnumerateArray(), item => item.GetString() == "CONTROL_TV_SCREEN");
    }

    [Fact]
    public void Register_includes_existing_client_key()
    {
        var json = WebOsProtocol.Register("abc-key");
        using var doc = JsonDocument.Parse(json);
        Assert.Equal("abc-key", doc.RootElement.GetProperty("payload").GetProperty("client-key").GetString());
    }

    [Fact]
    public void Request_uses_ssap_screen_uris()
    {
        Assert.Contains("turnOffScreen", WebOsProtocol.Request("1", WebOsProtocol.TurnOffScreen));
        Assert.Contains("turnOnScreen", WebOsProtocol.Request("2", WebOsProtocol.TurnOnScreen));
        Assert.Equal("ssap://com.webos.service.tvpower/power/turnOffScreen", WebOsProtocol.TurnOffScreen);
        Assert.Equal("ssap://com.webos.service.tvpower/power/turnOnScreen", WebOsProtocol.TurnOnScreen);
    }

    [Fact]
    public void Parse_registered_returns_client_key()
    {
        var message = WebOsProtocol.Parse("""{"type":"registered","payload":{"client-key":"k1"}}""");
        Assert.Equal(WebOsMessageType.Registered, message.Type);
        Assert.Equal("k1", message.ClientKey);
    }

    [Fact]
    public void Parse_prompt_and_success_and_error()
    {
        var prompt = WebOsProtocol.Parse("""{"type":"response","payload":{"pairingType":"PROMPT"}}""");
        Assert.Equal(WebOsMessageType.PairingPrompt, prompt.Type);

        var ok = WebOsProtocol.Parse("""{"type":"response","payload":{"returnValue":true}}""");
        Assert.Equal(WebOsMessageType.Success, ok.Type);

        var error = WebOsProtocol.Parse("""{"type":"error","error":"denied"}""");
        Assert.Equal(WebOsMessageType.Error, error.Type);
    }
}
