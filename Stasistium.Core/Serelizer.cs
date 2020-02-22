using Newtonsoft.Json.Linq;
using Stasistium.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Stasistium.Serelizer
{
    internal class JsonSerelizer
    {
        public static async Task Write(object baseCache, System.IO.Stream stream, bool indented = false)
        {
            var array = Write(baseCache);

            using var textWriter = new System.IO.StreamWriter(stream);
            using var jsonWriter = new Newtonsoft.Json.JsonTextWriter(textWriter) { Formatting = indented ? Newtonsoft.Json.Formatting.Indented : Newtonsoft.Json.Formatting.None };
            await array.WriteToAsync(jsonWriter).ConfigureAwait(false);
        }

        private static JArray Write(object baseCache)
        {
            var result = new JArray();
            var queu = new Queue<object>();
            int index = 0;
            var idLookup = new Dictionary<object, int>();
            Enqueue(baseCache);
            int Enqueue(object current)
            {
                if (idLookup.ContainsKey(current))
                    return idLookup[current];

                queu.Enqueue(current);
                idLookup.Add(current, index);
                var currentIndex = index;
                index++;
                return currentIndex;
            }

            var refKind = JValue.CreateString("ref");
            var scalarKind = JValue.CreateString("scalar");

            while (queu.TryDequeue(out var current))
            {

                var type = current.GetType();
                var typeName = type.AssemblyQualifiedName;

                var implementedInterfaces = new HashSet<Type>(type.GetInterfaces().Where(x => x.IsGenericType).Select(x => x.GetGenericTypeDefinition()).Concat(type.GetInterfaces().Where(x => !x.IsGenericType)));

                var currentJObject = new JObject();
                currentJObject.Add("type", typeName);
                result.Add(currentJObject);

                if (type.IsArray)
                {
                    var arrayElements = new JArray();
                    currentJObject.Add("elements", arrayElements);

                    var array = (System.Collections.IList)current;
                    for (int i = 0; i < array.Count; i++)
                        arrayElements.Add(GetValueObject(array[i]));

                }
                else if (current is System.Runtime.CompilerServices.ITuple tuple)
                {
                    var tupleArray = new JArray();
                    currentJObject.Add("tuple", tupleArray);
                    for (int i = 0; i < tuple.Length; i++)
                        tupleArray.Add(GetValueObject(tuple[i]));
                }
                else if (implementedInterfaces.Contains(typeof(IDictionary<,>)))
                {
                    var arrayElements = new JArray();
                    currentJObject.Add("map", arrayElements);

                    var enumerable = (System.Collections.IEnumerable)current;
                    foreach (var item in enumerable)
                    {
                        if (item is null)
                            continue;

                        var keyValuePairType = item.GetType();

                        var keyProperty = keyValuePairType.GetProperty(nameof(KeyValuePair<object, object>.Key));
                        var ValueProperty = keyValuePairType.GetProperty(nameof(KeyValuePair<object, object>.Value));

                        var key = keyProperty.GetValue(item);
                        var value = ValueProperty.GetValue(item);

                        var entry = new JObject();
                        arrayElements.Add(entry);

                        entry.Add("key", GetValueObject(key));
                        entry.Add("value", GetValueObject(value));
                    }
                }
                else if (implementedInterfaces.Contains(typeof(ICollection<>)))
                {
                    throw new NotImplementedException();
                }
                else
                {
                    var constructor = type.GetConstructor(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, Type.EmptyTypes, null);
                    if (constructor is null && !type.IsValueType)
                        throw new ArgumentException($"Graphe contains a type that has no default constructor. ({typeName})");

                    var properties = type.GetProperties();


                    var propertyArray = new JObject();
                    currentJObject.Add("propertys", propertyArray);

                    foreach (var item in properties)
                    {
                        var name = item.Name;
                        var value = item.GetValue(current);

                        propertyArray.Add(name, GetValueObject(value));
                    }
                }



                JObject GetValueObject(object? value)
                {
                    var valueObject = new JObject();
                    switch (value)
                    {
                        case null:
                            valueObject.Add("Kind", refKind);
                            valueObject.Add("value", JValue.FromObject(-1));
                            break;

                        case string s:
                            valueObject.Add("Kind", scalarKind);
                            valueObject.Add("value", JValue.CreateString(s));
                            break;

                        case DateTime dateTime:
                            valueObject.Add("Kind", scalarKind);
                            valueObject.Add("value", JValue.CreateString($"{dateTime.Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture)}|{dateTime.Kind.ToString()}"));
                            break;

                        case DateTimeOffset dateTime:
                            valueObject.Add("Kind", scalarKind);
                            valueObject.Add("value", JValue.CreateString($"{dateTime.Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture)}|{dateTime.Offset.Ticks.ToString(System.Globalization.CultureInfo.InvariantCulture)}"));
                            break;

                        case TimeSpan dateTime:
                            valueObject.Add("Kind", scalarKind);
                            valueObject.Add("value", JValue.FromObject(dateTime.Ticks));
                            break;

                        case int _:
                        case byte _:
                        case short _:
                        case long _:
                        case uint _:
                        case ushort _:
                        case ulong _:
                        case float _:
                        case double _:
                        case decimal _:
                            valueObject.Add("Kind", scalarKind);
                            valueObject.Add("value", JValue.FromObject(value));
                            break;

                        default:
                            valueObject.Add("Kind", refKind);
                            valueObject.Add("value", JValue.FromObject(Enqueue(value)));
                            break;
                    }
                    return valueObject;
                }
            }

            return result;
        }

        internal static async Task<T> Load<T>(System.IO.Stream stream)
        {
            using var textReader = new System.IO.StreamReader(stream);
            using var jsonReadr = new Newtonsoft.Json.JsonTextReader(textReader);

            var array = await JArray.LoadAsync(jsonReadr).ConfigureAwait(false);
            return (T)Load(array);
        }


        internal static object Load(JArray json)
        {
            if (json.Count == 0)
                throw new ArgumentException("There must be at least on value", nameof(json));

            var deserelizedObjects = new object[json.Count];

            // create Objects
            for (int i = 0; i < json.Count; i++)
            {
                var entry = json[i];

                var typeName = entry["type"]?.ToObject<string>();
                if (typeName is null)
                    throw new ArgumentException($"Object at index {i} does not have a type!");

                var type = Type.GetType(typeName);
                if (type is null)
                    throw new ArgumentException($"Can't find Type {typeName}");

                if (entry["propertys"] is JObject || entry["map"] is JArray || entry["tuple"] is JArray)
                {
                    var constructor = type.GetConstructor(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance, null, Type.EmptyTypes, null);
                    object currentObject;
                    if (constructor != null)
                        currentObject = constructor.Invoke(null);
                    else if (type.IsValueType)
                        currentObject = type.GetDefault()!; // value Type is never null
                    else
                        throw new NotSupportedException($"Type {type} must contain parameterless constructor.");

                    deserelizedObjects[i] = currentObject;


                }
                else if (entry["elements"] is JArray jsonElements)
                {
                    var elementType = type.GetElementType();
                    if (elementType is null)
                        throw new InvalidOperationException($"Type {type} should be an Array and have an ElementType");

                    deserelizedObjects[i] = Array.CreateInstance(elementType, jsonElements.Count);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }

            // Fill Objects
            for (int i = json.Count - 1; i >= 0; i--)
            //for (int i = 0; i < json.Count; i++)
            {
                var entry = json[i];

                var typeName = entry["type"]?.ToObject<string>();
                if (typeName is null)
                    throw new ArgumentException($"Object at index {i} does not have a type!");


                var type = Type.GetType(typeName)!; // Type wasn't null the first time, so this time it will also be not null.
                var currentObject = deserelizedObjects[i];

                object? GetValue(ValueWrapper valueWrapper, Type targetType)
                {
                    object value;
                    if (valueWrapper.Kind == ValueKind.@ref)
                    {
                        if ((long)valueWrapper.Value == -1)
                            return null;
                        value = deserelizedObjects[(int)(long)valueWrapper.Value];
                    }
                    else if (valueWrapper.Kind == ValueKind.scalar)
                    {
                        if (targetType == typeof(DateTime))
                        {
                            var txt = (string)valueWrapper.Value;
                            var splited = txt.Split('|');
                            var ticks = long.Parse(splited[0], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                            var kind = (DateTimeKind)Enum.Parse(typeof(DateTimeKind), splited[1]);
                            
                            value = new DateTime(ticks, kind);
                        }
                        else if (targetType == typeof(DateTimeOffset))
                        {
                            var txt = (string)valueWrapper.Value;
                            var splited = txt.Split('|');
                            var ticks = long.Parse(splited[0], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);
                            var offset = long.Parse(splited[1], System.Globalization.NumberStyles.Integer, System.Globalization.CultureInfo.InvariantCulture);

                            value = new DateTimeOffset(ticks, new TimeSpan(offset));
                        }
                        else if (targetType == typeof(TimeSpan))
                        {
                            value = TimeSpan.FromTicks((long)valueWrapper.Value);
                        }
                        else
                            value = valueWrapper.Value;
                    }
                    else
                        throw new NotSupportedException();

                    if (!targetType.IsAssignableFrom(value.GetType()))
                    {
                        var converter = System.ComponentModel.TypeDescriptor.GetConverter(targetType);
                        value = converter.ConvertFrom(value);
                    }

                    return value;
                }

                if (entry["propertys"] is JObject jsonPropertys)
                {

                    foreach (var pair in jsonPropertys)
                    {
                        var property = type.GetProperty(pair.Key);
                        if (property is null)
                            throw new ArgumentException($"Type {type} does not contains property {pair.Key}");

                        var valueWrapper = pair.Value?.ToObject<ValueWrapper>();
                        if (valueWrapper is null)
                            throw new ArgumentException($"Type {type} does not contains correct wrapper for property {pair.Key}");

                        var setMethod = property.GetSetMethod();
                        var getMethod = property.GetGetMethod();

                        if (setMethod is null)
                            throw new ArgumentException($"Type {type} does not have setter for property {pair.Key}");
                        if (getMethod is null)
                            throw new ArgumentException($"Type {type} does not have getter for property {pair.Key}");


                        var value = GetValue(valueWrapper, property.PropertyType);



                        property.SetValue(currentObject, value);
                    }
                }
                else if (entry["elements"] is JArray jsonElements)
                {
                    var array = (Array)currentObject;

                    for (int j = 0; j < array.Length; j++)
                    {
                        var valueWrapper = jsonElements[j].ToObject<ValueWrapper>();
                        if (valueWrapper is null)
                            throw new ArgumentException($"Type {type} does not contains correct wrapper for element {j}");
                        var value = GetValue(valueWrapper, type.GetElementType()!); // Elementtype wasn't null the first time.

                        array.SetValue(value, j);
                    }
                }
                else if (entry["map"] is JArray jsonMap)
                {

                    var interfaceType = type.GetInterfaces().Where(x => x.IsGenericType).FirstOrDefault(x => x.GetGenericTypeDefinition() == typeof(IDictionary<,>));
                    if (interfaceType is null)
                        throw new ArgumentException($"Type {type} does not implement IDctionary<,>");

                    var mapping = type.GetInterfaceMap(interfaceType);

                    var addMethod = mapping.TargetMethods.Where(x => x.GetParameters().Length == 2).First(x => x.Name == nameof(IDictionary<object, object>.Add));

                    var genericArguments = interfaceType.GetGenericArguments();
                    var keyType = genericArguments[0];
                    var valueType = genericArguments[1];

                    for (int j = 0; j < jsonMap.Count; j++)
                    {
                        var mapEntry = (JObject)jsonMap[j];


                        var keyWrapper = mapEntry["key"]?.ToObject<ValueWrapper>();
                        var valueWrapper = mapEntry["value"]?.ToObject<ValueWrapper>();

                        if (keyWrapper is null)
                            throw new ArgumentException($"Map entry did not contain key value.");
                        if (valueWrapper is null)
                            throw new ArgumentException($"Map entry did not contain 'value' value.");

                        var key = GetValue(keyWrapper, keyType);
                        var value = GetValue(valueWrapper, valueType);

                        addMethod.Invoke(currentObject, new object?[] { key, value });

                    }
                }
                else if (entry["tuple"] is JArray jsonTuple)
                {
                    for (int j = 0; j < jsonTuple.Count; j++)
                    {

                        var valueWrapper = jsonTuple[j].ToObject<ValueWrapper>();
                        if (valueWrapper is null)
                            throw new ArgumentException($"Type {type} does not contains correct wrapper for element {j}");


                        if (type.IsValueType)
                        {
                            var filed = type.GetField("Item" + (j + 1));
                            if (filed is null)
                                throw new ArgumentException("The tuple size is incorrect");
                            var value = GetValue(valueWrapper, filed.FieldType);
                            filed.SetValue(currentObject, value);
                        }
                        else
                        {
                            var filed = type.GetProperty("Item" + (j + 1));
                            if (filed is null)
                                throw new ArgumentException("The tuple size is incorrect");
                            var value = GetValue(valueWrapper, filed.PropertyType);
                            filed.SetValue(currentObject, value);
                        }
                    }
                }
                else
                {
                    throw new NotSupportedException();
                }

            }

            return deserelizedObjects[0];
        }

        private enum ValueKind
        {
            @ref,
            scalar
        }

#pragma warning disable CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.
#pragma warning disable CA1812 // Avoid uninstantiated internal classes
        private class ValueWrapper
        {
            public ValueKind Kind { get; set; }

            public object Value { get; set; }
        }
#pragma warning restore CA1812 // Avoid uninstantiated internal classes
#pragma warning restore CS8618 // Non-nullable field is uninitialized. Consider declaring as nullable.

    }
}
