using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Sandbox;
using Sandbox.Engine.Analytics;
using Sandbox.Engine.Networking;
using Sandbox.Engine.Utils;
using Sandbox.Game;
using Sandbox.Game.Gui;
using Sandbox.Game.GUI;
using Sandbox.Game.Gui.DebugInputComponents;
using Sandbox.Game.Localization;
using Sandbox.Game.Multiplayer;
using Sandbox.Graphics;
using Sandbox.Graphics.GUI;
using VRage;
using VRage.Audio;
using VRage.Game;
using VRage.Input;
using VRage.Utils;
using VRageMath;
using System.IO;
using VRage.FileSystem;

public abstract class MyCustomMainMenuBase : MyGuiScreenBase
{
	protected const float TEXT_LINE_HEIGHT = 0.014f;

	private MyGuiControlLabel m_warningLabel = new MyGuiControlLabel();

	protected bool m_pauseGame;

	protected bool m_musicPlayed;

	public bool ENABLE_DETAILED_BUILD_INFO = true;

	private static bool m_firstLoadup = true;

	private List<MyStringId> m_warningNotifications = new List<MyStringId>();

	private static readonly string BUILD_DATE = "Build: " + MySandboxGame.BuildDateTime.ToString("yyyy-MM-dd hh:mm", CultureInfo.InvariantCulture);

	private static readonly StringBuilder APP_VERSION = MyFinalBuildConstants.APP_VERSION_STRING;

	private const string STEAM_INACTIVE = "SERVICE NOT AVAILABLE";

	private const string NOT_OBFUSCATED = "NOT OBFUSCATED";

	private const string NON_OFFICIAL = " NON-OFFICIAL";

	private const string PROFILING = " PROFILING";

	private static readonly StringBuilder PLATFORM = new StringBuilder(Environment.Is64BitProcess ? " 64-bit" : " 32-bit");

	private static StringBuilder BranchName = new StringBuilder(50);

	public bool DrawBuildInformation { get; set; }

	public override string GetFriendlyName()
	{
		return "MyCustomMainMenu";
	}

	public override bool RegisterClicks()
	{
		return true;
	}

	public MyCustomMainMenuBase(bool pauseGame = false)
		: base(Vector2.Zero)
	{
		if (MyScreenManager.IsScreenOfTypeOpen(typeof(MyGuiScreenGamePlay)))
		{
			m_pauseGame = pauseGame;
			if (m_pauseGame && !Sync.MultiplayerActive)
			{
				MySandboxGame.PausePush();
			}
		}
		else
		{
			m_closeOnEsc = false;
		}
		m_drawEvenWithoutFocus = false;
		DrawBuildInformation = true;
	}

	public override bool Update(bool hasFocus)
	{
		if (!base.Update(hasFocus))
		{
			return false;
		}
		if (!m_musicPlayed)
		{
			if (MyGuiScreenGamePlay.Static == null)
			{
				MyAudio.Static.PlayMusic(MyPerGameSettings.MainMenuTrack);
			}
			m_musicPlayed = true;
		}
		if (MyCustomReloadTestComponent.Enabled && base.State == MyGuiScreenState.OPENED)
		{
			MyCustomReloadTestComponent.DoReload();
		}
		return true;
	}

