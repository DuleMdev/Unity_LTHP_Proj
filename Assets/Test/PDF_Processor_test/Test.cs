using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    public TextAsset pdfData;


    Dictionary<int, Get_PDF_Links.PageData> PDF_links;

    // Start is called before the first frame update
    void Start()
    {
        byte[] pdfByteArray = System.Convert.FromBase64String(SimpleJSON.JSON.Parse(pdfData.text)[C.JSONKeys.pdf].Value);

        PDF_links = Get_PDF_Links.GetPDFLinks(pdfByteArray); //  GetPDFLinks(pdfByteArray);

        List<int> pageNumbers = new List<int>(PDF_links.Keys);

        Debug.Log("Linkeket tartalmazó oldalak száma : " + pageNumbers.Count);

        for (int i = 0; i < pageNumbers.Count; i++)
        {
            string result = pageNumbers[i] + ". oldal\n";

            Get_PDF_Links.PageData pageData = PDF_links[pageNumbers[i]];

            for (int j = 0; j < pageData.listOfLink.Count; j++)
            {
                result += "  " + j + ". link\n" +
                    "      rect : " + pageData.listOfLink[j].rect + "\n" +
                    "      url : " + pageData.listOfLink[j].url + "\n";
            }

            Debug.Log(result);
        }

        Debug.Log("End");
    }

    /*
    Dictionary<int, PageData> GetPDFLinks(byte[] pdfData)
    {
        PDF_Processor pdf = new PDF_Processor(pdfData, false);

        Dictionary<int, PageData> pageLinks = new Dictionary<int, PageData>();

        Debug.Log(pdf.trailer["/Size"]);
        Debug.Log(pdf.trailer["/Root"]);
        Debug.Log(pdf.trailer["/Info"]);

        PDF_Object root = pdf.SolveIndirectObjectReference(pdf.trailer["/Root"]);
        Debug.Log("Root object\n" + root);

        Rect pageSize = new Rect();
        PDF_Object pages = pdf.SolveIndirectObjectReference(root["/Pages"]);
        Debug.Log("Pages object\n" + pages);

        if (pages.ContainsKey("/MediaBox"))
            pageSize = GetRect(pdf, pages["/MediaBox"]);

        PDF_Object kids = pdf.SolveIndirectObjectReference(pages["/Kids"]);
        Debug.Log("Kids object\n" + kids);

        for (int i = 0; i < kids.getLength; i++)
        {
            PageData pageData = new PageData();
            pageData.pageNumber = i + 1;
            pageData.pageSize = new Vector2(pageSize.width, pageSize.height);
            GetPageLinks(pdf, kids[i], pageData);

            if (pageData.listOfLink.Count > 0)
                pageLinks.Add(pageData.pageNumber, pageData);
        }

        return pageLinks;
    }


    void GetPageLinks(PDF_Processor pdf, PDF_Object pdf_page, PageData pageData)
    {
        pdf_page = pdf.SolveIndirectObjectReference(pdf_page);

        if (pdf_page.ContainsKey("/MediaBox"))
        {
            Rect pageSize = GetRect(pdf, pdf_page["/MediaBox"]);
            pageData.pageSize = new Vector2(pageSize.width, pageSize.height);
        }

        if (pdf_page.ContainsKey("/Annots"))
        {
            PDF_Object pdf_annots = pdf.SolveIndirectObjectReference(pdf_page["/Annots"]);

            for (int i = 0; i < pdf_annots.getLength; i++)
            {
                PDF_Object pdf_annot = pdf.SolveIndirectObjectReference(pdf_annots[i]);

                Link link = GetLink(pdf, pdf_annot);

                if (link != null)
                    pageData.listOfLink.Add(link);
            }
        }
    }

    Link GetLink(PDF_Processor pdf, PDF_Object pdf_annot)
    {
        if (VerifyKeyValue(pdf, pdf_annot, "/Type", "/Annot") &&
            VerifyKeyValue(pdf, pdf_annot, "/Subtype", "/Link"))
        {
            PDF_Object pdf_a = pdf.SolveIndirectObjectReference(pdf_annot["/A"]);

            if (VerifyKeyValue(pdf, pdf_a, "/Type", "/Action") &&
                VerifyKeyValue(pdf, pdf_a, "/S", "/URI"))
            {
                Link link = new Link();

                link.rect = GetRect(pdf, pdf_annot["/Rect"]);
                link.url = pdf.SolveIndirectObjectReference(pdf_a["/URI"]).getString();

                link.url = link.url.Substring(1, link.url.Length - 2);

                return link;
            }
        }

        return null;
    }

    bool VerifyKeyValue(PDF_Processor pdf, PDF_Object pdf_dictionary, string key, string value)
    {
        pdf_dictionary = pdf.SolveIndirectObjectReference(pdf_dictionary);

        if (pdf_dictionary.ContainsKey(key))
            return pdf.SolveIndirectObjectReference(pdf_dictionary[key]).getName() == value;

        return false;
    }

    Rect GetRect(PDF_Processor pdf, PDF_Object pdf_object)
    {
        pdf_object = pdf.SolveIndirectObjectReference(pdf_object);

        float p1x = getFloat(pdf.SolveIndirectObjectReference(pdf_object[0]));
        float p1y = getFloat(pdf.SolveIndirectObjectReference(pdf_object[1]));
        float p2x = getFloat(pdf.SolveIndirectObjectReference(pdf_object[2]));
        float p2y = getFloat(pdf.SolveIndirectObjectReference(pdf_object[3]));

        return new Rect(
            Mathf.Min(p1x, p2x),
            Mathf.Min(p1y, p2y),
            Mathf.Abs(p1x - p2x),
            Mathf.Abs(p1y - p2y));
    }

    float getFloat(PDF_Object pdf_object)
    {
        if (pdf_object.getObjectType == PDF_ObjectType.floatObject)
            return pdf_object.getFloat();

        if (pdf_object.getObjectType == PDF_ObjectType.integerObject)
            return pdf_object.getInteger();

        return -88888888;
    }

    public class PageData
    {
        public int pageNumber;

        public Vector2 pageSize;

        public List<Link> listOfLink;

        public PageData()
        {
            listOfLink = new List<Link>();
        }
    }

    public class Link
    {
        public string url;
        public Rect rect;

        public Link()
        {
        }
    }
    */
}
