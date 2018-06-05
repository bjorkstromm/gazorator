using Gazorator.Extensions;
using System;
using System.Globalization;
using System.IO;
using System.Text.Encodings.Web;

namespace Gazorator.Scripting
{
    public abstract class RazorScriptHostBase
    {
        public HtmlRenderer Html { get; } 

        protected TextWriter Output { get; }

        protected virtual HtmlEncoder HtmlEncoder { get; } = HtmlEncoder.Default;

        public RazorScriptHostBase(TextWriter output)
        {
            Output = output ?? throw new ArgumentNullException(nameof(output));
            Html = new HtmlRenderer();
        }

        public virtual void WriteLiteral(object value)
        {
           WriteLiteral(Convert.ToString(value, CultureInfo.InvariantCulture));
        }

        public virtual void WriteLiteral(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                Output.Write(value);
            }
        }

        public virtual void Write(string value)
        {
            WriteLiteral(HtmlEncoder.Encode(value));
        }

        public virtual void Write(object value)
        {
            if (value is IRazorLiteral element)
            {
                WriteLiteral(element.Render());
                return;
            }

            Write(Convert.ToString(value, CultureInfo.InvariantCulture));
        }
    }

    public sealed class RazorScriptHost : RazorScriptHostBase
    {
        public RazorScriptHost(TextWriter output) : base(output)
        {
        }
    }

    public sealed class RazorScriptHost<TModel> : RazorScriptHostBase
    {
        public RazorScriptHost(TModel model, TextWriter output) : base(output)
        {
            if (typeof(TModel).IsNullable() && model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            Model = model;
        }

        public TModel Model { get; }
    }
}
