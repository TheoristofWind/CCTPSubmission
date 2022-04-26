using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualTile : Tile 
{
    [SerializeField] private Sprite emptySprite;
    [SerializeField] private Sprite wallSprite;
    [SerializeField] private Sprite fruitSprite;
    [SerializeField] private SpriteRenderer tileSprite;

    public override void SetUpTile(TILE_TYPES _type)
    {
        type = _type;

        if (type == TILE_TYPES.Wall)
        {
            tileSprite.sprite = wallSprite;
        }
    }

    public override void PlaceFruit()
    {
        type = TILE_TYPES.Fruit;
        tileSprite.sprite = fruitSprite;
    }

    public override void FruitEaten()
    {
        type = TILE_TYPES.Empty;
        tileSprite.sprite = emptySprite;
    }
}
