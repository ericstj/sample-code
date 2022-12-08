using System;
using System.Configuration;

namespace lib
{
    public class Class1
    {
        // this will use configurationmanager through the portable ConfigurationManager assembly.  It expects that assembly to be a facade assembly to redirect to System.dll on .NETFramework
        public static string GetSetting() => ConfigurationManager.AppSettings.Get("LibSetting");
    }
}
