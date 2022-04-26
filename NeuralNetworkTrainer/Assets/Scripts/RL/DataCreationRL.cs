using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Threading;

public class DataCreationRL : DataCreationJob
{
    const float distCenterWall = 4096;
    const float distCenterBackGoal = 6000;
    const float distfloorCeiling = 2044;

    const float maxCarSpeed = 2300;
    const float maxCarAngularVelocity = 5.5f;
    const float maxBallSpeed = 6000;

    public DataCreationRL(ManualResetEvent doneEvent) : base(doneEvent)
    { }

    public override (List<List<float>>, List<List<float>>) GetInputsAndOutputs(System.Object json)
    {
        Game thisGame = JsonUtility.FromJson<Game>("{\"game\":" + (string)json + "}");
        
        return CreateInputsAndOutputsFromGame(thisGame);
    }

    private static (List<List<float>>, List<List<float>>) CreateInputsAndOutputsFromGame(Game game)
    {
        List<List<float>> inputs = new List<List<float>>();
        List<List<float>> outputs = new List<List<float>>();

        // Currently 27 inputs for 1v1 
        // 41 for 2v2  
        // 55 for 3v3

        foreach (Frame frame in game.game)
        {
            for (int p = 0; p < game.game[0].PlayerData.Length; p++)
            {
                Double3 aiPos = frame.PlayerData[p].position;
                Double3 aiRot = frame.PlayerData[p].rotation;
                Orientation aiOrientation = new Orientation((float)aiRot.x, (float)aiRot.y, (float)aiRot.z);

                List<float> frameInputs = GetAIInputsOfFrame(frame, aiPos, aiOrientation, p);
                frameInputs.AddRange(GetGameStateInputs(frame, aiPos, aiOrientation));

                for (int p2 = 0; p2 < game.game[0].PlayerData.Length; p2++)
                {
                    if (p2 != p)
                    {
                        frameInputs.AddRange(GetOtherPlayerInputs(frame, aiPos, aiOrientation, p2));
                    }
                }

                inputs.Add(frameInputs);
                outputs.Add(GetOuputsForFrame(frame.PlayerData[p]));
            }
        }

        return (inputs, outputs);
    }

    // Returns 13 different inputs. These inputs are all between -1 and 1 or 0 and 1
    private static List<float> GetAIInputsOfFrame(Frame frame, Double3 pos, Orientation orientation, int aiPlayerNb)
    {
        List<float> f_inputs = new List<float>();

        bool blueTeam = frame.PlayerData[aiPlayerNb].team == 0;
        Double3 angVel = frame.PlayerData[aiPlayerNb].angular_velocity;
        Vector3 relativeVel = Orientation.RelativeLocation(pos, Double3.Add(frame.PlayerData[aiPlayerNb].velocity, pos), orientation);

        // float -1 to 1 for x position
        // float -1 to 1 for y position
        // float 0 to 1 for z position
        f_inputs.Add(((float)pos.x / distCenterWall) * (blueTeam ? 1 : -1));
        f_inputs.Add(((float)pos.y / distCenterBackGoal) * (blueTeam ? 1 : -1));
        f_inputs.Add(((float)pos.z / distfloorCeiling));

        // float 0 to 1 for current boost
        f_inputs.Add((float)frame.PlayerData[aiPlayerNb].boost_level / 100);

        // float -1 to 1 for current relative x velocity 
        // float -1 to 1 for current relative y velocity 
        // float -1 to 1 for current relative z velocity 
        f_inputs.Add(relativeVel.x / maxCarSpeed);
        f_inputs.Add(relativeVel.y / maxCarSpeed);
        f_inputs.Add(relativeVel.z / maxCarSpeed);

        // float -1 to 1 for current x angular velocity 
        // float -1 to 1 for current y angular velocity 
        // float -1 to 1 for current z angular velocity 
        f_inputs.Add((float)angVel.x / maxCarAngularVelocity);
        f_inputs.Add((float)angVel.y / maxCarAngularVelocity);
        f_inputs.Add((float)angVel.z / maxCarAngularVelocity);

        // float -1 to 1 for bot x rotation
        // float -1 to 1 for bot y rotation
        // float -1 to 1 for bot z rotation
        f_inputs.Add((float)(orientation.Pitch / ((Math.PI) / 2)));
        f_inputs.Add((float)(orientation.Roll / Math.PI));
        f_inputs.Add((float)(orientation.Yaw / Math.PI));

        return f_inputs;
    }

    // Returns 7 different inputs.
    private static List<float> GetGameStateInputs(Frame frame, Double3 aiPos, Orientation aiOrientation)
    {
        List<float> f_inputs = new List<float>();
        Vector3 relPos = Orientation.RelativeLocation(aiPos, frame.GameState.ball.position, aiOrientation);
        Vector3 relVel = Orientation.RelativeLocation(aiPos, Double3.Add(aiPos, frame.GameState.ball.velocity), aiOrientation);

        // float -1 to 1 for ball relative x position
        // float -1 to 1 for ball relative y position
        // float -1 to 1 for ball relative z position
        f_inputs.Add(relPos.x / (distCenterWall * 2));
        f_inputs.Add(relPos.y / (distCenterBackGoal * 2));
        f_inputs.Add(relPos.z / (distfloorCeiling));

        // float -1 to 1 for ball relative x velocity
        // float -1 to 1 for ball relative y velocity
        // float -1 to 1 for ball relative z velocity
        f_inputs.Add(relVel.x / (maxBallSpeed + maxCarSpeed));
        f_inputs.Add(relVel.y / (maxBallSpeed + maxCarSpeed));
        f_inputs.Add(relVel.z / (maxBallSpeed + maxCarSpeed));

        // float 0 to 1 for time remaining
        f_inputs.Add((float)frame.GameState.seconds_remaining / 300);

        return f_inputs;
    }

