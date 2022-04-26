using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InputManager : MonoBehaviour
{
    public bool inputWorldDir = false;

    [SerializeField] private Grid grid;
    [SerializeField] private Text gameOverUI;
    private Snake snake;

    private bool inGame = true;

    void Start()
    {
        snake = grid.GetSnake();
    }

    // Update is called once per frame
    void Update()
    {
        if (!inGame)
        {
            if (Input.GetKeyDown("return"))
            {
                inGame = true;
                gameOverUI.transform.parent.gameObject.SetActive(false);
                snake = grid.ResetGame();
            }
            return;
        }

        bool gameOver = false;
        if (Input.GetKeyDown("right"))
        {
            gameOver = snake.MoveInDirection(Snake.DIRECTION.Right, !inputWorldDir);
        }
        if (Input.GetKeyDown("left"))
        {
            gameOver = snake.MoveInDirection(Snake.DIRECTION.Left, !inputWorldDir);
        }
        if (Input.GetKeyDown("up"))
        {
            gameOver = snake.MoveInDirection(Snake.DIRECTION.Up, !inputWorldDir);
        }
        if (Input.GetKeyDown("down") && inputWorldDir)
        {
            gameOver = snake.MoveInDirection(Snake.DIRECTION.Down, !inputWorldDir);
        }

        if (gameOver)
        {
            inGame = false;
            gameOverUI.text = "GAME OVER\nFinished with " + snake.GetSnakeLength().ToString() + " points";
            gameOverUI.transform.parent.gameObject.SetActive(true);
        }
    }   
}
