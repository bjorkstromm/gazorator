# Grazorator

![gazorator logo](./assets/grazorator-horizontal-logo.png)

Grazorator is a `dotnet` tool that empowers developers to generate XML based outputs using the power of the Razor View Engine.

Supported output formats include: 

- HTML
- XML
- XAML

## Getting Started

To get started install the tool using the `dotnet` cli with the following command.

```c#
dotnet tool install Grazorator --global
```

## Sample

There are three parts to running Grazorator:

1. The **Template**
2. The **Model** or data
3. The **Output**.

### Template

Templates can be any XML based language, and Razor syntax is recommended due to its flexibility. Templates are required to `inherit` from the type `Gazorator.Scripting.RazorScriptHost<T>`.

```html
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
</html>
```

### Models

Similar to Razor Views, Grazorator templates can take a view model. Here we see the usage of `WithModel` passing in a value.

```c#
using var writer = new StringWriter();
await Gazorator.Default
    .WithOutput(writer)
    .WithModel(new Model
    {
        MyProperty = 1234,
        Values = new List<int> {1, 2, 3, 4}
    })
    .ProcessTemplateAsync(template);
```

### Output

We can write the output to an instance of `TextWriter`.

## License

MIT License

Copyright (c) 2018 Martin Björkström

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
