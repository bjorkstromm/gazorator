using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using AndroidBinderator;
using System.Linq;
using System.Xml.Linq;

namespace Gazorator.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var writer = new StringWriter())
            {
                // Gazorator.Default
                //     .WithOutput(writer)
                //     .WithModel(new Model
                //     {
                //         MyProperty = 1234,
                //         Values = new List<int> { 1, 2, 3, 4 }
                //     }).ProcessAsync("./Views/Sample.cshtml").Wait();

                Gazorator.Default
                    .WithOutput(writer)
                    .WithModel(new BindingProjectModel())
                    .WithReferences(typeof(XDocument).Assembly)
                    .ProcessAsync("./Views/Xamarin.cshtml").Wait();

                System.Console.WriteLine(writer.ToString());
            }
        }
    }
}
