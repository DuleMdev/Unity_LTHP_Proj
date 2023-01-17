/*
source:
http://www.unicode.org/charts/





*/

using System.Collections.Generic;

namespace TTF_Font
{
    public class CharacterCodeClasses
    {
        static List<CharacterCodeClass> ListOfCharacterCodeClasses;
        static CharacterCodeClass _null;


        static CharacterCodeClasses()
        {
            _null = new CharacterCodeClass(0, 0, Categories.Unknown, MainGroup.Unknown, Groups.Unknown, Names.Unknown);

            ListOfCharacterCodeClasses = new List<CharacterCodeClass>
            {
                new CharacterCodeClass(0x0530, 0x058F, Categories.Scripts, MainGroup.European_Scripts, Groups.Armenian, Names.Unknown),
                new CharacterCodeClass(0xFb13, 0xFB17, Categories.Scripts, MainGroup.European_Scripts, Groups.Armenian, Names.Armenian_Ligatures),
                new CharacterCodeClass(0x102A0, 0x102DF, Categories.Scripts, MainGroup.European_Scripts, Groups.Carian, Names.Unknown),
                new CharacterCodeClass(0x10530, 0x1056F, Categories.Scripts, MainGroup.European_Scripts, Groups.Caucasian_Albanian, Names.Unknown),
                new CharacterCodeClass(0x10800, 0x1083F, Categories.Scripts, MainGroup.European_Scripts, Groups.Cypriot_Syllabary, Names.Unknown),
                new CharacterCodeClass(0x0400, 0x04FF, Categories.Scripts, MainGroup.European_Scripts, Groups.Cyrillic, Names.Unknown),
                new CharacterCodeClass(0x0500, 0x052F, Categories.Scripts, MainGroup.European_Scripts, Groups.Cyrillic, Names.Cyrillic_Supplement),
                new CharacterCodeClass(0x2DE0, 0x2DFF, Categories.Scripts, MainGroup.European_Scripts, Groups.Cyrillic, Names.Cyrillic_Extended_A),
                new CharacterCodeClass(0xA640, 0xA69F, Categories.Scripts, MainGroup.European_Scripts, Groups.Cyrillic, Names.Cyrillic_Extended_B),
                new CharacterCodeClass(0x1C80, 0x1C8F, Categories.Scripts, MainGroup.European_Scripts, Groups.Cyrillic, Names.Cyrillic_Extended_C),
                new CharacterCodeClass(0x10500, 0x1052F, Categories.Scripts, MainGroup.European_Scripts, Groups.Elbasan, Names.Unknown),
                new CharacterCodeClass(0x10A0, 0x10FF, Categories.Scripts, MainGroup.European_Scripts, Groups.Georgian, Names.Unknown),
                new CharacterCodeClass(0x1C90, 0x1CBF, Categories.Scripts, MainGroup.European_Scripts, Groups.Georgian, Names.Georgian_Extended),
                new CharacterCodeClass(0x2D00, 0x2D2F, Categories.Scripts, MainGroup.European_Scripts, Groups.Georgian, Names.Georgian_Supplement),
                new CharacterCodeClass(0x2C00, 0x2C5F, Categories.Scripts, MainGroup.European_Scripts, Groups.Glagolitic, Names.Unknown),
                new CharacterCodeClass(0x1E000, 0x1E02F, Categories.Scripts, MainGroup.European_Scripts, Groups.Glagolitic, Names.Glagolitic_Supplement),
                new CharacterCodeClass(0x10330, 0x1034F, Categories.Scripts, MainGroup.European_Scripts, Groups.Gothic, Names.Unknown),
                new CharacterCodeClass(0x0370, 0x03FF, Categories.Scripts, MainGroup.European_Scripts, Groups.Greek, Names.Unknown),
                new CharacterCodeClass(0x1F00, 0x1FFF, Categories.Scripts, MainGroup.European_Scripts, Groups.Greek, Names.Greek_Extended),
                new CharacterCodeClass(0x10140, 0x1018F, Categories.Scripts, MainGroup.European_Scripts, Groups.Greek, Names.Ancient_Greek_Numbers),
                new CharacterCodeClass(0x0000, 0x007F, Categories.Scripts, MainGroup.European_Scripts, Groups.Latin, Names.Unknown),
                new CharacterCodeClass(0x0080, 0x00FF, Categories.Scripts, MainGroup.European_Scripts, Groups.Latin, Names.Latin_1_Supplement),
                new CharacterCodeClass(0x0100, 0x017F, Categories.Scripts, MainGroup.European_Scripts, Groups.Latin, Names.Latin_Extended_A),
                new CharacterCodeClass(0x0180, 0x024F, Categories.Scripts, MainGroup.European_Scripts, Groups.Latin, Names.Latin_Extended_B),
                new CharacterCodeClass(0x2C60, 0x2C7F, Categories.Scripts, MainGroup.European_Scripts, Groups.Latin, Names.Latin_Extended_C),
                new CharacterCodeClass(0xA720, 0xA7FF, Categories.Scripts, MainGroup.European_Scripts, Groups.Latin, Names.Latin_Extended_D),
                new CharacterCodeClass(0xAB30, 0xAB6F, Categories.Scripts, MainGroup.European_Scripts, Groups.Latin, Names.Latin_Extended_E),
                new CharacterCodeClass(0x1E00, 0x1EFF, Categories.Scripts, MainGroup.European_Scripts, Groups.Latin, Names.Latin_Extended_Additional),
                new CharacterCodeClass(0xFB00, 0xFB06, Categories.Scripts, MainGroup.European_Scripts, Groups.Latin, Names.Latin_Ligatures),
                new CharacterCodeClass(0xFF00, 0xFF5E, Categories.Scripts, MainGroup.European_Scripts, Groups.Latin, Names.Fullwidth_Latin_Letters),
                new CharacterCodeClass(0x0250, 0x02AF, Categories.Scripts, MainGroup.European_Scripts, Groups.Latin, Names.IPA_Extensions),
                new CharacterCodeClass(0x1D00, 0x1D7F, Categories.Scripts, MainGroup.European_Scripts, Groups.Latin, Names.Phonetic_Extensions),
                new CharacterCodeClass(0x1D80, 0x1DBF, Categories.Scripts, MainGroup.European_Scripts, Groups.Latin, Names.Phonetic_Extensions_Supplement),


            };
        }

