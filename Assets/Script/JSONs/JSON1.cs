﻿//#define USE_SharpZipLib
#if !UNITY_WEBPLAYER
#define USE_FileIO
#endif

/* * * * *
 * A simple JSON Parser / builder
 * ------------------------------
 * 
 * It mainly has been written as a simple JSON parser. It can build a JSON string
 * from the node-tree, or generate a node tree from any valid JSON string.
 * 
 * If you want to use compression when saving to file / stream / B64 you have to include
 * SharpZipLib ( http://www.icsharpcode.net/opensource/sharpziplib/ ) in your project and
 * define "USE_SharpZipLib" at the top of the file
 * 
 * Written by Bunny83 
 * 2012-06-09
 * 
 * Features / attributes:
 * - provides strongly typed node classes and lists / dictionaries
 * - provides easy access to class members / array items / data values
 * - the parser ignores data types. Each value is a string.
 * - only double quotes (") are used for quoting strings.
 * - values and names are not restricted to quoted strings. They simply add up and are trimmed.
 * - There are only 3 types: arrays(JSONArray), objects(JSONClass) and values(JSONData)
 * - provides "casting" properties to easily convert to / from those types:
 *   int / float / double / bool
 * - provides a common interface for each node so no explicit casting is required.
 * - the parser try to avoid errors, but if malformed JSON is parsed the result is undefined
 * 
 * 
 * 2012-12-17 Update:
 * - Added internal JSONLazyCreator class which simplifies the construction of a JSON tree
 *   Now you can simple reference any item that doesn't exist yet and it will return a JSONLazyCreator
 *   The class determines the required type by it's further use, creates the type and removes itself.
 * - Added binary serialization / deserialization.
 * - Added support for BZip2 zipped binary format. Requires the SharpZipLib ( http://www.icsharpcode.net/opensource/sharpziplib/ )
 *   The usage of the SharpZipLib library can be disabled by removing or commenting out the USE_SharpZipLib define at the top
 * - The serializer uses different types when it comes to store the values. Since my data values
 *   are all of type string, the serializer will "try" which format fits best. The order is: int, float, double, bool, string.
 *   It's not the most efficient way but for a moderate amount of data it should work on all platforms.
 * 
 * * * * */
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using System.Text;

namespace SimpleJSON
{
    public enum JSONBinaryTag
    {
        Array = 1,
        Class = 2,
        Value = 3,
        IntValue = 4,
        DoubleValue = 5,
        BoolValue = 6,
        FloatValue = 7,
    }

    public class JSONNode
    {
        #region common interface
        public virtual void Add(string aKey, JSONNode aItem) { }
        public virtual JSONNode this[int aIndex] { get { return null; } set { } }
        public virtual JSONNode this[string aKey] { get { return null; } set { } }
        public virtual string Value { get { return ""; } set { } }
        public virtual int Count { get { return 0; } }

        protected JSONBinaryTag valueType = JSONBinaryTag.Value;

        public virtual bool ContainsKey(string aKey)
        {
            return false;
        }

        public virtual void Add(JSONNode aItem)
        {
            Add("", aItem);
        }

        public virtual JSONNode Remove(string aKey) { return null; }
        public virtual JSONNode Remove(int aIndex) { return null; }
        public virtual JSONNode Remove(JSONNode aNode) { return aNode; }

        public virtual IEnumerable<JSONNode> Childs { get { yield break; } }
        public IEnumerable<JSONNode> DeepChilds
        {
            get
            {
                foreach (var C in Childs)
                    foreach (var D in C.DeepChilds)
                        yield return D;
            }
        }

        public virtual IEnumerable<string> Keys { get { yield break; } }

        public override string ToString()
        {
            return "JSONNode";
        }
        public virtual string ToString(string aPrefix)
        {
            return "JSONNode";
        }

        #endregion common interface

