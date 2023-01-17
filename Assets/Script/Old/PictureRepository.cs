using UnityEngine;
using System.Collections;

public class PictureRepository : MonoBehaviour {

    public Texture2D[] pictures;

	// Use this for initialization
	void Awake () {
        Common.pictureRepository = this;
        Debug.Log(pictures[0].name);
    }

    // Megkeresi a képnevet a képraktárban és vissza adja
    // Ha nincs ilyen nevű kép a raktárban, akkor null értéket ad vissza
    public Texture2D GetPicture(string name) {

        foreach (Texture2D texture in pictures)
            if (texture != null && texture.name == name)
                return texture;

        return null;
    }
}
