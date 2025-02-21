using Xunit.Abstractions;

namespace Test;

public class TestOutputHelper
{
    private static ITestOutputHelper? _output;

    public static void Initialize(ITestOutputHelper output)
    {
        _output = output;
    }

    public static void WriteLine(string message)
    {
        _output?.WriteLine(message);
    }
}
