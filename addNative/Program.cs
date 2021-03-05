using System;
using System.Runtime.InteropServices;

namespace addNative
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Bar();
            }
            catch (System.Exception e)
            {
                // FileNotFoundException means it's not working.  BadImageFormatException means it worked
                Console.WriteLine(e);
            }
        }

        [DllImport("foo")]
        public static extern void Bar();
    }
}
