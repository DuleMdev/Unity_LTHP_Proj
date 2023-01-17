using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Get_PDF_Links
{
    public static Dictionary<int, PageData> GetPDFLinks(byte[] pdfData)
    {

        Dictionary<int, PageData> pageLinks = new Dictionary<int, PageData>();

        try
        {
            PDF_Processor pdf = new PDF_Processor(pdfData, false);

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
                pageData.pageNumber = i;
                pageData.pageSize = new Vector2(pageSize.width, pageSize.height);
                GetPageLinks(pdf, kids[i], pageData);

                if (pageData.listOfLink.Count > 0)
                    pageLinks.Add(pageData.pageNumber, pageData);
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e.ToString());
            Debug.LogError("Linkek kinyerése a pdf dokumentumból sikertelen.");
        }

        return pageLinks;
    }

    static void GetPageLinks(PDF_Processor pdf, PDF_Object pdf_page, PageData pageData)
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

    static Link GetLink(PDF_Processor pdf, PDF_Object pdf_annot)
    {
        if (//VerifyKeyValue(pdf, pdf_annot, "/Type", "/Annot") &&    // Idönként nincs az annotációban ez a kulcs - érték pár
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

    static bool VerifyKeyValue(PDF_Processor pdf, PDF_Object pdf_dictionary, string key, string value)
    {
        pdf_dictionary = pdf.SolveIndirectObjectReference(pdf_dictionary);

        if (pdf_dictionary.ContainsKey(key))
            return pdf.SolveIndirectObjectReference(pdf_dictionary[key]).getName() == value;

        return false;
    }

    static Rect GetRect(PDF_Processor pdf, PDF_Object pdf_object)
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

    static float getFloat(PDF_Object pdf_object)
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
    }
}
