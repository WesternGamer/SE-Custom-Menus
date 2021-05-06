

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;
using VRage.FileSystem;
using VRage.Utils;

namespace SE_Custom_Menus.Utill
{
    public partial class MyXmlUtill
    {
        private const string fileName = "config.xml";

        private HashSet<string> enabledPlugins = new HashSet<string>();
        private string filePath;

        [XmlArray]
        [XmlArrayItem("Id")]
        public string[] Plugins
        {
            get
            {
                return enabledPlugins.ToArray();
            }
            set
            {
                enabledPlugins = new HashSet<string>(value);
            }
        }

        public string ListHash { get; set; }

        public int Count => enabledPlugins.Count;

        public MyXmlUtill()
        {

        }

        public void Init(MyMenuPacksFinder plugins)
        {
            // Remove plugins from config that no longer exist
            List<string> toRemove = new List<string>();

            foreach (string id in enabledPlugins)
            {
                if (!plugins.IsInstalled(id))
                {
                    MyLog.Default.WriteLine($"{id} is no longer available.");
                    toRemove.Add(id);
                }
            }

            foreach (string id in toRemove)
                enabledPlugins.Remove(id);

            Save();
        }

        public void Disable()
        {
            enabledPlugins.Clear();
        }


        public void Save()
        {
            try
            {
                MyLog.Default.WriteLine("[SE Custom Menus]: Saving config.");
                XmlSerializer serializer = new XmlSerializer(typeof(MyXmlUtill));
                if (File.Exists(filePath))
                    File.Delete(filePath);
                FileStream fs = File.OpenWrite(filePath);
                serializer.Serialize(fs, this);
                fs.Flush();
                fs.Close();
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine($"[SE Custom Menus]: An error occurred while saving plugin config: " + e);
            }
        }

        public static MyXmlUtill Load(string mainDirectory)
        {
            string path = Path.Combine(mainDirectory, fileName);
            if (File.Exists(path))
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(MyXmlUtill));
                    FileStream fs = File.OpenRead(path);
                    MyXmlUtill config = (MyXmlUtill)serializer.Deserialize(fs);
                    fs.Close();
                    config.filePath = path;
                    return config;
                }
                catch (Exception e)
                {
                    MyLog.Default.WriteLine($"An error occurred while loading plugin config: " + e);
                }
            }


            var temp = new MyXmlUtill();
            temp.filePath = path;
            return temp;
        }

        public IEnumerator<string> GetEnumerator()
        {
            return enabledPlugins.GetEnumerator();
        }

        public bool IsEnabled(string id)
        {
            return enabledPlugins.Contains(id);
        }

        public void SetEnabled(string id, bool enabled)
        {
            if (enabled)
                enabledPlugins.Add(id);
            else
                enabledPlugins.Remove(id);
        }
    }
}