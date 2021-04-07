using Microsoft.Extensions.FileProviders;
using Stasistium;
using Stasistium.Documents;
using Stasistium.Razor;
using System;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;

namespace Stasistium.Stages
{
    public class RazorProviderStage : StageBase<IFileProvider, RazorProvider>
    {
        private readonly string id;
        private readonly string? viewStartId;

        public RazorProviderStage(string contentId, IGeneratorContext context, string? id = null, string? viewStartId = null, string? name = null) : base(context, name)
        {
            this.ContentId = contentId;
            this.id = id ?? Guid.NewGuid().ToString();
            this.viewStartId = viewStartId;
        }

        public string ContentId { get; }

        protected override Task<ImmutableList<IDocument<RazorProvider>>> Work(ImmutableList<IDocument<IFileProvider>> input, OptionToken options)
        {
            var renderConfiguration = new RenderConfiguration(this.ContentId) { ViewStartId = this.viewStartId };
            var fileProviders = input.Select(x => x.Value);
            var renderer = RazorViewToStringRenderer.GetRenderer(fileProviders, renderConfiguration);
            var render = new RazorProvider(renderer);
            var hash = this.Context.GetHashForString(string.Join(",", input.Select(x => x.Hash)));
            return Task.FromResult(ImmutableList.Create(this.Context.CreateDocument(render, hash, this.id)));
        }
    }
}
