using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Gazorator.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var writer = new StringWriter())
            {
                Gazorator.Default
                    .WithOutput(writer)
                    .WithModel(new Model
                    {
                        MyProperty = 1234,
                        Values = new List<int> { 1, 2, 3, 4 }
                    }).ProcessAsync("./Views/Sample.cshtml").Wait();

                System.Console.WriteLine(writer.ToString());
            }
        }
    }
}
