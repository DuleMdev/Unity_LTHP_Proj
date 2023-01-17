using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FootPrint : MonoBehaviour
{
    SpriteRenderer sprite;

    // Start is called before the first frame update
    void Awake()
    {
        sprite = GetComponent<SpriteRenderer>();
    }

    public void Initialize(Vector3 pos, Quaternion rotation, Color color, bool right)
    {
        transform.position = pos;
        transform.rotation = rotation;
        SetColor(color);
        SetSide(right);
    }

    public void SetColor(Color color)
    {
        sprite.color = color;
    }

    public void SetSide(bool right)
    {
        sprite.flipX = right;
    }
}
