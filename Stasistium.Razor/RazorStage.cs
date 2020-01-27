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
    public class RazorProviderStage<TInputItemCache, TInputCache> : GeneratedHelper.Single.Simple.OutputSingleInputSingleSimple0List1StageBase<IFileProvider, TInputItemCache, TInputCache, RazorProvider>
    where TInputItemCache : class
        where TInputCache : class
    {
        private readonly string id;
        private readonly string? viewStartId;

        public RazorProviderStage(StagePerformHandler<IFileProvider, TInputItemCache, TInputCache> inputList0, string contentId, string? id, string? viewStartId, IGeneratorContext context, string? name) : base(inputList0, context, name)
        {
            this.ContentId = contentId;
            this.id = id ?? Guid.NewGuid().ToString();
            this.viewStartId = viewStartId;
        }

        public string ContentId { get; }

        protected override Task<IDocument<RazorProvider>> Work(ImmutableList<IDocument<IFileProvider>> inputList0, OptionToken options)
        {
            var render = new RazorProvider(RazorViewToStringRenderer.GetRenderer(inputList0.Select(x => x.Value), new RenderConfiguration(this.ContentId) { ViewStartId = this.viewStartId }));
            var hash = this.Context.GetHashForString(string.Join(",", inputList0.Select(x => x.Hash)));

            return Task.FromResult(this.Context.Create(render, hash, this.id));
        }
    }


    public class RazorStage<T, TDocumentCache, TRendererCache> : GeneratedHelper.Single.Simple.OutputSingleInputSingleSimple2List0StageBase<T, TDocumentCache, RazorProvider, TRendererCache, string>
        where TDocumentCache : class
        where TRendererCache : class
    {
        private readonly StagePerformHandler<T, TDocumentCache> inputDocument;
        private readonly StagePerformHandler<RazorProvider, TRendererCache> inputRazor;

        public RazorStage(StagePerformHandler<T, TDocumentCache> inputDocument, StagePerformHandler<RazorProvider, TRendererCache> inputRazor, IGeneratorContext context, string? name) : base(inputDocument, inputRazor, context, name)
        {
            this.inputDocument = inputDocument;
            this.inputRazor = inputRazor;
        }


        protected override async Task<IDocument<string>> Work(IDocument<T> input, IDocument<RazorProvider> rendererDocument, OptionToken options)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (rendererDocument is null)
                throw new ArgumentNullException(nameof(rendererDocument));

            var renderer = rendererDocument.Value.Renderer;
            var result = await renderer.RenderViewToStringAsync(input.Id, input).ConfigureAwait(false);
            var output = input.With(result, this.Context.GetHashForString(result));
            return output;
        }

        public RazorStage<T, TModel, TDocumentCache, TRendererCache> WithModel<TModel>(Func<IDocument<T>, TModel> selector, string? name = null)
            where TModel : class
            => new RazorStage<T, TModel, TDocumentCache, TRendererCache>(this.inputDocument, this.inputRazor, selector, this.Context, name);
    }
    public class RazorStage<T, TModel, TDocumentCache, TRendererCache> : GeneratedHelper.Single.Simple.OutputSingleInputSingleSimple2List0StageBase<T, TDocumentCache, RazorProvider, TRendererCache, string>
        where TDocumentCache : class
        where TRendererCache : class
        where TModel : class
    {
        private readonly Func<IDocument<T>, TModel> selector;

        public RazorStage(StagePerformHandler<T, TDocumentCache> inputDocument, StagePerformHandler<RazorProvider, TRendererCache> inputRazor, Func<IDocument<T>, TModel> selector, IGeneratorContext context, string? name) : base(inputDocument, inputRazor, context, name)
        {
            this.selector = selector;
        }


        protected override async Task<IDocument<string>> Work(IDocument<T> input, IDocument<RazorProvider> rendererDocument, OptionToken options)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (rendererDocument is null)
                throw new ArgumentNullException(nameof(rendererDocument));

            var renderer = rendererDocument.Value.Renderer;
            var result = await renderer.RenderViewToStringAsync(input.Id, selector(input)).ConfigureAwait(false);
            var output = input.With(result, this.Context.GetHashForString(result));
            return output;
        }
    }
}
