using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Arranger
{
    public Vector2 availableArea;
    
    public int line { get; private set; }

    List<MonoBehaviour> listItems;  // Mely elemeket kell elrendezni
    public List<Vector2> listItemsPos;     // Hová kell őket elhelyezni
    public float ratio;         // Hogyan kell méretezni a tartalmazó objektumot

    float spaceBetweenItems;        // Az elemek között mekkora távolságot kell tartani (vízszintes)
    float spaceBetweenLines;        // A sorok között mekkora távolságot kell tartani (függőleges)

    public Arranger(List<MonoBehaviour> listItems, float spaceBetweenItems, float spaceBetweenLines)
    {
        this.listItems = listItems;
        this.spaceBetweenItems = spaceBetweenItems;
        this.spaceBetweenLines = spaceBetweenLines;
    }

    /// <summary>
    /// Az elemeket elrendezi. Ha a smootMove igaz, akkor animálva mennek az új helyükre, ha hamis, akkor a hívás után azonnal a helyükön lesznek.
    /// </summary>
    /// <param name="smoothMove"></param>
    public void Arrange(Vector2 availableArea, bool smoothMove)
    {
        this.availableArea = availableArea;

        ratio = 0;
        float tryRatio = 1;
        float ratioChange = 1;
        do
        {
            ratioChange /= 2;
            if (TryArrange(1 / tryRatio))
            {
                ratio = tryRatio;
                tryRatio += ratioChange;
            }
            else
            {
                tryRatio -= ratioChange;
            }

        } while (ratio < 1 && ratioChange > 0.000001);

        for (int i = 0; i < listItemsPos.Count; i++)
        {
            IWidthHeight size = ((IWidthHeight)listItems[i]);
            if (size != null)
                Debug.LogWarningFormat("{0} - x : {1} , y : {2}", (listItems[i] == null) ? "null" : listItems[i].name, size.GetWidth(), size.GetHeight());
        }

        Debug.LogWarning(ratio);

        if (smoothMove)
            PosSmoothMove();
        else
            PosImediatelly();
    }

    void PosImediatelly()
    {
        for (int i = 0; i < listItems.Count; i++)
            if (listItems[i] != null)
                listItems[i].transform.localPosition = new Vector3(listItemsPos[i].x, listItemsPos[i].y) / listItems[i].transform.parent.lossyScale.x;
        //listItems[i].transform.position = listItems[i].transform.parent.position + new Vector3(listItemsPos[i].x, listItemsPos[i].y);
    }

    void PosSmoothMove()
    {
        for (int i = 0; i < listItems.Count; i++)
            if (listItems[i] != null)
                iTween.MoveTo(listItems[i].gameObject, iTween.Hash("position", new Vector3(listItemsPos[i].x, listItemsPos[i].y) / listItems[i].transform.parent.lossyScale.x, "islocal", true, "easetype", iTween.EaseType.easeOutCubic, "time", 1));


        //listItems[i].transform.position = listItems[i].transform.parent.position + new Vector3(listItemsPos[i].x, listItemsPos[i].y);
    }

    public Vector2 GetPos(MonoBehaviour o) {
        for (int i = 0; i < listItems.Count; i++)
        {
            if (listItems[i] == o)
                return listItemsPos[i];
        }

        return Vector2.zero;
    }

    public int GetIndex(MonoBehaviour o)
    {
        for (int i = 0; i < listItems.Count; i++)
        {
            if (listItems[i] == o)
                return i;
        }

        return -1;
    }

    /// <summary>
    /// A megadott méretben megpróbálja elhelyezni az elemeket. Igaz értéket ad vissza ha az elemek elférnek a megadott méret mellett.
    /// </summary>
    /// <param name="ratio">Az elemek mérete amit meg kell próbálni elrendezni</param>
    /// <returns></returns>
    bool TryArrange(float ratio)
    {
        bool arrangeOK = true;  // Belefért a megadott területbe?

        Vector2 availableArea = this.availableArea * ratio;

        // Létrehozzunk minden item-nek egy Vector2-t ami a pozícióját fogja mutatni
        List<Vector2> listItemsPos = new List<Vector2>();
        for (int i = 0; i < listItems.Count; i++)
            listItemsPos.Add(new Vector2());

        int lines = 0;  // Hány sorba férnek ki a megadott elemek (sorszámláló)

        // Annyi elemet pakolunk egy sorba amennyi belefér vagy nem jön egy null elem vagy nem ürül ki az item lista

        int startIndex = 0; // Hol kezdődik az aktuális sor

        float yPos = 0;

        float maxHeight = 0;

        float firstRowHeight = 0;

        while (true)
        {
            // Ha null elem lenne a sor elején, akkor azt átugrorjuk
            while (startIndex < listItems.Count && listItems[startIndex] == null)
                startIndex++;

            // Ha elfogytak az elemek akkor kilépünk a ciklusból
            if (startIndex == listItems.Count)
                break;

            lines++; // Növeljük a sorok számát

            // Ha nem az első sor, akkor hozzáadjuk a sorok közti távolságot is
            yPos -= (yPos == 0) ? 0 : spaceBetweenLines;

            // Az első elemet mindenféleképpen hozzáadjuk a sorhoz
            float lineWidth = ((IWidthHeight)listItems[startIndex]).GetWidth(); // A sor szélessége
            listItemsPos[startIndex] = new Vector2(lineWidth / 2, yPos);
            int itemCount = 1;  // Hány elem van az aktuális sorba
            maxHeight = ((IWidthHeight)listItems[startIndex]).GetHeight();

            // Ha van még elem a listába és ha hozzáadjuk a sorhoz, akkor még nem fog kilógni a sorból, akkor hozzáadjuk
            while (startIndex + itemCount < listItems.Count &&  // Még nem fogytak el az elemek
                listItems[startIndex + itemCount] != null && // Null elemnél új sort kezdünk
                lineWidth + spaceBetweenItems + ((IWidthHeight)listItems[startIndex + itemCount]).GetWidth() <= availableArea.x) // Csak akkor tesszük a sorba ha belefér
            {
                float itemWidth = ((IWidthHeight)listItems[startIndex + itemCount]).GetWidth();
                lineWidth += spaceBetweenItems;
                listItemsPos[startIndex + itemCount] = new Vector2(lineWidth + itemWidth / 2, yPos);

                lineWidth += itemWidth; // ((IWidthHeight)listItems[startIndex + itemCount]).GetWidth();

                if (((IWidthHeight)listItems[startIndex + itemCount]).GetHeight() > maxHeight)
                    maxHeight = ((IWidthHeight)listItems[startIndex + itemCount]).GetHeight();

                itemCount++;
            }

            // Ha a sor hosszabb mint a rendelkezésre álló terület, akkor hiba (Ez csak úgy lehetséges, ha az első elem amit a sorba raktunk az már önmagában is hosszabb)
            if (lineWidth > availableArea.x)
                return false; // arrangeOK = false;

            // Berendezzük a sort vízszintesen
            ArrangeHorizontally(listItemsPos, startIndex, itemCount);

            if (yPos == 0)
                firstRowHeight = maxHeight;

            yPos -= maxHeight;

            startIndex += itemCount;
        }


        // Ha a szükséges hely több mint a rendelkezésre álló, akkor hiba
        if (Mathf.Abs(yPos) > availableArea.y)
            return false; //  arrangeOK = false;
        else {
            // Kiszámítjuk, hogy mennyi kell igazítani az elemek pozícióján, hogy a rendelkezésrá álló hely közepére essenek
            //float yDifferent = (availableArea.y - yPos) / 2;
            float yDifferent = yPos / 2; // (yPos - firstRowHeight) / 2;

            // Módosítjuk az elemek Y pozícióját
            for (int i = 0; i < listItemsPos.Count; i++)
            {
                Vector2 v = listItemsPos[i];
                listItemsPos[i] = new Vector2(v.x, v.y - yDifferent); // v.y - halfMaxHeight);
            }
            

            // Rögzítjük a pozíciókat
            this.listItemsPos = listItemsPos;
            this.ratio = ratio;
        }

        return true; // arrangeOK;
    }

    /// <summary>
    /// A megadott elemeket egy sorban helyezkednek el, de középre kell igazítani őket.
    /// Középre igazítja az elemeket a sorban mind vízszintesen mind függőlegesen.
    /// </summary>
    /// <param name="startIndex"></param>
    /// <param name="itemCount"></param>
    void ArrangeHorizontally(List<Vector2> listItemsPos, int startIndex, int itemCount) {
        float lineWidth = 0; 
        float maxHeight = 0; // Ez most nem használt

        // Kiszámoljuk az elemek szélességét
        for (int i = 0; i < itemCount; i++)
        {
            lineWidth += ((IWidthHeight)listItems[startIndex + i]).GetWidth();
            if (((IWidthHeight)listItems[startIndex + i]).GetHeight() > maxHeight)
                maxHeight = ((IWidthHeight)listItems[startIndex + i]).GetHeight();
        }

        // Hozzáadjuk az elemek közti távolságot
        lineWidth += spaceBetweenItems * (itemCount - 1);

        // Kiszámoljuk, hogy mennyivel kell elmozgatni az első elemet
        // az első elem szélességének fele - teljes sor szélességének fele
        //float halfWidth = (((IWidthHeight)listItems[startIndex]).GetWidth() - lineWidth) / 2;
        //float halfWidth =  (lineWidth - ((IWidthHeight)listItems[startIndex]).GetWidth()) / 2;
        float halfWidth = lineWidth / 2;



        // Meghatározzuk a legnagyobb elem magasságának felét
        float halfMaxHeight = maxHeight / 2;

        
        // Módosítjuk az elemek pozícióját
        for (int i = 0; i < itemCount; i++)
        {
            Vector2 v = listItemsPos[startIndex + i];
            listItemsPos[startIndex + i] = new Vector2(v.x - halfWidth, v.y - halfMaxHeight);
        }
        
    }
}
