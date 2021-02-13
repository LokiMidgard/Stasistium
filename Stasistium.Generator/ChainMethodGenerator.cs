﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;


namespace Stasistium.Generator
{
    [Generator]
    public class ChainMethodGenerator : ISourceGenerator
    {


        public void Initialize(GeneratorInitializationContext context)
        {
            // Register a syntax receiver that will be created for each generation pass
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {


            // retreive the populated receiver 
            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
                return;

            // we're going to create a new compilation that contains the attribute.
            // TODO: we should allow source generators to provide source during initialize, so that this step isn't required.
            var options = (CSharpParseOptions)((CSharpCompilation)context.Compilation).SyntaxTrees[0].Options;
            var compilation = context.Compilation;

            // loop over the candidate classes, and keep the ones that are actually annotated
            foreach (var @class in receiver.CandidateClasses)
            {

                var model = compilation.GetSemanticModel(@class.SyntaxTree);
                if (model is null)
                    continue;

                var classSymbol = model.GetDeclaredSymbol(@class);
                if (classSymbol is null)
                    continue;

                var baseOutput = classSymbol.AllInterfaces.FirstOrDefault(x => x.IsGenericType && x.ContainingNamespace.ToDisplayString() == "Stasistium.Stages" && x.MetadataName == "IStageBaseOutput`1");
                var baseinput1 = classSymbol.AllInterfaces.FirstOrDefault(x => x.IsGenericType && x.ContainingNamespace.ToDisplayString() == "Stasistium.Stages" && x.MetadataName == "IStageBaseInput`1");
                var baseinput2 = classSymbol.AllInterfaces.FirstOrDefault(x => x.IsGenericType && x.ContainingNamespace.ToDisplayString() == "Stasistium.Stages" && x.MetadataName == "IStageBaseInput`2");

                if (baseinput1 is not null && baseOutput is not null)
                {
                    var input = baseinput1.TypeArguments.Single();
                    var output = baseOutput.TypeArguments.Single();
                    var classSource = this.ProcessClass(classSymbol, input, output, context);
                    context.AddSource($"{classSymbol.Name}_Generate1Parameter.cs", SourceText.From(classSource, Encoding.UTF8));
                }
                if (baseinput2 is not null && baseOutput is not null)
                {
                    var input1 = baseinput2.TypeArguments.First();
                    var input2 = baseinput2.TypeArguments.Skip(1).Single();
                    var output = baseOutput.TypeArguments.Single();
                    var classSource = this.ProcessClass(classSymbol, input1, input2, output, context);
                    context.AddSource($"{classSymbol.Name}_Generate2Parameter.cs", SourceText.From(classSource, Encoding.UTF8));
                }

            }
        }

        private string ProcessClass(INamedTypeSymbol classSymbol, ITypeSymbol inputSymbol, ITypeSymbol outputSymbol, GeneratorExecutionContext context)
        {
            var outputName = outputSymbol.ToDisplayString();
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


                    if (typeName == "IGeneratorContext")
                    {
                        constructorParameterList.Add("input.Context");
                    }
                    else if (typeName == "string" && parameterName == "name")
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
                    parameterList.Add($"string? name = {defaultParameter})");
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
                source.AppendLine($@"
namespace Stasistium
{{

     public static partial class {classSymbol.Name}Extension
     {{
         public static Stasistium.Stages.IStageBaseOutput<{outputName}> {classSymbol.Name}{generic}({string.Join(", ", parameterList)})
            {typeConstraints}
         {{
             if (input is null)
                 throw new System.ArgumentNullException(nameof(input));
             var stage = new {classSymbol.ToDisplayString()}({string.Join(", ", constructorParameterList)});
             input.PostStages += (stage as Stasistium.Stages.IStageBaseInput<{inputName}>).DoIt;
             return stage;
         }}
     }}
}}
");
            }

            return source.ToString();
        }

        private string ProcessClass(INamedTypeSymbol classSymbol, ITypeSymbol inputSymbol1, ITypeSymbol inputSymbol2, ITypeSymbol outputSymbol, GeneratorExecutionContext context)
        {
            var outputName = outputSymbol.ToDisplayString();
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


                    if (typeName == "IGeneratorContext")
                    {
                        constructorParameterList.Add("input1.Context");
                    }
                    else if (typeName == "string" && parameterName == "name")
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
                    parameterList.Add($"string? name = {defaultParameter})");
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
                source.AppendLine($@"
namespace Stasistium
{{

     public static partial class {classSymbol.Name}Extension
     {{
         public static Stasistium.Stages.IStageBaseOutput<{outputName}> {classSymbol.Name}{generic}({string.Join(", ", parameterList)})
            {typeConstraints}
         {{
             if (input1 is null)
                 throw new System.ArgumentNullException(nameof(input1));
             if (input2 is null)
                 throw new System.ArgumentNullException(nameof(input2));
             var stage = new {classSymbol.ToDisplayString()}({string.Join(", ", constructorParameterList)});
             input1.PostStages += (stage as Stasistium.Stages.IStageBaseInput<{input1Name}, {input2Name}>).DoIt1;
             input2.PostStages += (stage as Stasistium.Stages.IStageBaseInput<{input1Name}, {input2Name}>).DoIt2;
             return stage;
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
