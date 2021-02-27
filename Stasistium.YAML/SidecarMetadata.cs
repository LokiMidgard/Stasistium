using Stasistium.Documents;
using System;
using System.Linq;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using System.IO;

namespace Stasistium.Stages
{
    public class SidecarMetadata<TMetadata> : StageBase<Stream, Stream>
    {
        private readonly MetadataUpdate<TMetadata>? update;

        [StageName("Sidecar")]
        public SidecarMetadata(string sidecarExtension, IGeneratorContext context, MetadataUpdate<TMetadata>? update = null, string? name = null) : base(context, name)
        {
            if (sidecarExtension is null)
                throw new ArgumentNullException(nameof(sidecarExtension));
            if (!sidecarExtension.StartsWith(".", StringComparison.InvariantCultureIgnoreCase))
                sidecarExtension = "." + sidecarExtension;
            this.SidecarExtension = sidecarExtension;
            this.update = update;
        }

        public string SidecarExtension { get; }

        protected override Task<ImmutableList<IDocument<Stream>>> Work(ImmutableList<IDocument<Stream>> input, OptionToken options)
        {
            var inputList = input;

            var sidecarLookup = inputList.Where(x => Path.GetExtension(x.Id) == this.SidecarExtension)
                .ToDictionary(x => Path.Combine(Path.GetDirectoryName(x.Id) ?? string.Empty, Path.GetFileNameWithoutExtension(x.Id)));

            var files = inputList.Where(x => Path.GetExtension(x.Id) != this.SidecarExtension);


            var list = files.Select(file =>
            {
                if (sidecarLookup.TryGetValue(file.Id, out var sidecar))
                {
                    var deserializer = new YamlDotNet.Serialization.DeserializerBuilder()
                        .WithNamingConvention(YamlDotNet.Serialization.NamingConventions.CamelCaseNamingConvention.Instance)
                        .Build();

                    var oldMetadata = file.Metadata;
                    MetadataContainer? newMetadata;
                    try
                    {
                        using var stream = sidecar.Value;
                        using var reader = new StreamReader(stream);
                        var metadata = deserializer.Deserialize<TMetadata>(reader);

                        if (metadata != null)
                            if (this.update != null)
                                newMetadata = oldMetadata.AddOrUpdate(metadata.GetType(), metadata, (oldValue, newValue) => this.update((TMetadata)oldValue! /*AllowNull is set, so why the warnign?*/, (TMetadata)newValue));
                            else
                                newMetadata = oldMetadata.Add(metadata.GetType(), metadata);
                        else
                            newMetadata = null;
                    }
                    catch (YamlDotNet.Core.YamlException e) when (e.InnerException is null) // Hope that only happens when it does not match.
                    {
                        newMetadata = null;
                    }

                    if (newMetadata != null)
                        file = file.With(newMetadata);


                    return file;
                }
                else
                    return file;
            }).ToImmutableList();

            return Task.FromResult(list);
        }


    }


}
