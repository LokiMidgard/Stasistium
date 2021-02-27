using AdaptMark.Parsers.Markdown;
using Stasistium.Documents;
using Stasistium.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public class MarkdownHeaderToMetadataStage<TMetadata> : StageBaseSimple<MarkdownDocument, MarkdownDocument>
        where TMetadata : class, new()
    {
        private readonly MetadataUpdate<TMetadata>? update;

        [StageName("MarkdownHeader")]
        public MarkdownHeaderToMetadataStage(MetadataUpdate<TMetadata>? update, IGeneratorContext context, string? name) : base(context, name)
        {
            this.update = update;
        }

        protected override Task<IDocument<MarkdownDocument>> Work(IDocument<MarkdownDocument> input, OptionToken options)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            var document = input.Value;
            var yamlHeader = document.Blocks.OfType<AdaptMark.Parsers.Markdown.Blocks.YamlHeaderBlock>();

            var newMarkdown = new MarkdownDocument
            {
                Blocks = document.Blocks.Where(x => !(x is AdaptMark.Parsers.Markdown.Blocks.YamlHeaderBlock)).ToList()
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
