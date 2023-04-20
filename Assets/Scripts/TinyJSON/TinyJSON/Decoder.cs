using System;
using System.IO;
using System.Text;

namespace TinyJSON
{
	public sealed class Decoder : IDisposable
	{
		private enum TOKEN
		{
			NONE = 0,
			CURLY_OPEN = 1,
			CURLY_CLOSE = 2,
			SQUARED_OPEN = 3,
			SQUARED_CLOSE = 4,
			COLON = 5,
			COMMA = 6,
			STRING = 7,
			NUMBER = 8,
			TRUE = 9,
			FALSE = 10,
			NULL = 11
		}

		private const string WHITE_SPACE = " \t\n\r";

		private const string WORD_BREAK = " \t\n\r{}[],:\"";

		private StringReader json;

		private char PeekChar
		{
			get
			{
				return Convert.ToChar(json.Peek());
			}
		}

		private char NextChar
		{
			get
			{
				return Convert.ToChar(json.Read());
			}
		}

		private string NextWord
		{
			get
			{
				StringBuilder stringBuilder = new StringBuilder();
				while (" \t\n\r{}[],:\"".IndexOf(PeekChar) == -1)
				{
					stringBuilder.Append(NextChar);
					if (json.Peek() == -1)
					{
						break;
					}
				}
				return stringBuilder.ToString();
			}
		}

		private TOKEN NextToken
		{
			get
			{
				ConsumeWhiteSpace();
				if (json.Peek() == -1)
				{
					return TOKEN.NONE;
				}
				switch (PeekChar)
				{
				case '{':
					return TOKEN.CURLY_OPEN;
				case '}':
					json.Read();
					return TOKEN.CURLY_CLOSE;
				case '[':
					return TOKEN.SQUARED_OPEN;
				case ']':
					json.Read();
					return TOKEN.SQUARED_CLOSE;
				case ',':
					json.Read();
					return TOKEN.COMMA;
				case '"':
					return TOKEN.STRING;
				case ':':
					return TOKEN.COLON;
				case '-':
				case '0':
				case '1':
				case '2':
				case '3':
				case '4':
				case '5':
				case '6':
				case '7':
				case '8':
				case '9':
					return TOKEN.NUMBER;
				default:
					switch (NextWord)
					{
					case "false":
						return TOKEN.FALSE;
					case "true":
						return TOKEN.TRUE;
					case "null":
						return TOKEN.NULL;
					default:
						return TOKEN.NONE;
					}
				}
			}
		}

		private Decoder(string jsonString)
		{
			json = new StringReader(jsonString);
		}

		public static Variant Decode(string jsonString)
		{
			using (Decoder decoder = new Decoder(jsonString))
			{
				return decoder.DecodeValue();
			}
		}

		public void Dispose()
		{
			json.Dispose();
			json = null;
		}

		private ProxyObject DecodeObject()
		{
			ProxyObject proxyObject = new ProxyObject();
			json.Read();
			while (true)
			{
				switch (NextToken)
				{
				case TOKEN.COMMA:
					continue;
				case TOKEN.NONE:
					return null;
				case TOKEN.CURLY_CLOSE:
					return proxyObject;
				}
				string text = DecodeString();
				if (text == null)
				{
					return null;
				}
				if (NextToken != TOKEN.COLON)
				{
					return null;
				}
				json.Read();
				proxyObject.Add(text, DecodeValue());
			}
		}

		private ProxyArray DecodeArray()
		{
			ProxyArray proxyArray = new ProxyArray();
			json.Read();
			bool flag = true;
			while (flag)
			{
				TOKEN nextToken = NextToken;
				switch (nextToken)
				{
				case TOKEN.NONE:
					return null;
				case TOKEN.SQUARED_CLOSE:
					flag = false;
					break;
				default:
					proxyArray.Add(DecodeByToken(nextToken));
					break;
				case TOKEN.COMMA:
					break;
				}
			}
			return proxyArray;
		}

		private Variant DecodeValue()
		{
			TOKEN nextToken = NextToken;
			return DecodeByToken(nextToken);
		}

		private Variant DecodeByToken(TOKEN token)
		{
			switch (token)
			{
			case TOKEN.STRING:
				return DecodeString();
			case TOKEN.NUMBER:
				return DecodeNumber();
			case TOKEN.CURLY_OPEN:
				return DecodeObject();
			case TOKEN.SQUARED_OPEN:
				return DecodeArray();
			case TOKEN.TRUE:
				return new ProxyBoolean(true);
			case TOKEN.FALSE:
				return new ProxyBoolean(false);
			case TOKEN.NULL:
				return null;
			default:
				return null;
			}
		}

		private Variant DecodeString()
		{
			StringBuilder stringBuilder = new StringBuilder();
			json.Read();
			bool flag = true;
			while (flag)
			{
				if (json.Peek() == -1)
				{
					flag = false;
					break;
				}
				char nextChar = NextChar;
				switch (nextChar)
				{
				case '"':
					flag = false;
					break;
				case '\\':
					if (json.Peek() == -1)
					{
						flag = false;
						break;
					}
					nextChar = NextChar;
					switch (nextChar)
					{
					case '"':
					case '/':
					case '\\':
						stringBuilder.Append(nextChar);
						break;
					case 'b':
						stringBuilder.Append('\b');
						break;
					case 'f':
						stringBuilder.Append('\f');
						break;
					case 'n':
						stringBuilder.Append('\n');
						break;
					case 'r':
						stringBuilder.Append('\r');
						break;
					case 't':
						stringBuilder.Append('\t');
						break;
					case 'u':
					{
						StringBuilder stringBuilder2 = new StringBuilder();
						for (int i = 0; i < 4; i++)
						{
							stringBuilder2.Append(NextChar);
						}
						stringBuilder.Append((char)Convert.ToInt32(stringBuilder2.ToString(), 16));
						break;
					}
					}
					break;
				default:
					stringBuilder.Append(nextChar);
					break;
				}
			}
			return new ProxyString(stringBuilder.ToString());
		}

		private Variant DecodeNumber()
		{
			return new ProxyNumber(NextWord);
		}

		private void ConsumeWhiteSpace()
		{
			while (" \t\n\r".IndexOf(PeekChar) != -1)
			{
				json.Read();
				if (json.Peek() == -1)
				{
					break;
				}
			}
		}
	}
}
