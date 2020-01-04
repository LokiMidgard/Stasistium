
using Stasistium.Documents;
using System.Collections.Immutable; 
using System.Threading.Tasks;
using Stasistium.Stages;
using System;

namespace Stasistium.Stages
{


    public class ConcatStage<T, TCache1> : GeneratedHelper.Multiple.Simple.OutputMultiSimpleInputSingle1List0StageBase<T, TCache1, T>
        where TCache1 : class
    {
        public ConcatStage(StagePerformHandler<T, TCache1> input1, GeneratorContext context) : base(input1, context)
        {
        }

        protected override Task<ImmutableList<IDocument<T>>> Work(IDocument<T> input1, OptionToken options)
        {
            return Task.FromResult(ImmutableList.Create(input1));
        }
    }



    public class ConcatStage<T, TCache1, TCache2> : GeneratedHelper.Multiple.Simple.OutputMultiSimpleInputSingle2List0StageBase<T, TCache1, T, TCache2, T>
        where TCache1 : class
        where TCache2 : class
    {
        public ConcatStage(StagePerformHandler<T, TCache1> input1, StagePerformHandler<T, TCache2> input2, GeneratorContext context) : base(input1, input2, context)
        {
        }

        protected override Task<ImmutableList<IDocument<T>>> Work(IDocument<T> input1, IDocument<T> input2, OptionToken options)
        {
            return Task.FromResult(ImmutableList.Create(input1, input2));
        }
    }



    public class ConcatStage<T, TCache1, TCache2, TCache3> : GeneratedHelper.Multiple.Simple.OutputMultiSimpleInputSingle3List0StageBase<T, TCache1, T, TCache2, T, TCache3, T>
        where TCache1 : class
        where TCache2 : class
        where TCache3 : class
    {
        public ConcatStage(StagePerformHandler<T, TCache1> input1, StagePerformHandler<T, TCache2> input2, StagePerformHandler<T, TCache3> input3, GeneratorContext context) : base(input1, input2, input3, context)
        {
        }

        protected override Task<ImmutableList<IDocument<T>>> Work(IDocument<T> input1, IDocument<T> input2, IDocument<T> input3, OptionToken options)
        {
            return Task.FromResult(ImmutableList.Create(input1, input2, input3));
        }
    }



    public class ConcatStage<T, TCache1, TCache2, TCache3, TCache4> : GeneratedHelper.Multiple.Simple.OutputMultiSimpleInputSingle4List0StageBase<T, TCache1, T, TCache2, T, TCache3, T, TCache4, T>
        where TCache1 : class
        where TCache2 : class
        where TCache3 : class
        where TCache4 : class
    {
        public ConcatStage(StagePerformHandler<T, TCache1> input1, StagePerformHandler<T, TCache2> input2, StagePerformHandler<T, TCache3> input3, StagePerformHandler<T, TCache4> input4, GeneratorContext context) : base(input1, input2, input3, input4, context)
        {
        }

        protected override Task<ImmutableList<IDocument<T>>> Work(IDocument<T> input1, IDocument<T> input2, IDocument<T> input3, IDocument<T> input4, OptionToken options)
        {
            return Task.FromResult(ImmutableList.Create(input1, input2, input3, input4));
        }
    }




    public class ConcatStageMany<T, TItemCache1, TCache1> : GeneratedHelper.Multiple.Simple.OutputMultiSimpleInputSingle0List1StageBase<T, TItemCache1, TCache1, T>
        where TItemCache1 : class
        where TCache1 : class
    {
        public ConcatStageMany(StagePerformHandler<T, TItemCache1, TCache1> input1, GeneratorContext context) : base(input1, context)
        {
        }

        protected override Task<ImmutableList<IDocument<T>>> Work(ImmutableList<IDocument<T>> input1, OptionToken options)
        {
            if (input1 is null)
                throw new ArgumentNullException(nameof(input1));

            return Task.FromResult(input1
            );
        }
    }



    public class ConcatStageMany<T, TItemCache1, TCache1, TItemCache2, TCache2> : GeneratedHelper.Multiple.Simple.OutputMultiSimpleInputSingle0List2StageBase<T, TItemCache1, TCache1, T, TItemCache2, TCache2, T>
        where TItemCache1 : class
        where TCache1 : class
        where TItemCache2 : class
        where TCache2 : class
    {
        public ConcatStageMany(StagePerformHandler<T, TItemCache1, TCache1> input1, StagePerformHandler<T, TItemCache2, TCache2> input2, GeneratorContext context) : base(input1, input2, context)
        {
        }

        protected override Task<ImmutableList<IDocument<T>>> Work(ImmutableList<IDocument<T>> input1, ImmutableList<IDocument<T>> input2, OptionToken options)
        {
            if (input1 is null)
                throw new ArgumentNullException(nameof(input1));

            return Task.FromResult(input1
                .AddRange(input2)
            );
        }
    }



