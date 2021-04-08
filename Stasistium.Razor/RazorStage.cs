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
                string result;
                try
                {
                    result = await renderer.RenderViewToStringAsync(doc.Id, this.selector(doc)).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    this.Context.Logger.Error($"Failed to render {doc.Id}:\n{e}");
                    result = $"<pre>\n{e}\n</pre>";
                }

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
                string result;

                try
                {
                    result = await renderer.RenderViewToStringAsync(doc.Id, doc).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    this.Context.Logger.Error($"Failed to render {doc.Id}:\n{e}");
                    result = $"<pre>\n{e}\n</pre>";
                }

                var output = doc.With(result, this.Context.GetHashForString(result));
                return output;
            })).ConfigureAwait(false);

            return inputs.ToImmutableList();
        }


    }
}
