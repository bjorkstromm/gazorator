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
                // Register directives.
                SectionDirective.Register(builder);

                // We replace the default document classifier, because we can't have namespace declaration ins script.
                var defaultDocumentClassifier = builder.Features
                    .OfType<IRazorDocumentClassifierPass>()
                    .FirstOrDefault(x => x.Order == 1000);
                builder.Features.Remove(defaultDocumentClassifier);
                builder.Features.Add(new CSharpScriptDocumentClassifierPass());
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
