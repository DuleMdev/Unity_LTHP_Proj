using UnityEngine;
using System.Collections;

public class SwitchPicture : MonoBehaviour {

    public Sprite[] sprites;

    SpriteRenderer spriteRenderer;

	// Use this for initialization
	void Awake () {
        spriteRenderer = GetComponent<SpriteRenderer>();

        ChangeSprite(false);
	}

    public void ChangeSprite(bool b) {
        if (!b)
            spriteRenderer.sprite = sprites[0];
        else
            spriteRenderer.sprite = sprites[1];
    }

    /// <summary>
    /// Visszaadja a második szörnyet a tömbből
    /// </summary>
    /// <returns></returns>
    public Sprite GetMonster() {
        if (sprites.Length > 0)
            return sprites[1];
        else
            return null;
    }
}
