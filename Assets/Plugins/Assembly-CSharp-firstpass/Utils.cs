using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using LitJson;
using UnityEngine;

public class Utils
{
	public class ComponentSerializer
	{
		public static string Save(MonoBehaviour obj)
		{
			return JsonMapper.ToJson(obj);
		}

		public static T Load<T>(string json)
		{
			return JsonMapper.ToObject<T>(json);
		}
	}

	private static int[] decimalDens = new int[13]
	{
		1000, 900, 500, 400, 100, 90, 50, 40, 10, 9,
		5, 4, 1
	};

	private static string[] romanDens = new string[13]
	{
		"M", "CM", "D", "CD", "C", "XC", "L", "XL", "X", "IX",
		"V", "IV", "I"
	};

	public static object GetPropertyValue(object src, string propName)
	{
		PropertyInfo property = src.GetType().GetProperty(propName);
		if (property == null)
		{
			FieldInfo field = src.GetType().GetField(propName);
			return field.GetValue(src);
		}
		return property.GetValue(src, null);
	}

	public static void SetPropertyValue(object src, string propName, object val)
	{
		PropertyInfo property = src.GetType().GetProperty(propName);
		if (property == null)
		{
			FieldInfo field = src.GetType().GetField(propName);
			field.SetValue(src, val);
		}
		property.SetValue(src, val, null);
	}

	public static void CallMethod(object src, string methodName, object arg)
	{
		CallMethod(src, methodName, new object[1] { arg });
	}

	public static void CallMethod(object src, string methodName, object[] args)
	{
		MethodInfo method = src.GetType().GetMethod(methodName);
		if (method != null)
		{
			method.Invoke(src, args);
		}
		else
		{
			Debug.LogError(string.Format("CallMethod(): Method not found: {0}.{1}", src.GetType().ToString(), methodName));
		}
	}

	public static float GetLocalAngleTowards(Transform sourceTransform, Vector3 targetPosition)
	{
		Vector3 direction = targetPosition - sourceTransform.position;
		Vector3 to = sourceTransform.InverseTransformDirection(direction);
		to.z = 0f;
		float num = Vector3.Angle(Vector3.up, to);
		if (to.x > 0f)
		{
			num *= -1f;
		}
		return num;
	}

	public static Vector3 GetLocalAnglesTowards(Transform sourceTransform, Vector3 targetPosition)
	{
		Vector3 direction = targetPosition - sourceTransform.position;
		Vector3 to = sourceTransform.InverseTransformDirection(direction);
		Vector3 to2 = new Vector3(to.x, to.y, 0f);
		float num = Vector3.Angle(Vector3.up, to2);
		if (to2.x > 0f)
		{
			num *= -1f;
		}
		float x = Vector3.Angle(Vector3.forward, to);
		return new Vector3(x, 0f, num);
	}

	public static bool PointInViewport(Camera cam, Vector3 worldPoint)
	{
		Vector3 vector = cam.WorldToViewportPoint(worldPoint);
		if (vector.z <= 0f || vector.x < 0f || vector.x > 1f || vector.y < 0f || vector.y > 1f)
		{
			return false;
		}
		return true;
	}

	public static bool pointInsideAABB(Vector2 min, Vector2 max, Vector2 point)
	{
		return point.x >= min.x && point.x <= max.x && point.y >= min.y && point.y <= max.y;
	}

	public static bool AABBcollide(Vector2 min, Vector2 max, Vector2 min2, Vector2 max2)
	{
		return min.x <= max2.x && max.x >= min2.x && max.y >= min2.y && min.y <= max2.y;
	}

	public static void ScaleToBounds(Renderer renderer, Vector3 maxBounds)
	{
		renderer.transform.localScale = Vector3.one;
		Vector3 size = renderer.bounds.size;
		Vector3 vector = maxBounds - size;
		float num = 0f;
		if (vector.x < 0f)
		{
			num = maxBounds.x / size.x;
		}
		else if (vector.y < 0f)
		{
			num = maxBounds.y / size.y;
		}
		else if (vector.z < 0f)
		{
			num = maxBounds.z / size.z;
		}
		if (num != 0f)
		{
			renderer.transform.localScale = Vector3.one * num;
		}
	}

	public static float ScaleAnimationSpeed(AnimationState clip, float targetSpeed)
	{
		float a = targetSpeed / clip.length;
		float b = clip.length / targetSpeed;
		if (targetSpeed > clip.length)
		{
			return Mathf.Min(a, b);
		}
		return Mathf.Max(a, b);
	}

