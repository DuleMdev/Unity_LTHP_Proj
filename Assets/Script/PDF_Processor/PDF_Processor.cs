using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using UnityEngine;

public class PDF_Processor
{
    byte[] pdfData;

    bool streamCopy;

    string PDF_String;

    Tokenizer tokenizer;

    class IndirectObjectData
    {
        public int startPos;
        public int generationNumber;
        public string type; // 'f' - free or 'n' - used
        public PDF_Object pdf_Object;

        public IndirectObjectData(int startPos, int generationNumber, string type)
        {
            this.startPos = startPos;
            this.generationNumber = generationNumber;
            this.type = type;
        }

        public override string ToString()
        {
            return string.Format("startPos : {0}\ngenerationNumber : {1}\ntype : {2}\npdf_object : \n{3}", startPos, generationNumber, type, pdf_Object == null ? null : pdf_Object.ToString("", "    "));
        }
    }

    List<IndirectObjectData> xrefTable = new List<IndirectObjectData>();
    PDF_Object _trailer;
    public PDF_Object trailer { get { return _trailer; } }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pdfByteArray"></param>
    /// <param name="streamCopy">A stream adatokra is szükség van-e</param>
    public PDF_Processor(byte[] pdfByteArray, bool streamCopy = true)
    {
        this.streamCopy = streamCopy;

        Processing(pdfByteArray);

        pdfData = null;
        tokenizer = null;
        PDF_String = null;
    }

    void Processing(byte[] pdfData)
    {
        this.pdfData = pdfData;

        PDF_String = System.Text.Encoding.ASCII.GetString(pdfData);
        tokenizer = new Tokenizer(PDF_String);

        // Beolvassuk az xref helyét
        TokenData tokenData = tokenizer.SearchToken(TokenType.startxref, tokenizer.size - 40, true);

        tokenizer.SetPos(tokenData.pos + tokenData.tokenValue.Length);

        tokenData = tokenizer.ReadToken(TokenType.integerValue);

        int xrefPos = int.Parse(tokenData.tokenValue);

        _trailer = null;
        PDF_Object trailer;
        do
        {
            tokenizer.SetPos(xrefPos);

            tokenizer.ReadToken(TokenType.xref);
            do
            {
                int firstObjectIndex = int.Parse(tokenizer.ReadToken(TokenType.integerValue).tokenValue);
                int objectCount = int.Parse(tokenizer.ReadToken(TokenType.integerValue).tokenValue);

                for (int i = 0; i < objectCount; i++)
                {
                    if (i >= xrefTable.Count || xrefTable[i] == null)
                    {
                        Common.ListAdd(xrefTable, firstObjectIndex + i, new IndirectObjectData(
                            int.Parse(tokenizer.ReadToken(TokenType.integerValue).tokenValue),
                            int.Parse(tokenizer.ReadToken(TokenType.integerValue).tokenValue),
                            tokenizer.ReadToken(TokenType.f, TokenType.n).tokenValue
                            ), null);
                    }
                    else
                    {
                        // Nem rögzítjük, mert már van egy ilyen objektum rögzítve, viszont ki kell olvesni az adatait
                        tokenizer.ReadToken(TokenType.integerValue);
                        tokenizer.ReadToken(TokenType.integerValue);
                        tokenizer.ReadToken(TokenType.f, TokenType.n);
                    }
                }

            } while (tokenizer.PeekToken().tokenType != TokenType.trailer);

            // trailer section feldolgozása
            tokenizer.ReadToken(TokenType.trailer);

            trailer = GetDictionaryObject();

            // Az első megtalált trailer, ami a legutolsó a fájlban elmentjük
            if (_trailer == null)
                _trailer = trailer;

            if (trailer.ContainsKey("/Prev"))
                xrefPos = trailer["/Prev"].getInteger();

        } while (trailer.ContainsKey("/Prev"));

        Debug.Log(_trailer.ToString("", "    "));

        // Végig megyünk az xref táblában található objektumokon és feldolgozzuk őket
        for (int i = 0; i < xrefTable.Count; i++)
        {
            if (xrefTable[i] == null)
                throw new Exception(string.Format("Az {0}. bejgyzés üres", i));

            if (xrefTable[i].pdf_Object != null)
                Debug.Log(String.Format("Az {0}. objectum már fel van dolgozva\n{1}", i, xrefTable[i]));
            else
            {
                Debug.Log(String.Format("Az {0}. object feldolgozásának megkezdése\n{1}", i, xrefTable[i]));

                xrefTable[i].pdf_Object = xrefTable[i].type == "f" ? new PDF_Object() : GetIndirectObject(xrefTable[i].startPos);

                Debug.Log(String.Format("Az {0}. object feldolgozva\n{1}", i, xrefTable[i]));
            }
        }
    }

