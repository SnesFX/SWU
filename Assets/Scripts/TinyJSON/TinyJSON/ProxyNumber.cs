using System;

namespace TinyJSON
{
	public sealed class ProxyNumber : Variant
	{
		private IConvertible value;

		public ProxyNumber(IConvertible value)
		{
			if (value is string)
			{
				this.value = Parse(value as string);
			}
			else
			{
				this.value = value;
			}
		}

		private IConvertible Parse(string value)
		{
			if (value.IndexOf('.') == -1)
			{
				if (value.IndexOf('-') == -1)
				{
					ulong result;
					ulong.TryParse(value, out result);
					return result;
				}
				long result2;
				long.TryParse(value, out result2);
				return result2;
			}
			double result3;
			double.TryParse(value, out result3);
			return result3;
		}

		public override bool ToBoolean(IFormatProvider provider)
		{
			return value.ToBoolean(provider);
		}

		public override byte ToByte(IFormatProvider provider)
		{
			return value.ToByte(provider);
		}

		public override char ToChar(IFormatProvider provider)
		{
			return value.ToChar(provider);
		}

		public override decimal ToDecimal(IFormatProvider provider)
		{
			return value.ToDecimal(provider);
		}

		public override double ToDouble(IFormatProvider provider)
		{
			return value.ToDouble(provider);
		}

		public override short ToInt16(IFormatProvider provider)
		{
			return value.ToInt16(provider);
		}

		public override int ToInt32(IFormatProvider provider)
		{
			return value.ToInt32(provider);
		}

		public override long ToInt64(IFormatProvider provider)
		{
			return value.ToInt64(provider);
		}

		public override sbyte ToSByte(IFormatProvider provider)
		{
			return value.ToSByte(provider);
		}

		public override float ToSingle(IFormatProvider provider)
		{
			return value.ToSingle(provider);
		}

		public override string ToString(IFormatProvider provider)
		{
			return value.ToString(provider);
		}

		public override ushort ToUInt16(IFormatProvider provider)
		{
			return value.ToUInt16(provider);
		}

		public override uint ToUInt32(IFormatProvider provider)
		{
			return value.ToUInt32(provider);
		}

		public override ulong ToUInt64(IFormatProvider provider)
		{
			return value.ToUInt64(provider);
		}
	}
}