        static public CharacterCodeClass GetCharacterCodeClass(int charCode)
        {
            foreach (var item in ListOfCharacterCodeClasses)
                if (item.from <= charCode && item.unto >= charCode)
                    return item;

            return _null;
        }
    }

    public class CharacterCodeClass
    {
        public int from;
        public int unto;

        public Categories category;
        public MainGroup mainGroup;
        public Groups group;
        public Names name;

        public string getName {
            get {
                if (name != Names.Unknown)
                    return name.ToString();

                if (group != Groups.Unknown)
                    return group.ToString();

                if (mainGroup != MainGroup.Unknown)
                    return mainGroup.ToString();

                return category.ToString();
            }
        }

        public CharacterCodeClass(int from, int unto, Categories category, MainGroup mainGroup, Groups group, Names name)
        {
            this.from = from;
            this.unto = unto;
            this.category = category;
            this.mainGroup = mainGroup;
            this.group = group;
            this.name = name;
        }

        public bool Contain(int code)
        {
            return code >= from && code <= unto;
        }
    }

    public enum Categories
    {
        Unknown,
        Scripts,
        Symbol_and_Punctuation
    }

    public enum MainGroup
    {
        Unknown,
        European_Scripts,
        African_Scripts,
        Middle_Eastern_Scripts,
        Central_Asian_Scripts,
        South_Asian_Scripts,
        Southeast_Asian_Scripts,
        Indonesia_and_Oceania_Scripts,
        East_Asian_Scripts,
        American_Scripts,

        Modifier_Letters,
        Combining_Marks,
        Other,

        Notational_Systems,
        Punctuation,
        Alphanumeric_Symbols,
        Technical_Symbols,
        Numbers_and_Digits,
        Mathematical_Symbols,
        Emoji_and_Pictographs,
        Other_Symbols,
        Specials,
        Private_Use,
        Surrogates,
        Noncharacters_in_Charts
    }

    public enum Groups
    {
        Unknown,

        Armenian,
        Carian,
        Caucasian_Albanian,
        Cypriot_Syllabary,
        Cyrillic,
        Elbasan,
        Georgian,
        Glagolitic,
        Gothic,
        Greek,
        Latin,
        Linear_A,
        Linear_B,
        Lycian,
        Lydian,
        Ogham,
        Old_Hungarian,
        Old_Italic,
        Old_Permic,
        Phaistos_Disc,
        Runic,
        Shavian,

    }

    public enum Names
    {
        Unknown,
        Armenian_Ligatures,
        Cyrillic_Supplement,
        Cyrillic_Extended_A,
        Cyrillic_Extended_B,
        Cyrillic_Extended_C,
        Georgian_Extended,
        Georgian_Supplement,
        Glagolitic_Supplement,
        Greek_Extended,
        Ancient_Greek_Numbers,
        Basic_Latin_ASCII,
        Latin_1_Supplement,
        Latin_Extended_A,
        Latin_Extended_B,
        Latin_Extended_C,
        Latin_Extended_D,
        Latin_Extended_E,
        Latin_Extended_Additional,
        Latin_Ligatures,
        Fullwidth_Latin_Letters,
        IPA_Extensions,
        Phonetic_Extensions,
        Phonetic_Extensions_Supplement,
        Linear_B_Syllabary,
        Linear_B_ideograams,
        Aegean_Numbers,
    }

}