        #region typecasting properties
        public virtual int AsInt
        {
            get
            {
                int v = 0;
                if (int.TryParse(Value, out v))
                    return v;
                return 0;
            }
            set
            {
                Value = value.ToString();
                valueType = JSONBinaryTag.IntValue;
            }
        }
        public virtual float AsFloat
        {
            get
            {
                float v = 0.0f;
                if (float.TryParse(Value, out v))
                    return v;
                return 0.0f;
            }
            set
            {
                Value = value.ToString();
                valueType = JSONBinaryTag.FloatValue;
            }
        }
        public virtual double AsDouble
        {
            get
            {
                double v = 0.0;
                if (double.TryParse(Value, out v))
                    return v;
                return 0.0;
            }
            set
            {
                Value = value.ToString();
                valueType = JSONBinaryTag.DoubleValue;
            }
        }
        public virtual bool AsBool
        {
            get
            {
                bool v = false;
                if (bool.TryParse(Value, out v))
                    return v;

                if (Value == "0")
                    return false;
                if (Value == "1")
                    return true;

                return !string.IsNullOrEmpty(Value);
            }
            set
            {
                Value = (value) ? "true" : "false";
                valueType = JSONBinaryTag.BoolValue;
            }
        }
        public virtual JSONArray AsArray
        {
            get
            {
                return this as JSONArray;
            }
        }
        public virtual JSONClass AsObject
        {
            get
            {
                return this as JSONClass;
            }
        }


        #endregion typecasting properties

        #region operators
        public static implicit operator JSONNode(string s)
        {
            return new JSONData(s);
        }
        public static implicit operator string(JSONNode d)
        {
            return (d == null) ? null : d.Value;
        }
        public static bool operator ==(JSONNode a, object b)
        {
            if (b == null && a is JSONLazyCreator)
                return true;
            return System.Object.ReferenceEquals(a, b);
        }

