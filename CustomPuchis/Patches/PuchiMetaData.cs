using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace CustomPuchis.Patches
{
    internal class PuchiMetaData
    {
        public string Id;
        // Technically optional, just defaults to 0
        public int Order = 0; 
        public WordListInfo NameInfo;
        public WordListInfo DescriptionInfo;
        // Optional. If missing, just takes all images in alphabetical order (hopefully it goes 8, 9, 10, and not 1, 10, 2)
        // I guess it's not my problem at that point, the user has multiple options to fix that
        public List<string> Files = new List<string>();
        public string SpriteFile;

        public string FolderPath;

        public string StringId => "custom_puchi_id_" + Id;

        public string NameKey => "costume_" + StringId;
        public string DescriptionKey => "costume_description_" + StringId;

        public string Name(string language) => GetLocalizedText(NameInfo, language);
        public int NameFont(string language) => GetLocalizedFontType(NameInfo, language);
        public string Description(string language) => GetLocalizedText(DescriptionInfo, language);
        public int DescriptionFont(string language) => GetLocalizedFontType(DescriptionInfo, language);

        string GetLocalizedText(WordListInfo info, string language)
        {
            if (info == null)
            {
                return string.Empty;
            }

            switch (language)
            {
                case nameof(SystemLanguage.Japanese): return info.jpText;
                case nameof(SystemLanguage.English): return info.enText;
                case nameof(SystemLanguage.French): return info.frText;
                case nameof(SystemLanguage.Italian): return info.itText;
                case nameof(SystemLanguage.German): return info.deText;
                case nameof(SystemLanguage.Spanish): return info.esText;
                case nameof(SystemLanguage.Chinese): 
                case nameof(SystemLanguage.ChineseTraditional): return info.tcText;
                case nameof(SystemLanguage.ChineseSimplified): return info.scText;
                case nameof(SystemLanguage.Korean): return info.krText;

                default:
                    return info.enText;
            }
        }

        int GetLocalizedFontType(WordListInfo info, string language)
        {
            if (info == null)
            {
                return 1;
            }

            switch (language)
            {
                case nameof(SystemLanguage.Japanese): return info.jpFontType;
                case nameof(SystemLanguage.English): return info.enFontType;
                case nameof(SystemLanguage.French): return info.frFontType;
                case nameof(SystemLanguage.Italian): return info.itFontType;
                case nameof(SystemLanguage.German): return info.deFontType;
                case nameof(SystemLanguage.Spanish): return info.esFontType;
                case nameof(SystemLanguage.Chinese):
                case nameof(SystemLanguage.ChineseTraditional): return info.tcFontType;
                case nameof(SystemLanguage.ChineseSimplified): return info.scFontType;
                case nameof(SystemLanguage.Korean): return info.krFontType;

                default:
                    return info.enFontType;
            }
        }
    }
}
