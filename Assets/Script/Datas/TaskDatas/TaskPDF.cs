using SimpleJSON;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*

    {
        "pdf" : "itt van a pdf file base64 kódolással"
    }

*/
public class TaskPDF : TaskAncestor {

    public byte[] pdfData;

    Dictionary<int, Get_PDF_Links.PageData> pdf_links;

    // A megadott JSON alapján inicializálja a változóit
    public TaskPDF(JSONNode jsonNode)
    {
        taskType = TaskType.PDF;
        InitDatas(jsonNode);
    }

    override protected void InitTaskDatas(JSONNode node)
    {
        error = false;

        id = "-1";
        time = 0; //  Végtelen idő
        pdfData = System.Convert.FromBase64String(node[C.JSONKeys.pdf].Value);

        
#if UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
        if (Common.configurationController.savePDF)
            System.IO.File.WriteAllBytes(@"d:\" + System.DateTime.Now.ToString("yyyy.MM.dd-HH mm ss") + ".pdf", pdfData);
#endif

        pdf_links = Get_PDF_Links.GetPDFLinks(pdfData);

        // Kiírjuk a PDF-ben található linkek számát
        string s = "Talált linkek\n";
        foreach (var item in pdf_links)
        {
            s += (item.Key + 1) + ". oldalon : " + item.Value.listOfLink.Count + " db\n";
        }
        Debug.Log(s);

        /*
        new Dictionary<int, PageData>();

        for (int i = 0; i < node["pdfLinks"].Count; i++)
        {
            PageData pageData = new PageData(node["pdfLinks"][i]);

            if (pdf_links.ContainsKey(pageData.pageNumber))
                pdf_links[pageData.pageNumber].listOfLink.AddRange(pageData.listOfLink);
            else
                pdf_links.Add(pageData.pageNumber, pageData);
        }
        */
    }

    public string getUrl(int pageNumber, Vector2 normalizedPoint)
    {
        // Ha van ilyen oldal, akkor ...
        if (pdf_links.ContainsKey(pageNumber))
        {
            Get_PDF_Links.PageData pageData = pdf_links[pageNumber];

            // Kiszámoljuk az oldalon a megadott pozíció helyét
            Vector2 position = new Vector2(
                normalizedPoint.x * pageData.pageSize.x,
                normalizedPoint.y * pageData.pageSize.y
                );

            // Lekérdezzük az oldal linkjeit
            List<Get_PDF_Links.Link> linkList = pageData.listOfLink;

            // Megnézzük, hogy egy linkre kattintottak-e
            for (int i = 0; i < linkList.Count; i++)
            {
                // Ha egy linkre kattintottak, akkor vissza adjuk az url-t
                if (linkList[i].rect.Contains(position))
                    return linkList[i].url;
            }
        }

        return null;
    }

    /*
    public class PageData
    {
        public int pageNumber;

        public Vector2 pageSize;

        public List<Link> listOfLink;

        public PageData(JSONNode jsonNode)
        {
            pageNumber = jsonNode["pageNumber"].AsInt;

            float px = jsonNode["pageSize"][0].AsFloat;
            float py = jsonNode["pageSize"][1].AsFloat;

            pageSize = new Vector2(px, py);

            listOfLink = new List<Link>();
            for (int i = 0; i < jsonNode["pageLinks"].Count; i++)
            {
                listOfLink.Add(new Link(jsonNode["pageLinks"][i]));
            }
        }
    }

    public class Link
    {
        public string url;
        public Rect rect;

        public Link(JSONNode jsonNode)
        {
            url = jsonNode["url"].Value;

            float p1x = jsonNode["rect"][0].AsFloat;
            float p1y = jsonNode["rect"][1].AsFloat;
            float p2x = jsonNode["rect"][2].AsFloat;
            float p2y = jsonNode["rect"][3].AsFloat;

            rect = new Rect(
                Mathf.Min(p1x, p2x),
                Mathf.Min(p1y, p2y),
                Mathf.Abs(p1x - p2x),
                Mathf.Abs(p1y - p2y));
        }
    }
    */
}