    public class ConcatStageMany<T, TItemCache1, TCache1, TItemCache2, TCache2, TItemCache3, TCache3> : GeneratedHelper.Multiple.Simple.OutputMultiSimpleInputSingle0List3StageBase<T, TItemCache1, TCache1, T, TItemCache2, TCache2, T, TItemCache3, TCache3, T>
        where TItemCache1 : class
        where TCache1 : class
        where TItemCache2 : class
        where TCache2 : class
        where TItemCache3 : class
        where TCache3 : class
    {
        public ConcatStageMany(StagePerformHandler<T, TItemCache1, TCache1> input1, StagePerformHandler<T, TItemCache2, TCache2> input2, StagePerformHandler<T, TItemCache3, TCache3> input3, GeneratorContext context) : base(input1, input2, input3, context)
        {
        }

        protected override Task<ImmutableList<IDocument<T>>> Work(ImmutableList<IDocument<T>> input1, ImmutableList<IDocument<T>> input2, ImmutableList<IDocument<T>> input3, OptionToken options)
        {
            if (input1 is null)
                throw new ArgumentNullException(nameof(input1));

            return Task.FromResult(input1
                .AddRange(input2)
                .AddRange(input3)
            );
        }
    }



    public class ConcatStageMany<T, TItemCache1, TCache1, TItemCache2, TCache2, TItemCache3, TCache3, TItemCache4, TCache4> : GeneratedHelper.Multiple.Simple.OutputMultiSimpleInputSingle0List4StageBase<T, TItemCache1, TCache1, T, TItemCache2, TCache2, T, TItemCache3, TCache3, T, TItemCache4, TCache4, T>
        where TItemCache1 : class
        where TCache1 : class
        where TItemCache2 : class
        where TCache2 : class
        where TItemCache3 : class
        where TCache3 : class
        where TItemCache4 : class
        where TCache4 : class
    {
        public ConcatStageMany(StagePerformHandler<T, TItemCache1, TCache1> input1, StagePerformHandler<T, TItemCache2, TCache2> input2, StagePerformHandler<T, TItemCache3, TCache3> input3, StagePerformHandler<T, TItemCache4, TCache4> input4, GeneratorContext context) : base(input1, input2, input3, input4, context)
        {
        }

        protected override Task<ImmutableList<IDocument<T>>> Work(ImmutableList<IDocument<T>> input1, ImmutableList<IDocument<T>> input2, ImmutableList<IDocument<T>> input3, ImmutableList<IDocument<T>> input4, OptionToken options)
        {
            if (input1 is null)
                throw new ArgumentNullException(nameof(input1));

            return Task.FromResult(input1
                .AddRange(input2)
                .AddRange(input3)
                .AddRange(input4)
            );
        }
    }



}

namespace Stasistium
{


    public static partial class StageExtensions
    {
        public static ConcatStage<T, TCache1> Concat<T, TCache1>(this StageBase<T, TCache1> input1)
            where TCache1 : class
        {
            if(input1 is null)
                 throw new ArgumentNullException(nameof(input1));
            return new ConcatStage<T, TCache1>(input1.DoIt, input1.Context);
        }
        public static ConcatStage<T, TCache1, TCache2> Concat<T, TCache1, TCache2>(this StageBase<T, TCache1> input1, StageBase<T, TCache2> input2)
            where TCache1 : class
            where TCache2 : class
        {
            if(input1 is null)
                 throw new ArgumentNullException(nameof(input1));
            if(input2 is null)
                 throw new ArgumentNullException(nameof(input2));
            return new ConcatStage<T, TCache1, TCache2>(input1.DoIt, input2.DoIt, input1.Context);
        }
        public static ConcatStage<T, TCache1, TCache2, TCache3> Concat<T, TCache1, TCache2, TCache3>(this StageBase<T, TCache1> input1, StageBase<T, TCache2> input2, StageBase<T, TCache3> input3)
            where TCache1 : class
            where TCache2 : class
            where TCache3 : class
        {
            if(input1 is null)
                 throw new ArgumentNullException(nameof(input1));
            if(input2 is null)
                 throw new ArgumentNullException(nameof(input2));
            if(input3 is null)
                 throw new ArgumentNullException(nameof(input3));
            return new ConcatStage<T, TCache1, TCache2, TCache3>(input1.DoIt, input2.DoIt, input3.DoIt, input1.Context);
        }
        public static ConcatStage<T, TCache1, TCache2, TCache3, TCache4> Concat<T, TCache1, TCache2, TCache3, TCache4>(this StageBase<T, TCache1> input1, StageBase<T, TCache2> input2, StageBase<T, TCache3> input3, StageBase<T, TCache4> input4)
            where TCache1 : class
            where TCache2 : class
            where TCache3 : class
            where TCache4 : class
        {
            if(input1 is null)
                 throw new ArgumentNullException(nameof(input1));
            if(input2 is null)
                 throw new ArgumentNullException(nameof(input2));
            if(input3 is null)
                 throw new ArgumentNullException(nameof(input3));
            if(input4 is null)
                 throw new ArgumentNullException(nameof(input4));
            return new ConcatStage<T, TCache1, TCache2, TCache3, TCache4>(input1.DoIt, input2.DoIt, input3.DoIt, input4.DoIt, input1.Context);
        }

