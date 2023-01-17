using UnityEngine;
using System.Collections;

/// <summary>
/// A játékok egy képi világát tartalmazza.
/// Egy játékhoz tartozhat több képi világ is.
/// 
/// A megfelelőt a layoutManager segítségével lehet kiválasztani.
/// 
/// Ez az szkript csak annyit tesz, hogy amelyik game objekten elhelyezkedik megkeresi az alatta levő SpriteRenderer komponenseket
/// és elhelyezi őket egy többen.
/// Majd ha szükség van valamelyik képre akkor a név alapján megkeresi és visszaadja.
/// </summary>
public class LayoutSet : MonoBehaviour {

    SpriteRenderer[] spriteRenderers;

	void Awake () {
        // Megkeressük a gameObject-en található SpriteRenderer komponenseket
        spriteRenderers = GetComponentsInChildren<SpriteRenderer>(true);
	}

    public Sprite GetSprite(string name) {
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer.name == name)
                return spriteRenderer.sprite;
        }

        return null;
    }

    public Color GetColor(string name)
    {
        foreach (SpriteRenderer spriteRenderer in spriteRenderers)
        {
            if (spriteRenderer.name == name)
                return spriteRenderer.color;
        }

        return Color.white;
    }
}
