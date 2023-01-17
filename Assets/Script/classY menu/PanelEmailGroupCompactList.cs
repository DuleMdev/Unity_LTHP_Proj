using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelEmailGroupCompactList : MonoBehaviour
{
    PanelEmailGroupCompact prefabEmailGroupCompact;

    // Start is called before the first frame update
    void Awake()
    {
        prefabEmailGroupCompact = GetComponentInChildren<PanelEmailGroupCompact>(true);

        prefabEmailGroupCompact.gameObject.SetActive(false);
    }

    /// <summary>
    /// Létrehozza az email csoportokat a lista szerint.
    /// A visszaadott érték a létrehozott elemek összes magassága.
    /// </summary>
    /// <param name="list"></param>
    /// <param name="callBack"></param>
    /// <returns></returns>
    public float Initialize(List<EmailGroup> list, string buttonNamePrefix, Common.CallBack_In_String callBack)
    {
        float posY = 0;

        for (int i = 0; i < list.Count; i++)
        {
            // Egy lista elem létrehozása
            PanelEmailGroupCompact newPrefabEmailGroupCompact = Instantiate(prefabEmailGroupCompact, transform).GetComponent<PanelEmailGroupCompact>();
            newPrefabEmailGroupCompact.gameObject.SetActive(true);

            newPrefabEmailGroupCompact.Initialize(list[i], buttonNamePrefix, i % 2 == 1, callBack);

            RectTransform newPrefabEmailGroupCompactTransform = newPrefabEmailGroupCompact.GetComponent<RectTransform>();
            newPrefabEmailGroupCompactTransform.anchoredPosition = new Vector2(0, posY);

            posY -= newPrefabEmailGroupCompactTransform.sizeDelta.y;
        }

        // Beállítjuk a panel méretét a létrehozott elemek méretére

        // 
        return -posY;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