        public static ConcatStageMany<T, TItemCache1, TCache1> Concat<T, TItemCache1, TCache1>(this MultiStageBase<T, TItemCache1, TCache1> input1)
            where TItemCache1 : class
        where TCache1 : class
        {
            if(input1 is null)
                 throw new ArgumentNullException(nameof(input1));
            return new ConcatStageMany<T, TItemCache1, TCache1>(input1.DoIt, input1.Context);
        }
        public static ConcatStageMany<T, TItemCache1, TCache1, TItemCache2, TCache2> Concat<T, TItemCache1, TCache1, TItemCache2, TCache2>(this MultiStageBase<T, TItemCache1, TCache1> input1, MultiStageBase<T, TItemCache2, TCache2> input2)
            where TItemCache1 : class
        where TCache1 : class
            where TItemCache2 : class
        where TCache2 : class
        {
            if(input1 is null)
                 throw new ArgumentNullException(nameof(input1));
            if(input2 is null)
                 throw new ArgumentNullException(nameof(input2));
            return new ConcatStageMany<T, TItemCache1, TCache1, TItemCache2, TCache2>(input1.DoIt, input2.DoIt, input1.Context);
        }
        public static ConcatStageMany<T, TItemCache1, TCache1, TItemCache2, TCache2, TItemCache3, TCache3> Concat<T, TItemCache1, TCache1, TItemCache2, TCache2, TItemCache3, TCache3>(this MultiStageBase<T, TItemCache1, TCache1> input1, MultiStageBase<T, TItemCache2, TCache2> input2, MultiStageBase<T, TItemCache3, TCache3> input3)
            where TItemCache1 : class
        where TCache1 : class
            where TItemCache2 : class
        where TCache2 : class
            where TItemCache3 : class
        where TCache3 : class
        {
            if(input1 is null)
                 throw new ArgumentNullException(nameof(input1));
            if(input2 is null)
                 throw new ArgumentNullException(nameof(input2));
            if(input3 is null)
                 throw new ArgumentNullException(nameof(input3));
            return new ConcatStageMany<T, TItemCache1, TCache1, TItemCache2, TCache2, TItemCache3, TCache3>(input1.DoIt, input2.DoIt, input3.DoIt, input1.Context);
        }
        public static ConcatStageMany<T, TItemCache1, TCache1, TItemCache2, TCache2, TItemCache3, TCache3, TItemCache4, TCache4> Concat<T, TItemCache1, TCache1, TItemCache2, TCache2, TItemCache3, TCache3, TItemCache4, TCache4>(this MultiStageBase<T, TItemCache1, TCache1> input1, MultiStageBase<T, TItemCache2, TCache2> input2, MultiStageBase<T, TItemCache3, TCache3> input3, MultiStageBase<T, TItemCache4, TCache4> input4)
            where TItemCache1 : class
        where TCache1 : class
            where TItemCache2 : class
        where TCache2 : class
            where TItemCache3 : class
        where TCache3 : class
            where TItemCache4 : class
        where TCache4 : class
        {
            if(input1 is null)
                 throw new ArgumentNullException(nameof(input1));
            if(input2 is null)
                 throw new ArgumentNullException(nameof(input2));
            if(input3 is null)
                 throw new ArgumentNullException(nameof(input3));
            if(input4 is null)
                 throw new ArgumentNullException(nameof(input4));
            return new ConcatStageMany<T, TItemCache1, TCache1, TItemCache2, TCache2, TItemCache3, TCache3, TItemCache4, TCache4>(input1.DoIt, input2.DoIt, input3.DoIt, input4.DoIt, input1.Context);
        }
    }
}
