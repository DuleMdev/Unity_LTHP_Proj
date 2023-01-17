using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelEmailGroupBigList : MonoBehaviour
{
    PanelEmailGroupBig prefabEmailGroupBig;

    // Start is called before the first frame update
    void Awake()
    {
        prefabEmailGroupBig = GetComponentInChildren<PanelEmailGroupBig>(true);

        prefabEmailGroupBig.gameObject.SetActive(false);
    }



    /// <summary>
    /// Létrehozza az email csoportokat a lista szerint.
    /// A visszaadott érték a létrehozott elemek összes magassága.
    /// </summary>
    /// <param name="list"></param>
    /// <param name="buttonNamePrefix">Az index a listában</param>
    /// <param name="callBack"></param>
    /// <returns></returns>
    public float Initialize(List<EmailGroup> list, string buttonNamePrefix, Common.CallBack_In_String callBack)
    {
        float margo = -50;

        Vector2 prefabSize = ((RectTransform)prefabEmailGroupBig.transform).sizeDelta;
        float fullSize = ((RectTransform)transform).rect.width;

        float basePosX1 = fullSize * 0.5f - prefabSize.x;
        float basePosX2 = basePosX1 + prefabSize.x;

        for (int i = 0; i < list.Count; i++)
        {
            // Egy lista elem létrehozása
            PanelEmailGroupBig newPrefabEmailGroupBig = Instantiate(prefabEmailGroupBig, transform).GetComponent<PanelEmailGroupBig>();
            newPrefabEmailGroupBig.gameObject.SetActive(true);

            newPrefabEmailGroupBig.Initialize(list[i], buttonNamePrefix, callBack);

            RectTransform newPrefabEmailGroupBigTransform = newPrefabEmailGroupBig.GetComponent<RectTransform>();
            newPrefabEmailGroupBigTransform.anchoredPosition = new Vector2(i % 2 == 0 ? basePosX1 : basePosX2, margo - ((i / 2) * prefabSize.y));
        }

        return 2 * -margo + ((list.Count + 1) / 2) * prefabSize.y;
    }

    // Update is called once per frame
    void Update()
    {

    }
}