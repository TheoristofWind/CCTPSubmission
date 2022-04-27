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
        const float distCenterWall = 4096;
        const float distCenterBackGoal = 6000;
        const float distfloorCeiling = 2044;

        const float maxCarSpeed = 2300;
        const float maxCarAngularVelocity = 5.5f;
        const float maxBallSpeed = 6000;

        List<float> inputs;

        public InputHandler()
        {
            inputs = new List<float>();
            Start();
        }

        public void Start()
        {
            for (int i = 0; i < 41; i++)
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

            SetAIInputs(carLocation, packet.Players[Index], carRotation);
            SetGameStateInputs(packet, carLocation, carRotation);

            int startingIndex = 20;
            for (int i = 0; i < 4; i++)
            {
                if (i != Index)
                {
                    GetOtherPlayerInputs(packet.Players[i], carLocation, carRotation, startingIndex);
                    startingIndex += 7;
                }
            }

        }

        private void SetAIInputs(Vector3 pos, Player player, Orientation orientation)
        {
            bool blueTeam = player.Team == 0;
            Vector3 angVel = player.Physics.AngularVelocity;
            Vector3 relativeVel = Orientation.RelativeLocation(pos, player.Physics.Velocity + pos, orientation);

            // float -1 to 1 for x position
            // float -1 to 1 for y position
            // float 0 to 1 for z position
            inputs[0] = ((pos.X / distCenterWall) * (blueTeam ? 1 : -1));
            inputs[1] = ((pos.Y / distCenterBackGoal) * (blueTeam ? 1 : -1));
            inputs[2] = ((pos.Z / distfloorCeiling));

            // float 0 to 1 for current boost
            inputs[3] = (player.Boost / 100);

            // float -1 to 1 for current relative x velocity 
            // float -1 to 1 for current relative y velocity 
            // float -1 to 1 for current relative z velocity 
            inputs[4] = (relativeVel.X / maxCarSpeed);
            inputs[5] = (relativeVel.Y / maxCarSpeed);
            inputs[6] = (relativeVel.Z / maxCarSpeed);

            // float -1 to 1 for current x angular velocity 
            // float -1 to 1 for current y angular velocity 
            // float -1 to 1 for current z angular velocity 
            inputs[7] = ((float)angVel.X / maxCarAngularVelocity);
            inputs[8] = ((float)angVel.Y / maxCarAngularVelocity);
            inputs[9] = ((float)angVel.Z / maxCarAngularVelocity);

            // float -1 to 1 for bot x rotation
            // float -1 to 1 for bot y rotation
            // float -1 to 1 for bot z rotation
            inputs[10] = ((float)(orientation.Pitch / ((Math.PI) / 2)));
            inputs[11] = ((float)(orientation.Roll / Math.PI));
            inputs[12] = ((float)(orientation.Yaw / Math.PI));
        }

        private void SetGameStateInputs(Packet packet, Vector3 aiPos, Orientation aiOrientation)
        {
            Vector3 relPos = Orientation.RelativeLocation(aiPos, packet.Ball.Physics.Location, aiOrientation);
            Vector3 relVel = Orientation.RelativeLocation(aiPos, aiPos + packet.Ball.Physics.Velocity, aiOrientation);

            // float -1 to 1 for ball relative x position
            // float -1 to 1 for ball relative y position
            // float -1 to 1 for ball relative z position
            inputs[13] = (relPos.X / (distCenterWall * 2));
            inputs[14] = (relPos.Y / (distCenterBackGoal * 2));
            inputs[15] = (relPos.Z / (distfloorCeiling));

            // float -1 to 1 for ball relative x velocity
            // float -1 to 1 for ball relative y velocity
            // float -1 to 1 for ball relative z velocity
            inputs[16] = (relVel.X / (maxBallSpeed + maxCarSpeed));
            inputs[17] = (relVel.Y / (maxBallSpeed + maxCarSpeed));
            inputs[18] = (relVel.Z / (maxBallSpeed + maxCarSpeed));

            // float 0 to 1 for time remaining
            inputs[19] = (packet.GameInfo.GameTimeRemaining / 300);
        }

        private void GetOtherPlayerInputs(Player player, Vector3 aiPos, Orientation aiOrientation, int startingIndex)
        {
            List<float> f_inputs = new List<float>();
            Vector3 relPos = Orientation.RelativeLocation(aiPos, player.Physics.Location, aiOrientation);
            Vector3 relVel = Orientation.RelativeLocation(aiPos, aiPos + player.Physics.Velocity, aiOrientation);

            // float -1 to 1 for opp relative x position
            // float -1 to 1 for opp relative y position
            // float -1 to 1 for opp relative z position
            inputs[startingIndex] = (relPos.X / (distCenterWall * 2));
            inputs[startingIndex + 1] = (relPos.Y / (distCenterBackGoal * 2));
            inputs[startingIndex + 2] = (relPos.Z / (distfloorCeiling));

            // float -1 to 1 for opp relative x velocity
            // float -1 to 1 for opp relative y velocity
            // float -1 to 1 for opp relative z velocity
            inputs[startingIndex + 3] = (relVel.X / (maxCarSpeed * 2));
            inputs[startingIndex + 4] = (relVel.Y / (maxCarSpeed * 2));
            inputs[startingIndex + 5] = (relVel.Z / (maxCarSpeed * 2));

            // float 0 to 1 for opp boost amount
            inputs[startingIndex + 6] = ((float)player.Boost / 100);
        }

        public List<float> GetInputs()
        {
            return inputs;
        }
    }
}
