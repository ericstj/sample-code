using System;
using System.Globalization;
using System.Threading;

namespace loc_sample
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length > 0)
            {
                var culture = new CultureInfo(args[0]);
                Thread.CurrentThread.CurrentUICulture = culture;
            }
            Console.WriteLine(resources.HelloMessage);
        }
    }
}
