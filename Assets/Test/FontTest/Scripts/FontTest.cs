using System.Collections;
using System.Collections.Generic;
using System.IO;
using TTF_Font;
using UnityEngine;

/*
cmap pos : 53700

három tábla van benne tábla
1. offset 28 = 53728
    4. formátum    
    hossz : 2852
    SegCount : 284 = 142


2. offset 2880 = 56580
    0. formátum
    hossz : 262


3. offset 3142 = 
    4. formátum
    hossz : 2852



    59696

*/



public class FontTest : MonoBehaviour
{
    public Font[] font;

    int fontIndex;

    TextMesh textMesh;

    //string fontFile = @"H:\Work\ClassY\Unity\git\ClassY\Assets\TEXDraw\Fonts\User\arial.ttf";
    string fontFile = @"C:\Users\Tipcike\Desktop\arial.ttf";

    string abc =
        "árvíztűrőtükörfúrógépÁRVÍZTŰRŐTÜKÖRFÚRÓGÉP" + // Magyar
        "aăâbcdefghiîjklmnopqrsștțuvwxyzAĂÂBCDEFGHIÎJKLMNOPQRSȘTȚUVWXYZ" + // Román
        "aáäbcčdďeéfghiíjklĺľmnňoóôpqrŕsštťuúvwxyýzžAÁÄBCČDĎDzDžEÉFGHChIÍJKLĹĽMNŇOÓÔPQRŔSŠTŤUÚVWXYÝZŽ" + // Szlovák
        "αβγδεζηθικλμνξοπρσςτυφχψωΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣϚΤΥΦΧΨΩ" + // Görög
        "abcdefghijklmnñopqrstuvwxyzABCDEFGHIJKLMNÑOPQRSTUVWXYZ" + // Spanyol
        "abcčćddžđefghijklljmnnjoprsštuvzžABCČĆDDžĐEFGHIJKLLjMNNjOPRSŠTUVZŽ" + // Horváth
        "aąbcčdeęėfghiįyjklmnoprsštuųūvzžAĄBCČDEĘĖFGHIĮYJKLMNOPRSŠTUŲŪVZŽ"; // Litván


    // Start is called before the first frame update
    void Awake()
    {
        textMesh = GetComponent<TextMesh>();
        fontIndex = 0;

        FontVerify(fontFile, abc);
        
        string result = "";
        //foreach (var item in FileSearch(@"H:\Work\ClassY\Unity\git\ClassY\Assets\Font", "*.ttf"))
        foreach (var item in FileSearch(@"J:\Fonts", "*.ttf"))
        {
            string s = FontVerify(item, abc);
            result += item + "\n" + s + "\n";
        }

        File.WriteAllText(@"J:\FontDatas", result);

        Debug.Log(result);

        WriteCodes();
    }

    List<string> FileSearch(string sDir, string searchPattern)
    {
        List<string> filePaths = new List<string>();

        try
        {
            foreach (string f in Directory.GetFiles(sDir, searchPattern))
            {
                filePaths.Add(f);
            }

            foreach (string d in Directory.GetDirectories(sDir))
            {
                filePaths.AddRange(FileSearch(d, searchPattern));
            }
        }
        catch (System.Exception excpt)
        {
            Debug.Log(excpt.Message);
        }

        return filePaths;
    }




    /// <summary>
    /// A metódus megvizsgálja, hogy a megadott szövegben található karakterek megtalálhatóak-e a font file-ban
    /// </summary>
    /// <param name="fontFileName"></param>
    /// <param name="chars"></param>
    string FontVerify(string fontFileName, string chars)
    {
        Debug.Log(fontFileName);

        TTF_FontProcessor fontData = new TTF_FontProcessor(fontFileName);

        Debug.Log(fontData.fontDirectory.numTables);

        foreach (var item in fontData.fontDirectory.listOfFontDirectoryEntries)
        {
            string s = item.tag + "\n" +
                "checksum : " + item.checkSum + "\n" +
                "offset : " + item.offset + "\n" +
                "length : " + item.length;

            Debug.Log(s);
        }

        foreach (var item in fontData.table_cmap.listOfSubtable)
        {
            string s = item.platformID + " " + item.platformIDenum + "\n";

            switch (item.platformID)
            {
                case 0:
                    s += item.platformSpecificID + " " + item.unicodePlatformSpecificsIDenum + "\n";
                    break;
                case 1:
                    s += item.platformSpecificID + "\n";
                    break;
                case 3:
                    s += item.platformSpecificID + " " + item.windowsPlatformSpeecificsIDenum + "\n";
                    break;
            }

            s += "offset : " + item.offset + "\n";

            if (item.format == 4)
            {
                format4 f = (format4)item.format_;

                for (int i = 0; i < f.segCountX2 / 2; i++)
                {
                    s += string.Format("{0} - {1} : delta {2,6:00000} : offset {3,6:00000}\n", f.startCode[i], f.endCode[i], f.idDelta[i], f.idRangeOffset[i]);

                    //f.startCode[i] + " - " + f.endCode[i] + " : delta - " + f.idDelta[i] + " : offset - " + f.idRangeOffset[i] + "\n";
                }
            }

            Debug.Log(s);
        }

        string returnValue = null;
        for (int i = 0; i < fontData.table_cmap.numberSubtables; i++)
        {
            if (fontData.table_cmap.listOfSubtable[i].format == 4)
            {
                string result = ((format4)fontData.table_cmap.listOfSubtable[i].format_).ContainStringTest(chars);
                Debug.Log("Hiányzó karakterek : " + result);
                Debug.Log(CharactersProcessor(result));

                returnValue += i + ". tábla\nHiányzó karakterek : " + result + "\n" + CharactersProcessor(result) + "\n";
            }
        }

        return returnValue == null ? "Nincsen 4. típuső tábla" : returnValue;
    }

