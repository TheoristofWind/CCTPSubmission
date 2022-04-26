using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class DataTest : DataCreationJob
{
    public DataTest(ManualResetEvent doneEvent) : base(doneEvent) { }

    public override (List<List<float>>, List<List<float>>) GetInputsAndOutputs(System.Object json)
    {
        List<List<float>> a = new List<List<float>>();
        List<List<float>> b = new List<List<float>>();

        return (a, b);
    }
}
