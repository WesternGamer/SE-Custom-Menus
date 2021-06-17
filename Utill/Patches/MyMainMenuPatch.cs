using HarmonyLib;
using Sandbox.Engine.Networking;
using Sandbox.Game.Screens;
using Sandbox.Graphics;
using Sandbox.Graphics.GUI;
using SpaceEngineers.Game.GUI;
using System.Text;
using VRage.Game;
using VRage.Utils;
using VRageMath;

namespace SE_Custom_Menus
{
    internal class MyMainMenuPatch
    {
        [HarmonyPatch(typeof(MyGuiScreenMainMenu), "CreateRightSection")]
        public class Patch_Main_Menu_News
        {
            public static bool Prefix()
            {
                return false;
            }
        }
    }
}
