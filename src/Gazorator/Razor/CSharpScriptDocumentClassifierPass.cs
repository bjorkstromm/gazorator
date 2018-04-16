using System.Linq;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace Gazorator.Razor
{
    internal sealed class CSharpScriptDocumentClassifierPass : DocumentClassifierPassBase
    {
        public override int Order => DefaultFeatureOrder;

        protected override string DocumentKind => "default";

        protected override bool IsMatch(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode)
        {
            return true;
        }

        protected override void OnDocumentStructureCreated(
            RazorCodeDocument codeDocument,
            NamespaceDeclarationIntermediateNode @namespace,
            ClassDeclarationIntermediateNode @class,
            MethodDeclarationIntermediateNode method)
        {
            var documentNode = codeDocument.GetDocumentIntermediateNode();

            foreach (var @using in @namespace.Children.OfType<UsingDirectiveIntermediateNode>())
            {
                documentNode.Children.Add(@using);
            }
            foreach (var child in method.Children)
            {
                documentNode.Children.Add(child);
            }

            documentNode.Children.Remove(@namespace);

            return;
        }
    }
}