        public static bool operator !=(JSONNode a, object b)
        {
            return !(a == b);
        }
        public override bool Equals(object obj)
        {
            return System.Object.ReferenceEquals(this, obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }


        #endregion operators

        internal static string Escape(string aText)
        {
            if (aText == null)
                return "null";

            StringBuilder result = new StringBuilder();

            //string result = "";
            foreach (char c in aText)
            {
                switch (c)
                {
                    case '\\': result.Append("\\\\"); break;
                    case '\"': result.Append("\\\""); break;
                    case '\n': result.Append("\\n"); break;
                    case '\r': result.Append("\\r"); break;
                    case '\t': result.Append("\\t"); break;
                    case '\b': result.Append("\\b"); break;
                    case '\f': result.Append("\\f"); break;
                    default: result.Append(c); break;
                }
            }
            return result.ToString();
        }

        public static JSONNode Parse(string aJSON)
        {
            Stack<JSONNode> stack = new Stack<JSONNode>();
            JSONNode ctx = null;
            int i = 0;
            StringBuilder Token = new StringBuilder();
            //string Token = "";
            string TokenName = "";
            bool QuoteMode = false; // idézőjel mód, ha igaz, akkor idézőjelben vagyunk
            bool WasQuote = false;  // Ha volt idézőjel, akkor ez true

            try
            {
                while (i < aJSON.Length)
                {
                    // Megjegyzések átugrása
                    if (!QuoteMode)
                    {
                        if (aJSON[i] == '/' && i + 1 < aJSON.Length && (aJSON[i + 1] == '/' || aJSON[i + 1] == '*'))
                        {
                            int indexOf = 0;
                            if (aJSON[i + 1] == '/')
                            {
                                // Megvizsgáljuk melyik sorvégjel van korábban, lehet, hogy csak az egyik van jelen
                                indexOf = aJSON.IndexOf("\n", i) + 1;
                                int indexOf2 = aJSON.IndexOf("\r", i) + 1;

                                // Ha legalább az egyik nem talált, akkor a nagyobbik értéket használjuk
                                if (indexOf < 0 || indexOf2 < 0)
                                    indexOf = Math.Max(indexOf, indexOf2);
                                else // Ha mindkettő talált, akkor a kisebbet
                                    indexOf = Math.Min(indexOf, indexOf2);
                            }
                            else {
                                indexOf = aJSON.IndexOf("*/", i + 2);
                                if (indexOf > -1)
                                    indexOf += 2;
                            }

                            if (indexOf < i)
                                indexOf = aJSON.Length;

                            i = indexOf;

                            continue;
                        }
                    }

                    if (QuoteMode)
                    {
                        switch (aJSON[i])
                        {
                            case '"':
                                QuoteMode = false;
                                WasQuote = true;
                                break;

                            case '\\':
                                ++i;
                                if (QuoteMode)
                                {
                                    char C = aJSON[i];
                                    switch (C)
                                    {
                                        case 't': Token.Append('\t'); break;
                                        case 'r': Token.Append('\r'); break;
                                        case 'n': Token.Append('\n'); break;
                                        case 'b': Token.Append('\b'); break;
                                        case 'f': Token.Append('\f'); break;
                                        case 'u':
                                            {
                                                string s = aJSON.Substring(i + 1, 4);
                                                Token.Append((char)int.Parse(s, System.Globalization.NumberStyles.AllowHexSpecifier));
                                                i += 4;
                                                break;
                                            }
                                        default:
                                            Token.Append(C);
                                            break;
                                    }
                                }
                                break;

                            default:
                                Token.Append(aJSON[i]);
                                break;
                        }
                    }
                    else
                    {
                        switch (aJSON[i])
                        {
                            case '{':
                                stack.Push(new JSONClass());
                                if (ctx == null)
                                {
                                    if (!string.IsNullOrWhiteSpace(TokenName) || !string.IsNullOrWhiteSpace(Token.ToString()))
                                        throw new Exception("JSON Parse: Illegal character(s) before first class");
                                }
                                else
                                {
                                    if (ctx is JSONArray)
                                    {
                                        if (!string.IsNullOrWhiteSpace(TokenName) || !string.IsNullOrWhiteSpace(Token.ToString()))
                                            throw new Exception("JSON Parse: Illegal character(s) before array item");
                                        ctx.Add(stack.Peek());
                                    }
                                    else if (TokenName != "")
                                        ctx.Add(TokenName, stack.Peek());
                                    else
                                        throw new Exception("JSON Parse: No class name");
                                }
                                TokenName = "";
                                Token.Length = 0;
                                ctx = stack.Peek();
                                break;

                            case '[':
                                stack.Push(new JSONArray());
                                if (ctx == null)
                                {
                                    if (!string.IsNullOrWhiteSpace(TokenName) || !string.IsNullOrWhiteSpace(Token.ToString()))
                                        throw new Exception("JSON Parse: Illegal character(s) before first array");
                                }
                                else
                                {
                                    if (ctx is JSONArray)
                                    {
                                        if (!string.IsNullOrWhiteSpace(TokenName) || !string.IsNullOrWhiteSpace(Token.ToString()))
                                            throw new Exception("JSON Parse: Illegal character(s) before array item");
                                        ctx.Add(stack.Peek());
                                    }
                                    else if (TokenName != "")
                                        ctx.Add(TokenName, stack.Peek());
                                    else
                                        throw new Exception("JSON Parse: No array name");
                                }
                                TokenName = "";
                                Token.Length = 0;
                                ctx = stack.Peek();
                                break;

                            case '}':
                            case ']':
                                if (stack.Count == 0)
                                    throw new Exception("JSON Parse: Too many closing brackets");

                                stack.Pop();
                                //if (Token.Length != 0)
                                {
                                    if (ctx is JSONArray)
                                    {
                                        if (!string.IsNullOrWhiteSpace(TokenName))
                                            throw new Exception("JSON Parse: Key / Value par in an array");
                                        if (Token.Length != 0 || WasQuote)
                                            ctx.Add(Token.ToString());
                                    }
                                    else if (TokenName != "")
                                    {
                                        if (string.IsNullOrWhiteSpace(TokenName) || (string.IsNullOrWhiteSpace(Token.ToString()) && !WasQuote))
                                            throw new Exception("JSON Parse: Not Key / Value pair in a class");
                                        ctx.Add(TokenName, Token.ToString());
                                    }
                                }
                                TokenName = "";
                                Token.Length = 0;
                                if (stack.Count > 0)
                                    ctx = stack.Peek();
                                else {
                                    // Elvileg már nem lehet több látható karakter a szövegben
                                    if (!string.IsNullOrWhiteSpace(aJSON.Substring(i + 1)))
                                        throw new Exception("JSON Parse: Illegal character(s) after close main bracket");
                                }
                                WasQuote = false;
                                break;

                            case ':':
                                if (ctx == null || ctx is JSONArray)
                                    throw new Exception("JSON Parse: Illegal character ':'");

                                TokenName = Token.ToString().Trim();
                                Token.Length = 0;
                                WasQuote = false;
                                break;

                            case '"':
                                if (WasQuote)
                                    throw new Exception("JSON Parse: Quate again");

                                QuoteMode = true;
                                break;

                            case ',':
                                //if (Token.Length != 0)
                                {
                                    if (ctx is JSONArray)
                                    {
                                        if (!string.IsNullOrWhiteSpace(TokenName))
                                            throw new Exception("JSON Parse: Key / Value par in an array");
                                        if (Token.Length != 0 || WasQuote)
                                            ctx.Add(Token.ToString());
                                    }
                                    else if (TokenName != "")
                                    {
                                        if (string.IsNullOrWhiteSpace(TokenName) || (string.IsNullOrWhiteSpace(Token.ToString()) && !WasQuote))
                                            throw new Exception("JSON Parse: Not Key / Value pair in a class");
                                        ctx.Add(TokenName, Token.ToString());
                                    }
                                }
                                TokenName = "";
                                Token.Length = 0;
                                WasQuote = false;
                                break;

                            case '\r':
                            case '\n':
                            case ' ':
                            case '\t':
                                break;

                            default:
                                Token.Append(aJSON[i]);
                                break;
                        }
                    }
                    ++i;
                }

                if (QuoteMode)
                    throw new Exception("JSON Parse: Quotation marks seems to be messed up.");

                if (stack.Count > 1)
                    throw new Exception("JSON Parse: Too few closing brackets");
            }
            catch (Exception e)
            {
                throw new Exception(e.Message + aJSON.SubstringSafe(i - 50, 50) + "\n*****\n" + aJSON.SubstringSafe(i, 50));
            }

            return ctx;
        }

        public virtual void Serialize(System.IO.BinaryWriter aWriter) { }

        public void SaveToStream(System.IO.Stream aData)
        {
            var W = new System.IO.BinaryWriter(aData);
            Serialize(W);
        }

#if USE_SharpZipLib
        public void SaveToCompressedStream(System.IO.Stream aData)
        {
            using (var gzipOut = new ICSharpCode.SharpZipLib.BZip2.BZip2OutputStream(aData))
            {
                gzipOut.IsStreamOwner = false;
                SaveToStream(gzipOut);
                gzipOut.Close();
            }
        }
 
        public void SaveToCompressedFile(string aFileName)
        {
#if USE_FileIO
            System.IO.Directory.CreateDirectory((new System.IO.FileInfo(aFileName)).Directory.FullName);
            using(var F = System.IO.File.OpenWrite(aFileName))
            {
                SaveToCompressedStream(F);
            }
#else
            throw new Exception("Can't use File IO stuff in webplayer");
#endif
        }
        public string SaveToCompressedBase64()
        {
            using (var stream = new System.IO.MemoryStream())
            {
                SaveToCompressedStream(stream);
                stream.Position = 0;
                return System.Convert.ToBase64String(stream.ToArray());
            }
        }
 
#else
        public void SaveToCompressedStream(System.IO.Stream aData)
        {
            throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
        }
        public void SaveToCompressedFile(string aFileName)
        {
            throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
        }
        public string SaveToCompressedBase64()
        {
            throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
        }
#endif

        public void SaveToFile(string aFileName)
        {
#if USE_FileIO
            System.IO.Directory.CreateDirectory((new System.IO.FileInfo(aFileName)).Directory.FullName);
            using (var F = System.IO.File.OpenWrite(aFileName))
            {
                SaveToStream(F);
            }
#else
            throw new Exception("Can't use File IO stuff in webplayer");
#endif
        }
        public string SaveToBase64()
        {
            using (var stream = new System.IO.MemoryStream())
            {
                SaveToStream(stream);
                stream.Position = 0;
                return System.Convert.ToBase64String(stream.ToArray());
            }
        }
        public static JSONNode Deserialize(System.IO.BinaryReader aReader)
        {
            JSONBinaryTag type = (JSONBinaryTag)aReader.ReadByte();
            switch (type)
            {
                case JSONBinaryTag.Array:
                    {
                        int count = aReader.ReadInt32();
                        JSONArray tmp = new JSONArray();
                        for (int i = 0; i < count; i++)
                            tmp.Add(Deserialize(aReader));
                        return tmp;
                    }
                case JSONBinaryTag.Class:
                    {
                        int count = aReader.ReadInt32();
                        JSONClass tmp = new JSONClass();
                        for (int i = 0; i < count; i++)
                        {
                            string key = aReader.ReadString();
                            var val = Deserialize(aReader);
                            tmp.Add(key, val);
                        }
                        return tmp;
                    }
                case JSONBinaryTag.Value:
                    {
                        return new JSONData(aReader.ReadString());
                    }
                case JSONBinaryTag.IntValue:
                    {
                        return new JSONData(aReader.ReadInt32());
                    }
                case JSONBinaryTag.DoubleValue:
                    {
                        return new JSONData(aReader.ReadDouble());
                    }
                case JSONBinaryTag.BoolValue:
                    {
                        return new JSONData(aReader.ReadBoolean());
                    }
                case JSONBinaryTag.FloatValue:
                    {
                        return new JSONData(aReader.ReadSingle());
                    }

                default:
                    {
                        throw new Exception("Error deserializing JSON. Unknown tag: " + type);
                    }
            }
        }

#if USE_SharpZipLib
        public static JSONNode LoadFromCompressedStream(System.IO.Stream aData)
        {
            var zin = new ICSharpCode.SharpZipLib.BZip2.BZip2InputStream(aData);
            return LoadFromStream(zin);
        }
        public static JSONNode LoadFromCompressedFile(string aFileName)
        {
#if USE_FileIO
            using(var F = System.IO.File.OpenRead(aFileName))
            {
                return LoadFromCompressedStream(F);
            }
#else
            throw new Exception("Can't use File IO stuff in webplayer");
#endif
        }
        public static JSONNode LoadFromCompressedBase64(string aBase64)
        {
            var tmp = System.Convert.FromBase64String(aBase64);
            var stream = new System.IO.MemoryStream(tmp);
            stream.Position = 0;
            return LoadFromCompressedStream(stream);
        }
#else
        public static JSONNode LoadFromCompressedFile(string aFileName)
        {
            throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
        }
        public static JSONNode LoadFromCompressedStream(System.IO.Stream aData)
        {
            throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
        }
        public static JSONNode LoadFromCompressedBase64(string aBase64)
        {
            throw new Exception("Can't use compressed functions. You need include the SharpZipLib and uncomment the define at the top of SimpleJSON");
        }
#endif

        public static JSONNode LoadFromStream(System.IO.Stream aData)
        {
            using (var R = new System.IO.BinaryReader(aData))
            {
                return Deserialize(R);
            }
        }
        public static JSONNode LoadFromFile(string aFileName)
        {
#if USE_FileIO
            using (var F = System.IO.File.OpenRead(aFileName))
            {
                return LoadFromStream(F);
            }
#else
            throw new Exception("Can't use File IO stuff in webplayer");
#endif
        }
        public static JSONNode LoadFromBase64(string aBase64)
        {
            var tmp = System.Convert.FromBase64String(aBase64);
            var stream = new System.IO.MemoryStream(tmp);
            stream.Position = 0;
            return LoadFromStream(stream);
        }
    } // End of JSONNode

