
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using ProtoBuf;
using System.Linq;

using System.IO.Compression;
using VRage.FileSystem;
using VRage.Utils;
using System.Security.Principal;
using System.Security.Permissions;

namespace SE_Custom_Menus.Utill
{
    public class MyMenuPacksFinder : IEnumerable<PluginData>
    {
        private readonly Dictionary<string, PluginData> plugins = new Dictionary<string, PluginData>();

        public int Count => plugins.Count;

        public PluginData this[string key]
        {
            get => plugins[key];
            set => plugins[key] = value;
        }

        public MyMenuPacksFinder(string mainDirectory, MyXmlUtill config)
        {



            MyLog.Default.WriteLine("[SE Custom Menus]: Finding installed Menu Packs...");
            FindWorkshopPlugins();
            FindLocalMenuPacks(mainDirectory);
            MyLog.Default.WriteLine($"[SE Custom Menus]: Found {plugins.Count} Menu Packs.");
            FindPluginGroups();
        }

        private void FindPluginGroups()
        {
            int groups = 0;
            foreach (var group in plugins.Values.Where(x => !string.IsNullOrWhiteSpace(x.GroupId)).GroupBy(x => x.GroupId))
            {
                groups++;
                foreach (PluginData data in group)
                    data.Group.AddRange(group.Where(x => x != data));
            }
            if (groups > 0)
                MyLog.Default.WriteLine($"[SE Custom Menus]: Found {groups} plugin groups.");
        }

        

        private static void Save(PluginData data, string path)
        {
            XmlSerializer xml = new XmlSerializer(typeof(PluginData));
            using (Stream file = File.Create(path))
            {
                xml.Serialize(file, data);
            }
        }


        public bool IsInstalled(string id)
        {
            return plugins.TryGetValue(id, out PluginData data) && data.Status != PluginStatus.NotInstalled;
        }

        private void FindLocalMenuPacks(string mainDirectory)
        {
            foreach (string zip in Directory.EnumerateFiles(Path.GetFullPath(Path.Combine(MyFileSystem.UserDataPath, "MenuPacks")), "*.zip", SearchOption.AllDirectories))
            {
                    LocalMenuPack local = new LocalMenuPack(zip);                    
                        plugins[local.Id] = local;
                UnZipMenuPack(zip, Path.GetFullPath(Path.Combine(MyFileSystem.UserDataPath, "MenuPacks\\Temp")));
            }
        }

        private void FindWorkshopPlugins()
        {
            string workshop = Path.GetFullPath(@"..\..\..\workshop\content\244850\");

            foreach (string mod in Directory.EnumerateDirectories(workshop))
            {

                try
                {
                    string folder = Path.GetFileName(mod);
                    if (ulong.TryParse(folder, out ulong modId) && SteamAPI.IsSubscribed(modId) && TryGetMenuPack(mod, out string newPlugin))
                    {
                        if (plugins.TryGetValue(folder, out PluginData data) && data is SteamPlugin steam)
                            steam.Init(newPlugin);
                        else
                            MyLog.Default.WriteLine($"[SE Custom Menus]: The item {folder} is not on the plugin list.");
                    }
                }
                catch (Exception e)
                {
                    MyLog.Default.WriteLine($"[SE Custom Menus]: An error occurred while searching {mod} for a plugin: {e}");
                }
            }
        }

        private bool TryGetMenuPack(string modRoot, out string pluginFile)
        {

            foreach (string file in Directory.EnumerateFiles(modRoot, "*.menupack"))
            {
                string name = Path.GetFileName(file);
                
                    pluginFile = file;
                    return true;
                
            }

            string sepm = Path.Combine(modRoot, "Data", "sepm-plugin.zip");
            if (File.Exists(sepm))
            {
                pluginFile = sepm;
                return true;
            }
            pluginFile = null;
            return false;
        }



        public IEnumerator<PluginData> GetEnumerator()
        {
            return plugins.Values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return plugins.Values.GetEnumerator();
        }

        public static void UnZipMenuPack(string input, string output)
        {
            
            try
            {
                AppDomain.CurrentDomain.SetPrincipalPolicy(PrincipalPolicy.WindowsPrincipal);
                PrincipalPermission principalPerm = new PrincipalPermission(null, "Administrators");
                principalPerm.Demand();
                ZipFile.CreateFromDirectory(input, output);
            }
            catch (Exception e)
            {
                MyLog.Default.WriteLine("[SE Custom Menus]: Error Unzipping Menu Pack: " + e.Message);
            }
        }
    }
}