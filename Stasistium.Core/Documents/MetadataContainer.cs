using Stasistium.Documents;
using Stasistium.Helper;
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Stasistium.Documents
{
    [return: MaybeNull]
    public delegate T MetadataUpdate<T>([AllowNull]T oldValue, T newValue);
    public delegate object? MetadataUpdate(object? oldValue, object newValue);

    public sealed class MetadataContainer
    {
        internal static MetadataContainer EmptyFromContext(GeneratorContext context) => new MetadataContainer(ImmutableDictionary<Type, object>.Empty, context);

        private readonly ImmutableDictionary<Type, object> values;

        private MetadataContainer(ImmutableDictionary<Type, object> values, IGeneratorContext context)
        {
            this.values = values ?? throw new ArgumentNullException(nameof(values));
            this.Context = context;
        }

        private string GenerateHash(ImmutableDictionary<Type, object> values)
        {
            var hash = new StringBuilder();
            foreach (var (key, value) in values.OrderBy(x => x.Key.FullName).Select(x => (key: x.Key.FullName, value: this.Context.GetHashForObject(x.Value))))
            {
                hash.Append("<");
                hash.Append(System.Net.WebUtility.HtmlEncode(key));
                hash.Append("><");
                hash.Append(System.Net.WebUtility.HtmlEncode(value));
                hash.Append(">");
            }
            return hash.ToString();
        }


        private string? hash;
        public string Hash
        {
            get
            {
                if (this.hash is null)
                    this.hash = this.GenerateHash(this.values);
                return this.hash;
            }
        }

        public IGeneratorContext Context { get; }

        [return: MaybeNull]
        public T GetValue<T>()
            where T : class
        {
            if (this.values.TryGetValue(typeof(T), out var obj))
                return (T)obj;
            return null!/*It may be null, so ther should no warning*/;
        }

        public MetadataContainer AddOrUpdate<T>(T value)
            where T : class => new MetadataContainer(this.values.SetItem(typeof(T), value), this.Context);

        public MetadataContainer AddOrUpdate<T>(T value, MetadataUpdate<T> updateCallback)
            where T : class
        {
            if (updateCallback is null)
                throw new ArgumentNullException(nameof(updateCallback));
            if (!this.values.TryGetValue(typeof(T), out object? obj))
                obj = null;
            var newValue = updateCallback(obj as T ?? default, value);
            if (newValue is null)
                return new MetadataContainer(this.values.Remove(typeof(T)), this.Context);
            return new MetadataContainer(this.values.SetItem(typeof(T), newValue), this.Context);
        }

        public MetadataContainer Add<T>(T value)
            where T : class => new MetadataContainer(this.values.Add(typeof(T), value), this.Context);

        public MetadataContainer? Update<T>(T value)
            where T : class
        {
            if (this.values.ContainsKey(typeof(T)))
                return new MetadataContainer(this.values.SetItem(typeof(T), value), this.Context);
            else
                return null;
        }

        public MetadataContainer AddOrUpdate(Type t, object value)
        {
            if (t is null)
                throw new ArgumentNullException(nameof(t));
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            return new MetadataContainer(this.values.SetItem(t, CheckTypeCast(t, value)), this.Context);
        }

        public MetadataContainer AddOrUpdate(Type t, object value, MetadataUpdate updateCallback)
        {
            if (t is null)
                throw new ArgumentNullException(nameof(t));
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            if (updateCallback is null)
                throw new ArgumentNullException(nameof(updateCallback));
            CheckTypeCast(t, value);
            if (!this.values.TryGetValue(t, out object? oldValue))
                oldValue = null;
            var newValue = CheckTypeCast(t, updateCallback(oldValue ?? t.GetDefault(), value));
            if (newValue is null)
                return new MetadataContainer(this.values.Remove(t), this.Context);
            return new MetadataContainer(this.values.SetItem(t, newValue), this.Context);
        }

        public MetadataContainer Add(Type t, object value)
        {
            if (t is null)
                throw new ArgumentNullException(nameof(t));
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            return new MetadataContainer(this.values.Add(t, CheckTypeCast(t, value)), this.Context);
        }

        public MetadataContainer? Update(Type t, object value)
        {
            if (t is null)
                throw new ArgumentNullException(nameof(t));
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            CheckTypeCast(t, value);
            if (this.values.ContainsKey(t))
                return new MetadataContainer(this.values.SetItem(t, value), this.Context);
            else
                return null;
        }


        [return: NotNullIfNotNull("value")]
        private static object? CheckTypeCast(Type keyType, object? value)
        {


            if (value != null && !keyType.IsAssignableFrom(value.GetType()))
                throw new InvalidCastException($"Type {value.GetType()} can't be assigned to {keyType}");
            if (keyType.IsValueType)
                throw new InvalidCastException($"Type null can't be assigned to value type {keyType}");
            return value;
        }

    }

}
