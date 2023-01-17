using UnityEngine;
using System.Collections;

public class KeyButton : MonoBehaviour {

    TextMesh textMeshLetterChar;
    BoxCollider2D boxCollider;

    public delegate void ButtonClick(KeyButton keyButton);

    public ButtonClick buttonClick;

    Color color;

    public string key { get { return textMeshLetterChar.text; } }

    // Use this for initialization
    void Awake () {
        textMeshLetterChar = Common.SearchGameObject(gameObject, "LetterChar").GetComponent<TextMesh>();
        boxCollider = GetComponent<BoxCollider2D>();
        color = textMeshLetterChar.color;
    }

    public void Init(string letter) {
        textMeshLetterChar.text = letter;
    }

    public void SetColor(Color color) {
        this.color = color;
        textMeshLetterChar.color = color;
    }


    /// <summary>
    /// A gomb áttetszővé válik és nem lehet a továbbiakban rákattintani.
    /// </summary>
    public void Deactivate() {
        SetColor(new Color(1, 1, 1, 0.5f)); // Félig áttetsző fehér
        boxCollider.enabled = false;
    }

    public void Flashing()
    {
        StartCoroutine(FlashingCoroutine(Color.red));
    }

    IEnumerator FlashingCoroutine(Color color)
    {
        for (int i = 0; i < 3; i++)
        {
            textMeshLetterChar.color = color;
            yield return new WaitForSeconds(0.2f);

            textMeshLetterChar.color = this.color;
            yield return new WaitForSeconds(0.2f);
        }
    }

    void OnMouseDown()
    {
        if (buttonClick != null)
            buttonClick(this);
    }
}
