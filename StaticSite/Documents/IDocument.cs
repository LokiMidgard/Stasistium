using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace StaticSite.Documents
{
    public interface IDocument
    {
        ReadOnlyMemory<byte> Hash { get; }
        string Id { get; }
        MetadataContainer Metadata { get; }


    }
    public interface IDocument<T> : IDocument
    {

        T Value { get; }
    }

    public interface IHashable
    {
        ReadOnlyMemory<byte> GenerateHash();

    }


    public static class Document
    {
        public static IDocument<T> Create<T>(T value, string Id, MetadataContainer metadata)
        {
            throw new NotImplementedException();
        }
    }






    public abstract class One2OneModuleBase<TIn, TOut>
    {

        internal class TransformDic
        {
            Dictionary<string, (ReadOnlyMemory<byte> input, ReadOnlyMemory<byte> output)> hash;
        }

        internal (IEnumerable<IDocument> documents, TransformDic newHash) Transform(IEnumerable<IDocument> documents, TransformDic old)
        {
            var result = new List<IDocument>();

            var cach = new TransformDic();

            foreach (var document in documents)
            {
                if (document is IDocument<TIn> input)
                {

                    result.Add(this.Transform(input));
                }
                else
                    result.Add(document);
            }

            return (result, cach);
        }


        protected abstract IDocument<TOut> Transform(IDocument<TIn> input);

    }
    public abstract class One2OneModule<TIn, TOut> : One2OneModuleBase<TIn, TOut>
    {

        protected sealed override IDocument<TOut> Transform(IDocument<TIn> input)
        {
            return Document.Create(this.Transform(input.Value), input.Id, input.Metadata);
        }

        protected abstract TOut Transform(TIn input);

    }



}
