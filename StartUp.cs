using HarmonyLib;
using Sandbox.Game;
using SE_Custom_Menus.Utill;
using System;
using System.IO;
using System.Security.Principal;
using VRage.FileSystem;
using VRage.Plugins;
using VRage.Utils;

public class StartUp : IPlugin
{
    public MyMenuPacksFinder List { get; }
    public MenuPacksConfig Config { get; }
    public StartUp()
    {
        MyLog.Default.WriteLine("[SE Custom Menus]: Plugin Loaded");
        Harmony harmony = new Harmony("SE_Custom_Menus");
        harmony.PatchAll();

        string menuPacksPath = Path.GetFullPath(Path.Combine(MyFileSystem.UserDataPath, "MenuPacks"));
        if (!Directory.Exists(menuPacksPath))
            Directory.CreateDirectory(menuPacksPath);
        string tempFilePath = Path.GetFullPath(Path.Combine(MyFileSystem.UserDataPath, "MenuPacks\\Temp"));
        if (!Directory.Exists(tempFilePath))
            Directory.CreateDirectory(tempFilePath);

        Config = MenuPacksConfig.Load(menuPacksPath);
        List = new MyMenuPacksFinder(menuPacksPath, Config);

        MyLog.Default.WriteLine("[SE Custom Menus]: Loading config.");
        Config.Init(List);

        
            
            

    }
    public void Dispose()
    {
        
    }

    public void Init(object gameInstance)
    {
        MyPerGameSettings.GUI.MainMenu = typeof(MyOldMainMenu);
    }

    public void Update()
    {
        
    }
}