    public class JSONArray : JSONNode, IEnumerable
    {
        private List<JSONNode> m_List = new List<JSONNode>();
        public override JSONNode this[int aIndex]
        {
            get
            {
                if (aIndex < 0 || aIndex >= m_List.Count)
                    return new JSONLazyCreator(this);
                return m_List[aIndex];
            }
            set
            {
                if (aIndex < 0 || aIndex >= m_List.Count)
                    m_List.Add(value);
                else
                    m_List[aIndex] = value;
            }
        }
        public override JSONNode this[string aKey]
        {
            get { return new JSONLazyCreator(this); }
            set { m_List.Add(value); }
        }
        public override int Count
        {
            get { return m_List.Count; }
        }
        public override void Add(string aKey, JSONNode aItem)
        {
            m_List.Add(aItem);
        }
        public override JSONNode Remove(int aIndex)
        {
            if (aIndex < 0 || aIndex >= m_List.Count)
                return null;
            JSONNode tmp = m_List[aIndex];
            m_List.RemoveAt(aIndex);
            return tmp;
        }
        public override JSONNode Remove(JSONNode aNode)
        {
            m_List.Remove(aNode);
            return aNode;
        }
        public override IEnumerable<JSONNode> Childs
        {
            get
            {
                foreach (JSONNode N in m_List)
                    yield return N;
            }
        }
        public IEnumerator GetEnumerator()
        {
            foreach (JSONNode N in m_List)
                yield return N;
        }
        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("[ ");
            foreach (JSONNode N in m_List)
            {
                if (result.Length > 2)
                    result.Append(", ");

                result.Append(N.ToString());
            }

