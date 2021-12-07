
using System.Runtime.CompilerServices;
namespace module;
internal class Mod
{
    [ModuleInitializer]
    internal static void Initialize()
    {
        Console.WriteLine("Initialized");
    }
}

public class TestClass
{
    public static void Test() => Console.WriteLine("Test");
}