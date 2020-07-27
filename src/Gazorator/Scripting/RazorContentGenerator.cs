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
        private IReadOnlyCollection<Assembly> _references;
        protected readonly DynamicViewBag _viewBag;

        protected RazorContentGeneratorBase(IEnumerable<Assembly> references, DynamicViewBag viewBag)
        {
            _references = new List<Assembly>(references);
            _viewBag = viewBag;
        }

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
            yield return MetadataReference.CreateFromFile(typeof(Action).Assembly.Location); // mscorlib or System.Private.Core
            yield return MetadataReference.CreateFromFile(typeof(IQueryable).Assembly.Location); // System.Core or System.Linq.Expressions
            yield return MetadataReference.CreateFromFile(typeof(Uri).Assembly.Location); // System
            yield return MetadataReference.CreateFromFile(typeof(System.Xml.XmlReader).Assembly.Location); // System.Xml
            yield return MetadataReference.CreateFromFile(typeof(System.Xml.Linq.XDocument).Assembly.Location); // System.Xml.Linq
            yield return MetadataReference.CreateFromFile(typeof(System.Data.DataTable).Assembly.Location); // System.Data

            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
            {
                if (entryAssembly.Location != null)
                {
                    yield return MetadataReference.CreateFromFile(entryAssembly.Location);
                }

                foreach (var reference in entryAssembly.GetReferencedAssemblies())
                {
                    var referencedAssembly = Assembly.Load(reference);
                    if (referencedAssembly.Location != null)
                    {
                        yield return MetadataReference.CreateFromFile(referencedAssembly.Location);
                    }
                }
            }

            foreach (var reference in _references.Where(r => r.Location != null))
            {
                yield return MetadataReference.CreateFromFile(reference.Location);
            }
        }

        protected abstract Type GetGlobalsType();

        protected abstract RazorScriptHostBase GetGlobalsObject();
    }

    internal sealed class RazorContentGenerator : RazorContentGeneratorBase
    {
        private readonly TextWriter _textWriter;

        public RazorContentGenerator(TextWriter textWriter, IEnumerable<Assembly> references, DynamicViewBag viewBag) : base(references, viewBag)
        {
            _textWriter = textWriter ?? throw new ArgumentNullException(nameof(textWriter));
        }

        protected override RazorScriptHostBase GetGlobalsObject()
        {
            return new RazorScriptHost(_textWriter, _viewBag);
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

        public RazorContentGenerator(TModel model, TextWriter textWriter, IEnumerable<Assembly> references, DynamicViewBag viewBag) : base(references, viewBag)
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
                    .Append(MetadataReference.CreateFromFile(typeof(Microsoft.CSharp.RuntimeBinder.CSharpArgumentInfo).Assembly.Location))
                    .Append(MetadataReference.CreateFromFile(typeof(TModel).Assembly.Location));
        }

        protected override RazorScriptHostBase GetGlobalsObject()
        {
            return _isDynamicAssembly ?
                new RazorScriptHostDynamic(ToExpandoObject(_model), _textWriter, _viewBag):
                new RazorScriptHost<TModel>(_model, _textWriter, _viewBag) as RazorScriptHostBase;
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
