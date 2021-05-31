using HarmonyLib;
using SpaceEngineers.Game.GUI;

namespace SE_Custom_Menus
{
    internal class MyMainMenuPatch
    {
        [HarmonyPatch(typeof(MyGuiScreenMainMenu), "MyGuiScreenIntroVideo")]
        public class Patch_MainMenu
        {
            public static bool Prefix()
            {
                return false;
            }
        }
    }
}
