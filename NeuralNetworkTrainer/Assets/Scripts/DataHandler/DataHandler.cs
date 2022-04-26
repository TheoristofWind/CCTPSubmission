using System.Collections;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System.Threading;

public class DataHandler
{
    Type dataCreationType;
    string[] fileNames;
    string directoryPath;
    bool canUseDirectory = false;

    public List<List<float>> inputs = new List<List<float>>();
    public List<List<float>> outputs = new List<List<float>>();

    public DataHandler(DataCreationType dataCreationMethod)
    {
        dataCreationType = Type.GetType(dataCreationMethod.ToString()); 
    }

    public DataHandler(DataCreationType dataCreationMethod, string directory)
    {
        dataCreationType = Type.GetType(dataCreationMethod.ToString());

        fileNames = FileHandler.GetAllFileNamesInDirectory(directory);
        directoryPath = directory;
        canUseDirectory = true;
    }

    private void CreateDataThreaded(TextAsset[] files)
    {
        if (files.Length > 63)
        {
            Debug.LogError("CANNOT MADE DATA FROM MORE THAN 60 FILES!");
            return;
        }

        if (files.Length <= 0)
        {
            Debug.LogError("CANNOT MADE DATA FROM 0 FILES!");
            return;
        }

        var doneEvents = new ManualResetEvent[files.Length];
        List<DataCreationJob> data = new List<DataCreationJob>();
        for (int i = 0; i < files.Length; i++)
        {
            doneEvents[i] = new ManualResetEvent(false);
            DataCreationJob dataCreationJob = DataCreationJob.GetDataCreationJobFromType(dataCreationType, doneEvents[i]);
            data.Add(dataCreationJob);
            ThreadPool.QueueUserWorkItem(dataCreationJob.Execute, files[i].text);
        }

        WaitHandle.WaitAll(doneEvents);

        foreach (var d in data)
        {
            inputs.AddRange(d.inputs);
            outputs.AddRange(d.outputs);
        }
    }

    // Returns files from the directory defined in directoryPath. if Length is set to -1, will return all files from the start. 
    public TextAsset[] GetFilesFromDirectory(int start = 0, int length = 1)
    {
        if (length == -1) length = fileNames.Length;

        List<TextAsset> textAssets = new List<TextAsset>();
        for (int i = start; i < start+length && i < fileNames.Length; i++)
        {
            string path = directoryPath + "/" + fileNames[i];
            var load = Resources.Load<TextAsset>(path);
            if (load != null)
            {
                textAssets.Add(load);
            }
            else
            {
                Debug.LogWarning("COULDNT LOAD " + fileNames[i] + ". Does file exist?");
            }
        }

        return textAssets.ToArray();
    }

    // Will create data from the directory set up in the constructor. If length is set to -1, will create data from all files in directory.
    public void CreateDataFromDirectory(int start = 0, int length = 1, bool threaded = true)
    {
        if (!canUseDirectory)
        {
            Debug.LogError("CANNOT USE DIRECTORY, DIRECTORY NOT SET UP IN CONSTRUCTOR");
        }

        if (threaded)
        {
            CreateDataThreaded(GetFilesFromDirectory(start, length));
        }
        else
        {
            CreateDataNotThreaded(GetFilesFromDirectory(start, length));
        }

        ShuffleLists();
    }

    public void ShuffleLists()
    {
        for (int n = outputs.Count -1; n > 0; n--)
        {
            int k = UnityEngine.Random.Range(0, n);
            List<float> outVal = new List<float>();
            outVal.AddRange(outputs[k]);
            List<float> inVal = new List<float>();
            inVal.AddRange(inputs[k]);
            outputs[k].Clear();
            outputs[k].AddRange(outputs[n]);
            inputs[k].Clear();
            inputs[k].AddRange(inputs[n]);
            outputs[n] = outVal;
            inputs[n] = inVal;
        }
    }

    private void CreateDataNotThreaded(TextAsset[] files)
    {
        foreach (var f in files)
        {
            ReadFromJsonFile(f.text, false);
        }
    }

    public void ReadFromJsonFile(string json, bool clear = true)
    {
        if (clear)
        {
            inputs.Clear();
            outputs.Clear();
        }

        (List<List<float>>, List<List<float>>) data = DataCreationJob.GetDataCreationJobFromType(dataCreationType, new ManualResetEvent(false)).GetInputsAndOutputs(json);

        inputs.AddRange(data.Item1);
        outputs.AddRange(data.Item2);
    }

    public int GetDataLength()
    {
        return fileNames.Length;
    }
}

