using System;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Stasistium.Razor;
using Stasistium.Stages;

namespace Stasistium
{

    public static class RazorStageExtension
    {

        public static RazorProviderStage<TInputItemCache, TInputCache> RazorProvider<TInputItemCache, TInputCache>(this MultiStageBase<IFileProvider, TInputItemCache, TInputCache> input, string contentProviderId, string? viewStartId = null, string? id = null, string? name = null)
            where TInputCache : class
            where TInputItemCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (contentProviderId is null)
                throw new ArgumentNullException(nameof(contentProviderId));
            return new RazorProviderStage<TInputItemCache, TInputCache>(input, contentProviderId, id, viewStartId, input.Context, name);
        }

        public static RazorStage<T, TDocumentCache, TRenderCache> Razor<T, TDocumentCache, TRenderCache>(this StageBase<T, TDocumentCache> input, StageBase<RazorProvider, TRenderCache> renderer, string? name = null)
            where TDocumentCache : class
            where TRenderCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (renderer is null)
                throw new ArgumentNullException(nameof(renderer));

            if (!input.Context.Equals(renderer.Context))
                throw new ArgumentException("Both inputs must use the same Context");

            return new RazorStage<T, TDocumentCache, TRenderCache>(input, renderer, input.Context, name);
        }

        public static FileProviderStage<TInputItemCache, TInputCache> FileProvider<TInputItemCache, TInputCache>(this MultiStageBase<Stream, TInputItemCache, TInputCache> input, string providerId, string? name = null)
            where TInputCache : class
            where TInputItemCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (providerId is null)
                throw new ArgumentNullException(nameof(providerId));
            return new FileProviderStage<TInputItemCache, TInputCache>(providerId, input, input.Context, name);
        }
    }
}
