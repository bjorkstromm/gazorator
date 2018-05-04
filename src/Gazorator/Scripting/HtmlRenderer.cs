using System.IO;

namespace Gazorator.Scripting
{
    public class HtmlRenderer
    {
        public IRazorLiteral Raw(string content)
        {
            return new RazorLiteral(content);
        }
    }
}