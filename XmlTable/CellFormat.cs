using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XmlTable
{
  
    public class CellFormat
    {
        public static Dictionary<string, string> formatList;
        public static void Init()
        {
            if (formatList == null)
            {
                formatList = new Dictionary<string, string>();
                //formatList.Add("true", "√");
                //formatList.Add("false", "×");
              
            }
        }
        public static string Format(string data)
        {
            Init();
            foreach (var kv in formatList)
            {
                if (data == kv.Key)
                {
                    return kv.Value;
                }
            }
            return data;
        }
    }
}