	public override bool Draw()
	{
		if (!base.Draw())
		{
			return false;
		}
		if (MySandboxGame.Config.EnablePerformanceWarnings)
		{
			if (!MyGameService.Service.GetInstallStatus(out var _))
			{
				if (!m_warningNotifications.Contains(MyCommonTexts.PerformanceWarningHeading_InstallInProgress))
				{
					m_warningNotifications.Add(MyCommonTexts.PerformanceWarningHeading_InstallInProgress);
				}
			}
			else if (MySandboxGame.Config.ExperimentalMode && !m_warningNotifications.Contains(MyCommonTexts.PerformanceWarningHeading_ExperimentalMode))
			{
				m_warningNotifications.Add(MyCommonTexts.PerformanceWarningHeading_ExperimentalMode);
			}
		}
		DrawGameLogo(m_transitionAlpha, MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, 44, 68), MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP, 1f, 1f, "MenuPacks\\MenuPack\\Textures\\minecraft.png");
		DrawPerformanceWarning(0, 0, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, "MenuPacks\\MenuPack\\Textures\\toasts.png", 0.3f, 0.1f);
		if (DrawBuildInformation)
		{
			DrawObfuscationStatus();
			DrawSteamStatus();
			DrawAppVersion();
		}
		return true; 
	}

	public void DrawGameLogo(float transitionAlpha, Vector2 position, MyGuiDrawAlignEnum alignment, float width, float length, string texturepath)
	{
		Color color = Color.White * transitionAlpha;
		MyGuiManager.DrawSpriteBatch(Path.GetFullPath(Path.Combine(MyFileSystem.UserDataPath, texturepath)), position, new Vector2(width / 800f, length), color, alignment);
	}
	public override bool CloseScreen(bool isUnloading = false)
	{
		if (m_pauseGame && !Sync.MultiplayerActive)
		{
			MySandboxGame.PausePop();
		}
		bool result = base.CloseScreen(isUnloading);
		m_firstLoadup = false;
		m_musicPlayed = false;
		return result;
	}

	public override void CloseScreenNow(bool isUnloading = false)
	{
		m_firstLoadup = false;
		base.CloseScreenNow(isUnloading);
	}

	public override void HandleInput(bool receivedFocusInThisUpdate)
	{
		if (MyInput.Static.IsNewGameControlPressed(MyControlsSpace.HELP_SCREEN) || MyControllerHelper.IsControl(MySpaceBindingCreator.CX_GUI, MyControlsGUI.BUTTON_Y))
		{
			if (MyInput.Static.IsAnyShiftKeyPressed() && MyPerGameSettings.GUI.PerformanceWarningScreen != null)
			{
				MyGuiAudio.PlaySound(MyGuiSounds.HudMouseClick);
				MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.PerformanceWarningScreen));
			}
			else
			{
				MyGuiAudio.PlaySound(MyGuiSounds.HudMouseClick);
				MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.HelpScreen));
			}
		}
		if (MyControllerHelper.IsControl(MyControllerHelper.CX_BASE, MyControlsSpace.WARNING_SCREEN) && MyPerGameSettings.GUI.PerformanceWarningScreen != null)
		{
			MyGuiAudio.PlaySound(MyGuiSounds.HudMouseClick);
			MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen(MyPerGameSettings.GUI.PerformanceWarningScreen));
		}
		base.HandleInput(receivedFocusInThisUpdate);
	}

	public override void LoadContent()
	{
		base.LoadContent();
		RecreateControls(constructor: true);
	}

	public override bool HideScreen()
	{
		m_firstLoadup = false;
		return base.HideScreen();
	}

	public override int GetTransitionOpeningTime()
	{
		if (m_firstLoadup)
		{
			return 1500;
		}
		return base.GetTransitionOpeningTime();
	}

	private void DrawPerformanceWarning(int xPosition, int yPosition, MyGuiDrawAlignEnum alignment, string texturepath, float length, float width)
	{
		if (!Controls.Contains(m_warningLabel))
		{
			Controls.Add(m_warningLabel);
		}
		if (m_warningNotifications.Count != 0)
		{
			Vector2 position = MyGuiManager.ComputeFullscreenGuiCoordinate(alignment, xPosition, yPosition);
			position -= new Vector2(length / 1.5f, 0f);
			
			MyGuiManager.DrawSpriteBatch(Path.GetFullPath(Path.Combine(MyFileSystem.UserDataPath, texturepath)), position, new Vector2(length / 1.5f, width / 1.5f), Color.White, MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER);
			StringBuilder stringBuilder = new StringBuilder();
			if (MyInput.Static.IsJoystickLastUsed)
			{
				stringBuilder.AppendFormat(MyCommonTexts.PerformanceWarningCombinationGamepad, MyControllerHelper.GetCodeForControl(MyControllerHelper.CX_BASE, MyControlsSpace.WARNING_SCREEN));
			}
			else
			{
				stringBuilder.AppendFormat(MyCommonTexts.PerformanceWarningCombination, MyGuiSandbox.GetKeyName(MyControlsSpace.HELP_SCREEN));
			}
			MyGuiManager.DrawString("White", MyTexts.GetString(m_warningNotifications[0]), position + new Vector2(0.09f, -0.011f), 0.7f, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
			MyGuiManager.DrawString("White", stringBuilder.ToString(), position + new Vector2(0.09f, 0.018f), 0.6f, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
			stringBuilder.Clear();
			MyGuiManager.DrawString("White", stringBuilder.AppendFormat("({0})", m_warningNotifications.Count).ToString(), position + new Vector2(0.177f, -0.023f), 0.55f, null, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_CENTER);
			m_warningNotifications.RemoveAt(0);
		}
	}

	private void DrawBuildDate()
	{
		Vector2 normalizedCoord = MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM);
		normalizedCoord.Y -= 0f;
		MyGuiManager.DrawString("BuildInfo", BUILD_DATE, normalizedCoord, 0.6f, new Color(MyGuiConstants.LABEL_TEXT_COLOR * m_transitionAlpha, 0.6f), MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM);
	}

	private void DrawAppVersion()
	{
		if (ENABLE_DETAILED_BUILD_INFO)
		{
			Vector2 zero = Vector2.Zero;
			Vector2 normalizedCoord = MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, 8, 8);
			if (!string.IsNullOrEmpty(MyGameService.BranchName))
			{
				BranchName.Clear();
				BranchName.Append(" Branch: ");
				BranchName.Append(MyGameService.BranchName);
				zero = MyGuiManager.MeasureString("BuildInfoHighlight", BranchName, 0.6f);
				MyGuiManager.DrawString("BuildInfoHighlight", BranchName.ToString(), normalizedCoord, 0.6f, new Color(MyGuiConstants.LABEL_TEXT_COLOR * m_transitionAlpha, 0.6f), MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM);
				normalizedCoord.X -= zero.X;
			}
			MyGuiManager.DrawString("BuildInfo", "Space Engineers Version: " + MyFinalBuildConstants.APP_VERSION_STRING_DOTS.ToString(), normalizedCoord, 0.6f, new Color(MyGuiConstants.LABEL_TEXT_COLOR * m_transitionAlpha, 0.6f), MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM);
		}
		else
        {
			Vector2 zero = Vector2.Zero;
			Vector2 normalizedCoord = MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, 8, 8);
			if (!string.IsNullOrEmpty(MyGameService.BranchName))
			{
				BranchName.Clear();
				BranchName.Append(" ");
				BranchName.Append(MyGameService.BranchName);
				zero = MyGuiManager.MeasureString("BuildInfoHighlight", BranchName, 0.6f);
				MyGuiManager.DrawString("BuildInfoHighlight", BranchName.ToString(), normalizedCoord, 0.6f, new Color(MyGuiConstants.LABEL_TEXT_COLOR * m_transitionAlpha, 0.6f), MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM);
				normalizedCoord.X -= zero.X;
			}
			MyGuiManager.DrawString("BuildInfo", MyFinalBuildConstants.APP_VERSION_STRING_DOTS.ToString(), normalizedCoord, 0.6f, new Color(MyGuiConstants.LABEL_TEXT_COLOR * m_transitionAlpha, 0.6f), MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM);
		}
	}

	private void DrawSteamStatus()
	{
		if (!MyGameService.IsActive)
		{
			Vector2 normalizedCoord = MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM);
			normalizedCoord.Y -= 0.028f;
			MyGuiManager.DrawString("BuildInfo", "SERVICE NOT AVAILABLE".ToString(), normalizedCoord, 0.6f, new Color(MyGuiConstants.LABEL_TEXT_COLOR * m_transitionAlpha, 0.6f), MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM);
		}
	}

	private void DrawObfuscationStatus()
	{
		if (MyPerGameSettings.ShowObfuscationStatus && !MyObfuscation.Enabled)
		{
			Vector2 normalizedCoord = MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM);
			normalizedCoord.Y -= 0.0420000032f;
			MyGuiManager.DrawString("BuildInfoHighlight", "NOT OBFUSCATED", normalizedCoord, 0.6f, new Color(MyGuiConstants.LABEL_TEXT_COLOR * m_transitionAlpha, 0.6f), MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM);
		}
	}

	protected MyGuiControlButton MakeButton(Vector2 position, MyStringId text, Action<MyGuiControlButton> onClick, MyStringId? tooltip = null, MyStringId? gamepadHelpTextId = null)
	{
		MyGuiControlButton myGuiControlButton = new MyGuiControlButton(position, MyGuiControlButtonStyleEnum.StripeLeft, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM, null, MyTexts.Get(text), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, onClick);
		if (tooltip.HasValue)
		{
			myGuiControlButton.SetToolTip(MyTexts.GetString(tooltip.Value));
		}
		myGuiControlButton.BorderEnabled = false;
		myGuiControlButton.BorderSize = 0;
		myGuiControlButton.BorderHighlightEnabled = false;
		myGuiControlButton.BorderColor = Vector4.Zero;
		if (gamepadHelpTextId.HasValue)
		{
			myGuiControlButton.GamepadHelpTextId = gamepadHelpTextId.Value;
		}
		return myGuiControlButton;
	}

	protected void CheckLowMemSwitchToLow()
	{
		if (MySandboxGame.Config.LowMemSwitchToLow != MyConfig.LowMemSwitch.TRIGGERED)
		{
			return;
		}
		MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, messageCaption: MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), messageText: MyTexts.Get(MySpaceTexts.LowMemSwitchToLowQuestion), okButtonText: null, cancelButtonText: null, yesButtonText: null, noButtonText: null, callback: delegate (MyGuiScreenMessageBox.ResultEnum result)
		{
			if (result == MyGuiScreenMessageBox.ResultEnum.YES)
			{
				MySandboxGame.Config.LowMemSwitchToLow = MyConfig.LowMemSwitch.ARMED;
				MySandboxGame.Config.SetToLowQuality();
				MySandboxGame.Config.Save();
				if (MySpaceAnalytics.Instance != null)
				{
					MySpaceAnalytics.Instance.ReportSessionEnd("Exit to Windows");
				}
				MyScreenManager.CloseAllScreensNowExcept(null, isUnloading: true);
				MySandboxGame.ExitThreadSafe();
			}
			else
			{
				MySandboxGame.Config.LowMemSwitchToLow = MyConfig.LowMemSwitch.USER_SAID_NO;
				MySandboxGame.Config.Save();
			}
		}));
	}

	public abstract void OpenUserRelatedScreens();

	public abstract void CloseUserRelatedScreens();
}
