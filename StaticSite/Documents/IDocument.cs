﻿using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;

namespace StaticSite.Documents
{
    public interface IDocument
    {
        string Hash { get; }
        string Id { get; }
        MetadataContainer Metadata { get; }

        IDocument With(MetadataContainer metadata);

    }
    public interface IDocument<T> : IDocument
    {

        T Value { get; }

        IDocument<TNew> With<TNew>(TNew newItem, string newHash);
        IDocument<TNew> With<TNew>(Func<TNew> newItem, string newHash);
        new IDocument<T> With(MetadataContainer metadata);
    }




 
}
