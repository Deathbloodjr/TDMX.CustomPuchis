using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomPuchis.Patches
{
    internal class PuchiMetaData
    {
        public string Id;
        // Technically optional, just defaults to 0
        public int Order = 0; 
        public WordListInfo Name;
        public WordListInfo Description;
        // Optional. If missing, just takes all images in alphabetical order (hopefully it goes 8, 9, 10, and not 1, 10, 2)
        // I guess it's not my problem at that point, the user has multiple options to fix that
        public List<string> Files = new List<string>();

        public string FolderPath;

        public string StringId => "custom_puchi_id_" + Id;

        public string NameKey => "costume_" + StringId;
        public string DescriptionKey => "costume_description_" + StringId;
    }
}
