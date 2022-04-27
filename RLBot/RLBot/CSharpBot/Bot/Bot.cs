using System.Drawing;
using System.Numerics;
using Bot.Utilities.Processed.BallPrediction;
using Bot.Utilities.Processed.FieldInfo;
using Bot.Utilities.Processed.Packet;
using RLBotDotNet;
using System;

namespace Bot
{
    // We want to our bot to derive from Bot, and then implement its abstract methods.
    class Bot : RLBotDotNet.Bot
    {
        NeuralNetworkHandler ai = new NeuralNetworkHandler();

        public Bot(string botName, int botTeam, int botIndex) : base(botName, botTeam, botIndex) {
        }

        public override Controller GetOutput(rlbot.flat.GameTickPacket gameTickPacket)
        {
            Packet packet = new Packet(gameTickPacket);

            ai.GetInputHandler().SetInputs(packet, Index, Renderer);

            Controller con = ai.GetOutput(Renderer);

            Renderer.DrawString2D(con.Throttle.ToString() + " \n" + con.Steer.ToString() + " \n", Color.Aqua, new Vector2(10, 100), 1, 1);
            return con;
        }
        

        internal new FieldInfo GetFieldInfo() => new FieldInfo(base.GetFieldInfo());
        internal new BallPrediction GetBallPrediction() => new BallPrediction(base.GetBallPrediction());
    }
}