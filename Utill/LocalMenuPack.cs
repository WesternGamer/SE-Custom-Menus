﻿using SE_Custom_Menus.Utill;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using VRage;

namespace SE_Custom_Menus.Utill
{
    public class LocalMenuPack : PluginData
    {
        public override string Source => MyTexts.GetString(MyCommonTexts.Local);

        public override string Id
        {
            get
            {
                return base.Id;
            }
            set
            {
                base.Id = value;
                if (File.Exists(value))
                    FriendlyName = Path.GetFileName(value);
            }
        }


        private LocalMenuPack()
        {

        }

        public LocalMenuPack(string zip)
        {
            Id = zip;
            Status = PluginStatus.None;
        }

        public override Assembly GetAssembly()
        {
            if (File.Exists(Id))
            {
                Assembly a = Assembly.LoadFile(Id);
                Version = a.GetName().Version;
                return a;
            }
            return null;
        }

        public override string ToString()
        {
            return Id;
        }

        public override void Show()
        {
            string file = Path.GetFullPath(Id);
            if (File.Exists(file))
                Process.Start("explorer.exe", $"/select, \"{file}\"");
        }
    }
}