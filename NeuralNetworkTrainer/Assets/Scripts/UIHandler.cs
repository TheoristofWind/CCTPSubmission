using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIHandler : MonoBehaviour
{
    public InputField mutationRateField;
    public Text debugText;
    public AIVisual ai;

    private int trainFor = -1;
    private int nbInputs = 0;
    private int nbOutputs = 0;
    private int nbHiddenLayers = 0;
    private int nodesPerLayer = 0;
    private float mutationRate = 0;

    private string trainingLocation = "TrainingData";
    private string testLocation = "TestData";
    private string readLocation = "test.txt";
    private string writeLocation = "test.txt";

    private bool neuralNetCreated = false;

    public void ChangeInput(string val)
    {
        if (!int.TryParse(val, out nbInputs))
        {
            debugText.text = "nbInputs not changed, string " + val + " could not be parsed.";
        }
    }

    public void ChangeOutput(string val)
    {
        if (!int.TryParse(val, out nbOutputs))
        {
            debugText.text = "nbOutputs not changed, string " + val + " could not be parsed.";
        }
    }

    public void ChangeNbHiddenLayers(string val)
    {
        if (!int.TryParse(val, out nbHiddenLayers))
        {
            debugText.text = "nbHiddenLayers not changed, string " + val + " could not be parsed.";
        }
    }

    public void ChangeNodesPerLayer(string val)
    {
        if (!int.TryParse(val, out nodesPerLayer))
        {
            debugText.text = "nodesPerLayer not changed, string " + val + " could not be parsed.";
        }
    }

    public void ChangeMutationRate(string val)
    {
        if (!float.TryParse(val, out mutationRate))
        {
            debugText.text = "mutationRate not changed, string " + val + " could not be parsed.";
            return;
        }
        if (neuralNetCreated)
        {
            ai.ChangeMutationRate(mutationRate);
        }
        else
        {
            mutationRateField.text = val;
        }
    }

    public void ChangeTrainFor(string val)
    {
        int train = 0;
        if (!int.TryParse(val, out train))
        {
            debugText.text = val + " could not be parsed.";
            return;
        }

        trainFor = train;
    }

    public void ChangeTrainFilesFor(string val)
    {
        int train = 0;
        if (!int.TryParse(val, out train))
        {
            debugText.text = val + " could not be parsed.";
            return;
        }

        ai.trainDataFilesNumber = train;
    }

    public void ChangeTestFilesFor(string val)
    {
        int train = 0;
        if (!int.TryParse(val, out train))
        {
            debugText.text = val + " could not be parsed.";
            return;
        }

        ai.testDataFilesNumber = train;
    }

    public void ChangeTrainingLocation(string val)
    {
        trainingLocation = val;
    }

    public void ChangeTestLocation(string val)
    {
        testLocation = val;
    }

    public void ChangeReadLocation(string val)
    {
        readLocation = val;
    }

    public void ChangeWriteLocation(string val)
    {
        writeLocation = val;
        ai.writeNNLocation = writeLocation;
    }

    public void StopTraining()
    {
        ai.StopTraining();
    }

    public void ChangeTrainEvery(string val)
    {
        int train = 0;
        if (!int.TryParse(val, out train))
        {
            debugText.text = val + " could not be parsed.";
            return;
        }

        ai.trainAt = train;
    }

    public void CreateNeuralNetwork()
    {
        if (neuralNetCreated)
        {
            debugText.text = "<color=red>NEURAL NET ALREADY CREATED.</color>";
            return;
        }

        ai.CreateNeuralNetwork(nbInputs, nbOutputs, nbHiddenLayers, nodesPerLayer, mutationRate);
        debugText.text = "Created neural network with:\n"
            + nbInputs.ToString() + " nbInputs\n"
            + nbOutputs.ToString() + " nbOutputs\n"
            + nbHiddenLayers.ToString() + " nbHiddenLayers\n"
            + nodesPerLayer.ToString() + " nodesPerLayer\n";

        neuralNetCreated = true;
    }

    public void WriteNeuralNetworkToFile()
    {
        if (!neuralNetCreated)
        {
            debugText.text = "<color=red>Please create the neural network first!!!</color>";
            return;
        }

        ai.WriteToFile(writeLocation);
        debugText.text = "Neural net written to file: " + writeLocation;
    }

    public void ReadNeuralNetworkFromFile()
    {
        if (neuralNetCreated)
        {
            debugText.text = "<color=red>NEURAL NET ALREADY CREATED.</color>";
            return;
        }

        ai.ReadFromFile(readLocation);
        neuralNetCreated = true;
        debugText.text = "Created neural network from file: " + readLocation;
    }

    public void TrainNeuralNetworkFromFile()
    {
        ai.TrainNeuralNetworkFromDirectory(trainingLocation, testLocation, trainFor);
    }

    void Update()
    {
        if (ai.isTraining)
        {
            debugText.text = "Iterations: " + ai.iterations.ToString() + 
                "\nError amount last tested: " + ai.previousErrorValue.ToString() + 
                "\nTrained: " + ai.nbOfTimeTrained.ToString() +
                "\nReplay Files Used: " + ai.totalTrain.ToString();
        }
    }
}