    PDF_Object GetNextPDFObject()
    {
        TokenData token = tokenizer.PeekToken();

        // Feldolgozzuk a tokeneket
        switch (token.tokenType)
        {
            case TokenType.boolTrue:
            case TokenType.boolFalse:
                tokenizer.ReadToken();
                return new PDF_Object(bool.Parse(token.tokenValue));
            case TokenType.floatValue:
                tokenizer.ReadToken();
                return new PDF_Object(FloatParse(token.tokenValue));
            case TokenType.integerValue:
                tokenizer.ReadToken();
                if (tokenizer.PeekToken().tokenType == TokenType.integerValue &&
                    tokenizer.Peek2Token().tokenType == TokenType.referenceSignal)
                {
                    int objectNumber = int.Parse(token.tokenValue);
                    int generationNumber = int.Parse(tokenizer.ReadToken().tokenValue);
                    tokenizer.ReadToken(); // Átlépjük a referenceSignált is az R -t is
                    return new PDF_Object(objectNumber, generationNumber);
                }
                else
                    return new PDF_Object(int.Parse(token.tokenValue));
            case TokenType.stringNormal:
                tokenizer.ReadToken();
                return new PDF_Object(token.tokenValue);
            case TokenType.stringHexa:
                tokenizer.ReadToken();
                return new PDF_Object(token.tokenValue);
            case TokenType.null_:
                tokenizer.ReadToken();
                return new PDF_Object();
            case TokenType.name:
                tokenizer.ReadToken();
                return new PDF_Object(token.tokenValue, true);
            case TokenType.dictionaryOpen:
                return GetDictionaryObject();
            case TokenType.squareOpenBracket:
                return GetArrayObject();
        }

        throw new Exception("A beolvasott token nem PDF object-hez tartozik : " + token.tokenType);
    }

    float FloatParse(string floatValue)
    {
        string[] split = floatValue.Split('.', ',');
        return float.Parse(split[0] + CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator + split[1]);
    }

    PDF_Object GetArrayObject()
    {
        PDF_Object pdfObject = new PDF_Object();
        pdfObject.setArrayObject();

        tokenizer.ReadToken(TokenType.squareOpenBracket);

        while (tokenizer.PeekToken().tokenType != TokenType.squareCloseBracket)
        {
            pdfObject.AddArrayItem(GetNextPDFObject());
        }

        tokenizer.ReadToken(TokenType.squareCloseBracket);

        return pdfObject;
    }

    PDF_Object GetDictionaryObject()
    {
        PDF_Object pdfObject = new PDF_Object();
        pdfObject.SetDictionaryObject();

        tokenizer.ReadToken(TokenType.dictionaryOpen);

        while (tokenizer.PeekToken().tokenType != TokenType.dictionaryClose)
        {
            pdfObject.AddDictionaryItem(GetNextPDFObject(), GetNextPDFObject());
        }

        tokenizer.ReadToken(TokenType.dictionaryClose);

        return pdfObject;
    }

