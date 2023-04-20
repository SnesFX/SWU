using System;
using System.Globalization;

namespace TinyJSON
{
	public abstract class Variant : IConvertible
	{
		protected static IFormatProvider formatProvider = new NumberFormatInfo();

		public virtual Variant this[string key]
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public virtual Variant this[int index]
		{
			get
			{
				throw new NotSupportedException();
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		public void Make<T>(out T item)
		{
			JSON.MakeInto<T>(this, out item);
		}

		public T Make<T>()
		{
			T item;
			JSON.MakeInto<T>(this, out item);
			return item;
		}

		public virtual TypeCode GetTypeCode()
		{
			return TypeCode.Object;
		}

		public virtual object ToType(Type conversionType, IFormatProvider provider)
		{
			throw new InvalidCastException(string.Concat("Cannot convert ", GetType(), " to ", conversionType.Name));
		}

		public virtual DateTime ToDateTime(IFormatProvider provider)
		{
			throw new InvalidCastException(string.Concat("Cannot convert ", GetType(), " to DateTime"));
		}

		public virtual bool ToBoolean(IFormatProvider provider)
		{
			throw new InvalidCastException(string.Concat("Cannot convert ", GetType(), " to Boolean"));
		}

		public virtual byte ToByte(IFormatProvider provider)
		{
			throw new InvalidCastException(string.Concat("Cannot convert ", GetType(), " to Byte"));
		}

		public virtual char ToChar(IFormatProvider provider)
		{
			throw new InvalidCastException(string.Concat("Cannot convert ", GetType(), " to Char"));
		}

		public virtual decimal ToDecimal(IFormatProvider provider)
		{
			throw new InvalidCastException(string.Concat("Cannot convert ", GetType(), " to Decimal"));
		}

		public virtual double ToDouble(IFormatProvider provider)
		{
			throw new InvalidCastException(string.Concat("Cannot convert ", GetType(), " to Double"));
		}

		public virtual short ToInt16(IFormatProvider provider)
		{
			throw new InvalidCastException(string.Concat("Cannot convert ", GetType(), " to Int16"));
		}

		public virtual int ToInt32(IFormatProvider provider)
		{
			throw new InvalidCastException(string.Concat("Cannot convert ", GetType(), " to Int32"));
		}

		public virtual long ToInt64(IFormatProvider provider)
		{
			throw new InvalidCastException(string.Concat("Cannot convert ", GetType(), " to Int64"));
		}

		public virtual sbyte ToSByte(IFormatProvider provider)
		{
			throw new InvalidCastException(string.Concat("Cannot convert ", GetType(), " to SByte"));
		}

		public virtual float ToSingle(IFormatProvider provider)
		{
			throw new InvalidCastException(string.Concat("Cannot convert ", GetType(), " to Single"));
		}

		public virtual string ToString(IFormatProvider provider)
		{
			throw new InvalidCastException(string.Concat("Cannot convert ", GetType(), " to String"));
		}

		public virtual ushort ToUInt16(IFormatProvider provider)
		{
			throw new InvalidCastException(string.Concat("Cannot convert ", GetType(), " to UInt16"));
		}

		public virtual uint ToUInt32(IFormatProvider provider)
		{
			throw new InvalidCastException(string.Concat("Cannot convert ", GetType(), " to UInt32"));
		}

		public virtual ulong ToUInt64(IFormatProvider provider)
		{
			throw new InvalidCastException(string.Concat("Cannot convert ", GetType(), " to UInt64"));
		}

		public override string ToString()
		{
			return ToString(formatProvider);
		}

		public static implicit operator bool(Variant variant)
		{
			return variant.ToBoolean(formatProvider);
		}

		public static implicit operator float(Variant variant)
		{
			return variant.ToSingle(formatProvider);
		}

		public static implicit operator double(Variant variant)
		{
			return variant.ToDouble(formatProvider);
		}

		public static implicit operator ushort(Variant variant)
		{
			return variant.ToUInt16(formatProvider);
		}

		public static implicit operator short(Variant variant)
		{
			return variant.ToInt16(formatProvider);
		}

		public static implicit operator uint(Variant variant)
		{
			return variant.ToUInt32(formatProvider);
		}

		public static implicit operator int(Variant variant)
		{
			return variant.ToInt32(formatProvider);
		}

		public static implicit operator ulong(Variant variant)
		{
			return variant.ToUInt64(formatProvider);
		}

		public static implicit operator long(Variant variant)
		{
			return variant.ToInt64(formatProvider);
		}

		public static implicit operator decimal(Variant variant)
		{
			return variant.ToDecimal(formatProvider);
		}

		public static implicit operator string(Variant variant)
		{
			return variant.ToString(formatProvider);
		}
	}
}
