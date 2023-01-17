using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// parent transform-ot megjegyezzük, hogy tudjuk hová kell visszarakni az elemet ha helyre kell tenni.
/// 
/// Mozgatás közben egy másik elemre tesszük rá, hogy minden felett legyen az elem.
/// </summary>

public class CharacterSelectDrag : MonoBehaviour
{
    RectTransform initialParentTransform;
    Image imageAvatar;

    Transform hold;

    bool _dragEnabled;

    public bool dragEnabled
    {
        get { return _dragEnabled || transform.parent != initialParentTransform; }
        set { _dragEnabled = value; }
    }

    public CharacterData characterData { get; private set; }

    // Start is called before the first frame update
    void Awake()
    {
        initialParentTransform = (RectTransform)transform.parent;
        imageAvatar = gameObject.SearchChild("ImageAvatar").GetComponent<Image>();
    }

    public void Initialize(CharacterData characterData, Transform hold)
    {
        this.characterData = characterData;
        this.hold = hold;

        EmailGroupPictureController.instance.GetPictureFromUploadsDir(characterData.image, 
            (Sprite sprite) => 
            { 
                imageAvatar.sprite = sprite;
                imageAvatar.GetComponent<RectTransform>().sizeDelta = new Vector2(sprite.texture.width, sprite.texture.height);
            });
    }

    public void SetDragPos(Vector3 grabWorldPos)
    {
        transform.SetParent(hold);
        transform.position = grabWorldPos;
    }

    public void GotoInitialPos()
    {
        transform.SetParent(initialParentTransform);
        transform.localPosition = Vector3.zero;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
