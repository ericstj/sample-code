using System;

public class Class1
{
    public static (string, string) GetVersion()
    {
        return (Environment.Version.ToString(), Environment.OSVersion.ToString());
    }

}
