using System;
using System.IO;
using Microsoft.Extensions.FileProviders;
using Stasistium.Razor;
using Stasistium.Stages;

namespace Stasistium
{

    public static class RazorStageExtension
    {

        public static RazorProviderStage<TInputItemCache, TInputCache> RazorProvider<TInputItemCache, TInputCache>(this MultiStageBase<IFileProvider, TInputItemCache, TInputCache> input, string contentProviderId, string? id = null)
            where TInputCache : class
            where TInputItemCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (contentProviderId is null)
                throw new ArgumentNullException(nameof(contentProviderId));
            return new RazorProviderStage<TInputItemCache, TInputCache>(input.DoIt, contentProviderId, id, input.Context);
        }

        public static RazorStage<T, TDocumentCache, TRenderCache> Razor<T, TDocumentCache, TRenderCache>(this StageBase<T, TDocumentCache> input, StageBase<RazorProvider, TRenderCache> renderer)
            where TDocumentCache : class
            where TRenderCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (renderer is null)
                throw new ArgumentNullException(nameof(renderer));

            if (!ReferenceEquals(input.Context, renderer.Context))
                throw new ArgumentException("Both inputs must use the same Context");

            return new RazorStage<T, TDocumentCache, TRenderCache>(input.DoIt, renderer.DoIt, input.Context);
        }

        public static FileProviderStage<TInputItemCache, TInputCache> FileProvider<TInputItemCache, TInputCache>(this MultiStageBase<Stream, TInputItemCache, TInputCache> input, string providerId)
            where TInputCache : class
            where TInputItemCache : class
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));
            if (providerId is null)
                throw new ArgumentNullException(nameof(providerId));
            return new FileProviderStage<TInputItemCache, TInputCache>(providerId, input.DoIt, input.Context);
        }
    }
}
