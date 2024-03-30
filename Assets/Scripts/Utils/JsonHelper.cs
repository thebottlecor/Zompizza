using System;
using System.IO;
using System.Text;
using UnityEngine;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class JsonHelper
{
    private static readonly string serialize = "7d19b3d1cd745946915a3cd5a9b0441bf6a676230d5029dbb275d750e1db13d2ba278bb3e7eed2ea25e16bdb7981f113d586b4a84e344e0f33f1cce275bb21f54bba5c9700d3ceed1712e3142";

    public static void GetJsonFileNames(string folderName, List<string> fileNamesList)
    {
        DirectoryInfo di = new DirectoryInfo(folderName);
        foreach (FileInfo f in di.GetFiles())
        {
            if (f.Extension.ToLower().CompareTo(".json") == 0)
            {
                string strInFileName = f.Name;
                //di.FullName + "/" + f.Name;
                fileNamesList.Add(strInFileName);
            }
        }
    }

    public static void CreateJsonFile(string createPath, string fileName, string folderName, string jsonData, bool crypt)
    {
        DirectoryInfo di = new DirectoryInfo(createPath + "/" + folderName);
        if (di.Exists == false)
            di.Create();

        FileStream fileStream = new FileStream(string.Format("{0}/{1}/{2}.json", createPath, folderName, fileName), FileMode.Create);
        if (crypt)
            jsonData = Encrypt(jsonData, serialize);
        byte[] data = Encoding.UTF8.GetBytes(jsonData);
        fileStream.Write(data, 0, data.Length);
        fileStream.Close();
    }

    public static T LoadJsonFile<T>(string path, bool crypt) where T : class
    {
        try
        {
            FileStream fileStream = new FileStream(path, FileMode.Open);
            byte[] data = new byte[fileStream.Length];
            fileStream.Read(data, 0, data.Length);
            fileStream.Close();
            string jsonData = Encoding.UTF8.GetString(data);
            if (crypt)
                jsonData = Decrypt(jsonData, serialize);
            return JsonUtility.FromJson<T>(jsonData);
        }
        catch
        {
            return null;
        }
    }

    public static void DeleteJsonFile(string path)
    {
        try
        {
            if (File.Exists(path))
                File.Delete(path);
        }
        catch
        {

        }
    }

    public static string ObjectToJson(object obj)
    {
        return JsonUtility.ToJson(obj);
    }

    private static string Decrypt(string textToDecrypt, string key)
    {
        RijndaelManaged rijndaelCipher = new RijndaelManaged
        {
            Mode = CipherMode.CBC,
            Padding = PaddingMode.PKCS7,
            KeySize = 128,
            BlockSize = 128
        };

        byte[] encryptedData = Convert.FromBase64String(textToDecrypt);
        byte[] pwdBytes = Encoding.UTF8.GetBytes(key);
        byte[] keyBytes = new byte[16];

        int len = pwdBytes.Length;
        if (len > keyBytes.Length)
            len = keyBytes.Length;

        Array.Copy(pwdBytes, keyBytes, len);
        rijndaelCipher.Key = keyBytes;
        rijndaelCipher.IV = keyBytes;
        byte[] plainText = rijndaelCipher.CreateDecryptor().TransformFinalBlock(encryptedData, 0, encryptedData.Length);

        return Encoding.UTF8.GetString(plainText);
    }

    private static string Encrypt(string textToEncrypt, string key)
    {
        RijndaelManaged rijndaelCipher = new RijndaelManaged
        {
            Mode = CipherMode.CBC,
            Padding = PaddingMode.PKCS7,
            KeySize = 128,
            BlockSize = 128
        };

        byte[] pwdBytes = Encoding.UTF8.GetBytes(key);
        byte[] keyBytes = new byte[16];

        int len = pwdBytes.Length;
        if (len > keyBytes.Length)
            len = keyBytes.Length;

        Array.Copy(pwdBytes, keyBytes, len);
        rijndaelCipher.Key = keyBytes;
        rijndaelCipher.IV = keyBytes;
        ICryptoTransform transform = rijndaelCipher.CreateEncryptor();
        byte[] plainText = Encoding.UTF8.GetBytes(textToEncrypt);

        return Convert.ToBase64String(transform.TransformFinalBlock(plainText, 0, plainText.Length));
    }

    public static string SaveFileNameEncrypt(string textToEncrypt)
    {
        string result = Encrypt(textToEncrypt, serialize);
        result = Regex.Replace(result, @"[^a-zA-Z0-9]", "0");
        return result;
    }
}
