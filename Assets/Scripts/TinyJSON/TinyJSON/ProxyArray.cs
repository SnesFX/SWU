using System.Collections;
using System.Collections.Generic;

namespace TinyJSON
{
	public sealed class ProxyArray : Variant, IEnumerable<Variant>, IEnumerable
	{
		private List<Variant> list;

		public override Variant this[int index]
		{
			get
			{
				return list[index];
			}
			set
			{
				list[index] = value;
			}
		}

		public int Count
		{
			get
			{
				return list.Count;
			}
		}

		public ProxyArray()
		{
			list = new List<Variant>();
		}

		IEnumerator<Variant> IEnumerable<Variant>.GetEnumerator()
		{
			return list.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return list.GetEnumerator();
		}

		public void Add(Variant item)
		{
			list.Add(item);
		}
	}
}
