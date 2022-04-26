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
    class InputHandler
    {
        const int nbInputs = 9;
        List<float> inputs;
        //Renderer Renderer;

        public InputHandler()
        {
            inputs = new List<float>();
            Start();
        }

        public void Start()
        {
            for (int i = 0; i < nbInputs; i++)
            {
                inputs.Add(0);
            }
        }

        public void SetInputs(Packet packet, int Index, RLBotDotNet.Renderer.Renderer Renderer)
        {
            Vector3 ballLocation = packet.Ball.Physics.Location;
            Vector3 carLocation = packet.Players[Index].Physics.Location;
            Orientation carRotation = packet.Players[Index].Physics.Rotation;

            Vector3 ballRelativeLocation = Orientation.RelativeLocation(carLocation, ballLocation, carRotation);


            inputs[0] = (ballRelativeLocation.X > 40 ? 1.0f : 0.0f);
            inputs[2] = (ballRelativeLocation.X < -40 ? 1.0f : 0.0f);
            inputs[1] = 1.0f - (inputs[0] + inputs[2]);
            inputs[3] = (ballRelativeLocation.Y < -40 ? 1.0f : 0.0f);
            inputs[4] = 1.0f - (inputs[3] + inputs[5]);
            inputs[5] = (ballRelativeLocation.Y > 40 ? 1.0f : 0.0f);
            inputs[6] = (ballRelativeLocation.Z < -100 ? 1.0f : 0.0f);
            inputs[8] = (ballRelativeLocation.Z > 200 ? 1.0f : 0.0f);
            inputs[7] = 1.0f - (inputs[6] + inputs[8]);

            for (int i = 0; i < inputs.Count; i++)
            {
                //Renderer.DrawString2D(inputs[i].ToString() + "  ->", Color.White, new Vector2(350, 40*i+10), 2, 2);
            }
        }

        public List<float> GetInputs()
        {
            return inputs;
        }
    }
}