            result.Append(" ]");

            return result.ToString();

            /*
            string result = "[ ";
            foreach (JSONNode N in m_List)
            {
                if (result.Length > 2)
                    result += ", ";
                result += N.ToString();
            }
            result += " ]";
            return result;
            */
        }
        public override string ToString(string aPrefix)
        {
            StringBuilder result = new StringBuilder();

            result.Append("[ ");
            foreach (JSONNode N in m_List)
            {
                if (result.Length > 3)
                    result.Append(", ");

                result.Append("\n");
                result.Append(aPrefix);
                result.Append("   ");

                result.Append(N.ToString(aPrefix + "   "));
            }

            result.Append("\n");
            result.Append(aPrefix);
            result.Append("]");

            return result.ToString();

            /*
            string result = "[ ";
            foreach (JSONNode N in m_List)
            {
                if (result.Length > 3)
                    result += ", ";
                result += "\n" + aPrefix + "   ";
                result += N.ToString(aPrefix + "   ");
            }
            result += "\n" + aPrefix + "]";
            return result;
            */
        }
        public override void Serialize(System.IO.BinaryWriter aWriter)
        {
            aWriter.Write((byte)JSONBinaryTag.Array);
            aWriter.Write(m_List.Count);
            for (int i = 0; i < m_List.Count; i++)
            {
                m_List[i].Serialize(aWriter);
            }
        }
    } // End of JSONArray

