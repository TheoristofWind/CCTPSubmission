using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using UnityEditor;

public class FileHandler
{
    public static void WriteToFile(string text, string fileName, string fileLocation)
    {
        try
        {
            File.WriteAllText(fileLocation + fileName, text);
        }
        catch (DirectoryNotFoundException)
        {
            if (!Directory.Exists(fileLocation))
            {
                Directory.CreateDirectory(fileLocation);
                WriteToFile(text, fileName, fileLocation);
            }
        }
        catch (UnauthorizedAccessException)
        {
            Debug.LogError("Couldnt write to file with location: " + fileLocation + fileName);
        }
    }

    public static string[] ReadFromFileLineSeperation(string fileName, string fileLocation)
    {
        return File.ReadAllLines(fileLocation + fileName);
    }

    public static string ReadFromFile(string fileName, string fileLocation)
    {
        return File.ReadAllText(fileLocation + fileName);
    }

    public static string[] GetAllFileNamesInDirectory(string dir, string type = "*.json", bool inRessourceFolder = true)
    {
        if (inRessourceFolder) dir = "Assets\\Resources\\" + dir;

        string[] files = Directory.GetFiles(dir, type);

        char[] delimiter = { '\\', '.' };
        for (int i = 0; i < files.Length; i++)
        {
            string[] sec = files[i].Split(delimiter);
            files[i] = sec[sec.Length - 2];
        }

        return files;
    }
}
