using System;

namespace Gazorator.Scripting
{
    internal sealed class RazorLiteral : IRazorLiteral
    {
        private string Content { get; }

        string IRazorLiteral.Render()
        {
            return Content;
        }

        internal RazorLiteral(string content)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
        }
    }
}