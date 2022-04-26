using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public enum TILE_TYPES {
        Empty,
        Wall,
        Fruit
    }
    
    protected TILE_TYPES type;

    public virtual void SetUpTile(TILE_TYPES _type)
    {
        type = _type;
    }

    public TILE_TYPES GetType()
    {
        return type;
    }

    public virtual void PlaceFruit()
    {
        type = TILE_TYPES.Fruit;
    }

    public virtual void FruitEaten()
    {
        type = TILE_TYPES.Empty;
    }
}
