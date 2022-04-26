using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public abstract class DataCreationJob
{
    public List<List<float>> inputs;
    public List<List<float>> outputs;
    private ManualResetEvent _doneEvent;

    public DataCreationJob(ManualResetEvent doneEvent)
    {
        inputs = new List<List<float>>();
        outputs = new List<List<float>>();
        _doneEvent = doneEvent;
    }

    public void Execute(System.Object json)
    {
        (List<List<float>>, List<List<float>>) inOut = GetInputsAndOutputs(json);
        inputs = inOut.Item1;
        outputs = inOut.Item2;

        _doneEvent.Set();
    }

    public abstract (List<List<float>>, List<List<float>>) GetInputsAndOutputs(System.Object json);

    public static DataCreationJob GetDataCreationJobFromType(Type type, ManualResetEvent doneEvent)
    {
        return (DataCreationJob)Activator.CreateInstance(type, doneEvent);
    }
}