using System;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;

namespace Gazorator.Razor
{
    internal sealed class CSharpScriptRazorGenerator
    {
        private readonly RazorProjectEngine _projectEngine;

        public CSharpScriptRazorGenerator(string directoryRoot)
        {
            var fileSystem = RazorProjectFileSystem.Create(directoryRoot);
            var projectEngine = RazorProjectEngine.Create(RazorConfiguration.Default, fileSystem, builder =>
            {
                //builder
                //    .SetNamespace("Remove")
                //    .SetBaseType("Microsoft.Extensions.RazorViews.BaseView")
                //    .ConfigureClass((document, @class) =>
                //    {
                //        @class.ClassName = Path.GetFileNameWithoutExtension(document.Source.FilePath);
                //        @class.Modifiers.Clear();
                //        @class.Modifiers.Add("internal");
                //    });

                FunctionsDirective.Register(builder);
                InheritsDirective.Register(builder);
                SectionDirective.Register(builder);

                builder.Features.Remove(builder.Features.OfType<IRazorDocumentClassifierPass>().Single());
                builder.Features.Add(new CSharpScriptDocumentClassifierPass());

                //configure?.Invoke(builder);

//                builder.AddDefaultImports(@"
//@using System
//@using System.Threading.Tasks
//");
            });

            _projectEngine = projectEngine;
        }

        public string Generate(string filePath)
        {
            var razorItem = _projectEngine.FileSystem.GetItem(filePath);
            var codeDocument = _projectEngine.Process(razorItem);
            var csharpDocument = codeDocument.GetCSharpDocument();

            if (csharpDocument.Diagnostics.Any())
            {
                var diagnostics = string.Join(Environment.NewLine, csharpDocument.Diagnostics);
                throw new InvalidOperationException($"One or more parse errors encountered. This will not prevent the generator from continuing: {Environment.NewLine}{diagnostics}.");
            }

            return csharpDocument.GeneratedCode;
        }
    }
}