	public static float StepRound(float value, float step)
	{
		float num = step / 2f;
		float num2 = value % step;
		value = ((!(num2 > num)) ? (value - num2) : (value + (step - num2)));
		return value;
	}

	public static Color FromHTMLColor(string HTMLcolor)
	{
		if (HTMLcolor[0] == '#')
		{
			HTMLcolor = HTMLcolor.Replace("#", string.Empty);
		}
		int num = Convert.ToInt32(HTMLcolor.Substring(0, 2), 16);
		int num2 = Convert.ToInt32(HTMLcolor.Substring(2, 2), 16);
		int num3 = Convert.ToInt32(HTMLcolor.Substring(4, 2), 16);
		return new Color((float)num / 255f, (float)num2 / 255f, (float)num3 / 255f);
	}

	public static string ToHTMLColor(Color color)
	{
		string format = "#{0}{1}{2}";
		int num = (int)(color.r * 255f);
		int num2 = (int)(color.g * 255f);
		int num3 = (int)(color.b * 255f);
		return string.Format(format, num.ToString("X"), num2.ToString("X"), num3.ToString("X"));
	}

	public static string GetFileNameFromFleetName(string name, string fileType)
	{
		string text = string.Empty;
		string[] array = name.Split(' ');
		foreach (string text2 in array)
		{
			string text3 = text2.ToLower();
			string[] array2 = new string[4] { "'", "\"", ",", "." };
			string[] array3 = array2;
			foreach (string oldValue in array3)
			{
				text3 = text3.Replace(oldValue, string.Empty);
			}
			text = text + text3 + "_";
		}
		text = text.Remove(text.Length - 1, 1);
		return text + "." + fileType + ".json";
	}

	public static string Serialize(object pocoObject)
	{
		return JsonMapper.ToJson(pocoObject);
	}

	public static string LoadTextFile(string path)
	{
		if (File.Exists(path))
		{
			StreamReader streamReader = new StreamReader(path, Encoding.UTF8);
			string result = streamReader.ReadToEnd();
			streamReader.Close();
			return result;
		}
		throw new IOException("File not found: " + path);
	}

	public static string LoadJsonFromFile(string filename)
	{
		string text = Application.dataPath + "/Resources/Data/" + filename + ".txt";
		TextAsset textAsset = Resources.Load("Data/" + filename) as TextAsset;
		if (textAsset == null)
		{
			throw new IOException("File not found: " + filename);
		}
		return textAsset.text;
	}

	public static bool SaveJsonToFile(string filename, string jsonString)
	{
		FileStream fileStream = File.Open(filename, FileMode.Create, FileAccess.Write, FileShare.None);
		byte[] bytes = new UTF8Encoding(true).GetBytes(jsonString);
		fileStream.Write(bytes, 0, bytes.Length);
		fileStream.Close();
		Debug.Log("Wrote to " + filename);
		return true;
	}

	public static string toOrdinal(int number)
	{
		return string.Concat(str1: (number % 10 == 1) ? "st" : ((number % 10 == 2) ? "nd" : ((number % 10 != 3) ? "th" : "rd")), str0: number.ToString());
	}

	public static float[,] Gridify(Vector3[] points)
	{
		float num = 9999.9f;
		float num2 = -9999.9f;
		float num3 = 9999.9f;
		float num4 = -9999.9f;
		for (int i = 0; i < points.Length; i++)
		{
			Vector3 vector = points[i];
			if (vector.x < num)
			{
				num = vector.x;
			}
			if (vector.x > num2)
			{
				num2 = vector.x;
			}
			if (vector.z < num3)
			{
				num3 = vector.z;
			}
			if (vector.z > num4)
			{
				num4 = vector.z;
			}
		}
		Vector3 vector2 = new Vector3(Mathf.Floor(num), 0f, Mathf.Floor(num3));
		int num5 = (int)(num2 - num);
		int num6 = (int)(num4 - num3);
		Debug.Log(string.Concat("origin is ", vector2, "   size is (", num5, ",", num6, ")"));
		float[,] array = new float[num5 + 1, num6 + 1];
		for (int j = 0; j < points.Length; j++)
		{
			Vector3 vector3 = points[j];
			Vector3 vector4 = vector3;
			int num7 = (int)Mathf.Floor(vector4.x);
			int num8 = (int)Mathf.Floor(vector4.z);
			array[num7, num8] = vector3.y;
		}
		return array;
	}

