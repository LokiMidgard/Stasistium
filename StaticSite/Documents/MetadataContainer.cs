using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace StaticSite
{
    public sealed class MetadataContainer
    {

        public static readonly MetadataContainer Empty = new MetadataContainer(ImmutableDictionary<Type, object>.Empty);

        private readonly ImmutableDictionary<Type, object> values;

        private MetadataContainer(ImmutableDictionary<Type, object> values)
        {
            this.values = values ?? throw new ArgumentNullException(nameof(values));
        }

        [return: MaybeNull]
        public T GetValue<T>()
            where T : class
        {
            if (this.values.TryGetValue(typeof(T), out var obj))
                return (T)obj;
            return null;
        }

        public MetadataContainer AddOrUpdate<T>(T value)
            where T : class => new MetadataContainer(this.values.SetItem(typeof(T), value));

        public MetadataContainer Add<T>(T value)
    where T : class => new MetadataContainer(this.values.Add(typeof(T), value));
        public MetadataContainer? Update<T>(T value)
    where T : class
        {
            if (this.values.ContainsKey(typeof(T)))
                return new MetadataContainer(this.values.SetItem(typeof(T), value));
            else
                return null;
        }
    }

}
