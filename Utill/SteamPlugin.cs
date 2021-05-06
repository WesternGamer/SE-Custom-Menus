using ProtoBuf;
using Sandbox.Graphics.GUI;
using SE_Custom_Menus.Utill;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using VRage.Utils;

namespace SE_Custom_Menus.Utill
{ 
    [ProtoContract]
    
    [ProtoInclude(102, typeof(WorkshopPlugin))]
    public abstract class SteamPlugin : PluginData
    {
        [XmlIgnore]
        public ulong WorkshopId { get; private set; }

        [XmlArray]
        [ProtoMember(1)]
        public string[] AllowedHashes { get; set; }

        public override string Id
        {
            get
            {
                return base.Id;
            }
            set
            {
                base.Id = value;
                WorkshopId = ulong.Parse(Id);
            }
        }

        protected abstract string HashFile { get; }
        protected string root, sourceFile, hashFile;

        protected SteamPlugin()
        {
        }

        public void Init(string sourceFile)
        {
            Status = PluginStatus.None;
            this.sourceFile = sourceFile;
            root = Path.GetDirectoryName(sourceFile);
            hashFile = Path.Combine(root, HashFile);

            CheckForUpdates();
        }

        protected virtual void CheckForUpdates()
        {
            if (File.Exists(hashFile))
            {
                string oldHash = File.ReadAllText(hashFile);
                string newHash = GetHash1(sourceFile);
                if (oldHash != newHash)
                    Status = PluginStatus.PendingUpdate;
            }
            else
            {
                Status = PluginStatus.PendingUpdate;
            }
        }

        public override Assembly GetAssembly()
        {
            if (Status == PluginStatus.PendingUpdate)
            {
                MyLog.Default.WriteLine("[SE Custom Menus]: Updating " + this);
                ApplyUpdate();
                if (Status == PluginStatus.PendingUpdate)
                {
                    File.WriteAllText(hashFile, GetHash1(sourceFile));
                    Status = PluginStatus.Updated;
                }
                else
                {
                    return null;
                }

            }
            string dll = GetAssemblyFile();
            if (dll == null || !File.Exists(dll))
                return null;
            Assembly a = Assembly.LoadFile(dll);
            Version = a.GetName().Version;
            return a;
        }

        public static string GetHash1(string file)
        {
            using (SHA1Managed sha = new SHA1Managed())
            {
                return GetHash(file, sha);
            }
        }

        public static string GetHash(string file, HashAlgorithm hash)
        {
            using (FileStream fileStream = new FileStream(file, FileMode.Open))
            {
                using (BufferedStream bufferedStream = new BufferedStream(fileStream))
                {
                    byte[] data = hash.ComputeHash(bufferedStream);
                    StringBuilder sb = new StringBuilder(2 * data.Length);
                    foreach (byte b in data)
                        sb.AppendFormat("{0:x2}", b);
                    return sb.ToString();
                }
            }
        }

        protected abstract void ApplyUpdate();
        protected abstract string GetAssemblyFile();

        public override void Show()
        {
            MyGuiSandbox.OpenUrl("https://steamcommunity.com/workshop/filedetails/?id=" + Id, UrlOpenMode.SteamOrExternalWithConfirm);
        }

        
    }
}