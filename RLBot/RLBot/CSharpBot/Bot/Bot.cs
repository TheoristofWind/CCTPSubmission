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

        // We want the constructor for our Bot to extend from RLBotDotNet.Bot, but we don't want to add anything to it.
        // You might want to add logging initialisation or other types of setup up here before the bot starts.
        public Bot(string botName, int botTeam, int botIndex) : base(botName, botTeam, botIndex) {
        }

        public override Controller GetOutput(rlbot.flat.GameTickPacket gameTickPacket)
        {
            // We process the gameTickPacket and convert it to our own internal data structure.
            Packet packet = new Packet(gameTickPacket);

            ai.GetInputHandler().SetInputs(packet, Index, Renderer);

            // Get the data required to drive to the ball.
            //Vector3 ballLocation = packet.Ball.Physics.Location;
            //Vector3 carLocation = packet.Players[Index].Physics.Location;
            //Orientation carRotation = packet.Players[Index].Physics.Rotation;
            //
            //
            //Renderer.DrawString2D(carRotation.Pitch.ToString() + " : " + carRotation.Yaw.ToString() + " : " + carRotation.Roll.ToString(), Color.Aqua, new Vector2(0, 50), 1, 1);
            //Renderer.DrawString2D(packet.GameInfo.SecondsElapsed.ToString(), Color.Aqua, new Vector2(0, 100), 1, 1);

            Controller con = ai.GetOutput(Renderer);

            Renderer.DrawString2D(con.Throttle.ToString() + " \n" + con.Steer.ToString() + " \n", Color.Aqua, new Vector2(10, 100), 1, 1);
            return con;
            /*
            // Find where the ball is relative to us.
            Vector3 ballRelativeLocation = Orientation.RelativeLocation(carLocation, ballLocation, carRotation);

            // Decide which way to steer in order to get to the ball.
            // If the ball is to our left, we steer left. Otherwise we steer right.
            float steer;
            if (ballRelativeLocation.Y > 0)
                steer = 1;
            else
                steer = -1;
            

            // Examples of rendering in the game
            Renderer.DrawString3D("Ball", Color.Black, ballLocation, 3, 3);
            Renderer.DrawString3D(steer > 0 ? "Right" : "Left", Color.Aqua, carLocation, 3, 3);
            Renderer.DrawString2D(carLocation.X.ToString() + " : " + carLocation.Y.ToString() + " : " + carLocation.Z.ToString(), Color.Aqua, new Vector2(0, 0), 1, 1);
            Renderer.DrawString2D(carRotation.Pitch.ToString() + " : " + carRotation.Yaw.ToString() + " : " + carRotation.Roll.ToString(), Color.Aqua, new Vector2(0, 50), 1, 1);
            Renderer.DrawString2D(carRotation.Up.ToString() + " : " + carRotation.Right.ToString() + " : " + carRotation.Forward.ToString(), Color.Aqua, new Vector2(0, 100), 1, 1);
            Renderer.DrawLine3D(Color.Red, carLocation, ballLocation);

            // This controller will contain all the inputs that we want the bot to perform.
            return new Controller
            {
                // Set the throttle to 1 so the bot can move.
                Throttle = 1,
                Steer = 0,
                Boost = true,
            };*/
        }
        
        // Hide the old methods that return Flatbuffers objects and use our own methods that
        // use processed versions of those objects instead.
        internal new FieldInfo GetFieldInfo() => new FieldInfo(base.GetFieldInfo());
        internal new BallPrediction GetBallPrediction() => new BallPrediction(base.GetBallPrediction());
    }
}