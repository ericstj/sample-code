// See https://aka.ms/new-console-template for more information

using System.Diagnostics.Contracts;
using System.IO.Compression;

Test(() => nslib.Class1.Get().GetType().FullName);
Test(() => nslib.Class1.Get2().GetType().FullName);
Test(() => nslib.Class1.Get3().GetType().FullName);


void Test(Func<string> action)
{
    try
    {
        Console.WriteLine(action());
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Exception: {ex.GetType().FullName}: {ex.Message}");
    }
}