    void OnGUI()
    {
        if (GUI.Button(new Rect(10, 10, 150, 50), "Next font"))
        {
            fontIndex++;

            if (fontIndex == font.Length)
                fontIndex = 0;

            textMesh.font = font[fontIndex];
            textMesh.text = textMesh.text + ".";
        }

        GUI.Label(new Rect(10, 70, 300, 50), font[fontIndex].name);
    }

    // Update is called once per frame
    void Update()
    {
        
    }



    void WriteCodes()
    {
        Debug.Log("Hungarian character set:\n" + CharactersProcessor("árvíztűrőtükörfúrógépÁRVÍZTŰRŐTÜKÖRFÚRÓGÉP"));
        Debug.Log("Román character set:\n" + CharactersProcessor("aăâbcdefghiîjklmnopqrsștțuvwxyzAĂÂBCDEFGHIÎJKLMNOPQRSȘTȚUVWXYZ"));
        Debug.Log("Szlovák character set:\n" + CharactersProcessor("aáäbcčdďeéfghiíjklĺľmnňoóôpqrŕsštťuúvwxyýzžAÁÄBCČDĎDzDžEÉFGHChIÍJKLĹĽMNŇOÓÔPQRŔSŠTŤUÚVWXYÝZŽ"));
        Debug.Log("Görög character set:\n" + CharactersProcessor("αβγδεζηθικλμνξοπρσςτυφχψωΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣϚΤΥΦΧΨΩ"));
        Debug.Log("Spanyol character set:\n" + CharactersProcessor("abcdefghijklmnñopqrstuvwxyzABCDEFGHIJKLMNÑOPQRSTUVWXYZ"));
        Debug.Log("Horváth character set:\n" + CharactersProcessor("abcčćddžđefghijklljmnnjoprsštuvzžABCČĆDDžĐEFGHIJKLLjMNNjOPRSŠTUVZŽ"));
        Debug.Log("Litván character set:\n" + CharactersProcessor("aąbcčdeęėfghiįyjklmnoprsštuųūvzžAĄBCČDEĘĖFGHIĮYJKLMNOPRSŠTUŲŪVZŽ"));



        string s = "aąbcčdeęėfghiįyjklmnoprsštuųūvzžAĄBCČDEĘĖFGHIĮYJKLMNOPRSŠTUŲŪVZŽ";
        Debug.Log(s.ToUpper());

        // Croation   - ABCČĆDDžĐEFGHIJKLLjMNNjOPRSŠTUVZŽ
        // Litván - aąbcčdeęėfghiįyjklmnoprsštuųūvzž

        /*
                árvíztűrő tükörfúrógép
        ÁRVÍZTŰRŐ TÜKÖRFÚRÓGÉP
        Román: aăâbcdefghiîjklmnopqrsștțuvwxyz
        AĂÂBCDEFGHIÎJKLMNOPQRSȘTȚUVWXYZ
        Szlovák: aáäbcčdďeéfghiíjklĺľmnňoóôpqrŕsštťuúvwxyýzž
        AÁÄBCČDĎDzDžEÉFGHChIÍJKLĹĽMNŇOÓÔPQRŔSŠTŤUÚVWXYÝZŽ
        Görög: αβγδεζηθικλμνξοπρσςτυφχψω
        ΑΒΓΔΕΖΗΘΙΚΛΜΝΞΟΠΡΣϚΤΥΦΧΨΩ
        Spanyol: abcdefghijklmnñopqrstuvwxyz
        ABCDEFGHIJKLMNÑOPQRSTUVWXYZ
        */
    }


    string CharactersProcessor(string text)
    {
        Dictionary<CharacterCodeClass, string> dictionary = new Dictionary<CharacterCodeClass, string>();

        foreach (char c in text)
        {
            CharacterCodeClass codeClass = CharacterCodeClasses.GetCharacterCodeClass(c);

            // Lekérdezzük az eddig tárolt karaktereket
            string chars = dictionary.ContainsKey(codeClass) ? dictionary[codeClass] : "";

            // Ha a tárolni kívánt karakter nincs a string-ben, akkor hozzáadjuk
            if (chars.IndexOf(c) < 0)
                chars += c;

            if (dictionary.ContainsKey(codeClass))
                dictionary[codeClass] = chars;
            else
                dictionary.Add(codeClass, chars);
        }

        string resultText = "";

        List<CharacterCodeClass> list = new List<CharacterCodeClass>(dictionary.Keys);
        list.Sort(delegate (CharacterCodeClass x, CharacterCodeClass y)
            {
                return x.name.CompareTo(y.name);
            });

        foreach (var codeClass in list) // dictionary.Keys.Sort())
        {
            string result = "";

            foreach (char c in dictionary[codeClass])
            {
                if (!string.IsNullOrEmpty(result))
                    result += ", ";

                result += string.Format("{0} {1:X}", c, (int)c);
            }

            resultText += "\n" + codeClass.getName + "\n" + result + "\n";
        }

        return resultText;
    }





}
