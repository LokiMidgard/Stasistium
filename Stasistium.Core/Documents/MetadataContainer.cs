﻿using Stasistium.Documents;
using Stasistium.Helper;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;

namespace Stasistium.Documents
{
    [return: MaybeNull]
    public delegate T MetadataUpdate<T>([AllowNull] T oldValue, T newValue);
    public delegate object? MetadataUpdate(object? oldValue, object newValue);

    public sealed class MetadataContainer 
    {
        internal static MetadataContainer EmptyFromContext(GeneratorContext context) => new(ImmutableDictionary<Type, object>.Empty, context);

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
                hash.Append('<');
                hash.Append(System.Net.WebUtility.HtmlEncode(key));
                hash.Append("><");
                hash.Append(System.Net.WebUtility.HtmlEncode(value));
                hash.Append('>');
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

        public IEnumerable<Type> Keys => this.values.Keys;

        public IGeneratorContext Context { get; }

        public T GetValue<T>()
            where T : class
        {
            if (this.values.TryGetValue(typeof(T), out var obj))
                return (T)obj;
            // Its a Generic parameter
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            throw new ArgumentOutOfRangeException(nameof(T), $"No entry of Type {typeof(T)}");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
        }

        public T? TryGetValue<T>()
           where T : class
        {
            if (this.values.TryGetValue(typeof(T), out var obj))
                return (T)obj;
            return null;
        }

        public object GetValue(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (this.values.TryGetValue(type, out var obj))
                return obj;
            // Its a Generic parameter
#pragma warning disable CA2208 // Instantiate argument exceptions correctly
            throw new ArgumentOutOfRangeException(nameof(type), $"No entry of Type {type}");
#pragma warning restore CA2208 // Instantiate argument exceptions correctly
        }

        public object? TryGetValue(Type type)
        {
            if (type is null)
                throw new ArgumentNullException(nameof(type));
            if (this.values.TryGetValue(type, out var obj))
                return obj;
            return null;
        }



        public MetadataContainer AddOrUpdate<T>(T value)
            where T : class => new(this.values.SetItem(typeof(T), value), this.Context);

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

        public MetadataContainer Add(MetadataContainer container)
        {
            if (container is null)
                throw new ArgumentNullException(nameof(container));
            var current = this;
            foreach (var item in container.values)
                current = current.Add(item.Key, item.Value);
            return current;
        }
        public MetadataContainer AddOrUpdate(MetadataContainer container)
        {
            if (container is null)
                throw new ArgumentNullException(nameof(container));
            var current = this;
            foreach (var item in container.values)
                current = current.AddOrUpdate(item.Key, item.Value);
            return current;
        }

        public MetadataContainer Add<T>(T value)
            where T : class => new(this.values.Add(typeof(T), value), this.Context);

        public MetadataContainer Update<T>(T value, out T? oldValue)
            where T : class
        {
            if (this.values.ContainsKey(typeof(T)))
            {
                oldValue = (T)this.values[typeof(T)];
                return new MetadataContainer(this.values.SetItem(typeof(T), value), this.Context);
            }
            else
            {
                oldValue = default;
                return this;
            }
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

        public MetadataContainer Update(Type t, object value)
        {
            if (t is null)
                throw new ArgumentNullException(nameof(t));
            if (value is null)
                throw new ArgumentNullException(nameof(value));
            CheckTypeCast(t, value);
            if (this.values.ContainsKey(t))
                return new MetadataContainer(this.values.SetItem(t, value), this.Context);
            else
                return this;
        }
        public MetadataContainer Remove(Type t)
        {
            if (t is null)
                throw new ArgumentNullException(nameof(t));
            if (this.values.ContainsKey(t))
                return new MetadataContainer(this.values.Remove(t), this.Context);
            else
                return this;
        }
        public MetadataContainer Remove<T>()
            where T : class => this.Remove(typeof(T));


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
