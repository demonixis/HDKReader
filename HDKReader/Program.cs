using System;

namespace HDKReader
{
    // https://github.com/ChristophHaag/OpenHMD
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("HDK Reader");
            Console.WriteLine();

            var core = new HDKCore();
            core.Initialize();

            Console.WriteLine("Press a key to exit");
            Console.ReadKey();
        }
    }
}
