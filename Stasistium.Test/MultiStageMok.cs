using Stasistium.Documents;
using Stasistium.Stages;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;

namespace Stasistium.Test
{
    public class MultiStageMok : MultiStageBase<string, string, ImmutableList<(string id, string content)>>
    {
        public MultiStageMok(IGeneratorContext context, string? name = null) : base(context, name)
        {
        }

        public ImmutableList<(string id, string content)> Current { get; set; } = ImmutableList<(string id, string content)>.Empty;
        public int Called { get; set; }

        public ImmutableList<ImmutableList<(string id, string content)>?> CalledCaches { get; private set; } = ImmutableList<ImmutableList<(string id, string content)>?>.Empty;

        protected override Task<StageResultList<string, string, ImmutableList<(string id, string content)>>> DoInternal([AllowNull] ImmutableList<(string id, string content)>? cache, OptionToken options)
        {
            this.CalledCaches = this.CalledCaches.Add(cache);
            this.Called++;

            bool hasChanges = cache is null
                || !this.Current.SequenceEqual(cache);

            var list = this.Current.Select(c =>
            {

                var nullable = cache?.Select(x => x as (string id, string content)?)
                    .FirstOrDefault(x => x.Value.id == c.id);

                var hasChanges = nullable is null
                    || nullable.Value.content != c.content;
                var document = this.Context.Create(c.content, c.content, c.id);
                return StageResult.Create(document, document.Hash, hasChanges, document.Id);
            }).ToImmutableList();

            return Task.FromResult(StageResultList.Create(list, this.Current, hasChanges, this.Current.Select(x => x.id).ToImmutableList()));
        }
    }
}
