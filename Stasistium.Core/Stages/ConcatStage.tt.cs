﻿
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
    }
}
