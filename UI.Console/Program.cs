using System;
using Ps3DiscDumper;

namespace UIConsole
{
    internal static class Program
    {
        static void Main(string[] args)
        {
            Console.Title = "PS3 Disc Dumper";
            if (args.Length != 2)
            {
                Console.WriteLine("Expected two arguments: input and output folders");
                return;
            }

            new Dumper().Dump(args[0], args[1]);
        }
    }
}