    PDF_Object GetIndirectObject(int pos = -1)
    {
        if (pos >= 0)
            tokenizer.SetPos(pos);

        int objectNumber = int.Parse(tokenizer.ReadToken(TokenType.integerValue).tokenValue);
        int generationNumber = int.Parse(tokenizer.ReadToken(TokenType.integerValue).tokenValue);

        tokenizer.ReadToken(TokenType.objNameBegin); // obj

        PDF_Object objData = GetNextPDFObject();

        byte[] streamData = null;
        if (tokenizer.PeekToken().tokenType == TokenType.stream) // stream
        {
            tokenizer.ReadToken(TokenType.stream);

            int streamPos = tokenizer.getPos;

            // Ha van stream akkor az objData-nak szótárnak kell lennie amiben van egy /Length kulcs
            int length = SolveIndirectObjectReference(objData["/Length"]).getInteger();

            // Kimásoljuk a byte adatokat
            //streamData = System.Text.Encoding.ASCII.GetBytes(PDF_String.Substring(streamPos, length));

            if (streamCopy)
            {
                streamData = new byte[length];
                System.Array.Copy(pdfData, streamPos, streamData, 0, length);
            }

            // beállítjuk a stream adatok mögötti pozíciót
            tokenizer.SetPos(streamPos + length);

            tokenizer.ReadToken(TokenType.endstream); // endstream
        }

        tokenizer.ReadToken(TokenType.objNameEnd); // endobj

        return new PDF_Object(objectNumber, generationNumber, objData, streamData);
    }

    public PDF_Object SolveIndirectObjectReference(PDF_Object pdf_Object)
    {
        if (pdf_Object.getObjectType != PDF_ObjectType.referenceObject)
            return pdf_Object;

        if (pdf_Object.GetObjectNumber >= xrefTable.Count || xrefTable[pdf_Object.GetObjectNumber].type == "f")
            return new PDF_Object(); // Egy null értékű object vissza adása

        if (xrefTable[pdf_Object.GetObjectNumber].pdf_Object == null)
            xrefTable[pdf_Object.GetObjectNumber].pdf_Object = GetIndirectObject(xrefTable[pdf_Object.GetObjectNumber].startPos);

        return SolveIndirectObjectReference(xrefTable[pdf_Object.GetObjectNumber].pdf_Object.indirectData);
    }

    PDF_Object ProcessingIndirectObject(int index)
    {
        // Ha még nem volt feldolgozva, akkor feldolgozzuk
        if (xrefTable[index].pdf_Object == null)
        {
            // Ha törölve van, akkor egy üres object lesz
            if (xrefTable[index].type == "f")
                xrefTable[index].pdf_Object = new PDF_Object();
            else
                xrefTable[index].pdf_Object = GetIndirectObject(xrefTable[index].startPos);
        }

        return xrefTable[index].pdf_Object;
    }


    class RegMaker
    {
        Dictionary<string, string> dic = new Dictionary<string, string>();

        public RegMaker()
        {

        }

        public void Add(string key, string value)
        {
            dic.Add(key, value);
        }


    }

    enum TokenType
    {
        empty,
        error,

        whiteSpace,

        remark,
        objNameBegin,
        objNameEnd,
        dictionaryOpen,
        dictionaryClose,

        boolTrue,
        boolFalse,
        floatValue,
        integerValue,

        parenthesesOpen,
        parenthesesClose,
        stringHexa,
        stringNormal,
        squareOpenBracket,
        squareCloseBracket,

        stream,
        endstream,

        startxref,
        trailer,
        xref,

        f,  // free xref table 
        n,  // used xref table 

        null_,

        name,

        referenceSignal,

        endOfFile,
    }

    struct TokenData
    {
        public TokenType tokenType;
        public string tokenValue;
        public int pos; // Hol kezdődik a megtalált token

        public TokenData(TokenType tokenType, string tokenValue, int pos)
        {
            this.tokenType = tokenType;
            this.tokenValue = tokenValue;
            this.pos = pos;
        }
    }


