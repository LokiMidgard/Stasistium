using Stasistium.Documents;
using Stasistium.Stages;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Stasistium.Test
{
    public class SingleAssertStage<T> : StageBase<string, T>
        where T : class
    {
        public SingleAssertStage(IGeneratorContext context, string? name = null) : base(context, name)
        {
        }

        protected override Task<StageResult<string, T>> DoInternal([AllowNull] T cache, OptionToken options)
        {

            throw new System.NotImplementedException();

        }
    }


}
