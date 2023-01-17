using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

static public class RegexSplitter
{
    static public RegexSplitterResult Split(string text, params string[] regPatterns)
    {
        RegexSplitterResult regexSplitterResult = new RegexSplitterResult();

        string unknowChars = "";

        while (text.Length > 0)
        {
            bool success = false;

            for (int i = 0; i < regPatterns.Length; i++)
            {
                Match match = Regex.Match(text, "^" + regPatterns[i]);

                if (match.Success)
                {
                    success = true;

                    AddRegexData(regexSplitterResult, 0, unknowChars, null);
                    unknowChars = "";

                    AddRegexData(regexSplitterResult, i + 1, match.Value, match);
                    text = text.Substring(match.Length);
                }
            }

            if (!success)
            {
                unknowChars += text[0].ToString();
                text = text.Substring(1);
            }
        }

        AddRegexData(regexSplitterResult, 0, unknowChars, null);

        return regexSplitterResult;
    }

    static void AddRegexData(RegexSplitterResult regexSplitterResult, int index, string value, Match match)
    {
        if (value.Length > 0)
            regexSplitterResult.Add(index, value, match);
    }

    public class RegexSplitterResult
    {
        List<RegexsplitterResultData> list = new List<RegexsplitterResultData>();

        public int Count
        {
            get { return list.Count; }
        }

        public RegexsplitterResultData this[int i]
        {
            get { return list[i]; }
        }

        public void Add(int index, string value, Match match)
        {
            list.Add(new RegexsplitterResultData(index, value, match));
        }

        override public string ToString()
        {
            string s = "";
            for (int i = 0; i < list.Count; i++)
            {
                s += list[i].index + ". " + list[i].value + "\n";
            }

            return s;
        }

    }

    public struct RegexsplitterResultData
    { 
        public readonly int index;
        public readonly string value;
        public Match match;

        public RegexsplitterResultData(int index, string value, Match match)
        {
            this.index = index;
            this.value = value;
            this.match = match;
        }
    }
}
