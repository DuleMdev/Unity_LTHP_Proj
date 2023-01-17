using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PDF_Object
{
    bool b;
    int i;
    float f;
    string s; // lehet string és /name is
    List<PDF_Object> array;
    Dictionary<string, PDF_Object> dictionary;
    public byte[] streamData;
    public PDF_Object indirectData;

    PDF_ObjectType objectType;
    public PDF_ObjectType getObjectType { get { return objectType; } }

    // Reference object
    int objectNumber;
    public int GetObjectNumber
    {
        get
        {
            if (objectType != PDF_ObjectType.referenceObject)
                throw new Exception("Az objektum nem reference típusú : " + objectType);

            return objectNumber;
        }
    }

    int generationNumber;
    public int GetGenerationNumber
    {
        get
        {
            if (objectType != PDF_ObjectType.referenceObject)
                throw new Exception("Az objektum nem reference típusú : " + objectType);

            return generationNumber;
        }
    }

    public int getLength
    {
        get
        {
            if (objectType == PDF_ObjectType.arrayObject)
                return array.Count;
            else if (objectType == PDF_ObjectType.dictionaryObject)
                return dictionary.Count;
            else
                throw new Exception("Az objektumnak nincs length paramétere : " + objectType);
        }
    }

    public PDF_Object()
    {
        objectType = PDF_ObjectType.nullObject;
    }

    public PDF_Object(bool b)
    {
        this.b = b;
        objectType = PDF_ObjectType.booleanObject;
    }

    public PDF_Object(int i)
    {
        this.i = i;
        objectType = PDF_ObjectType.integerObject;
    }

    public PDF_Object(float f)
    {
        this.f = f;
        objectType = PDF_ObjectType.floatObject;
    }

    public PDF_Object(string s, bool name = false)
    {
        this.s = s;
        objectType = name ? PDF_ObjectType.nameObject : PDF_ObjectType.stringObject;
    }

    public PDF_Object(byte[] streamData)
    {
        this.streamData = streamData;
        objectType = PDF_ObjectType.streamObject;
    }

    // Indirect objectum létrehozása
    public PDF_Object(int objectNumber, int generationNumber, PDF_Object indirectData, byte[] streamData)
    {
        this.objectNumber = objectNumber;
        this.generationNumber = generationNumber;
        this.streamData = streamData;
        this.indirectData = indirectData;
        objectType = PDF_ObjectType.indirectObject;
    }

    public PDF_Object(int objectNumber, int generationNumber)
    {
        this.objectNumber = objectNumber;
        this.generationNumber = generationNumber;
        objectType = PDF_ObjectType.referenceObject;
    }

    public void setArrayObject()
    {
        objectType = PDF_ObjectType.arrayObject;
        array = new List<PDF_Object>();
    }

    public void AddArrayItem(PDF_Object pdfObject)
    {
        if (objectType != PDF_ObjectType.arrayObject)
            throw new Exception("Az objectum nem array típusú így az add metódus végrehajtása nem lehetséges.");

        array.Add(pdfObject);
    }

    public void SetDictionaryObject()
    {
        objectType = PDF_ObjectType.dictionaryObject;
        dictionary = new Dictionary<string, PDF_Object>();
    }

    public void AddDictionaryItem(PDF_Object key, PDF_Object value)
    {
        if (objectType != PDF_ObjectType.dictionaryObject)
            throw new Exception("Az objectum nem dictionary típusú így az add metódus végrehajtása nem lehetséges.");

        if (key.objectType != PDF_ObjectType.nameObject)
            throw new Exception("A Dictionary kulcs értéke csak nameObject lehet.");

        dictionary.Add(key.getName(), value);
    }

    public bool getBool()
    {
        if (objectType != PDF_ObjectType.booleanObject)
            throw new Exception("Ez az objectum nem logikai.");

        return b;
    }

    public int getInteger()
    {
        if (objectType != PDF_ObjectType.integerObject)
            throw new Exception("Ez az objectum nem integer.");

        return i;
    }

    public float getFloat()
    {
        if (objectType != PDF_ObjectType.floatObject)
            throw new Exception("Ez az objectum nem float.");

        return f;
    }

    public string getString()
    {
        if (objectType != PDF_ObjectType.stringObject)
            throw new Exception("Ez az objectum nem string.");

        return s;
    }

    public string getName()
    {
        if (objectType != PDF_ObjectType.nameObject)
            throw new Exception("Ez az objectum nem nameObject.");

        return s;
    }

    public PDF_Object this[int i]
    {
        get
        {
            if (objectType != PDF_ObjectType.arrayObject)
                throw new Exception("Az objektumot nem lehet indexelni. " + objectType);
            return array[i];
        }
    }

    public PDF_Object this[string s]
    {
        get
        {
            if (objectType != PDF_ObjectType.dictionaryObject)
                throw new Exception("Az objektumot nem lehet indexelni. " + objectType);

            return dictionary[s];
        }
    }

    public bool ContainsKey(string key)
    {
        if (objectType != PDF_ObjectType.dictionaryObject)
            throw new Exception("Az objektum nem szótár típusú. " + objectType);

        return dictionary.ContainsKey(key);
    }

    public override string ToString()
    {
        return ToString("", "    ");
    }

    public string ToString(string indent, string indentInc)
    {
        string result = "";
        switch (objectType)
        {
            case PDF_ObjectType.booleanObject:
                return b.ToString();
                break;
            case PDF_ObjectType.integerObject:
                return i.ToString();
                break;
            case PDF_ObjectType.floatObject:
                return f.ToString();
                break;
            case PDF_ObjectType.stringObject:
                return s;
                break;
            case PDF_ObjectType.nameObject:
                return s;
                break;
            case PDF_ObjectType.arrayObject:
                result = "[\n";
                for (int i = 0; i < array.Count; i++)
                {
                    result += indent + indentInc + array[i].ToString(indent + indentInc, indentInc) + "\n";
                }
                result += indent + "]";
                return result;
                break;
            case PDF_ObjectType.dictionaryObject:
                result = "<<\n";
                foreach (var keyValue in dictionary)
                    result += string.Format(indent + indentInc + "{0} {1}\n", keyValue.Key, keyValue.Value.ToString(indent + indentInc, indentInc));
                result += indent + ">>";
                return result;
                break;
            case PDF_ObjectType.nullObject:
                return "null";
                break;
            case PDF_ObjectType.indirectObject:
                result = string.Format("{0} {1} obj\n", objectNumber, generationNumber);
                result += indirectData.ToString(indent, indentInc);
                result += "\nendobj";
                return result;
                break;
            case PDF_ObjectType.referenceObject:
                return string.Format("{0} {1} R", objectNumber, generationNumber);
                break;
            default:
                break;
        }

        return "";
    }
}
