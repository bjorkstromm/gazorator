using System;
using System.Collections.Generic;
using System.Dynamic;
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
        protected Action<dynamic> ConfigureViewBag { get; }

        protected Gazorator(TextWriter output = null, Action<dynamic> configureViewBag = null, params Assembly[] references)
        {
            Output = output ?? TextWriter.Null;
            ConfigureViewBag = configureViewBag;
            References = new List<Assembly>(references);
        }

        public static Gazorator Default => new DefaultGazorator(TextWriter.Null);

        public Gazorator<TModel> WithModel<TModel>(TModel model)
        {
            return new Gazorator<TModel>(Output, model, ConfigureViewBag, References.ToArray());
        }

        public Gazorator WithOutput(TextWriter output)
        {
            return new DefaultGazorator(output, ConfigureViewBag, References.ToArray());
        }

        public Gazorator WithReferences(params Assembly[] references)
        {
            return new DefaultGazorator(Output, ConfigureViewBag, references);
        }

        public Gazorator WithViewBag(Action<dynamic> configureViewBag)
        {
            return new DefaultGazorator(Output, configureViewBag, References.ToArray());
        }

        public virtual Task ProcessAsync(string filePath)
        {
            var razorGenerator = new CSharpScriptRazorGenerator(Path.GetDirectoryName(filePath));
            var csharpScript = razorGenerator.Generate(filePath);

            var viewBag = new DynamicViewBag();
            ConfigureViewBag?.Invoke(viewBag);

            var razorContentGenerator = new RazorContentGenerator(Output, References, viewBag);
            return razorContentGenerator.Generate(csharpScript);
        }

        public virtual async Task ProcessTemplateAsync(string template)
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                using (var stream = File.OpenWrite(tempFile))
                using (var writer = new StreamWriter(stream))
                {
                    await writer.WriteAsync(template);
                }

                await ProcessAsync(tempFile);
            }
            finally
            {
                if (File.Exists(tempFile))
                {
                    File.Delete(tempFile);
                }
            }
        }

        private sealed class DefaultGazorator : Gazorator
        {
            public DefaultGazorator(TextWriter output = null, Action<dynamic> configureViewBag = null, params Assembly[] references) : base(output, configureViewBag, references)
            {
            }
        }
    }

    public sealed class Gazorator<TModel> : Gazorator
    {
        private readonly TModel _model;

        internal Gazorator(TextWriter output, TModel model, Action<dynamic> configureViewBag, params Assembly[] references) : base(output, configureViewBag, references)
        {
            _model = model;
        }

        public new Gazorator<TModel> WithOutput(TextWriter output)
        {
            return new Gazorator<TModel>(output, _model, ConfigureViewBag, References.ToArray());
        }

        public new Gazorator<TModel> WithReferences(params Assembly[] references)
        {
            return new Gazorator<TModel>(Output, _model, ConfigureViewBag, references);
        }

        public Gazorator WithViewBag(Action<dynamic> configureViewBag)
        {
            return new Gazorator<TModel>(Output, _model, configureViewBag, References.ToArray());
        }

        public override Task ProcessAsync(string filePath)
        {
            var razorGenerator = new CSharpScriptRazorGenerator(Path.GetDirectoryName(filePath));
            var csharpScript = razorGenerator.Generate(filePath);

            var viewBag = new DynamicViewBag();
            ConfigureViewBag?.Invoke(viewBag);

            var razorContentGenerator = new RazorContentGenerator<TModel>(_model, Output, References, viewBag);
            return razorContentGenerator.Generate(csharpScript);
        }
    }
}
