using System;
using System.Collections;
using System.Reflection;
using System.Text;

namespace TinyJSON
{
	public sealed class Encoder
	{
		private const string INDENT = "\t";

		private StringBuilder builder;

		private bool prettyPrint;

		private int indent;

		private Encoder(bool prettyPrint = false)
		{
			this.prettyPrint = prettyPrint;
			builder = new StringBuilder();
			indent = 0;
		}

		public static string Encode(object obj, bool prettyPrint = false)
		{
			Encoder encoder = new Encoder(prettyPrint);
			encoder.EncodeValue(obj);
			return encoder.builder.ToString();
		}

		private void AppendIndent()
		{
			for (int i = 0; i < indent; i++)
			{
				builder.Append("\t");
			}
		}

		private void EncodeValue(object value)
		{
			string value2;
			IList value3;
			IDictionary value4;
			if (value == null)
			{
				builder.Append("null");
			}
			else if ((value2 = value as string) != null)
			{
				EncodeString(value2);
			}
			else if (value is bool)
			{
				builder.Append(value.ToString().ToLower());
			}
			else if (value is Enum)
			{
				EncodeString(value.ToString());
			}
			else if ((value3 = value as IList) != null)
			{
				EncodeList(value3);
			}
			else if ((value4 = value as IDictionary) != null)
			{
				EncodeDictionary(value4);
			}
			else if (value is char)
			{
				EncodeString(value.ToString());
			}
			else
			{
				EncodeOther(value);
			}
		}

		private void EncodeObject(object value)
		{
			builder.Append('{');
			if (prettyPrint)
			{
				builder.Append('\n');
				indent++;
			}
			bool flag = true;
			FieldInfo[] fields = value.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			FieldInfo[] array = fields;
			foreach (FieldInfo fieldInfo in array)
			{
				if (Attribute.GetCustomAttributes(fieldInfo).Any((Attribute attr) => attr is Skip))
				{
					continue;
				}
				if (!flag)
				{
					builder.Append(',');
					if (prettyPrint)
					{
						builder.Append('\n');
					}
				}
				if (prettyPrint)
				{
					AppendIndent();
				}
				EncodeString(fieldInfo.Name);
				builder.Append(':');
				if (prettyPrint)
				{
					builder.Append(' ');
				}
				EncodeValue(fieldInfo.GetValue(value));
				flag = false;
			}
			if (prettyPrint)
			{
				builder.Append('\n');
				indent--;
				AppendIndent();
			}
			builder.Append('}');
		}

		private void EncodeDictionary(IDictionary value)
		{
			bool flag = true;
			builder.Append('{');
			if (prettyPrint)
			{
				builder.Append('\n');
				indent++;
			}
			IEnumerator enumerator = value.Keys.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object current = enumerator.Current;
					if (!flag)
					{
						builder.Append(',');
						if (prettyPrint)
						{
							builder.Append('\n');
						}
					}
					if (prettyPrint)
					{
						AppendIndent();
					}
					EncodeString(current.ToString());
					builder.Append(':');
					if (prettyPrint)
					{
						builder.Append(' ');
					}
					EncodeValue(value[current]);
					flag = false;
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}
			if (prettyPrint)
			{
				builder.Append('\n');
				indent--;
				AppendIndent();
			}
			builder.Append('}');
		}

		private void EncodeList(IList value)
		{
			bool flag = true;
			builder.Append('[');
			if (prettyPrint)
			{
				builder.Append('\n');
				indent++;
			}
			IEnumerator enumerator = value.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object current = enumerator.Current;
					if (!flag)
					{
						builder.Append(',');
						if (prettyPrint)
						{
							builder.Append('\n');
						}
					}
					if (prettyPrint)
					{
						AppendIndent();
					}
					EncodeValue(current);
					flag = false;
				}
			}
			finally
			{
				IDisposable disposable;
				if ((disposable = enumerator as IDisposable) != null)
				{
					disposable.Dispose();
				}
			}
			if (prettyPrint)
			{
				builder.Append('\n');
				indent--;
				AppendIndent();
			}
			builder.Append(']');
		}

		private void EncodeString(string value)
		{
			builder.Append('"');
			char[] array = value.ToCharArray();
			char[] array2 = array;
			foreach (char c in array2)
			{
				switch (c)
				{
				case '"':
					builder.Append("\\\"");
					continue;
				case '\\':
					builder.Append("\\\\");
					continue;
				case '\b':
					builder.Append("\\b");
					continue;
				case '\f':
					builder.Append("\\f");
					continue;
				case '\n':
					builder.Append("\\n");
					continue;
				case '\r':
					builder.Append("\\r");
					continue;
				case '\t':
					builder.Append("\\t");
					continue;
				}
				int num = Convert.ToInt32(c);
				if (num >= 32 && num <= 126)
				{
					builder.Append(c);
				}
				else
				{
					builder.Append("\\u" + Convert.ToString(num, 16).PadLeft(4, '0'));
				}
			}
			builder.Append('"');
		}

		private void EncodeOther(object value)
		{
			if (value is float || value is double || value is int || value is uint || value is long || value is sbyte || value is byte || value is short || value is ushort || value is ulong || value is decimal)
			{
				builder.Append(value.ToString());
			}
			else
			{
				EncodeObject(value);
			}
		}
	}
}
