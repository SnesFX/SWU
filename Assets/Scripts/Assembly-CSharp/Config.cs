using System;
using System.Collections.Generic;
using TinyJSON;
using UnityEngine;

[Serializable]
public class Config
{
	public bool validateTags;

	public List<string> validTags = new List<string>();

	[Skip]
	public const string filename = "config.json";

	public static Config Load()
	{
		Config item = null;
		string json = Utils.LoadTextFile(Application.dataPath + "/../config.json");
		JSON.MakeInto<Config>(JSON.Load(json), out item);
		return item;
	}
}