	public static Texture2D DrawGridOnTexture(float[,] grid)
	{
		int length = grid.GetLength(0);
		int length2 = grid.GetLength(1);
		Texture2D texture2D = new Texture2D(length, length2, TextureFormat.RGB24, false);
		texture2D.filterMode = FilterMode.Point;
		for (int i = 0; i < grid.GetLength(1); i++)
		{
			for (int j = 0; j < grid.GetLength(0); j++)
			{
				float num = grid[j, i];
				texture2D.SetPixel(j, i, new Color(num, num, num, num));
			}
		}
		texture2D.Apply();
		return texture2D;
	}

	public static float[,] Normalize(float[,] grid)
	{
		int length = grid.GetLength(0);
		int length2 = grid.GetLength(1);
		float[,] array = new float[length, length2];
		float num = float.PositiveInfinity;
		float num2 = -1f;
		for (int i = 0; i < length2; i++)
		{
			for (int j = 0; j < length; j++)
			{
				float num3 = grid[j, i];
				if (num3 > num2)
				{
					num2 = num3;
				}
				else if (num3 < num)
				{
					num = num3;
				}
			}
		}
		for (int i = 0; i < length2; i++)
		{
			for (int j = 0; j < length; j++)
			{
				array[j, i] = (grid[j, i] - num) / (num2 - num);
				if (array[j, i] > 1f || array[j, i] < 0f)
				{
					Debug.Log("ERROR: normalized to " + array[j, i]);
				}
			}
		}
		return array;
	}

	public static void TranslateUVs(GameObject obj, Vector2 offset)
	{
		MeshFilter meshFilter = obj.GetComponent(typeof(MeshFilter)) as MeshFilter;
		Mesh mesh = meshFilter.mesh;
		Vector2[] array = new Vector2[mesh.uv.Length];
		for (int i = 0; i < mesh.uv.Length; i++)
		{
			array[i] = new Vector2(mesh.uv[i].x + offset.x, mesh.uv[i].y + offset.y);
		}
		mesh.uv = array;
	}

	public static void SetUVs(GameObject obj, Vector2 offset)
	{
		MeshFilter meshFilter = obj.GetComponent(typeof(MeshFilter)) as MeshFilter;
		Mesh mesh = meshFilter.mesh;
		Vector2[] array = new Vector2[mesh.uv.Length];
		for (int i = 0; i < mesh.uv.Length; i++)
		{
			array[i] = new Vector2(offset.x + 0.01f, offset.y - 0.01f);
		}
		mesh.uv = array;
	}

	public static void TranslateWeaponUVs(GameObject obj, Vector2 offset)
	{
		MeshFilter meshFilter = obj.GetComponent(typeof(MeshFilter)) as MeshFilter;
		Mesh mesh = meshFilter.mesh;
		Vector2[] array = new Vector2[mesh.uv.Length];
		for (int i = 0; i < mesh.uv.Length; i++)
		{
			if (mesh.uv[i].x > 0.25f)
			{
				array[i] = new Vector2(mesh.uv[i].x + offset.x, mesh.uv[i].y + offset.y);
			}
			else
			{
				array[i] = mesh.uv[i];
			}
		}
		mesh.uv = array;
	}

	private static List<string> WrapText(string text, double pixels, string fontFamily, float emSize)
	{
		string[] array = text.Split(new string[1] { " " }, StringSplitOptions.None);
		List<string> list = new List<string>();
		StringBuilder stringBuilder = new StringBuilder();
		double num = 0.0;
		string[] array2 = array;
		foreach (string text2 in array2)
		{
		}
		if (stringBuilder.Length > 0)
		{
			list.Add(stringBuilder.ToString());
		}
		return list;
	}

	public static string WordWrap(string text, int width)
	{
		StringBuilder stringBuilder = new StringBuilder();
		if (width < 1)
		{
			return text;
		}
		int i = 0;
		while (i < text.Length)
		{
			int num = text.IndexOf(Environment.NewLine, i);
			int num2 = ((num != -1) ? (num + Environment.NewLine.Length) : (num = text.Length));
			if (num > i)
			{
				do
				{
					int num3 = num - i;
					if (num3 > width)
					{
						num3 = BreakLine(text, i, width);
					}
					stringBuilder.Append(text, i, num3);
					stringBuilder.Append(Environment.NewLine);
					for (i += num3; i < num && char.IsWhiteSpace(text[i]); i++)
					{
					}
				}
				while (num > i);
			}
			else
			{
				stringBuilder.Append(Environment.NewLine);
			}
			i = num2;
		}
		return stringBuilder.ToString();
	}

