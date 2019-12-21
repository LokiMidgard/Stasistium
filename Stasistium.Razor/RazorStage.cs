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

        public RazorProviderStage(StagePerformHandler<IFileProvider, TInputItemCache, TInputCache> inputList0, string contentId, string? id, GeneratorContext context) : base(inputList0, context)
        {
            this.ContentId = contentId;
            this.id = id ?? Guid.NewGuid().ToString();
        }

        public string ContentId { get; }

        protected override Task<IDocument<RazorProvider>> Work(ImmutableList<IDocument<IFileProvider>> inputList0, OptionToken options)
        {
            var render = new RazorProvider(RazorViewToStringRenderer.GetRenderer(inputList0.Select(x => x.Value), this.ContentId));
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

        public RazorStage(StagePerformHandler<T, TDocumentCache> inputDocument, StagePerformHandler<RazorProvider, TRendererCache> inputRazor, GeneratorContext context) : base(inputDocument, inputRazor, context)
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
            var result = await renderer.RenderViewToStringAsync(input.Id, input.Metadata).ConfigureAwait(false);
            var output = input.With(result, this.Context.GetHashForString(result));
            return output;
        }

        public RazorStage<T, TModel, TDocumentCache, TRendererCache> WithModel<TModel>()
            where TModel : class
            => new RazorStage<T, TModel, TDocumentCache, TRendererCache>(this.inputDocument, this.inputRazor, this.Context);
    }
    public class RazorStage<T, TModel, TDocumentCache, TRendererCache> : GeneratedHelper.Single.Simple.OutputSingleInputSingleSimple2List0StageBase<T, TDocumentCache, RazorProvider, TRendererCache, string>
        where TDocumentCache : class
        where TRendererCache : class
        where TModel : class
    {

        public RazorStage(StagePerformHandler<T, TDocumentCache> inputDocument, StagePerformHandler<RazorProvider, TRendererCache> inputRazor, GeneratorContext context) : base(inputDocument, inputRazor, context)
        {
        }


        protected override async Task<IDocument<string>> Work(IDocument<T> input, IDocument<RazorProvider> rendererDocument, OptionToken options)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (rendererDocument is null)
                throw new ArgumentNullException(nameof(rendererDocument));

            var renderer = rendererDocument.Value.Renderer;
            var result = await renderer.RenderViewToStringAsync(input.Id, input.Metadata.GetValue<TModel>()).ConfigureAwait(false);
            var output = input.With(result, this.Context.GetHashForString(result));
            return output;
        }
    }
}
