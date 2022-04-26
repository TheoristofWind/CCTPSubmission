using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;
using System.Threading;

public class AIVisual : MonoBehaviour
{
    NeuralNetwork neuralNet;
    DataHandler trainingData;
    DataHandler testData;

    [Header("Data Conversion Method")]
    [Tooltip("This should be the name of the class to use in creating the data")]
    public DataCreationType dataCreationType;

    public bool isTraining { get; private set; }
    public bool shouldTest { get; private set; }
    public int progress { get; private set; }
    public int iterations { get; private set; }
    public int nbOfTimeTrained { get; private set; }
    public float previousErrorValue { get; private set; }
    public int train { get; private set; }

    [HideInInspector] public int trainFor = -1;
    [HideInInspector] public int trainAt = 10000;
    [HideInInspector] public int nbPerThreadWanted = 1000;

    public int totalTrain { get; private set; }

    [HideInInspector] public int trainDataFilesNumber = 10;
    [HideInInspector] public int testDataFilesNumber = -1;

    [HideInInspector]
    public string writeNNLocation = "./NeuralNetwork/";

    private string trainDir = "";
    private string testDir = "";

    private int aiNumber = 0;

    //storing error values
    private string errorValues = "";
    private int errorValLength = 0;
    private int fileNumber = 0;

    List<List<(List<float>, List<List<float>>)>> wantedChanges;

    void Start()
    {
        progress = 0;
        iterations = 0;
        nbOfTimeTrained = 0;
        previousErrorValue = 0;
        train = 0;
        totalTrain = 0;
        isTraining = false;
        shouldTest = false;
    }

    public void CreateNeuralNetwork(int numberInputs, int numberOutPuts, int hiddenLayers, int nodePerLayer, float mutationRate)
    {
        neuralNet = new NeuralNetwork(numberInputs, numberOutPuts, hiddenLayers, nodePerLayer, mutationRate);
    }

    public void WriteToFile(string name)
    {
        neuralNet.WriteNeuralNetworkToFile(name, "./NeuralNetwork/");
    }

    public void ReadFromFile(string name)
    {
        neuralNet = new NeuralNetwork(name, "./");
    }

    public void ChangeMutationRate(float rate)
    {
        neuralNet.SetNewMutationRate(rate);
    }

    public void Update()
    {
        Train("network_it_" + aiNumber.ToString() + ".txt", writeNNLocation);
        Test();
    }

    #region Test Neural Network

    public void TestNetwork()
    {
        float errorVal = 0;
        for (int t = 0; t < testData.inputs.Count; t++)
        {
            List<float> outputs = neuralNet.GetOuputFromInput(testData.inputs[t]);
            for (int i = 0; i < outputs.Count; i++)
            {
                errorVal += Math.Abs(outputs[i] - testData.outputs[t][i]);
            }
        }

        previousErrorValue = errorVal / (testData.inputs.Count * neuralNet.GetNbOutputs());
        errorValues += previousErrorValue.ToString() + "\n";
        errorValLength++;

        FileHandler.WriteToFile(errorValues, "errorValues" + fileNumber.ToString() + ".txt", writeNNLocation + "/ErrorValues/");
        if (errorValLength > 1000)
        {
            errorValues = "";
            errorValLength = 0;
            fileNumber++;
        }
    }

    public void Test()
    {
        if (!shouldTest || !isTraining) return;

        TestNetwork();
    }

    #endregion

    #region Training Neural Network

    public void StopTraining()
    {
        isTraining = false;
        shouldTest = false;
    }

    public void Train(string netName, string netLocation)
    {
        if (!isTraining) return;

        if (aiNumber >= trainFor && trainFor > 0)
        {
            isTraining = false;
            return;
        }

        aiNumber++;

        neuralNet.WriteNeuralNetworkToFile(netName, netLocation);

        TrainAIThreads(netName, netLocation, nbPerThreadWanted, progress, trainAt);

        progress += trainAt;
        iterations = progress;

        if (progress + trainAt > trainingData.inputs.Count)
        {
            if (totalTrain < trainingData.GetDataLength())
            {
                trainingData.CreateDataFromDirectory(totalTrain, 10);

                totalTrain += 10;

                progress = 0;
            }
            else
            {
                isTraining = false;
                Debug.Log("RAN OUT OF TRAINING DATA");
            }
        }
    }

