using System.Collections;
//using System;
using System.Collections.Generic;
using UnityEngine;

public class Grid : MonoBehaviour
{
    [SerializeField] private int rows;
    [SerializeField] private int collums;
    [SerializeField] private float tileWidth;
    [SerializeField] private float tileHeight;

    [SerializeField] private GameObject tilePrefab;
    [SerializeField] private GameObject snakePrefab;

    private Dictionary<Vector2Int, Tile> tiles;
    private List<Vector2Int> possibleFruitLocations;
    private Vector2Int currentFruitLocation;

    private Snake snake;

    // Start is called before the first frame update
    void Awake()
    {
        CreateGrid();
    }

    void CreateGrid()
    {
        tiles = new Dictionary<Vector2Int, Tile>();
        possibleFruitLocations = new List<Vector2Int>();
        
        for (int x = 0; x < collums; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                GameObject t = Instantiate(tilePrefab, transform);
                
                Vector2Int pos = new Vector2Int(x, y);

                tiles[pos] = t.GetComponent<Tile>();
                tiles[pos].SetUpTile((x != 0 && y != 0 && x != collums-1 && y != rows -1) ? Tile.TILE_TYPES.Empty : Tile.TILE_TYPES.Wall);

                t.transform.position = new Vector3(pos.x * tileWidth, pos.y * tileHeight, 0);

                if (tiles[pos].GetType() != Tile.TILE_TYPES.Wall)
                {
                    possibleFruitLocations.Add(pos);
                }
            }
        }
        SpawnSnake();
        SpawnFruit(new Vector2Int(-1, -1));
    }

    void SpawnFruit(Vector2Int noSpawn, bool skipFirstCheck = false)
    {
        if (skipFirstCheck || possibleFruitLocations.Count > snake.GetSnakeLength())
        {
            int r = UnityEngine.Random.Range(0, possibleFruitLocations.Count);
            currentFruitLocation = possibleFruitLocations[r];

            if (snake.CheckLocation(currentFruitLocation) || currentFruitLocation == noSpawn)
            {
                SpawnFruit(noSpawn, true);
                return;
            }

            tiles[currentFruitLocation].PlaceFruit();
            return;
        }
        Debug.Log("CANNOT SPAWN");

    }

    public void SpawnSnake()
    {
        if (possibleFruitLocations.Count > 0)
        {
            int r = UnityEngine.Random.Range(0, possibleFruitLocations.Count);
            Vector2Int snakePos = possibleFruitLocations[r];

            GameObject s = Instantiate(snakePrefab);
            s.transform.position = new Vector3(snakePos.x * tileWidth, snakePos.y * tileHeight, 0);

            snake = s.GetComponent<Snake>().SetUp(snakePos, Snake.DIRECTION.Up, this);
        }
    }

    public Tile.TILE_TYPES GetTileTypeOfPosition(Vector2Int pos)
    {
        if (!tiles.ContainsKey(pos)) return Tile.TILE_TYPES.Wall;

        return tiles[pos].GetType();
    }

    public void EatFruit()
    {
        tiles[currentFruitLocation].FruitEaten();
        SpawnFruit(currentFruitLocation);
    }

    public Snake GetSnake()
    {
        return snake;
    }

    public Snake ResetGame()
    {
        snake.DestroyAllObjects();
        EatFruit();
        SpawnSnake();
        return snake;
    }

    public Vector2Int GetFruitLocation()
    {
        return currentFruitLocation;
    }
}
