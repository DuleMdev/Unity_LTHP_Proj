using System.Collections;
using System.Collections.Generic;
using UnityEngine;

class PDFViewerActionHandler : Paroxe.PdfRenderer.Internal.Viewer.PDFViewerDefaultActionHandler
{
    public delegate void CallBack(string s);

    CallBack callBack;

    public PDFViewerActionHandler(CallBack callBack)
    {
        this.callBack = callBack;
    }

    new public void HandleUriAction(Paroxe.PdfRenderer.IPDFDevice device, string uri)
    {
        if (uri.Trim().Substring(uri.Length - 4).ToLower().Contains("pdf"))
        {
#if !UNITY_WEBGL
            device.LoadDocumentFromWeb(uri, "", 0);
#endif
        }
        else if (device.AllowOpenURL)
        {
            if (uri.Trim().ToLowerInvariant().StartsWith("http:")
                || uri.Trim().ToLowerInvariant().StartsWith("https:")
                || uri.Trim().ToLowerInvariant().StartsWith("ftp:"))
            {
                if (callBack != null)
                    callBack(uri);
                //Application.OpenURL(uri);
            }
        }
    }

}