    public void TrainAIThreads(string netName, string netLocation, int nbPerThread, int start, int totalNbToTest)
    {
        int end = start + totalNbToTest;
        var doneEvents = new List<ManualResetEvent>();
        List<ThreadedAI> wantedOutputs = new List<ThreadedAI>();

        if (totalNbToTest > nbPerThread*64)
        {
            nbPerThread = (int)Math.Ceiling((float)totalNbToTest / 64.0f) + 1;
        }


        int ind = 0;
        for (int i = start; i < end; i += nbPerThread)
        {
            if (i + nbPerThread > end)
            {
                nbPerThread = end - i;
            }
            doneEvents.Add(new ManualResetEvent(false));
            ThreadedAI aiWO = new ThreadedAI(netName, netLocation, neuralNet.GetMutationRate(), trainingData.inputs.GetRange(i, nbPerThread), trainingData.outputs.GetRange(i, nbPerThread), doneEvents[ind]);
            wantedOutputs.Add(aiWO);
            ThreadPool.QueueUserWorkItem(aiWO.CalculateWantedChanges, i);

            ind++;
        }

        WaitHandle.WaitAll(doneEvents.ToArray());

        foreach (var a in wantedOutputs)
        {
            wantedChanges.AddRange(a.wantedChanges);
        }

        neuralNet.Train(neuralNet.AverageWantedChanges(wantedChanges));
        nbOfTimeTrained++;

        wantedChanges.Clear();
    }

    public void OLDTrainAIThreads(string netName, string netLocation, int nbThreads, int start, int totalNbToTest)
    {
        int numberPerThread = (int)Math.Floor((float)totalNbToTest / (float)nbThreads) + 1;
        int end = start + totalNbToTest;
        var doneEvents = new ManualResetEvent[nbThreads];

        List<ThreadedAI> wantedOutputs = new List<ThreadedAI>();

        for (int i = 0; i < nbThreads; i++)
        {
            doneEvents[i] = new ManualResetEvent(false);
            ThreadedAI aiWO = new ThreadedAI(netName, netLocation, neuralNet.GetMutationRate(), trainingData.inputs.GetRange(start, numberPerThread), trainingData.outputs.GetRange(start, numberPerThread), doneEvents[i]);
            wantedOutputs.Add(aiWO);
            ThreadPool.QueueUserWorkItem(aiWO.CalculateWantedChanges, i);
            start += numberPerThread;
            if (start + numberPerThread > end)
            {
                numberPerThread = end - start;
            }
        }

        WaitHandle.WaitAll(doneEvents);

        foreach (var a in wantedOutputs)
        {
            wantedChanges.AddRange(a.wantedChanges);
        }
        
        neuralNet.Train(neuralNet.AverageWantedChanges(wantedChanges));
        nbOfTimeTrained++;
        wantedChanges.Clear();
    }

    public void TrainNeuralNetworkFromDirectory(string trainDataLocation, string testDataLocation, int _trainFor)
    {
        if (progress == 0)
        {
            trainingData = new DataHandler(dataCreationType, trainDataLocation);
            testData = new DataHandler(dataCreationType, testDataLocation);
            wantedChanges = new List<List<(List<float>, List<List<float>>)>>();
            train = trainAt;
        }

        trainFor = _trainFor > 0 ? aiNumber + _trainFor : -1;

        trainingData.CreateDataFromDirectory(0, trainDataFilesNumber);
        testData.CreateDataFromDirectory(0, testDataFilesNumber);

        testDir = testDataLocation;
        trainDir = trainDataLocation;
        progress = 0;
        isTraining = true;
        shouldTest = true;
    }

    public void TrainNeuralNetworkFromMultipleFiles(List<string> jsons)
    {
        if (progress == 0)
        {
            trainingData = new DataHandler(dataCreationType);
            wantedChanges = new List<List<(List<float>, List<List<float>>)>>();
            train = trainAt;
        }

        foreach (var json in jsons)
        {
            trainingData.ReadFromJsonFile(json, false);
        }

        progress = 0;
        isTraining = true;
    }

    public void TrainNeuralNetworkFromFile(string json)
    {
        if (progress == 0)
        {
            trainingData = new DataHandler(dataCreationType);
            wantedChanges = new List<List<(List<float>, List<List<float>>)>>();
            train = trainAt;
        }

        trainingData.ReadFromJsonFile(json);
        testData = trainingData;
        progress = 0;
        isTraining = true;
    }

#endregion
}

public class ThreadedAI
{
    private NeuralNetwork neuralNet;
    private ManualResetEvent _doneEvent;
    public List<List<(List<float>, List<List<float>>)>> wantedChanges;
    private List<List<float>> inputs;
    private List<List<float>> outputs;

    public ThreadedAI(string name, string fileLocation, float mutationRate, List<List<float>> _inputs, List<List<float>> _outputs, ManualResetEvent doneEvent)
    {
        neuralNet = new NeuralNetwork(name, fileLocation);
        neuralNet.SetNewMutationRate(mutationRate);
        _doneEvent = doneEvent;
        inputs = _inputs;
        outputs = _outputs;
        wantedChanges = new List<List<(List<float>, List<List<float>>)>>();
    }

    public void CalculateWantedChanges(System.Object number)
    {
        for (int i = 0; i < inputs.Count; i++)
        {
            neuralNet.GetOuputFromInput(inputs[i]);
            wantedChanges.Add(neuralNet.GetWantedChanges(outputs[i]));
        }

        _doneEvent.Set();
    }

    public void SetDoneEvent(System.Object number)
    {
        _doneEvent.Set();
    }
}
