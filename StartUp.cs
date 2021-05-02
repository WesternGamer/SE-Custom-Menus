using HarmonyLib;
using Sandbox.Game;
using System.IO;     
using VRage.FileSystem;
using VRage.Plugins;
public class StartUp : IPlugin
{
    public StartUp()
    {
        Harmony harmony = new Harmony("SE_Custom_Menus");
        harmony.PatchAll();

        string menuPacksPath = Path.GetFullPath(Path.Combine(MyFileSystem.UserDataPath, "MenuPacks"));
        if (!Directory.Exists(menuPacksPath))
            Directory.CreateDirectory(menuPacksPath);
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