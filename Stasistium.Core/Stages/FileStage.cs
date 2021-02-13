
using Stasistium.Documents;
using Stasistium.Stages;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public class FileStage : StageBaseSimple< string, Stream>
    {
        public FileStage(IGeneratorContext context, string? name) : base(context, name)
        {
        }

        protected override Task<IDocument<Stream>> Work(IDocument<string> input, OptionToken options)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (options is null)
                throw new ArgumentNullException(nameof(options));
            var file = new FileInfo(input.Value);
            if (!file.Exists)
                throw this.Context.Exception($"File \"{file.FullName}\" does not exists");

            var document = new FileDocument(file, file.Directory, null, this.Context) as IDocument<Stream>;
            document = document.With(input.Metadata);

            return Task.FromResult(document);
        }
    }

}