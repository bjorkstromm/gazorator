using System.IO;
using System.Threading.Tasks;
using Gazorator.Razor;
using Gazorator.Scripting;

namespace Gazorator
{
    public abstract class Gazorator
    {
        protected TextWriter Output { get; }

        protected Gazorator(TextWriter output = null)
        {
            Output = output ?? TextWriter.Null;
        }

        public static Gazorator Default => new DefaultGazorator(TextWriter.Null);

        public Gazorator<TModel> WithModel<TModel>(TModel model)
        {
            return new Gazorator<TModel>(Output, model);
        }

        public Gazorator WithOutput(TextWriter output)
        {
            return new DefaultGazorator(output);
        }

        public virtual Task ProcessAsync(string filePath)
        {
            var razorGenerator = new CSharpScriptRazorGenerator(Path.GetDirectoryName(filePath));
            var csharpScript = razorGenerator.Generate(filePath);
            var razorContentGenerator = new RazorContentGenerator(Output);
            return razorContentGenerator.Generate(csharpScript);
        }

        private sealed class DefaultGazorator : Gazorator
        {
            public DefaultGazorator(TextWriter output = null) : base(output)
            {
            }
        }
    }

    public sealed class Gazorator<TModel> : Gazorator
    {
        private readonly TModel _model;

        internal Gazorator(TextWriter output, TModel model) : base(output)
        {
            _model = model;
        }

        public new Gazorator<TModel> WithOutput(TextWriter output)
        {
            return new Gazorator<TModel>(output, _model);
        }

        public override Task ProcessAsync(string filePath)
        {
            var razorGenerator = new CSharpScriptRazorGenerator(Path.GetDirectoryName(filePath));
            var csharpScript = razorGenerator.Generate(filePath);
            var razorContentGenerator = new RazorContentGenerator<TModel>(_model, Output);
            return razorContentGenerator.Generate(csharpScript);
        }
    }
}
