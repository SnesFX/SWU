using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace TinyJSON
{
	public static class JSON
	{
		private static BindingFlags instanceBindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		private static BindingFlags staticBindingFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		private static MethodInfo decodeTypeMethod = typeof(JSON).GetMethod("DecodeType", staticBindingFlags);

		private static MethodInfo decodeListMethod = typeof(JSON).GetMethod("DecodeList", staticBindingFlags);

		private static MethodInfo decodeDictionaryMethod = typeof(JSON).GetMethod("DecodeDictionary", staticBindingFlags);

		private static MethodInfo decodeArrayMethod = typeof(JSON).GetMethod("DecodeArray", staticBindingFlags);

		public static Variant Load(string json)
		{
			if (json == null)
			{
				throw new ArgumentNullException("json");
			}
			return Decoder.Decode(json);
		}

		public static string Dump(object data, bool prettyPrint = false)
		{
			return Encoder.Encode(data, prettyPrint);
		}

		public static void MakeInto<T>(Variant data, out T item)
		{
			item = DecodeType<T>(data);
		}

		private static T DecodeType<T>(Variant data)
		{
			Type typeFromHandle = typeof(T);
			if (typeFromHandle.IsEnum)
			{
				return (T)Enum.Parse(typeFromHandle, data.ToString());
			}
			if (typeFromHandle.IsPrimitive || typeFromHandle == typeof(string))
			{
				return (T)Convert.ChangeType(data, typeFromHandle);
			}
			if (typeFromHandle.IsArray)
			{
				MethodInfo methodInfo = decodeArrayMethod.MakeGenericMethod(typeFromHandle.GetElementType());
				return (T)methodInfo.Invoke(null, new object[1] { data });
			}
			if (typeof(IList).IsAssignableFrom(typeFromHandle))
			{
				MethodInfo methodInfo2 = decodeListMethod.MakeGenericMethod(typeFromHandle.GetGenericArguments());
				return (T)methodInfo2.Invoke(null, new object[1] { data });
			}
			if (typeof(IDictionary).IsAssignableFrom(typeFromHandle))
			{
				MethodInfo methodInfo3 = decodeDictionaryMethod.MakeGenericMethod(typeFromHandle.GetGenericArguments());
				return (T)methodInfo3.Invoke(null, new object[1] { data });
			}
			T val = Activator.CreateInstance<T>();
			foreach (KeyValuePair<string, Variant> item in (IEnumerable<KeyValuePair<string, Variant>>)(data as ProxyObject))
			{
				FieldInfo field = typeFromHandle.GetField(item.Key, instanceBindingFlags);
				if (field != null && !Attribute.GetCustomAttributes(field).Any((Attribute attr) => attr is Skip))
				{
					MethodInfo methodInfo4 = decodeTypeMethod.MakeGenericMethod(field.FieldType);
					if (typeFromHandle.IsValueType)
					{
						object obj = val;
						field.SetValue(obj, methodInfo4.Invoke(null, new object[1] { item.Value }));
						val = (T)obj;
					}
					else
					{
						field.SetValue(val, methodInfo4.Invoke(null, new object[1] { item.Value }));
					}
				}
			}
			MethodInfo[] methods = typeFromHandle.GetMethods(instanceBindingFlags);
			foreach (MethodInfo methodInfo5 in methods)
			{
				if (methodInfo5.GetCustomAttributes(false).Any((object attr) => attr is Load))
				{
					if (methodInfo5.GetParameters().Length == 0)
					{
						methodInfo5.Invoke(val, null);
						continue;
					}
					methodInfo5.Invoke(val, new object[1] { data });
				}
			}
			return val;
		}

		private static List<T> DecodeList<T>(Variant data)
		{
			List<T> list = new List<T>();
			foreach (Variant item in (IEnumerable<Variant>)(data as ProxyArray))
			{
				list.Add(DecodeType<T>(item));
			}
			return list;
		}

		private static Dictionary<K, V> DecodeDictionary<K, V>(Variant data)
		{
			Dictionary<K, V> dictionary = new Dictionary<K, V>();
			foreach (KeyValuePair<string, Variant> item in (IEnumerable<KeyValuePair<string, Variant>>)(data as ProxyObject))
			{
				K key = (K)Convert.ChangeType(item.Key, typeof(K));
				V value = DecodeType<V>(item.Value);
				dictionary.Add(key, value);
			}
			return dictionary;
		}

		private static T[] DecodeArray<T>(Variant data)
		{
			ProxyArray proxyArray = data as ProxyArray;
			int count = proxyArray.Count;
			T[] array = new T[count];
			int num = 0;
			foreach (Variant item in (IEnumerable<Variant>)(data as ProxyArray))
			{
				array[num++] = DecodeType<T>(item);
			}
			return array;
		}

		private static void SupportTypeForAOT<T>()
		{
			DecodeType<T>(null);
			DecodeList<T>(null);
			DecodeArray<T>(null);
			DecodeDictionary<short, T>(null);
			DecodeDictionary<ushort, T>(null);
			DecodeDictionary<int, T>(null);
			DecodeDictionary<uint, T>(null);
			DecodeDictionary<long, T>(null);
			DecodeDictionary<ulong, T>(null);
			DecodeDictionary<float, T>(null);
			DecodeDictionary<double, T>(null);
			DecodeDictionary<bool, T>(null);
			DecodeDictionary<string, T>(null);
		}

		private static void SupportValueTypesForAOT()
		{
			SupportTypeForAOT<short>();
			SupportTypeForAOT<ushort>();
			SupportTypeForAOT<int>();
			SupportTypeForAOT<uint>();
			SupportTypeForAOT<long>();
			SupportTypeForAOT<ulong>();
			SupportTypeForAOT<float>();
			SupportTypeForAOT<double>();
			SupportTypeForAOT<bool>();
			SupportTypeForAOT<string>();
		}
	}
}
