using Sandbox.Game;
using System.IO;
using VRage.FileSystem;
using VRage.Plugins;
public class StartUp : IPlugin
{
    public StartUp()
    {
        
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