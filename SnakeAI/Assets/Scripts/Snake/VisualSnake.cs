using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VisualSnake : Snake
{
    [SerializeField] private GameObject bodyPrefab;

    //[SerializeField] private Transform head;
    public Transform head;

    private List<Transform> bodyParts = new List<Transform>();

    protected override void SetUpVisuals()
    {
        MoveSnakeVisuals(Snake.DIRECTION.Up);
    }

    protected override void MoveSnakeVisuals(Snake.DIRECTION dir)
    {
        head.position = new Vector3(headPos.x, headPos.y, 0);
        for (int i = 0; i < bodyParts.Count; i++)
        {
            bodyParts[i].position = new Vector3(bodyPos[i].x, bodyPos[i].y, 0);
        }
        float rot = dir == Snake.DIRECTION.Up ? 0 : dir == Snake.DIRECTION.Down ? 180 : dir == Snake.DIRECTION.Left ? 90 : -90;

        head.rotation = Quaternion.Euler(0, 0, rot);
    }

    protected override void EatFruitVisuals()
    {
        GameObject b = Instantiate(bodyPrefab, transform);
        b.GetComponent<SpriteRenderer>().color = new Color(10.0f / (bodyParts.Count + 10), 10.0f / (bodyParts.Count + 10), 10.0f / (bodyParts.Count + 10), 1);
        b.GetComponent<SpriteRenderer>().sortingOrder = 100-bodyParts.Count;
        bodyParts.Add(b.transform);
    }

    public override void DestroyAllObjects()
    {
        for (int i = 0; i < bodyParts.Count; i++)
        {
            Destroy(bodyParts[i].gameObject);
        }
        Destroy(this.gameObject);
    }
}
