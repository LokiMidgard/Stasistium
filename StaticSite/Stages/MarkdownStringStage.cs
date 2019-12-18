﻿using Microsoft.Toolkit.Parsers.Markdown;
using StaticSite.Documents;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace StaticSite.Stages
{
    public class MarkdownStringStage<TPreviousCache> : GeneratedHelper.Single.Simple.OutputSingleInputSingleSimple1List0StageBase<string, TPreviousCache, MarkdownDocument>
        where TPreviousCache : class
    {
        public MarkdownStringStage(StagePerformHandler<string, TPreviousCache> input, GeneratorContext context) : base(input, context)
        {
        }

        protected override Task<IDocument<MarkdownDocument>> Work(IDocument<string> input, OptionToken options)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            var document = new MarkdownDocument();
            document.Parse(input.Value);

            var hash = this.Context.GetHashForString(document.ToString());
            return Task.FromResult(input.With(document, hash));
        }

    }
    public class MarkdownHeaderToMetadataStage<TMetadata, TPreviousCache> : GeneratedHelper.Single.Simple.OutputSingleInputSingleSimple1List0StageBase<MarkdownDocument, TPreviousCache, MarkdownDocument>
        where TMetadata : class, new()
        where TPreviousCache : class
    {
        private readonly MetadataUpdate<TMetadata>? update;

        public MarkdownHeaderToMetadataStage(StagePerformHandler<MarkdownDocument, TPreviousCache> input, MetadataUpdate<TMetadata>? update, GeneratorContext context) : base(input, context)
        {
            this.update = update;
        }

        protected override Task<IDocument<MarkdownDocument>> Work(IDocument<MarkdownDocument> input, OptionToken options)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            var document = input.Value;
            var yamlHeader = document.Blocks.OfType<Microsoft.Toolkit.Parsers.Markdown.Blocks.YamlHeaderBlock>();

            var newMarkdown = new MarkdownDocument
            {
                Blocks = document.Blocks.Where(x => !(x is Microsoft.Toolkit.Parsers.Markdown.Blocks.YamlHeaderBlock)).ToList()
            };
            var newDocument = input.With(newMarkdown, this.Context.GetHashForString(newMarkdown.ToString()));
            var metadata = newDocument.Metadata;
            var metadataEntrys = yamlHeader.Select(x =>
            {
                if (TryGetObjecFrom<TMetadata>(x.Children.ToDictionary(x => x.Key, x => x.Value as object), out var parsed))
                    return parsed;
                return null;
            })!.Where<TMetadata>(x => !(x is null));

            foreach (var entry in metadataEntrys)
            {
                if (this.update is null)
                    metadata = metadata.AddOrUpdate(entry);
                else
                    metadata = metadata.AddOrUpdate(entry, this.update);
            }

            newDocument.With(metadata);

            return Task.FromResult(newDocument);
        }

        private static bool TryGetObjecFrom<T>(IDictionary<string, object> source, out T parsed)
            where T : class, new()
        {

            object obj = new T();
            parsed = (T)obj;
            return TryGetObject(source, obj);

            bool TryGetObject(IDictionary<string, object> source, object obj)
            {
                if (source is null)
                    throw new System.ArgumentNullException(nameof(source));

                var someObjectType = typeof(T);

                foreach (var item in source)
                {
                    var propertyInfo = someObjectType.GetProperty(item.Key);

                    if (propertyInfo is null)
                        return false;

                    object? value;

                    var propertyType = propertyInfo.PropertyType;


                    switch (propertyInfo.PropertyType)
                    {

                        case Type t when t == typeof(string):
                            value = item.Value?.ToString();
                            break;


                        default:
                            if (item.Value is IDictionary<string, object> subDirectory)
                            {
                                var defaultConstructor = propertyType.GetConstructor(System.Type.EmptyTypes);
                                if (defaultConstructor is null)
                                    throw new ArgumentException($"The Object of type {propertyType} does not contains a default constructor.", nameof(parsed));
                                value = defaultConstructor.Invoke(null);
                                if (!TryGetObject(subDirectory, value))
                                    return false;
                            }
                            else
                            {

                                var converter = System.ComponentModel.TypeDescriptor.GetConverter(propertyType);

                                if (item.Value is null)
                                    value = propertyType.GetDefault();
                                else if (propertyType.IsAssignableFrom(item.Value.GetType()))
                                    value = item.Value;
                                else if (converter.CanConvertFrom(item.Value.GetType()))
                                    value = converter.ConvertFrom(item.Value);
                                else
                                    return false;
                            }

                            break;
                    }

                    propertyInfo.SetValue(obj, value, null);
                }

                return true;
            }
        }

    }
}