    public class JSONClass : JSONNode, IEnumerable
    {
        private Dictionary<string, JSONNode> m_Dict = new Dictionary<string, JSONNode>();
        public override JSONNode this[string aKey]
        {
            get
            {
                if (m_Dict.ContainsKey(aKey))
                    return m_Dict[aKey];
                else
                    return new JSONLazyCreator(this, aKey);
            }
            set
            {
                if (m_Dict.ContainsKey(aKey))
                    m_Dict[aKey] = value;
                else
                    m_Dict.Add(aKey, value);
            }
        }
        public override JSONNode this[int aIndex]
        {
            get
            {
                if (aIndex < 0 || aIndex >= m_Dict.Count)
                    return null;
                return m_Dict.ElementAt(aIndex).Value;
            }
            set
            {
                if (aIndex < 0 || aIndex >= m_Dict.Count)
                    return;
                string key = m_Dict.ElementAt(aIndex).Key;
                m_Dict[key] = value;
            }
        }
        public override int Count
        {
            get { return m_Dict.Count; }
        }

        public override bool ContainsKey(string aKey) {
            return m_Dict.ContainsKey(aKey);
        }

        public override void Add(string aKey, JSONNode aItem)
        {
            if (!string.IsNullOrEmpty(aKey))
            {
                if (m_Dict.ContainsKey(aKey))
                    m_Dict[aKey] = aItem;
                else
                    m_Dict.Add(aKey, aItem);
            }
            else
                m_Dict.Add(Guid.NewGuid().ToString(), aItem);
        }

        public override JSONNode Remove(string aKey)
        {
            if (!m_Dict.ContainsKey(aKey))
                return null;
            JSONNode tmp = m_Dict[aKey];
            m_Dict.Remove(aKey);
            return tmp;
        }
        public override JSONNode Remove(int aIndex)
        {
            if (aIndex < 0 || aIndex >= m_Dict.Count)
                return null;
            var item = m_Dict.ElementAt(aIndex);
            m_Dict.Remove(item.Key);
            return item.Value;
        }
        public override JSONNode Remove(JSONNode aNode)
        {
            try
            {
                var item = m_Dict.Where(k => k.Value == aNode).First();
                m_Dict.Remove(item.Key);
                return aNode;
            }
            catch
            {
                return null;
            }
        }

        public override IEnumerable<JSONNode> Childs
        {
            get
            {
                foreach (KeyValuePair<string, JSONNode> N in m_Dict)
                    yield return N.Value;
            }
        }

        public override IEnumerable<string> Keys
        {
            get
            {
                foreach (var key in m_Dict.Keys)
                    yield return key;
            }
        }