    class Tokenizer
    {
        class TokenRequest
        {
            public TokenType tokenType;
            public Regex regex;
            public int pos; // Hol van a következő egyezés

            public TokenRequest(TokenType tokenType, string pattern)
            {
                this.tokenType = tokenType;
                regex = new Regex(pattern);
            }
        }

        string stringData;

        List<TokenRequest> listOfTokenRegexes = new List<TokenRequest>();

        int pos;
        public int getPos { get { return pos; } }

        TokenData nextData;
        TokenData next2Data;

        public int size {
            get { return stringData.Length; }
        }

        public Tokenizer(string text)
        {
            stringData = text;

            AddToken(TokenType.whiteSpace, @"\s+"); // WhiteSpace
            AddToken(TokenType.remark, "%[^\r\n]*"); // % karakter után a sor végéig minden
            AddToken(TokenType.objNameBegin, "obj"); // az obj karakter sorozat
            AddToken(TokenType.objNameEnd, "endobj"); // az endobj karakter sorozat
            AddToken(TokenType.dictionaryOpen, "<<");
            AddToken(TokenType.dictionaryClose, ">>");

            AddToken(TokenType.boolTrue, "true");
            AddToken(TokenType.boolFalse, "false");
            AddToken(TokenType.floatValue, @"[+-]?(\d+\.\d*|\d*\.\d+)"); // pl. -78.5487
            AddToken(TokenType.integerValue, "[+-]?[0-9]+"); // pl.  -45
            AddToken(TokenType.parenthesesOpen, @"\(");
            AddToken(TokenType.parenthesesClose, @"\)");
            AddToken(TokenType.stringHexa, @"<(\s*[a-fA-F0-9]+)+\s*>"); // pl. <AFD145E>
            AddToken(TokenType.squareOpenBracket, @"\[");
            AddToken(TokenType.squareCloseBracket, @"\]");
            AddToken(TokenType.stream, "stream\r?\n"); // A stream karakterek után a kocsi visza nem kötelező, de az új sor igen és ha vannak, akkor ebben a sorrendben kell lenniük
            AddToken(TokenType.endstream, "endstream");

            AddToken(TokenType.null_, "null");

            AddToken(TokenType.name, @"/[^%/(){}<>[\]\s]+"); // / jel után bármi whitespace és delimiter karaktereken kívűl (The delimiter characters : ( ) < > [ ] { } / %) pl. /name
            AddToken(TokenType.referenceSignal, "R");


            AddToken(TokenType.startxref, "startxref");
            AddToken(TokenType.trailer, "trailer");
            AddToken(TokenType.xref, "xref");

            AddToken(TokenType.f, "f");
            AddToken(TokenType.n, "n");

            SetPos(0);

            //nextData = GetNextToken();
        }

        void AddToken(TokenType tokenType, string pattern)
        {
            listOfTokenRegexes.Add(new TokenRequest(tokenType, pattern));
        }

        // indirect reference pl. 12 0 R  Ami azt jelenti, hogy 12 object szám a nulla jelent, hogy még 0-szor lett módoítva és az R kulcsszó ami a reference gondolom

        public TokenData ReadToken()
        {
            TokenData result = nextData;

            nextData = next2Data;

            do
            {
                next2Data = GetNextToken();

                if (next2Data.tokenType == TokenType.parenthesesOpen)
                    next2Data = GetString(next2Data);

            } while (next2Data.tokenType == TokenType.whiteSpace || next2Data.tokenType == TokenType.remark);

            return result;
        }

        public TokenData ReadToken(params TokenType[] validTokenlist)
        {
            TokenData tokenData = ReadToken();

            if (!Array.Exists(validTokenlist, tokenType => tokenType == tokenData.tokenType))
            {
                string errorText = "Remélt token(ek)\n";

                for (int i = 0; i < validTokenlist.Length; i++)
                    errorText += validTokenlist[i] + "\n";

                errorText +=
                    "A talált token" + "\n" +
                    "Token type : " + tokenData.tokenType + "\n" +
                    "Token pos : " + tokenData.pos + "\n" +
                    "Token value : " + tokenData.tokenValue;

                throw new System.Exception(errorText);
            }

            return tokenData;
        }

