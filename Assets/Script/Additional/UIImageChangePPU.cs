using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Egy Image komponensbe tett képnek a pixel per unit értékét kicseréli a megadottra.
/// </summary>
public class UIImageChangePPU : MonoBehaviour
{
    [Tooltip("Az új pixel per unit érték")]
    public float newPixelPerUnit;

    Image image;


    // Start is called before the first frame update
    void Awake()
    {
        image = GetComponent<Image>();
        RefreshPixelPerUnit();
    }

    public void RefreshPixelPerUnit(float? newPixelPerUnit = null, Sprite newSprite = null)
    {
        if (newPixelPerUnit != null)
            this.newPixelPerUnit = newPixelPerUnit.Value;

        //image.sprite
        Sprite sprite = image.sprite;

        if (newSprite != null)
            sprite = newSprite;

        if (sprite != null)
        {
            image.sprite = Sprite.Create(sprite.texture, sprite.rect, sprite.pivot, this.newPixelPerUnit, 0, SpriteMeshType.FullRect, sprite.border);
        }
    } 

    // Update is called once per frame
    void Update()
    {
        
    }
}
