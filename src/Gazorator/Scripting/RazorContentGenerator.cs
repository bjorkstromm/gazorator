using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Gazorator.Extensions;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;

namespace Gazorator.Scripting
{
    internal abstract class RazorContentGeneratorBase
    {
        public Task Generate(string csharpScript)
        {
            var options = ScriptOptions.Default
                .WithReferences(GetMetadataReferences())
                .WithImports("System")
                .WithMetadataResolver(ScriptMetadataResolver.Default);

            var roslynScript = CSharpScript.Create(csharpScript, options, GetGlobalsType());

            var compilation = roslynScript.GetCompilation();
            var diagnostics = compilation.GetDiagnostics().Where(d => d.Severity == DiagnosticSeverity.Error);

            if (diagnostics.Any())
            {
                var errorMessages = string.Join(Environment.NewLine, diagnostics.Select(x => x.ToString()));
                throw new InvalidOperationException($"Error(s) occurred when compiling build script:{Environment.NewLine}{errorMessages}");
            }

            return roslynScript.RunAsync(GetGlobalsObject());
        }

        protected virtual IEnumerable<MetadataReference> GetMetadataReferences()
        {
            var entryAssembly = Assembly.GetEntryAssembly();
            yield return MetadataReference.CreateFromFile(entryAssembly.Location);

            foreach (var reference in entryAssembly.GetReferencedAssemblies())
            {
                yield return MetadataReference.CreateFromFile(Assembly.Load(reference).Location);
            }
        }

        protected abstract Type GetGlobalsType();

        protected abstract RazorScriptHostBase GetGlobalsObject();
    }

    internal sealed class RazorContentGenerator : RazorContentGeneratorBase
    {
        private readonly TextWriter _textWriter;

        public RazorContentGenerator(TextWriter textWriter)
        {
            _textWriter = textWriter ?? throw new ArgumentNullException(nameof(textWriter));
        }

        protected override RazorScriptHostBase GetGlobalsObject()
        {
            return new RazorScriptHost(_textWriter);
        }

        protected override Type GetGlobalsType()
        {
            return typeof(RazorScriptHost);
        }
    }

    internal sealed class RazorContentGenerator<TModel> : RazorContentGeneratorBase
    {
        private readonly TextWriter _textWriter;
        private readonly TModel _model;
        private readonly bool _isDynamicAssembly;

        public RazorContentGenerator(TModel model, TextWriter textWriter)
        {
            _textWriter = textWriter ?? throw new ArgumentNullException(nameof(textWriter));
            if (typeof(TModel).IsNullable() && model == null)
            {
                throw new ArgumentNullException(nameof(model));
            }
            if (typeof(TModel).IsNotPublic)
            {
                throw new ArgumentException($"{typeof(TModel).GetType().FullName} must be public.");
            }
            _model = model;
            _isDynamicAssembly = typeof(TModel).Assembly.IsDynamic ||
                string.IsNullOrEmpty(typeof(TModel).Assembly.Location);
        }

        protected override IEnumerable<MetadataReference> GetMetadataReferences()
        {
            return _isDynamicAssembly ?
                base.GetMetadataReferences()
                    .Append(MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location)) :
                base.GetMetadataReferences()
                    .Append(MetadataReference.CreateFromFile(typeof(TModel).Assembly.Location));
        }

        protected override RazorScriptHostBase GetGlobalsObject()
        {
            return _isDynamicAssembly ?
                new RazorScriptHostDynamic(ToExpandoObject(_model), _textWriter):
                new RazorScriptHost<TModel>(_model, _textWriter) as RazorScriptHostBase;
        }

        protected override Type GetGlobalsType()
        {
            return _isDynamicAssembly ?
                typeof(RazorScriptHostDynamic) :
                typeof(RazorScriptHost<TModel>);
        }

        private static ExpandoObject ToExpandoObject(TModel model)
        {
            IDictionary<string, object> expando = new ExpandoObject();

            foreach(var property in typeof(TModel).GetProperties())
            {
                expando.Add(property.Name, property.GetValue(model));
            }

            return (ExpandoObject)expando;
        }
    }
}
