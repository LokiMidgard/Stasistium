using Microsoft.Extensions.FileProviders;
using Stasistium;
using Stasistium.Documents;
using Stasistium.Razor;
using System;
using System.Collections.Immutable;
using System.IO;
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


    public class RazorStage<T, TModel> : StageBase<T, RazorProvider, string>
        where TModel : class
    {
        private readonly Func<IDocument<T>, TModel> selector;

        public RazorStage(Func<IDocument<T>, TModel> selector, IGeneratorContext context, string? name) : base(context, name)
        {
            this.selector = selector;
        }


        protected override async Task<ImmutableList<IDocument<string>>> Work(ImmutableList<IDocument<T>> inputDocument, ImmutableList<IDocument<RazorProvider>> inputrendererList, OptionToken options)
        {
            if (inputDocument is null)
                throw new ArgumentNullException(nameof(inputDocument));
            if (inputrendererList is null)
                throw new ArgumentNullException(nameof(inputrendererList));
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            var inputRenderer = inputrendererList.Single();

            var inputs = await Task.WhenAll(inputDocument.Select(async doc =>
            {
                var renderer = inputRenderer.Value.Renderer;
                var result = await renderer.RenderViewToStringAsync(inputRenderer.Id, this.selector(doc)).ConfigureAwait(false);
                var output = doc.With(result, this.Context.GetHashForString(result));
                return output;
            })).ConfigureAwait(false);

            return inputs.ToImmutableList();
        }


    }

    public class RazorStage<T> : StageBase<T, RazorProvider, string>
    {

        public RazorStage(IGeneratorContext context, string? name) : base(context, name)
        {
        }


        protected override async Task<ImmutableList<IDocument<string>>> Work(ImmutableList<IDocument<T>> inputDocument, ImmutableList<IDocument<RazorProvider>> inputrendererList, OptionToken options)
        {
            if (inputDocument is null)
                throw new ArgumentNullException(nameof(inputDocument));
            if (inputrendererList is null)
                throw new ArgumentNullException(nameof(inputrendererList));
            if (options is null)
                throw new ArgumentNullException(nameof(options));

            var inputRenderer = inputrendererList.Single();

            var inputs = await Task.WhenAll(inputDocument.Select(async doc =>
            {
                var renderer = inputRenderer.Value.Renderer;
                var result = await renderer.RenderViewToStringAsync(doc.Id, doc).ConfigureAwait(false);
                var output = doc.With(result, this.Context.GetHashForString(result));
                return output;
            })).ConfigureAwait(false);

            return inputs.ToImmutableList();
        }


    }
}
