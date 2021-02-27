using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

[assembly: CLSCompliant(false)]
namespace Stasistium.Generator
{

    [Generator]
    public class ChainMethodGenerator : ISourceGenerator
    {
        

        private const string ATTRIBUTES = @"
namespace Stasistium
{

    [System.AttributeUsage(System.AttributeTargets.Constructor, Inherited = false, AllowMultiple = false)]
    sealed class StageNameAttribute : System.Attribute
    {

        public StageNameAttribute(string name)
        {
            this.Name = name;
        }

        public string Name { get; }
    }
}";

        public void Initialize(GeneratorInitializationContext context)
        {
            // Register a syntax receiver that will be created for each generation pass
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {

            try
            {

                // retreive the populated receiver 
                if (context.SyntaxReceiver is not SyntaxReceiver receiver)
                    return;

                // we're going to create a new compilation that contains the attribute.
                // TODO: we should allow source generators to provide source during initialize, so that this step isn't required.
                var options = (CSharpParseOptions)((CSharpCompilation)context.Compilation).SyntaxTrees[0].Options;


                context.AddSource("Attributes.cs", ATTRIBUTES);
                var compilation = context.Compilation.AddSyntaxTrees(CSharpSyntaxTree.ParseText(ATTRIBUTES, options));



                //if (!System.Diagnostics.Debugger.IsAttached)
                //    System.Diagnostics.Debugger.Launch();


                // loop over the candidate classes, and keep the ones that are actually annotated
                foreach (var @class in receiver.CandidateClasses)
                {

                    var model = compilation.GetSemanticModel(@class.SyntaxTree);
                    if (model is null)
                        continue;

                    var classSymbol = model.GetDeclaredSymbol(@class);
                    if (classSymbol is null)
                        continue;


                    var baseOutput = classSymbol.AllInterfaces.FirstOrDefault(x => x.IsGenericType && x.ConstructUnboundGenericType().ToDisplayString() == "Stasistium.Stages.IStageBaseOutput<>");
                    var baseinput1 = classSymbol.AllInterfaces.FirstOrDefault(x => x.IsGenericType && x.ConstructUnboundGenericType().ToDisplayString() == "Stasistium.Stages.IStageBaseInput<>");
                    var baseinput2 = classSymbol.AllInterfaces.FirstOrDefault(x => x.IsGenericType && x.ConstructUnboundGenericType().ToDisplayString() == "Stasistium.Stages.IStageBaseInput<,>");

                    if (baseinput1 is not null)
                    {
                        var input = baseinput1.TypeArguments.Single();
                        var output = baseOutput?.TypeArguments.Single();
                        var classSource = ProcessClass(classSymbol, input, output);
                        context.AddSource($"{classSymbol.Name}{classSymbol.TypeArguments.Length}_Generate1Parameter.cs", SourceText.From(classSource, Encoding.UTF8));
                    }
                    if (baseinput2 is not null)
                    {
                        var input1 = baseinput2.TypeArguments.First();
                        var input2 = baseinput2.TypeArguments.Skip(1).Single();
                        var output = baseOutput?.TypeArguments.Single();
                        var classSource = ProcessClass(classSymbol, input1, input2, output);
                        context.AddSource($"{classSymbol.Name}{classSymbol.TypeArguments.Length}_Generate2Parameter.cs", SourceText.From(classSource, Encoding.UTF8));
                    }

                }
            }
            catch (Exception e)
            {
                var str = PrependError(e.ToString());
                context.AddSource($"errors_Generate2Parameter.cs", SourceText.From(str, Encoding.UTF8));
            }
        }

        private static string PrependError(string orinal)
        {
            using var reader = new StringReader(orinal);
            var builder = new StringBuilder();

            for (string line = reader.ReadLine(); line is not null; line = reader.ReadLine())
            {
                builder.Append("#error ");
                builder.AppendLine(line);
            }
            return builder.ToString();
        }

        private static string ProcessClass(INamedTypeSymbol classSymbol, ITypeSymbol inputSymbol, ITypeSymbol? outputSymbol)
        {
            var outputType = outputSymbol is null
                ? "void"
                : $"Stasistium.Stages.IStageBaseOutput<{outputSymbol.ToDisplayString()}>";
            var inputName = inputSymbol.ToDisplayString();

            var constructors = classSymbol.Constructors.Where(x => !x.IsStatic);

            var source = new StringBuilder();

            foreach (var constructor in constructors)
            {
                var parameterList = new List<string>
                {
                    $"this Stasistium.Stages.IStageBaseOutput<{inputName}> input"
                };
                var constructorParameterList = new List<string>();
                bool hasName = false;
                object? defaultName = null;

                foreach (var p in constructor.Parameters)
                {
                    var typeName = p.Type.ToDisplayString();
                    var parameterName = p.Name;
                    //throw new Exception("Fails");


                    if (typeName == "Stasistium.Documents.IGeneratorContext")
                    {
                        constructorParameterList.Add("input.Context");
                    }
                    else if ((typeName == "string?" || typeName == "System.String?") && parameterName == "name")
                    {
                        hasName = true;
                        constructorParameterList.Add("name");
                        defaultName = p.HasExplicitDefaultValue ? p.ExplicitDefaultValue : null;
                    }
                    else
                    {
                        constructorParameterList.Add(parameterName);
                        if (p.HasExplicitDefaultValue)
                        {
                            var defaultParameter = p.ExplicitDefaultValue is null
                                    ? "null"
                                    : $"\"{p.ExplicitDefaultValue}\"";

                            parameterList.Add($"{typeName} {parameterName} = {defaultParameter}");
                        }
                        else
                            parameterList.Add($"{typeName} {parameterName}");
                    }
                }
                if (hasName)
                {
                    var defaultParameter = defaultName is null
                        ? "null"
                        : $"\"{defaultName}\"";
                    parameterList.Add($"string? name = {defaultParameter}");
                }

                string generic = "";
                string typeConstraints = "";
                if (classSymbol.IsGenericType)
                {
                    generic = $"<{ string.Join(", ", classSymbol.TypeArguments)}>";

                    // somehow construct the where statement...
                    var parts = classSymbol.ToDisplayParts(new SymbolDisplayFormat(genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeTypeConstraints));
                    foreach (var part in parts.SkipWhile(x => x.Kind != SymbolDisplayPartKind.Keyword && x.ToString() != "where"))
                        typeConstraints += part.ToString();

                }

                string methodName;
                var nameAttribute = classSymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass?.ToDisplayString() == "Stasistium.StageNameAttribute");
                if (nameAttribute is not null && nameAttribute.ConstructorArguments.First().Value is string name)
                    methodName = name;
                else
                    methodName = classSymbol.Name;

                if (methodName.EndsWith("Stage", comparisonType: StringComparison.OrdinalIgnoreCase))
                    methodName = methodName.Substring(0, methodName.Length - "Stage".Length);
                source.AppendLine($@"
namespace Stasistium
{{

     public static partial class {classSymbol.Name}Extension
     {{
         public static {outputType} {methodName}{generic}({string.Join(", ", parameterList)})
            {typeConstraints}
         {{
             if (input is null)
                 throw new System.ArgumentNullException(nameof(input));
             var stage = new {classSymbol.ToDisplayString()}({string.Join(", ", constructorParameterList)});
             input.PostStages += (stage as Stasistium.Stages.IStageBaseInput<{inputName}>).DoIt;
            {(outputSymbol is null ? "" : "return stage;")}
         }}
     }}
}}
");
            }

            return source.ToString();
        }

        private static string ProcessClass(INamedTypeSymbol classSymbol, ITypeSymbol inputSymbol1, ITypeSymbol inputSymbol2, ITypeSymbol? outputSymbol)
        {
            var outputType = outputSymbol is null
     ? "void"
     : $"Stasistium.Stages.IStageBaseOutput<{outputSymbol.ToDisplayString()}>";
            var input1Name = inputSymbol1.ToDisplayString();
            var input2Name = inputSymbol2.ToDisplayString();

            var constructors = classSymbol.Constructors.Where(x => !x.IsStatic);

            var source = new StringBuilder();

            foreach (var constructor in constructors)
            {
                var parameterList = new List<string>
                {
                    $"this Stasistium.Stages.IStageBaseOutput<{input1Name}> input1",
                    $"Stasistium.Stages.IStageBaseOutput<{input2Name}> input2"
                };
                var constructorParameterList = new List<string>();
                bool hasName = false;
                object? defaultName = null;

                foreach (var p in constructor.Parameters)
                {
                    var typeName = p.Type.ToDisplayString();
                    var parameterName = p.Name;
                    //throw new Exception("Fails");


                    if (typeName == "Stasistium.Documents.IGeneratorContext")
                    {
                        constructorParameterList.Add("input1.Context");
                    }
                    else if ((typeName == "string?" || typeName == "System.String?") && parameterName == "name")
                    {
                        hasName = true;
                        constructorParameterList.Add("name");
                        defaultName = p.HasExplicitDefaultValue ? p.ExplicitDefaultValue : null;
                    }
                    else
                    {
                        constructorParameterList.Add(parameterName);
                        if (p.HasExplicitDefaultValue)
                        {
                            var defaultParameter = p.ExplicitDefaultValue is null
                                    ? "null"
                                    : $"\"{p.ExplicitDefaultValue}\"";

                            parameterList.Add($"{typeName} {parameterName} = {defaultParameter}");
                        }
                        else
                            parameterList.Add($"{typeName} {parameterName}");
                    }
                }
                if (hasName)
                {
                    var defaultParameter = defaultName is null
                        ? "null"
                        : $"\"{defaultName}\"";
                    parameterList.Add($"string? name = {defaultParameter}");
                }

                string generic = "";
                string typeConstraints = "";
                if (classSymbol.IsGenericType)
                {
                    generic = $"<{ string.Join(", ", classSymbol.TypeArguments)}>";

                    // somehow construct the where statement...
                    var parts = classSymbol.ToDisplayParts(new SymbolDisplayFormat(genericsOptions: SymbolDisplayGenericsOptions.IncludeTypeParameters | SymbolDisplayGenericsOptions.IncludeTypeConstraints));
                    foreach (var part in parts.SkipWhile(x => x.Kind != SymbolDisplayPartKind.Keyword && x.ToString() != "where"))
                        typeConstraints += part.ToString();

                }

                string methodName;
                var nameAttribute = classSymbol.GetAttributes().FirstOrDefault(x => x.AttributeClass?.ToDisplayString() == "Stasistium.StageNameAttribute");
                if (nameAttribute is not null && nameAttribute.ConstructorArguments.First().Value is string name)
                    methodName = name;
                else
                    methodName = classSymbol.Name;

                if (methodName.EndsWith("Stage", comparisonType: StringComparison.OrdinalIgnoreCase))
                    methodName = methodName.Substring(0, methodName.Length - "Stage".Length);

                source.AppendLine($@"
namespace Stasistium
{{

     public static partial class {classSymbol.Name}Extension
     {{
         public static {outputType} {methodName}{generic}({string.Join(", ", parameterList)})
            {typeConstraints}
         {{
             if (input1 is null)
                 throw new System.ArgumentNullException(nameof(input1));
             if (input2 is null)
                 throw new System.ArgumentNullException(nameof(input2));
             var stage = new {classSymbol.ToDisplayString()}({string.Join(", ", constructorParameterList)});
             input1.PostStages += (stage as Stasistium.Stages.IStageBaseInput<{input1Name}, {input2Name}>).DoIt1;
             input2.PostStages += (stage as Stasistium.Stages.IStageBaseInput<{input1Name}, {input2Name}>).DoIt2;
             {(outputSymbol is null ? "" : "return stage;")}
         }}
     }}
}}
");
            }

            return source.ToString();
        }



        /// <summary>
        /// Created on demand before each generation pass
        /// </summary>
        class SyntaxReceiver : ISyntaxReceiver
        {
            public List<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

            /// <summary>
            /// Called for every syntax node in the compilation, we can inspect the nodes and save any information useful for generation
            /// </summary>
            public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
            {
                // any field with at least one attribute is a candidate for property generation
                if (syntaxNode is ClassDeclarationSyntax classDeclaration
                    && !classDeclaration.Modifiers.Any(SyntaxKind.AbstractKeyword))
                {
                    this.CandidateClasses.Add(classDeclaration);
                }
            }
        }
    }
}
