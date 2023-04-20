using System;
using System.Collections.Generic;
using UnityEngine;

public class EnumListAttribute : PropertyAttribute
{
	private Type E;

	private List<string> valueNames = new List<string>();

	public EnumListAttribute(Type T)
	{
		E = T;
		valueNames = new List<string>();
		Array values = Enum.GetValues(E);
		for (int i = 0; i < values.Length; i++)
		{
			valueNames.Add(values.GetValue(i).ToString());
		}
	}

	public string GetName(int index)
	{
		if (index >= 0 && index < valueNames.Count)
		{
			return valueNames[index];
		}
		return "    " + index;
	}
}
