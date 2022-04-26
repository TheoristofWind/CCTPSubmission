using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Snake : MonoBehaviour
{
    public enum DIRECTION
    {
        Up, 
        Right,
        Down,
        Left
    }

    protected Grid grid;

    protected DIRECTION headDir;
    protected Vector2Int headPos;
    protected List<Vector2Int> bodyPos = new List<Vector2Int>();

    public Snake SetUp(Vector2Int _pos, DIRECTION _dir, Grid _grid)
    {
        headPos = _pos;
        headDir = _dir;
        grid = _grid;
        SetUpVisuals();
        return this;
    }

    protected virtual void SetUpVisuals()
    {
    }

    public static DIRECTION ConvertDirectionFromFacingDir(DIRECTION dir, DIRECTION facingDir)
    {
        return (DIRECTION)(((int)facingDir + (int)dir) % 4);
    }

    public static DIRECTION MinusDirection(DIRECTION dir, DIRECTION facingDir)
    {
        return (DIRECTION)(((int)dir - (int)facingDir + 4) % 4);
    }

    public bool MoveInDirection(DIRECTION dir, bool convertDir = false)
    {
        if (convertDir) dir = ConvertDirectionFromFacingDir(dir, headDir);

        Vector2Int newHeadPos = headPos;
        switch (dir)
        {
            case DIRECTION.Up:
                newHeadPos.y += 1;
                break;
            case DIRECTION.Right:
                newHeadPos.x += 1;
                break;
            case DIRECTION.Left:
                newHeadPos.x -= 1;
                break;
            case DIRECTION.Down:
                newHeadPos.y -= 1;
                break;
        }

        Tile.TILE_TYPES t = grid.GetTileTypeOfPosition(newHeadPos);

        if (t == Tile.TILE_TYPES.Fruit)
        {
            EatFruit();
        }
        else if (t == Tile.TILE_TYPES.Wall || CheckLocation(newHeadPos))
        {
            // death!;
            //Debug.Log("GameOver");
            return true;
        }

        headDir = dir;
        MoveSnake(newHeadPos);
        MoveSnakeVisuals(dir);

        return false;
    }

    void MoveSnake(Vector2Int newHeadPos)
    {
        if (bodyPos.Count > 0)
        {
            for (int i = bodyPos.Count - 1; i > 0; i--)
            {
                bodyPos[i] = bodyPos[i - 1];
            }
            bodyPos[0] = headPos;
        }
        headPos = newHeadPos;
    }

    protected virtual void MoveSnakeVisuals(DIRECTION dir)
    {
    }

    public void EatFruit()
    {
        bodyPos.Add(new Vector2Int());
        grid.EatFruit();
        EatFruitVisuals();
    }

    protected virtual void EatFruitVisuals()
    {
    }

    public bool CheckLocation(Vector2Int toCheck)
    {
        return headPos == toCheck || bodyPos.Contains(toCheck);
    }

    public int GetSnakeLength()
    {
        return bodyPos.Count + 1;
    }

    public virtual void DestroyAllObjects()
    {
        Destroy(this.gameObject);
    }

    public DIRECTION GetHeadDir()
    {
        return headDir;
    }

    public Vector2Int GetSnakeHeadPos()
    {
        return headPos;
    }
}
