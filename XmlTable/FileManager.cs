using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
namespace XmlTable
{

    public class FileManager
    {
        public static string Serialize<T>(T t)
        {
            using (StringWriter sw = new StringWriter())
            {
                if (t == null || t.GetType() == null) { return ""; }
                XmlSerializer xz = new XmlSerializer(t.GetType());
                xz.Serialize(sw, t);
                return sw.ToString();
            }
        }
        public static T Deserialize<T>(string s) where T : class
        {
            using (StringReader sr = new StringReader(s))
            {
                XmlSerializer xz = new XmlSerializer(typeof(T));
                return xz.Deserialize(sr) as T;
            }
        }
        public static string Save(string path, string data)
        {
            using (var file = System.IO.File.Create(path))
            {
                using (var sw = new System.IO.StreamWriter(file))
                {
                    sw.Write(data);
                }
            }
            return path;
        }
        public static string Load(string path)
        {
            string data = "";
            if (!System.IO.File.Exists(path))
            {
                return data;
            }
            using (var file = System.IO.File.Open(path, System.IO.FileMode.Open))
            {
                using (var sw = new System.IO.StreamReader(file))
                {
                    while (!sw.EndOfStream)
                    {

                        data += sw.ReadLine();
                    }
                }
            }
            return data;
        }
    }
}



