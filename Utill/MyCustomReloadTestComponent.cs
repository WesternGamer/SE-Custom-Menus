using System;
using System.Diagnostics;
using System.Linq;
using Sandbox;
using Sandbox.Engine.Networking;
using Sandbox.Game.World;
using VRage.Game.Components;

[MySessionComponentDescriptor(MyUpdateOrder.AfterSimulation)]
internal class MyCustomReloadTestComponent : MySessionComponentBase
{
	public static bool Enabled;

	public override void UpdateAfterSimulation()
	{
		if (Enabled && MySandboxGame.IsGameReady && MySession.Static != null && MySession.Static.ElapsedPlayTime.TotalSeconds > 5.0)
		{
			GC.Collect(2, GCCollectionMode.Forced);
			MySandboxGame.Log.WriteLine(string.Format("RELOAD TEST, Game GC: {0} B", GC.GetTotalMemory(forceFullCollection: false).ToString("##,#")));
			MySandboxGame.Log.WriteLine(string.Format("RELOAD TEST, Game WS: {0} B", Process.GetCurrentProcess().PrivateMemorySize64.ToString("##,#")));
			MySessionLoader.UnloadAndExitToMenu();
		}
	}

	public static void DoReload()
	{
		GC.Collect(2, GCCollectionMode.Forced);
		MySandboxGame.Log.WriteLine(string.Format("RELOAD TEST, Menu GC: {0} B", GC.GetTotalMemory(forceFullCollection: false).ToString("##,#")));
		MySandboxGame.Log.WriteLine(string.Format("RELOAD TEST, Menu WS: {0} B", Process.GetCurrentProcess().PrivateMemorySize64.ToString("##,#")));
		Tuple<string, MyWorldInfo> tuple = Enumerable.FirstOrDefault(Enumerable.OrderByDescending(MyLocalCache.GetAvailableWorldInfos(), (Tuple<string, MyWorldInfo> s) => s.Item2.LastSaveTime));
		if (tuple != null)
		{
			MySessionLoader.LoadSingleplayerSession(tuple.Item1);
		}
	}
}
