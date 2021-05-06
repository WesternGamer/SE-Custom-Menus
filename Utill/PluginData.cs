
using ProtoBuf;
using SE_Custom_Menus.Utill;
using System;
using System.Collections.Generic;
using System.Reflection;

using System.Xml.Serialization;
using VRage.Utils;

namespace SE_Custom_Menus.Utill
{
    [XmlInclude(typeof(WorkshopPlugin))]
    
    
    [ProtoContract]
    [ProtoInclude(100, typeof(SteamPlugin))]
    
    public abstract class PluginData : IEquatable<PluginData>
    {
        public abstract string Source { get; }

        [XmlIgnore]
        public Version Version { get; protected set; }

        [XmlIgnore]
        public virtual PluginStatus Status { get; set; } = PluginStatus.NotInstalled;
        public virtual string StatusString
        {
            get
            {
                switch (Status)
                {
                    case PluginStatus.NotInstalled:
                        return "Not installed.";
                    case PluginStatus.PendingUpdate:
                        return "Pending Update";
                    case PluginStatus.Updated:
                        return "Updated";
                    case PluginStatus.Error:
                        return "Error!";
                    case PluginStatus.Blocked:
                        return "Not whitelisted!";
                    default:
                        return "";
                }
            }
        }

        [ProtoMember(1)]
        public virtual string Id { get; set; }

        [ProtoMember(2)]
        public string FriendlyName { get; set; } = "Unknown";

        [ProtoMember(3)]
        public bool Hidden { get; set; } = false;

        [ProtoMember(4)]
        public string GroupId { get; set; }

        [XmlIgnore]
        public List<PluginData> Group { get; } = new List<PluginData>();

        protected PluginData()
        {

        }

        public abstract Assembly GetAssembly();

        public bool TryLoadAssembly(out Assembly a)
        {
            try
            {
                // Get the file path
                a = GetAssembly();
                if (Status == PluginStatus.Blocked)
                    return false;

                if (a == null)
                {
                    MyLog.Default.WriteLine("[SE Custom Menus]: Failed to load " + ToString());
                    
                    return false;
                }

                
                return true;
            }
            catch (Exception e)
            {
                string name = ToString();
                MyLog.Default.WriteLine($"[SE Custom Menus]: Failed to load {name} because of an error: " + e);
                if (e is MissingMemberException)
                    MyLog.Default.WriteLine($"[SE Custom Menus]: Is {name} up to date?");
                MyLog.Default.Flush();
                
                a = null;
                return false;
            }
        }


        public override bool Equals(object obj)
        {
            return Equals(obj as PluginData);
        }

        public bool Equals(PluginData other)
        {
            return other != null &&
                   Id == other.Id;
        }

        public override int GetHashCode()
        {
            return 2108858624 + EqualityComparer<string>.Default.GetHashCode(Id);
        }

        public static bool operator ==(PluginData left, PluginData right)
        {
            return EqualityComparer<PluginData>.Default.Equals(left, right);
        }

        public static bool operator !=(PluginData left, PluginData right)
        {
            return !(left == right);
        }

        public override string ToString()
        {
            return Id + '|' + FriendlyName;
        }

        

        public abstract void Show();
    }
}