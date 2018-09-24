using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Gazorator.Razor;
using Gazorator.Scripting;

namespace Gazorator
{
    public abstract class Gazorator
    {
        protected TextWriter Output { get; }
        protected IEnumerable<Assembly> References { get; } = new List<Assembly>();

        protected Gazorator(TextWriter output = null, params Assembly[] references)
        {
            Output = output ?? TextWriter.Null;
            References = new List<Assembly>(references);
        }

        public static Gazorator Default => new DefaultGazorator(TextWriter.Null);

        public Gazorator<TModel> WithModel<TModel>(TModel model)
        {
            return new Gazorator<TModel>(Output, model, References.ToArray());
        }

        public Gazorator WithOutput(TextWriter output)
        {
            return new DefaultGazorator(output, References.ToArray());
        }

        public Gazorator WithReferences(params Assembly[] references)
        {
            return new DefaultGazorator(Output, references);
        }

        public virtual Task ProcessAsync(string filePath)
        {
            var razorGenerator = new CSharpScriptRazorGenerator(Path.GetDirectoryName(filePath));
            var csharpScript = razorGenerator.Generate(filePath);
            var razorContentGenerator = new RazorContentGenerator(Output, References);
            return razorContentGenerator.Generate(csharpScript);
        }

        private sealed class DefaultGazorator : Gazorator
        {
            public DefaultGazorator(TextWriter output = null, params Assembly[] references) : base(output, references)
            {
            }
        }
    }

    public sealed class Gazorator<TModel> : Gazorator
    {
        private readonly TModel _model;

        internal Gazorator(TextWriter output, TModel model, params Assembly[] references) : base(output, references)
        {
            _model = model;
        }

        public new Gazorator<TModel> WithOutput(TextWriter output)
        {
            return new Gazorator<TModel>(output, _model, References.ToArray());
        }

        public new Gazorator<TModel> WithReferences(params Assembly[] references)
        {
            return new Gazorator<TModel>(Output, _model, references);
        }

        public override Task ProcessAsync(string filePath)
        {
            var razorGenerator = new CSharpScriptRazorGenerator(Path.GetDirectoryName(filePath));
            var csharpScript = razorGenerator.Generate(filePath);
            var razorContentGenerator = new RazorContentGenerator<TModel>(_model, Output, References);
            return razorContentGenerator.Generate(csharpScript);
        }
    }
}