    // Returns 7 different inputs.
    private static List<float> GetOtherPlayerInputs(Frame frame, Double3 aiPos, Orientation aiOrientation, int playerNb)
    {
        List<float> f_inputs = new List<float>();
        Vector3 relPos = Orientation.RelativeLocation(aiPos, frame.PlayerData[playerNb].position, aiOrientation);
        Vector3 relVel = Orientation.RelativeLocation(aiPos, Double3.Add(aiPos, frame.PlayerData[playerNb].velocity), aiOrientation);

        // float -1 to 1 for opp relative x position
        // float -1 to 1 for opp relative y position
        // float -1 to 1 for opp relative z position
        f_inputs.Add(relPos.x / (distCenterWall * 2));
        f_inputs.Add(relPos.y / (distCenterBackGoal * 2));
        f_inputs.Add(relPos.z / (distfloorCeiling));

        // float -1 to 1 for opp relative x velocity
        // float -1 to 1 for opp relative y velocity
        // float -1 to 1 for opp relative z velocity
        f_inputs.Add(relVel.x / (maxCarSpeed * 2));
        f_inputs.Add(relVel.y / (maxCarSpeed * 2));
        f_inputs.Add(relVel.z / (maxCarSpeed * 2));

        // float 0 to 1 for opp boost amount
        f_inputs.Add((float)frame.PlayerData[playerNb].boost_level / 100);

        return f_inputs;
    }

    private static List<float> GetOuputsForFrame(PlayerData aiPlayer)
    {
        List<float> f_outputs = new List<float>();

        f_outputs.Add(aiPlayer.throttle > 0 ? aiPlayer.throttle : 0);
        f_outputs.Add(aiPlayer.throttle < 0 ? aiPlayer.throttle : 0);
        f_outputs.Add(aiPlayer.steer > 0 ? aiPlayer.steer : 0);
        f_outputs.Add(aiPlayer.steer < 0 ? aiPlayer.steer : 0);
        f_outputs.Add(aiPlayer.jump ? 1 : 0);
        f_outputs.Add(aiPlayer.boosting ? 1 : 0);
        f_outputs.Add(aiPlayer.roll > 0 ? aiPlayer.roll : 0);
        f_outputs.Add(aiPlayer.roll < 0 ? aiPlayer.roll : 0);
        f_outputs.Add(aiPlayer.handbrake ? 1 : 0);

        return f_outputs;
    }
}

#region Game Structs 

[Serializable]
public struct Game
{
    public Frame[] game;
}

[Serializable]
public struct Frame
{
    public GameState GameState;
    public PlayerData[] PlayerData;
}

[Serializable]
public struct GameState
{
    public double time;
    public int seconds_remaining;
    public double deltatime;
    public Object ball;
}

[Serializable]
public struct PlayerData
{
    public int index;
    public string name;
    public int team;
    public Double3 position;
    public Double3 velocity;
    public Double3 rotation;
    public Double3 angular_velocity;
    public bool boosting;
    public int boost_level;
    public float throttle;
    public float steer;
    public float pitch;
    public float yaw;
    public float roll;
    public bool jump;
    public bool handbrake;
}

[Serializable]
public struct Object
{
    public Double3 position;
    public Double3 velocity;
    public Double3 rotation;
}

[Serializable]
public struct Double3
{
    public double x;
    public double y;
    public double z;

    public static Double3 Add(Double3 a, Double3 b)
    {
        a.x += b.x;
        a.y += b.y;
        a.z += b.z;

        return a;
    }

    public static Vector3 Sub(Double3 a, Double3 b)
    {
        Vector3 r = new Vector3((float)a.x, (float)a.y, (float)a.z);
        r.x -= (float)b.x;
        r.y -= (float)b.y;
        r.z -= (float)b.z;

        return r;
    }
}

#endregion

public struct Orientation
{
    public float Pitch;
    public float Roll;
    public float Yaw;

    public Vector3 Forward;
    public Vector3 Right;
    public Vector3 Up;

    public Orientation(float x, float y, float z)
    {
        Pitch = x;
        Roll = y;
        Yaw = z;

        float cp = (float)Math.Cos(Pitch);
        float cy = (float)Math.Cos(Yaw);
        float cr = (float)Math.Cos(Roll);
        float sp = (float)Math.Sin(Pitch);
        float sy = (float)Math.Sin(Yaw);
        float sr = (float)Math.Sin(Roll);

        Forward = new Vector3(cp * cy, cp * sy, sp);
        Right = new Vector3(cy * sp * sr - cr * sy, sy * sp * sr + cr * cy, -cp * sr);
        Up = new Vector3(-cr * cy * sp - sr * sy, -cr * sy * sp + sr * cy, cp * cr);
    }

    public static Vector3 RelativeLocation(Double3 start, Double3 target, Orientation orientation)
    {
        Vector3 startToTarget = Double3.Sub(target, start);
        float x = Vector3.Dot(startToTarget, orientation.Forward);
        float y = Vector3.Dot(startToTarget, orientation.Right);
        float z = Vector3.Dot(startToTarget, orientation.Up);

        return new Vector3(x, y, z);
    }
}
