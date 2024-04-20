using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.IO;
using System.Text;

public class CSVReader
{
	private static readonly string SPLIT_RE = @",(?=(?:[^""]*""[^""]*"")*(?![^""]*""))";
	private	static readonly string LINE_SPLIT_RE = @"\r\n|\n\r|\n|\r";
	private static readonly char[] TRIM_CHARS = { '\"' };

	private static Dictionary<T, Dictionary<string, object>> Read<T>(string file)
	{
		var list = new Dictionary<T, Dictionary<string, object>>();

		//StreamReader sr = new StreamReader(Application.streamingAssetsPath + "/" + file);
		//var data = sr.ReadToEnd();

		//TextAsset data = Resources.Load(file) as TextAsset;

		var lines = Regex.Split(file, LINE_SPLIT_RE);

		if (lines.Length <= 1) return list;

		var header = Regex.Split(lines[0], SPLIT_RE); // lines[0] == 1За (key, kr, en, jp ...)
		for (int i = 0; i < header.Length; i++)
			header[i] = header[i].TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");

		Action<Dictionary<T, Dictionary<string, object>>, string, Dictionary<string, object>> action = AA_Null;
		switch (header[0])
		{
			case "Idx":
				action = AA;
				break;
			//case "ResourceType":
			//	action = AA2;
			//	break;
			//case "BuildingType":
			//	action = AA3;
			//	break;
			case "String":
				action = AA4;
				break;
            case "KeyMap":
                action = AA5;
                break;
            case "KeyCode":
				action = AA6;
				break;
		}

		for (var i = 1; i < lines.Length; i++)
		{
			var values = Regex.Split(lines[i], SPLIT_RE);
			if (values.Length == 0 || values[0] == "") continue;
			values[0] = values[0].TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "");

			var entry = new Dictionary<string, object>();
			for (var j = 1; j < header.Length && j < values.Length; j++)
			{
				string value = values[j];
				value = value.TrimStart(TRIM_CHARS).TrimEnd(TRIM_CHARS).Replace("\\", "").Replace("@","\n");
				object finalvalue = value;
                if (int.TryParse(value, out int n))
                {
                    finalvalue = n;
                }
                else if (float.TryParse(value, out float f))
                {
                    finalvalue = f;
                }			
                entry[header[j]] = finalvalue;
			}
			action(list, values[0], entry);
		}
		return list;
	}

	private static void AA_Null<T>(Dictionary<T, Dictionary<string, object>> list, string key, Dictionary<string, object> entry)
	{

	}

	private static void AA<T>(Dictionary<T, Dictionary<string, object>> list, string key, Dictionary<string, object> entry)
	{
		int.TryParse(key, out int n);
		(list as Dictionary<int, Dictionary<string, object>>).Add(n, entry);
	}
	//private static void AA2<T>(Dictionary<T, Dictionary<string, object>> list, string key, Dictionary<string, object> entry)
	//{
	//	if (Enum.TryParse(typeof(ResourceType), key, out object n))
	//		(list as Dictionary<ResourceType, Dictionary<string, object>>).Add((ResourceType)n, entry);
	//}
	//private static void AA3<T>(Dictionary<T, Dictionary<string, object>> list, string key, Dictionary<string, object> entry)
	//{
	//	if (Enum.TryParse(typeof(BuildingType), key, out object n))
	//		(list as Dictionary<BuildingType, Dictionary<string, object>>).Add((BuildingType)n, entry);
	//}
	private static void AA4<T>(Dictionary<T, Dictionary<string, object>> list, string key, Dictionary<string, object> entry)
	{
		(list as Dictionary<string, Dictionary<string, object>>).Add(key, entry);
	}
    private static void AA5<T>(Dictionary<T, Dictionary<string, object>> list, string key, Dictionary<string, object> entry)
    {
        if (Enum.TryParse(typeof(KeyMap), key, out object n))
            (list as Dictionary<KeyMap, Dictionary<string, object>>).Add((KeyMap)n, entry);
    }
    private static void AA6<T>(Dictionary<T, Dictionary<string, object>> list, string key, Dictionary<string, object> entry)
	{
		if (Enum.TryParse(typeof(KeyCode), key, out object n))
			(list as Dictionary<KeyCode, Dictionary<string, object>>).Add((KeyCode)n, entry);
	}

	public static Dictionary<T, Dictionary<string, object>> ReadCSV<T>(string filename)
	{
		string filePath = $"{Application.streamingAssetsPath}/{filename}";
		StreamReader reader = new StreamReader(filePath, Encoding.BigEndianUnicode);
		string value = reader.ReadToEnd();
		reader.Close();

		return Read<T>(value);
	}

	public static void WriteCSV(string rowData, string filePath)
	{
		if (File.Exists(filePath))
        {
			File.Delete(filePath);
        }

		StringBuilder stringBuilder = new StringBuilder(rowData);
		Stream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
		StreamWriter outStream = new StreamWriter(fileStream, Encoding.BigEndianUnicode);

		stringBuilder.Replace("\n ", "@");

		outStream.WriteLine(stringBuilder);
		outStream.Close();
	}

}