using System;
using System.Collections.Generic;
using TinyJSON;
using UnityEngine;

[Serializable]
public class WorkshopModPack
{
	[Skip]
	public string filename;

	[Skip]
	public string changenote = "Version 1.0";

	public string publishedfileid = string.Empty;

	public string contentfolder = string.Empty;

	public string previewfile = string.Empty;

	public int visibility = 2;

	public string title = "My New Mod Pack";

	public string description = "Description goes here";

	public string metadata = string.Empty;

	public List<string> tags = new List<string>();

	public void ValidateTags()
	{
		Config config = Config.Load();
		if (!config.validateTags)
		{
			return;
		}
		for (int i = 0; i < tags.Count; i++)
		{
			if (!config.validTags.Contains(tags[i]))
			{
				Debug.LogError("Removing invalid tag: " + tags[i]);
				tags.RemoveAt(i);
				i--;
			}
		}
	}

	public static WorkshopModPack Load(string filename)
	{
		WorkshopModPack item = null;
		string json = Utils.LoadTextFile(filename);
		JSON.MakeInto<WorkshopModPack>(JSON.Load(json), out item);
		item.filename = filename;
		return item;
	}

	public void Save(string filename)
	{
		string jsonString = JSON.Dump(this, true);
		Utils.SaveJsonToFile(filename, jsonString);
		Debug.Log("Saved modpack to file: " + filename);
	}
}
