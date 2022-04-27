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

        NeuralNetwork neuralNet = new NeuralNetwork("AI.txt", "./NeuralNetwork/"); // "./NeuralNetwork/ is located in bin/Debug/NeuralNetwork/
        InputHandler inputH = new InputHandler();

        public InputHandler GetInputHandler()
        {
            return inputH;
        }

        public Controller GetOutput(RLBotDotNet.Renderer.Renderer Renderer)
        {
            List<float> outputs = neuralNet.GetOuputFromInput(inputH.GetInputs());

            if (outputs.Count == 0) return new Controller();

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
    }
}
