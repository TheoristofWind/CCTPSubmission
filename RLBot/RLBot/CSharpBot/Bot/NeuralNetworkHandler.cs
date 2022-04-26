using System;
using System.Drawing;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Bot.Utilities.Processed.BallPrediction;
using Bot.Utilities.Processed.FieldInfo;
using Bot.Utilities.Processed.Packet;
using RLBotDotNet;

namespace Bot
{
    class NeuralNetworkHandler
    {
        enum OutputMapping
        {
            Throttle = 0,
            Break,
            Left,
            Right,
            Jump,
            Boost,
            Q,
            E,
            Shift,
        }

        const int numberInputs = 9;
        const int numberOutPuts = 9;
        const int hiddenLayers = 1;
        const int nodePerLayer = 9;
        const float mutationRate = 1.0f;

        NeuralNetwork neuralNet = new NeuralNetwork(numberInputs, numberOutPuts, hiddenLayers, nodePerLayer, mutationRate);
        InputHandler inputH = new InputHandler();

        public InputHandler GetInputHandler()
        {
            return inputH;
        }

        public Controller GetOutput(RLBotDotNet.Renderer.Renderer Renderer)
        {
            List<float> outputs = neuralNet.GetOuputFromInput(inputH.GetInputs());
            //List<float> outputs = GetDebugOutputs();

            for (int i = 0; i < outputs.Count; i++)
            {
                Renderer.DrawString2D(outputs[i].ToString(), Color.Aqua, new Vector2(500, 40*i+10), 2, 2);
                if (outputs[i] < 0)
                {
                    outputs[i] = 0;
                }

                if (outputs[i] > 1)
                {
                    outputs[i] = 1;
                }
            }

            //Renderer.DrawString2D(outputs[(int)OutputMapping.Jump].ToString() + " : " + (outputs[(int)OutputMapping.Jump] > 0.5f).ToString(), Color.SteelBlue, new Vector2(800, 410), 2, 2);

            return new Controller
            {
                Throttle = outputs[(int)OutputMapping.Throttle] - outputs[(int)OutputMapping.Break],
                Steer = outputs[(int)OutputMapping.Right] - outputs[(int)OutputMapping.Left],
                Jump = outputs[(int)OutputMapping.Jump] > 0.5f,
                Boost = outputs[(int)OutputMapping.Boost] > 0.5f,
                Handbrake = outputs[(int)OutputMapping.Shift] > 0.5f,
                Pitch = outputs[(int)OutputMapping.Throttle] - outputs[(int)OutputMapping.Break],
                Yaw = outputs[(int)OutputMapping.Right] - outputs[(int)OutputMapping.Left],
                Roll = outputs[(int)OutputMapping.Q] - outputs[(int)OutputMapping.E]
            };
        }

        List<float> GetDebugOutputs()
        {
            List<float> d = inputH.GetInputs();
            //d.Add(1); //w
            //d.Add(0); //s
            //d.Add(1); //a
            //d.Add(0); //d

            d.Add(0); //lmb
            d.Add(0); //rmb
            d.Add(0); //q 
            d.Add(0); //e
            d.Add(0); //shift

            return d;
        }
    }
}