        public TokenData PeekToken()
        {
            return nextData;
        }

        public TokenData Peek2Token()
        {
            return next2Data;
        }

        public void SetPos(int pos)
        {
            this.pos = pos;

            for (int i = 0; i < listOfTokenRegexes.Count; i++)
                listOfTokenRegexes[i].pos = 0;

            ReadToken();
            ReadToken();
        }

        /// <summary>
        /// A megadott pozíciótól kezdődően keresi valahol a megadott tokent
        /// </summary>
        /// <param name="tokenType"></param>
        /// <param name="startPos"></param>
        /// <param name="mustExist"></param>
        /// <returns></returns>
        public TokenData SearchToken(TokenType tokenType, int startPos = 0, bool mustExist = false)
        {
            TokenRequest tokenRequest = GetTokenRequest(tokenType);

            Match match = tokenRequest.regex.Match(stringData, startPos);

            if (match.Success)
                return new TokenData(tokenType, match.Value, match.Index);

            if (mustExist)
                throw new System.Exception("Nem található a keresett token : " + tokenType);

            return new TokenData(TokenType.error, "A keresett token nem található!", startPos);
        }

        /// <summary>
        /// A megtalált ( jel után egy string van, ezt keresi meg. és vissza adja az egészet pl. "(alma)"
        /// </summary>
        /// <param name="tokenData"></param>
        /// <returns></returns>
        public TokenData GetString(TokenData tokenData)
        {
            int parentheses = 0;

            int pos = tokenData.pos + tokenData.tokenValue.Length + 1;

            while (pos < stringData.Length)
            {
                switch (stringData[pos])
                {
                    case '\\':
                        pos++;
                        break;
                    case '(':
                        parentheses++;
                        break;
                    case ')':
                        if (parentheses > 0)
                            parentheses--;
                        else
                        {
                            tokenData.tokenType = TokenType.stringNormal;
                            tokenData.tokenValue = stringData.Substring(tokenData.pos, pos - tokenData.pos + 1);
                            this.pos = pos + 1;
                            //SetPos(pos + 1);

                            return tokenData;
                        }
                        break;

                    default:
                        break;
                }

                pos++;
            }

            throw new System.Exception("A fájlnak vége, de a stringnek még nincs : pos = " + tokenData.pos);

            //return tokenData;
        }

        TokenRequest GetTokenRequest(TokenType tokenType)
        {
            for (int i = 0; i < listOfTokenRegexes.Count; i++)
            {
                if (listOfTokenRegexes[i].tokenType == tokenType)
                    return listOfTokenRegexes[i];
            }

            return null;
        }

        TokenData GetNextToken()
        {
            if (pos >= stringData.Length)
            {
                return new TokenData(TokenType.endOfFile, "", pos);
            }
            else {
                // Végig megyünk a reguláris kifejezéseken és amelyik az aktuális pos-tól kezdődik azt adjuk vissza
                for (int i = 0; i < listOfTokenRegexes.Count; i++)
                {
                    TokenRequest tokenRequest = listOfTokenRegexes[i];

                    if (tokenRequest.pos <= pos)
                    {
                        Match match = tokenRequest.regex.Match(stringData, pos);

                        if (match.Success)
                        {
                            tokenRequest.pos = match.Index;

                            if (match.Index == pos)
                            {
                                pos += match.Value.Length;
                                return new TokenData(tokenRequest.tokenType, match.Value, match.Index);
                            }
                        }
                        else
                        {
                            tokenRequest.pos = stringData.Length;
                        }
                    }
                }

                Debug.Log(Common.SubstringSafe(stringData, pos, 100));

                return new TokenData(TokenType.error, "Nincs egyező token", pos);
            }
        }
    }
}
