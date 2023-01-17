using UnityEngine;
using System.Collections;

using UnityEngine.UI;
using System;

public class Letter : MonoBehaviour, IWidthHeight {

    TextMesh textMeshLetterChar;
    SpriteRenderer spriteRendererBackground;
    GameObject move;

    public string letter { get; private set; }

	// Use this for initialization
	public void Awake () {
        move = Common.SearchGameObject(gameObject, "move").gameObject;
        spriteRendererBackground =  Common.SearchGameObject(gameObject, "background").GetComponent<SpriteRenderer>();
        textMeshLetterChar = Common.SearchGameObject(gameObject, "LetterChar").GetComponent<TextMesh>();
	}

    public void Init(string letter) {
        this.letter = letter;
        SetSize(0.001f);
        textMeshLetterChar.text = "";
    }

    // Beállítja az elem méretét a megadott méretűre
    public void SetSize(float size) {
        move.transform.localScale = Vector3.one * size;
    }

    /// <summary>
    /// Össze zsugorítjuk az elemet egy másodperc alatt
    /// </summary>
    public void Hide() {
        StartCoroutine(HideCoroutine());
        //iTween.ScaleTo(move, Vector3.one * 0.001f, 1);
    }

    public IEnumerator HideCoroutine() {
        iTween.ScaleTo(move, Vector3.one * 0.001f, 1);
        yield return new WaitForSeconds(1);
        Destroy(gameObject);
    }

    /// <summary>
    /// Előugrik az elem a megadott másodperc alatt
    /// </summary>
    /// <remarks>
    /// Alap esetben az animációs idő 1 másodperc.
    /// Viszont preview nézet esetében 0 másodperc lesz.
    /// </remarks>
    public void Show(float animTime = 1) {
        spriteRendererBackground.sortingOrder = 0;
        if (animTime > 0)
            iTween.ScaleTo(move, iTween.Hash("islocal", true, "scale", Vector3.one, "time", animTime, "easeType", iTween.EaseType.easeOutElastic));
        else
            move.transform.localScale = Vector3.one;
    }

    /// <summary>
    /// Láthatóvá teszi a karaktert.
    /// </summary>
    public void ShowText() {
        textMeshLetterChar.text = letter.ToString();
    }

    /// <summary>
    /// Beállítjuk, hogy a háttér látszódjon-e vagy sem.
    /// </summary>
    /// <param name="enabled"></param>
    public void SetBackground(bool enabled) {
        spriteRendererBackground.enabled = enabled;
    }

    public void Flashing()
    {
        StartCoroutine(FlashingCoroutine(Color.green));
    }

    IEnumerator FlashingCoroutine(Color color)
    {
        for (int i = 0; i < 3; i++)
        {
            yield return new WaitForSeconds(0.2f);

            textMeshLetterChar.color = color;

            yield return new WaitForSeconds(0.2f);

            textMeshLetterChar.color = Color.white;
        }
    }

    public void SetPictures(Sprite background, Color backgroundColor) {
        spriteRendererBackground.sprite = background;
        spriteRendererBackground.color = backgroundColor;
    }

    public float GetHeight() {
        return spriteRendererBackground.GetComponent<Renderer>().bounds.size.y;
    }

    public float GetWidth() {
        return spriteRendererBackground.GetComponent<Renderer>().bounds.size.x;
    }
}
