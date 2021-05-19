using System;
using lib;

namespace app
{
    class Program
    {
        static void Main(string[] args)
        {
            foreach (var user in EventLogHelper.GetEventUsers())
            {
                Console.WriteLine(user);
            }
        }
    }
}