        public IEnumerator GetEnumerator()
        {
            foreach (KeyValuePair<string, JSONNode> N in m_Dict)
                yield return N;
        }
        public override string ToString()
        {
            StringBuilder result = new StringBuilder();
            result.Append("{");
            foreach (KeyValuePair<string, JSONNode> N in m_Dict)
            {
                if (result.Length > 2)
                    result.Append(", ");

                result.Append("\"");
                result.Append(Escape(N.Key));
                result.Append("\":");
                result.Append(N.Value.ToString());
            }
            result.Append("}");

            return result.ToString();

            /*
            string result = "{";
            foreach (KeyValuePair<string, JSONNode> N in m_Dict)
            {
                if (result.Length > 2)
                    result += ", ";
                result += "\"" + Escape(N.Key) + "\":" + N.Value.ToString();
            }
            result += "}";
            return result;
            */
        }
        public override string ToString(string aPrefix)
        {
            StringBuilder result = new StringBuilder();
            result.Append("{ ");
            foreach (KeyValuePair<string, JSONNode> N in m_Dict)
            {
                if (result.Length > 3)
                    result.Append(", ");

                result.Append("\n");
                result.Append(aPrefix);
                result.Append("   ");

                result.Append("\"");
                result.Append(Escape(N.Key));
                result.Append("\" : ");
                result.Append(N.Value.ToString(aPrefix + "   "));
            }
            result.Append("\n");
            result.Append(aPrefix);
            result.Append("}");

            return result.ToString();

            /*
            string result = "{ ";
            foreach (KeyValuePair<string, JSONNode> N in m_Dict)
            {
                if (result.Length > 3)
                    result += ", ";
                result += "\n" + aPrefix + "   ";
                result += "\"" + Escape(N.Key) + "\" : " + N.Value.ToString(aPrefix + "   ");
            }
            result += "\n" + aPrefix + "}";
            return result;
            */
        }
        public override void Serialize(System.IO.BinaryWriter aWriter)
        {
            aWriter.Write((byte)JSONBinaryTag.Class);
            aWriter.Write(m_Dict.Count);
            foreach (string K in m_Dict.Keys)
            {
                aWriter.Write(K);
                m_Dict[K].Serialize(aWriter);
            }
        }
    } // End of JSONClass

    public class JSONData : JSONNode
    {
        private string m_Data;
        public override string Value
        {
            get { return m_Data; }
            set { m_Data = value; }
        }
        public JSONData(string aData)
        {
            m_Data = aData;
        }
        public JSONData(float aData)
        {
            AsFloat = aData;
        }
        public JSONData(double aData)
        {
            AsDouble = aData;
        }
        public JSONData(bool aData)
        {
            AsBool = aData;
        }
        public JSONData(int aData)
        {
            AsInt = aData;
        }

        public override string ToString()
        {
            bool asString = false;
            switch (valueType)
            {
                default:
                    asString = true;
                    break;
                case JSONBinaryTag.BoolValue:
                case JSONBinaryTag.IntValue:
                case JSONBinaryTag.DoubleValue:
                case JSONBinaryTag.FloatValue:
                    asString = false;
                    break;
            }

            if (asString)
            {
                return "\"" + Escape(m_Data) + "\"";
            }
            else {
                return m_Data;
            }
        }
        public override string ToString(string aPrefix)
        {
            return ToString();
        }

        public override void Serialize(System.IO.BinaryWriter aWriter)
        {
            var tmp = new JSONData("");

            tmp.AsInt = AsInt;
            if (tmp.m_Data == this.m_Data)
            {
                aWriter.Write((byte)JSONBinaryTag.IntValue);
                aWriter.Write(AsInt);
                return;
            }
            tmp.AsFloat = AsFloat;
            if (tmp.m_Data == this.m_Data)
            {
                aWriter.Write((byte)JSONBinaryTag.FloatValue);
                aWriter.Write(AsFloat);
                return;
            }
            tmp.AsDouble = AsDouble;
            if (tmp.m_Data == this.m_Data)
            {
                aWriter.Write((byte)JSONBinaryTag.DoubleValue);
                aWriter.Write(AsDouble);
                return;
            }

            tmp.AsBool = AsBool;
            if (tmp.m_Data == this.m_Data)
            {
                aWriter.Write((byte)JSONBinaryTag.BoolValue);
                aWriter.Write(AsBool);
                return;
            }
            aWriter.Write((byte)JSONBinaryTag.Value);
            aWriter.Write(m_Data);
        }
    } // End of JSONData

    internal class JSONLazyCreator : JSONNode
    {
        private JSONNode m_Node = null;
        private string m_Key = null;

        public JSONLazyCreator(JSONNode aNode)
        {
            m_Node = aNode;
            m_Key = null;
        }
        public JSONLazyCreator(JSONNode aNode, string aKey)
        {
            m_Node = aNode;
            m_Key = aKey;
        }

