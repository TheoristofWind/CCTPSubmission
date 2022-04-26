using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class AIHandler : MonoBehaviour
{
    [Header("Training process")]
    [SerializeField] private int nbIterationBeforeTrain;
    [SerializeField] private int nbTimesToTrain;
    [SerializeField] private int nbMovesPerGame;
    [SerializeField] private int nbGamesPlayedForError;
    [SerializeField] private float valueToReach;

    [Header("Imitation Learning rand")]
    [SerializeField] private float modChance = 0;

    [Header("Reward")]
    [SerializeField] private float gotFruitReward;
    [SerializeField] private float hitWallReward;
    [SerializeField] private float towardsFruiReward;
    [SerializeField] private float awayFromFruitReward;

    [Header("Neural Network setting")]
    [SerializeField] private int inputs;
    [SerializeField] private int outputs;
    [SerializeField] private int layers;
    [SerializeField] private int nodesPerLayer;
    [SerializeField] private float mutationRate;

    [SerializeField] private Grid grid;
    private Snake snake;

    private NeuralNetwork neuralNet;
    private bool gameOver = false;

    public float timePerMove = 0.1f;
    private bool move = false;
    private float time = 0;

    private int iterations = 0;
    private int turns = 0;

    string error = "";

    // Start is called before the first frame update
    void Start()
    {
        neuralNet = new NeuralNetwork(inputs, outputs, layers, nodesPerLayer, mutationRate);
        snake = grid.GetSnake();

        //TrainNetwork();
        //CalculateErrorForNetwork();
    }

    void Update()
    {
        if (move)
        {
            time -= Time.deltaTime;
            if (time < 0)
            {
                turns++;
                time = timePerMove;
                MakeMoveWithoutTrain();
                move = !gameOver;
                if (gameOver)
                {
                    Debug.Log("SCORE: " + snake.GetSnakeLength() + " in " + turns + " turns.");
                    turns = 0;
                }
            }
        }

        if (Input.GetKeyDown("return"))
        {
            move = false;
            error = "";
            TrainNetwork();
            Debug.Log(error);
            //CalculateErrorForNetwork();
        }

        if (Input.GetKeyDown("space"))
        {
            snake = grid.ResetGame();
            
        }

        if (Input.GetKeyDown("up"))
        {
            move = !move;
        }

        if (Input.GetKeyDown("down"))
        {
            Debug.Log("INPUTS ARE: ");
            foreach (float i in GetInputs())
            {
                Debug.Log(i);
            }
        }
    }

    void TrainNetwork()
    {
        for (int t = 0; t < nbTimesToTrain; t++)
        {
            List<List<(List<float>, List<List<float>>)>> allWantedChanges = new List<List<(List<float>, List<List<float>>)>>();
            for (int n = 0; n < nbIterationBeforeTrain; n++)
            {
                snake = grid.ResetGame();
                gameOver = false;

                for (int i = 0; i < nbMovesPerGame && !gameOver; i++)
                {
                    allWantedChanges.Add(MakeMove());
                }
            }
            neuralNet.Train(neuralNet.AverageWantedChanges(allWantedChanges));
            iterations++;

            (float, float) scoreTurns = CalculateErrorForNetwork();
            //error += "Has Completed " + iterations + " iterations with an average score of " + scoreTurns.Item1 + " in games with " + scoreTurns.Item2 + " turns on average.\n";
            error += scoreTurns.Item1.ToString() + "\n";
        }
        //(float, float) scoreTurns = CalculateErrorForNetwork();
        ////error += "Has Completed " + iterations + " iterations with an average score of " + scoreTurns.Item1 + " in games with " + scoreTurns.Item2 + " turns on average.\n";
        //error += scoreTurns.Item1.ToString() + "\n";
    }

    (float, float) CalculateErrorForNetwork()
    {
        float score = 0;
        float turns = 0;
        for (int t = 0; t < nbGamesPlayedForError; t++)
        {
            snake = grid.ResetGame();
            gameOver = false;

            for (int i = 0; i < nbMovesPerGame && !gameOver; i++)
            {
                MakeMoveWithoutTrain();
                turns++;
            }
            score += snake.GetSnakeLength();
        }
        score /= nbGamesPlayedForError;
        turns /= nbGamesPlayedForError;
        return (score, turns);


        //float avError = 0;
        //for (int i = 0; i < 10; i++)
        //{
        //    List<float> input = GetDebugInputs();
        //    List<float> inputs = GetDebugWantedOutputs(input);
        //    List<float> output = neuralNet.GetOuputFromInput(input);
        //
        //    float error = 0;
        //    for (int y = 0; y < output.Count; y++)
        //    {
        //        error += Mathf.Abs(output[y] - inputs[y]);
        //    }
        //    avError += error;
        //}
        //avError /= 10;
        //return avError;
    }

    List<(List<float>, List<List<float>>)> MakeMove()
    {
        List<float> inputs = GetInputs();
        List<float> output = neuralNet.GetOuputFromInput(inputs);

        Snake.DIRECTION moveDir = Snake.DIRECTION.Right;
        if (output[0] > output[1] && output[0] > output[2])
        {
            moveDir = Snake.DIRECTION.Left;
        }
        if (output[1] > output[0] && output[1] > output[2])
        {
            moveDir = Snake.DIRECTION.Up;
        }
        List<(List<float>, List<List<float>>)> o = neuralNet.GetWantedChanges(GetWantedOutputs());
        gameOver = snake.MoveInDirection(moveDir, true);

        return o;
    }

    void MakeMoveWithoutTrain()
    {
        List<float> inputs = GetInputs();
        List<float> output = neuralNet.GetOuputFromInput(inputs);

        Snake.DIRECTION moveDir = Snake.DIRECTION.Right;
        if (output[0] > output[1] && output[0] > output[2])
        {
            moveDir = Snake.DIRECTION.Left;
        }
        if (output[1] > output[0] && output[1] > output[2])
        {
            moveDir = Snake.DIRECTION.Up;
        }
        gameOver = snake.MoveInDirection(moveDir, true);
    }

    List<(List<float>, List<List<float>>)> MakeMoveReward()
    {
        List<float> inputs = GetInputs();
        List<float> output = neuralNet.GetOuputFromInput(inputs);

        Snake.DIRECTION moveDir = Snake.DIRECTION.Right;

        if (output[0] != output[0])
        {
            Debug.LogError("OUTPUT IS NAN :(");
        }

        if (output[0] > output[1] && output[0] > output[2])
        {
            moveDir = Snake.DIRECTION.Left;
        }
        if (output[1] > output[0] && output[1] > output[2])
        {
            moveDir = Snake.DIRECTION.Up;
        }

        Vector2Int fPos = grid.GetFruitLocation();
        float fruitDist = Vector2Int.Distance(snake.GetSnakeHeadPos(), fPos);

        gameOver = snake.MoveInDirection(moveDir, true);

        float reward = gameOver ? hitWallReward : (fPos != grid.GetFruitLocation() ? gotFruitReward : (Vector2Int.Distance(snake.GetSnakeHeadPos(), fPos) < fruitDist ? towardsFruiReward : (Vector2Int.Distance(snake.GetSnakeHeadPos(), fPos) > fruitDist ? awayFromFruitReward : 0)));

        return neuralNet.GetWantedChanges(GetWantedOuputsFromRewardAndInput(moveDir, reward));
    }

    List<(List<float>, List<List<float>>)> DebugTrainTest()
    {
        List<float> inputs = GetDebugInputs();
        List<float> output = neuralNet.GetOuputFromInput(inputs);

        return neuralNet.GetWantedChanges(GetDebugWantedOutputs(inputs));
    }

    List<float> GetDebugInputs()
    {
        //TESTING

        List<float> input = new List<float>();
        //input.Add(0);
        //input.Add(1);

        int r = UnityEngine.Random.Range(0, 4);

        input.Add(r == 0 ? 1.0f : 0.0f);
        input.Add(r == 1 ? 1.0f : 0.0f);
        input.Add(r == 2 ? 1.0f : 0.0f);
        input.Add(r == 3 ? 1.0f : 0.0f);

        return input;
        //END OF TESTING
    }

    List<float> GetInputs()
    {
        List<float> input = new List<float>();

        Snake.DIRECTION dir = snake.GetHeadDir();
        Vector2Int snakePos = snake.GetSnakeHeadPos();
        Vector2Int fruitPos = grid.GetFruitLocation();

        input.Add(CheckPosition(snakePos, dir, Snake.DIRECTION.Left) ? 1.0f : 0.0f);
        input.Add(CheckPosition(snakePos, dir, Snake.DIRECTION.Up) ? 1.0f : 0.0f);
        input.Add(CheckPosition(snakePos, dir, Snake.DIRECTION.Right) ? 1.0f : 0.0f);

        List<bool> dirOfFruit = GetDirectionOfFruit(snakePos, fruitPos, dir);

        input.Add(dirOfFruit[0] ? 1.0f : 0.0f);
        input.Add(dirOfFruit[1] ? 1.0f : 0.0f);
        input.Add(dirOfFruit[2] ? 1.0f : 0.0f);
        input.Add(dirOfFruit[3] ? 1.0f : 0.0f);
        return input;
    }

    List<float> GetDebugWantedOutputs(List<float> inputs)
    {
        List<float> o = new List<float>();

        o.Add(inputs[0] > 0.5f ? 1.0f : 0.0f);
        o.Add(inputs[1] > 0.5f ? 1.0f : 0.0f);
        o.Add(inputs[2] > 0.5f || inputs[3] > 0.5f ? 1.0f : 0.0f);

        return o;
    }

    List<float> GetWantedOuputsFromRewardAndInput(Snake.DIRECTION input, float reward)
    {
        List<float> output = new List<float>();

        output.Add(input == Snake.DIRECTION.Left ? reward/10 : 0.0f);
        output.Add(input == Snake.DIRECTION.Up ? reward/10 : 0.0f);
        output.Add(input == Snake.DIRECTION.Right ? reward/10 : 0.0f);

        return output;
    }

    List<float> GetWantedOutputs()
    {
        List<float> o = new List<float>();

        if (UnityEngine.Random.Range(0.0f, 1.0f) < modChance)
        {
            int r = UnityEngine.Random.Range(0, 3);
            int r2 = UnityEngine.Random.Range(0, 3);
            o.Add(r2 == 0 ? -1.0f : (r == 0 ? 1.0f :0.0f));
            o.Add(r2 == 1 ? -1.0f : (r == 1 ? 1.0f : 0.0f));
            o.Add(r2 == 2 ? -1.0f : (r == 2 ? 1.0f : 0.0f));

            return o;
        }

        Snake.DIRECTION dir = snake.GetHeadDir();
        Vector2Int snakePos = snake.GetSnakeHeadPos();
        Vector2Int fruitPos = grid.GetFruitLocation();

        List<bool> dirOfFruit = GetDirectionOfFruit(snakePos, fruitPos, dir);

        o.Add(dirOfFruit[0] ? 1.0f : 0.0f);
        o.Add(dirOfFruit[1] ? 1.0f : 0.0f);
        o.Add(dirOfFruit[2] || (dirOfFruit[3] && !dirOfFruit[0] && !dirOfFruit[1]) ? 1.0f : 0.0f);

        o[0] = CheckPosition(snakePos, dir, Snake.DIRECTION.Left) ? -1.0f : o[0];
        o[1] = CheckPosition(snakePos, dir, Snake.DIRECTION.Up) ? -1.0f : o[1];
        o[2] = CheckPosition(snakePos, dir, Snake.DIRECTION.Right) ? -1.0f : o[2];

        return o;
    }

    List<bool> GetDirectionOfFruit(Vector2Int snakePos, Vector2Int fruitPos, Snake.DIRECTION headDir)
    {
        snakePos.x -= fruitPos.x;
        snakePos.y -= fruitPos.y;

        List<bool> worldPos = new List<bool>();

        worldPos.Add(snakePos.x > 0);
        worldPos.Add(snakePos.y < 0);
        worldPos.Add(snakePos.x < 0);
        worldPos.Add(snakePos.y > 0);

        List<bool> pos = new List<bool>();

        pos.Add(worldPos[(int)headDir]);
        pos.Add(worldPos[((int)headDir + 1) % 4]);
        pos.Add(worldPos[((int)headDir + 2) % 4]);
        pos.Add(worldPos[((int)headDir + 3) % 4]);

        return pos;
    }

    bool CheckPosition(Vector2Int snakePos, Snake.DIRECTION headDir, Snake.DIRECTION dir)
    {
        dir = Snake.ConvertDirectionFromFacingDir(dir, headDir);
         
        switch (dir)
        {
            case Snake.DIRECTION.Up:
                snakePos.y += 1;
                break;
            case Snake.DIRECTION.Down:
                snakePos.y -= 1;
                break;
            case Snake.DIRECTION.Left:
                snakePos.x -= 1;
                break;
            case Snake.DIRECTION.Right:
                snakePos.x += 1;
                break;
        }

        return grid.GetTileTypeOfPosition(snakePos) == Tile.TILE_TYPES.Wall || snake.CheckLocation(snakePos);
    }
}