	private static int BreakLine(string text, int pos, int max)
	{
		int num = max;
		while (num >= 0 && !char.IsWhiteSpace(text[pos + num]))
		{
			num--;
		}
		if (num < 0)
		{
			return max;
		}
		while (num >= 0 && char.IsWhiteSpace(text[pos + num]))
		{
			num--;
		}
		return num + 1;
	}

	public static string GetMd5Hash(string input)
	{
		MD5 mD = MD5.Create();
		byte[] array = mD.ComputeHash(Encoding.UTF8.GetBytes(input));
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < array.Length; i++)
		{
			stringBuilder.Append(array[i].ToString("x2"));
		}
		return stringBuilder.ToString();
	}

	public static Transform FindInHierarchy(Transform current, string name)
	{
		if (current.name == name)
		{
			return current;
		}
		for (int i = 0; i < current.childCount; i++)
		{
			Transform transform = FindInHierarchy(current.GetChild(i), name);
			if (transform != null)
			{
				return transform;
			}
		}
		return null;
	}

	public static string GetFullPath(Transform obj)
	{
		string text = "/" + obj.name;
		while (obj.parent != null)
		{
			obj = obj.parent;
			text = "/" + obj.name + text;
		}
		return text;
	}

	public static Texture2D LoadTextureFromFile(string savedImageName)
	{
		Texture2D texture2D = new Texture2D(2, 2);
		try
		{
			byte[] data = File.ReadAllBytes(savedImageName);
			texture2D.LoadImage(data);
			return texture2D;
		}
		catch
		{
			Debug.LogWarning("Tried to load preview file which doesn't exist (yet?): " + savedImageName);
			return null;
		}
	}

	public static void SetLayer(Transform root, int layer)
	{
		Stack<Transform> stack = new Stack<Transform>();
		stack.Push(root);
		while (stack.Count != 0)
		{
			Transform transform = stack.Pop();
			transform.gameObject.layer = layer;
			IEnumerator enumerator = transform.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					Transform t = (Transform)enumerator.Current;
					stack.Push(t);
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
		}
	}

	public static void PrepareVisualPrefab(GameObject obj)
	{
		obj.transform.localScale = Vector3.one;
		SetLayer(obj.transform, LayerMask.NameToLayer("GUI"));
		TrailRenderer[] componentsInChildren = obj.GetComponentsInChildren<TrailRenderer>();
		foreach (TrailRenderer trailRenderer in componentsInChildren)
		{
			trailRenderer.gameObject.SetActive(false);
		}
		AudioSource[] componentsInChildren2 = obj.GetComponentsInChildren<AudioSource>();
		foreach (AudioSource audioSource in componentsInChildren2)
		{
			audioSource.enabled = false;
		}
	}

	public static void AddBackfacesToMesh(Mesh mesh)
	{
		int num = 0;
		Vector3[] vertices = mesh.vertices;
		Vector2[] uv = mesh.uv;
		Vector3[] normals = mesh.normals;
		int num2 = vertices.Length;
		Vector3[] array = new Vector3[num2 * 2];
		Vector2[] array2 = new Vector2[num2 * 2];
		Vector3[] array3 = new Vector3[num2 * 2];
		for (num = 0; num < num2; num++)
		{
			array[num] = (array[num + num2] = vertices[num]);
			array2[num] = (array2[num + num2] = uv[num]);
			array3[num] = normals[num];
			array3[num + num2] = -normals[num];
		}
		int[] triangles = mesh.triangles;
		int num3 = triangles.Length;
		int[] array4 = new int[num3 * 2];
		for (int i = 0; i < num3; i += 3)
		{
			array4[i] = triangles[i];
			array4[i + 1] = triangles[i + 1];
			array4[i + 2] = triangles[i + 2];
			num = i + num3;
			array4[num] = triangles[i] + num2;
			array4[num + 2] = triangles[i + 1] + num2;
			array4[num + 1] = triangles[i + 2] + num2;
		}
		mesh.vertices = array;
		mesh.uv = array2;
		mesh.normals = array3;
		mesh.triangles = array4;
	}
}
