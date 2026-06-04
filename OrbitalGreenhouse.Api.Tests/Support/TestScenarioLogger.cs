using Xunit.Abstractions;

namespace OrbitalGreenhouse.Api.Tests.Support;

/// <summary>Writes highly visible scenario banners to test and console output.</summary>
public sealed class TestScenarioLogger
{
    private readonly ITestOutputHelper? _output;

    public TestScenarioLogger(ITestOutputHelper? output = null) => _output = output;

    public void Begin(string scenarioId, string title)
    {
        var line = new string('═', 62);
        Write("");
        Write($"╔{line}╗");
        Write($"║  {scenarioId} | {Pad(title, 56)}║");
        Write($"╚{line}╝");
    }

    public void Step(string message) => Write($"  ► {message}");

    public void Data(string key, object? value) => Write($"    {key,-12}: {value}");

    public void Pass(string summary)
    {
        Write($"  ✔ RESULTADO : {summary}");
        Write($"  ✔ STATUS    : PASSED");
        Write("");
    }

    public void Fail(string summary)
    {
        Write($"  ✖ RESULTADO : {summary}");
        Write($"  ✖ STATUS    : FAILED");
        Write("");
    }

    private void Write(string text)
    {
        Console.WriteLine(text);
        _output?.WriteLine(text);
    }

    private static string Pad(string text, int width) =>
        text.Length >= width ? text[..width] : text.PadRight(width);
}
