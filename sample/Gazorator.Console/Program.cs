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
        private const string template = @"
@inherits Gazorator.Scripting.RazorScriptHost<Gazorator.Console.Model>
@{ var helloWorld = ""Hello World!""; }
@{ var year = DateTime.Now.Year; }

<!DOCTYPE html>
<html lang=""en"">
<head>
    <title>Add Numbers</title>
    <meta charset=""utf-8"" />
    <style type=""text/css"">
        body {
            background-color: beige;
            font-family: Verdana, Arial;
            margin: 50px;
        }

        div {
            padding: 10px;
            border-style: solid;
            width: 250px;
        }
    </style>
</head>
<body>
    <div>
        <p>@helloWorld</p>
        <p>It's year @year!</p>
        <p>@Model.MyProperty</p>
        @foreach (var x in Model.Values)
        {
            <p>@x</p>
        }
        @Html.Raw(""<h2>Output some html!</h2>"")
    </div>
</body>
</html>";
        static async Task Main(string[] args)
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

                await Gazorator.Default
                    .WithOutput(writer)
                    // .WithModel(new BindingProjectModel())
                    // .WithReferences(typeof(XDocument).Assembly)
                    //.ProcessAsync("./Views/Xamarin.cshtml")
                    .WithModel(new Model
                    {
                        MyProperty = 1234,
                        Values = new List<int> {1, 2, 3, 4}
                    })
                    .ProcessTemplateAsync(template);

                System.Console.WriteLine(writer.ToString());
            }
        }
    }
}
