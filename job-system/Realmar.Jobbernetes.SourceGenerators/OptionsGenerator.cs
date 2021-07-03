using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Realmar.Jobbernetes.SourceGenerators
{
    [Generator]
    public class OptionsGenerator : ISourceGenerator
    {
        private const string Template01 = @"
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Realmar.Jobbernetes.Infrastructure.Options
{
    public static class ServiceCollectionExtensions
    {
        public static void Configure<TOptions>(this IServiceCollection services,
                                               IConfiguration     configuration,
                                               string?            sectionName = null)
            where TOptions : class =>
            services.Configure<TOptions>(
                options => configuration.GetSection(sectionName ?? typeof(TOptions).Name)
                                        .Bind(options));

        public static void ConfigureAllOptions(this IServiceCollection services, IConfiguration configuration)
        {";

        private const string Template02 = @"            Configure<{0}>(services, configuration);";

        private const string Template03 = @"        }
    }
}";

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var rx      = (SyntaxReceiver) context.SyntaxContextReceiver!;
            var options = rx.Classes;
            var sb      = new StringBuilder();

            sb.Append(Template01);
            sb.AppendLine();

            foreach (var option in options)
            {
                sb.AppendLine(string.Format(Template02, $"{option.Namespace}.{option.TypeName}"));
            }

            sb.Append(Template03);

            context.AddSource("OptionsUtils.generated.cs", sb.ToString());
        }

        private readonly struct Option
        {
            public string TypeName  { get; }
            public string Namespace { get; }

            public Option(string typeName, string ns)
            {
                TypeName  = typeName;
                Namespace = ns;
            }
        }

        private class SyntaxReceiver : ISyntaxContextReceiver
        {
            private readonly Visitor      _visitor = new();
            public           List<Option> Classes => _visitor.Classes;

            public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
            {
                if (context.Node is CSharpSyntaxNode node)
                {
                    node.Accept(_visitor);
                }
            }
        }

        private class Visitor : CSharpSyntaxVisitor
        {
            public List<Option> Classes { get; } = new();

            public override void VisitClassDeclaration(ClassDeclarationSyntax node)
            {
                if (node.Modifiers.Any(token => token.Text == "abstract"))
                {
                    return;
                }

                SyntaxNode current = node;

                while (current != null && current is NamespaceDeclarationSyntax == false)
                {
                    current = current.Parent;
                }

                if (current != null)
                {
                    var ns   = (NamespaceDeclarationSyntax) current;
                    var name = ns.Name.ToString();

                    if (name.StartsWith($"{nameof(Realmar)}.{nameof(Jobbernetes)}.Infrastructure.Options"))
                    {
                        Classes.Add(new(node.Identifier.Text, name));
                    }
                }
            }
        }
    }
}