        private void Set(JSONNode aVal)
        {
            if (m_Key == null)
            {
                m_Node.Add(aVal);
            }
            else
            {
                m_Node.Add(m_Key, aVal);
            }
            m_Node = null; // Be GC friendly.
        }

        public override JSONNode this[int aIndex]
        {
            get
            {
                return new JSONLazyCreator(this);
            }
            set
            {
                var tmp = new JSONArray();
                tmp.Add(value);
                Set(tmp);
            }
        }

        public override JSONNode this[string aKey]
        {
            get
            {
                return new JSONLazyCreator(this, aKey);
            }
            set
            {
                var tmp = new JSONClass();
                tmp.Add(aKey, value);
                Set(tmp);
            }
        }
        public override void Add(JSONNode aItem)
        {
            var tmp = new JSONArray();
            tmp.Add(aItem);
            Set(tmp);
        }
        public override void Add(string aKey, JSONNode aItem)
        {
            var tmp = new JSONClass();
            tmp.Add(aKey, aItem);
            Set(tmp);
        }
        public static bool operator ==(JSONLazyCreator a, object b)
        {
            if (b == null)
                return true;
            return System.Object.ReferenceEquals(a, b);
        }

        public static bool operator !=(JSONLazyCreator a, object b)
        {
            return !(a == b);
        }
        public override bool Equals(object obj)
        {
            if (obj == null)
                return true;
            return System.Object.ReferenceEquals(this, obj);
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return "\"\"";
        }
        public override string ToString(string aPrefix)
        {
            return "\"\"";
        }

        public override int AsInt
        {
            get
            {
                JSONData tmp = new JSONData(0);
                Set(tmp);
                return 0;
            }
            set
            {
                JSONData tmp = new JSONData(value);
                Set(tmp);
            }
        }
        public override float AsFloat
        {
            get
            {
                JSONData tmp = new JSONData(0.0f);
                Set(tmp);
                return 0.0f;
            }
            set
            {
                JSONData tmp = new JSONData(value);
                Set(tmp);
            }
        }
        public override double AsDouble
        {
            get
            {
                JSONData tmp = new JSONData(0.0);
                Set(tmp);
                return 0.0;
            }
            set
            {
                JSONData tmp = new JSONData(value);
                Set(tmp);
            }
        }
        public override bool AsBool
        {
            get
            {
                JSONData tmp = new JSONData(false);
                Set(tmp);
                return false;
            }
            set
            {
                JSONData tmp = new JSONData(value);
                Set(tmp);
            }
        }
        public override JSONArray AsArray
        {
            get
            {
                JSONArray tmp = new JSONArray();
                Set(tmp);
                return tmp;
            }
        }
        public override JSONClass AsObject
        {
            get
            {
                JSONClass tmp = new JSONClass();
                Set(tmp);
                return tmp;
            }
        }
    } // End of JSONLazyCreator

    public static class JSON
    {
        public static JSONNode Parse(string aJSON)
        {
            return JSONNode.Parse(aJSON);
        }
    }

    public static class test
    {
        public static JSONClass TestClass()
        {
            JSONClass container = new JSONClass();
            JSONClass subContainer = new JSONClass();
            JSONArray subArray = new JSONArray();

            subContainer["key1"] = "value1";

            subArray[0].AsInt = 0;
            subArray[1].AsInt = 1;
            subArray[2] = "2";
            subArray[3] = "3";

            container["boolean true"].AsBool = true;
            container["boolean false"].AsBool = false;
            container["int 0"].AsInt = 0;
            container["int 1"].AsInt = 1;
            container["float 0"].AsFloat = 0.0f;
            container["float 1"].AsFloat = 1.0f;
            container["double 0"].AsDouble = 0.0;
            container["double 1"].AsDouble = 1.0;
            container["string hello"] = "hello";
            container["string 0"] = "0";
            container["class"] = subContainer;
            container["array"] = subArray;

            return container;
        }

        public static string TestString()
        {
            return TestClass().ToString();
        }

        public static bool HasExpectedOutput()
        {
            string actualOutput = TestString();
            string expectedOutput = "{\"boolean true\":true, \"boolean false\":false, \"int 0\":0, \"int 1\":1, \"float 0\":0, \"float 1\":1, \"double 0\":0, \"double 1\":1, \"string hello\":\"hello\", \"string 0\":\"0\", \"class\":{\"key1\":\"value1\"}, \"array\":[ 0, 1, \"2\", \"3\" ]}";
            return actualOutput == expectedOutput;
        }
    }
}
