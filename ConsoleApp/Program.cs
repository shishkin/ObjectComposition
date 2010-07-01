using System;
using System.Linq;

namespace DataContextInteraction
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Dynamic data example:");
            DynamicDataExample.Run();

            Console.WriteLine("\n\nStatic roles example:");
            StaticRolesExample.Run();
        }
    }
}
