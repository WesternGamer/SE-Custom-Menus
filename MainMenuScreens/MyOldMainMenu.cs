using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EmptyKeys.UserInterface.Mvvm;
using ParallelTasks;
using Sandbox;
using Sandbox.Engine.Analytics;
using Sandbox.Engine.Networking;
using Sandbox.Engine.Platform.VideoMode;
using Sandbox.Engine.Utils;
using Sandbox.Game;
using Sandbox.Game.Gui;
using Sandbox.Game.Localization;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Screens;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.Screens.ViewModels;
using Sandbox.Game.World;
using Sandbox.Graphics;
using Sandbox.Graphics.GUI;
using Sandbox.Gui;
using SpaceEngineers.Game.GUI;
using VRage;
using VRage.Audio;
using VRage.Game;
using VRage.Game.News;
using VRage.GameServices;
using VRage.Input;
using VRage.Utils;
using VRageMath;
using VRageRender;
using VRageRender.Messages;

public class MyOldMainMenu : MyGuiScreenMainMenu
{
	private MyGuiControlElementGroup m_elementGroup;

	private MyGuiControlNews m_newsControl;

	private MyBadgeHelper m_myBadgeHelper;

	private MyGuiScreenIntroVideo m_backgroundScreen;
    private MyGuiControlElementGroup m_elemtents;

    public MyOldMainMenu()
		: this(pauseGame: false)
	{
	}

	public MyOldMainMenu(bool pauseGame)
		: base(pauseGame)
	{
		m_myBadgeHelper = new MyBadgeHelper();
		//Checks if the game is at the main menu or pause menu. Adds the background screen if the game is at the main menu.
		if (!pauseGame && MyGuiScreenGamePlay.Static == null)
		{
			AddBackgroundVideo();
		}
		//Draws the badges under the Space Engineers Logo
		MyGuiSandbox.DrawGameLogoHandler = m_myBadgeHelper.DrawGameLogo;
		//Decides if the game should use keyboard and mouse controls or Xbox Controls.
		MyInput.Static.IsJoystickLastUsed = MySandboxGame.Config.ControllerDefaultOnStart || MyPlatformGameSettings.CONTROLLER_DEFAULT_ON_START;
	}

	/// <summary>
	/// Plays the background video and also checks if it is enabled.
	/// </summary>
	private void AddBackgroundVideo()
	{
		//Checks if the main menu background video is enabled at a internal config.
		if (MyFakes.ENABLE_MENU_VIDEO_BACKGROUND)
		{
			//What actually plays the background video.
			MyGuiSandbox.AddScreen(m_backgroundScreen = MyGuiScreenIntroVideo.CreateBackgroundScreen());
		}
	}

	public override void RecreateControls(bool constructor)
	{
		base.RecreateControls(constructor);
		if (!m_pauseGame)
		{
			m_elemtents = new MyGuiControlElementGroup();
			m_elemtents.HighlightChanged += OnHighlightChange;
			if (MyGuiScreenGamePlay.Static == null)
			{
				MyGuiControlButton button = null;
				foreach (var c in Controls)
				{
					if (c is MyGuiControlButton)
					{
						m_elemtents.Add(c);
						if (((MyGuiControlButton)c).Text == MyTexts.GetString(MyCommonTexts.ScreenMenuButtonContinueGame))
							button = (MyGuiControlButton)c;
					}
				}
				if (button != null)
				{
					int index = Controls.IndexOf(button);
					MyGuiControlButton newGameButton = MakeButton(MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, -1000, -1000), MyCommonTexts.ScreenMenuButtonContinueGame, NullButtonAction);
					Controls.Add(newGameButton);
					m_elemtents.Add(newGameButton);
					newGameButton.Name = button.Name;
					Controls[index] = newGameButton;
					newGameButton.SetToolTip(button.Tooltips);
				}
				MyGuiControlButton button2 = null;
				foreach (var c in Controls)
				{
					if (c is MyGuiControlButton)
					{
						m_elemtents.Add(c);
						if (((MyGuiControlButton)c).Text == MyTexts.GetString(MyCommonTexts.ScreenMenuButtonCampaign))
							button2 = (MyGuiControlButton)c;
					}
				}
				if (button2 != null)
				{
					int index = Controls.IndexOf(button2);
					MyGuiControlButton newGameButton = MakeButton(MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, -1000, -1000), MyCommonTexts.ScreenMenuButtonCampaign, NullButtonAction);
					Controls.Add(newGameButton);
					m_elemtents.Add(newGameButton);
					newGameButton.Name = button2.Name;
					Controls[index] = newGameButton;
					newGameButton.SetToolTip(button2.Tooltips);
				}
				MyGuiControlButton button3 = null;
				foreach (var c in Controls)
				{
					if (c is MyGuiControlButton)
					{
						m_elemtents.Add(c);
						if (((MyGuiControlButton)c).Text == MyTexts.GetString(MyCommonTexts.ScreenMenuButtonLoadGame))
							button3 = (MyGuiControlButton)c;
					}
				}
				if (button3 != null)
				{
					int index = Controls.IndexOf(button3);
					MyGuiControlButton newGameButton = MakeButton(MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, -1000, -1000), MyCommonTexts.ScreenMenuButtonCampaign, NullButtonAction);
					Controls.Add(newGameButton);
					m_elemtents.Add(newGameButton);
					newGameButton.Name = button3.Name;
					Controls[index] = newGameButton;
					newGameButton.SetToolTip(button3.Tooltips);
				}
				MyGuiControlButton button4 = null;
				foreach (var c in Controls)
				{
					if (c is MyGuiControlButton)
					{
						m_elemtents.Add(c);
						if (((MyGuiControlButton)c).Text == MyTexts.GetString(MyCommonTexts.ScreenMenuButtonJoinGame))
							button4 = (MyGuiControlButton)c;
					}
				}
				if (button4 != null)
				{
					int index = Controls.IndexOf(button4);
					MyGuiControlButton newGameButton = MakeButton(MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, -1000, -1000), MyCommonTexts.ScreenMenuButtonCampaign, NullButtonAction);
					Controls.Add(newGameButton);
					m_elemtents.Add(newGameButton);
					newGameButton.Name = button4.Name;
					Controls[index] = newGameButton;
					newGameButton.SetToolTip(button4.Tooltips);
				}
				MyGuiControlButton button5 = null;
				foreach (var c in Controls)
				{
					if (c is MyGuiControlButton)
					{
						m_elemtents.Add(c);
						if (((MyGuiControlButton)c).Text == MyTexts.GetString(MyCommonTexts.ScreenMenuButtonOptions))
							button5 = (MyGuiControlButton)c;
					}
				}
				if (button5 != null)
				{
					int index = Controls.IndexOf(button5);
					MyGuiControlButton newGameButton = MakeButton(MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, -1000, -1000), MyCommonTexts.ScreenMenuButtonCampaign, NullButtonAction);
					Controls.Add(newGameButton);
					m_elemtents.Add(newGameButton);
					newGameButton.Name = button5.Name;
					Controls[index] = newGameButton;
					newGameButton.SetToolTip(button5.Tooltips);
				}
				MyGuiControlButton button6 = null;
				foreach (var c in Controls)
				{
					if (c is MyGuiControlButton)
					{
						m_elemtents.Add(c);
						if (((MyGuiControlButton)c).Text == MyTexts.GetString(MyCommonTexts.ScreenMenuButtonInventory))
							button6 = (MyGuiControlButton)c;
					}
				}
				if (button6 != null)
				{
					int index = Controls.IndexOf(button6);
					MyGuiControlButton newGameButton = MakeButton(MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, -1000, -1000), MyCommonTexts.ScreenMenuButtonCampaign, NullButtonAction);
					Controls.Add(newGameButton);
					m_elemtents.Add(newGameButton);
					newGameButton.Name = button6.Name;
					Controls[index] = newGameButton;
					newGameButton.SetToolTip(button6.Tooltips);
				}
				MyGuiControlButton button7 = null;
				foreach (var c in Controls)
				{
					if (c is MyGuiControlButton)
					{
						m_elemtents.Add(c);
						if (((MyGuiControlButton)c).Text == MyTexts.GetString(MyCommonTexts.ScreenMenuButtonExitToWindows))
							button7 = (MyGuiControlButton)c;
					}
				}
				if (button7 != null)
				{
					int index = Controls.IndexOf(button7);
					MyGuiControlButton newGameButton = MakeButton(MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, -1000, -1000), MyCommonTexts.ScreenMenuButtonCampaign, NullButtonAction);
					Controls.Add(newGameButton);
					m_elemtents.Add(newGameButton);
					newGameButton.Name = button7.Name;
					Controls[index] = newGameButton;
					newGameButton.SetToolTip(button7.Tooltips);
				}
				MyGuiControlNews News = null;
				foreach (var c in Controls)
				{
					if (c is MyGuiControlNews)
					{
						m_elemtents.Add(c);
						if (((MyGuiControlNews)c).Position == MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM) - 5f * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA)
							News = (MyGuiControlNews)c;
					}
				}
				if (News != null)
				{
					int index = Controls.IndexOf(News);
					News = new MyGuiControlNews
					{
						Position = MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, -1000, -1000),
						Size = new Vector2(0.4f, 0.28f),
						OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP
					};
					Controls.Add(News);
					float num = News.Size.X - 0.004f;
					float num2 = 0.407226563f;
					float num3 = num * num2 * 1.33333337f;
					Vector2 size = new Vector2(News.Size.X, num3 + 0.052f);
					Controls[index] = News;

				}
				MyGuiControlDLCBanners DlcBanners = null;
				foreach (var c in Controls)
				{
					if (c is MyGuiControlDLCBanners)
					{
						m_elemtents.Add(c);
						if (((MyGuiControlDLCBanners)c).Visible == false)
							DlcBanners = (MyGuiControlDLCBanners)c;
					}
				}
				if (DlcBanners != null)
				{
					int index = Controls.IndexOf(DlcBanners);
					DlcBanners = new MyGuiControlDLCBanners
					{
						Position = MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, -1000, -1000),
						Size = new Vector2(0.4f, 0.28f),
						OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP
					};
					Controls.Add(DlcBanners);
					float num = DlcBanners.Size.X - 0.004f;
					float num2 = 0.407226563f;
					float num3 = num * num2 * 1.33333337f;
					Vector2 size = new Vector2(DlcBanners.Size.X, num3 + 0.052f);
					Controls[index] = DlcBanners;

				}
			}
		}




		MyGuiControlButton button8 = null;
		foreach (var c in Controls)
		{
			if (c is MyGuiControlButton)
			{
				m_elemtents.Add(c);
				if (((MyGuiControlButton)c).Text == MyTexts.GetString(MyCommonTexts.ScreenMenuButtonSave))
					button8 = (MyGuiControlButton)c;
			}
		}
		if (button8 != null)
		{
			int index = Controls.IndexOf(button8);
			MyGuiControlButton newGameButton = MakeButton(MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, -1000, -1000), MyCommonTexts.ScreenMenuButtonContinueGame, NullButtonAction);
			Controls.Add(newGameButton);
			m_elemtents.Add(newGameButton);
			newGameButton.Name = button8.Name;
			Controls[index] = newGameButton;
			newGameButton.SetToolTip(button8.Tooltips);
		}
		MyGuiControlButton button9 = null;
		foreach (var c in Controls)
		{
			if (c is MyGuiControlButton)
			{
				m_elemtents.Add(c);
				if (((MyGuiControlButton)c).Text == MyTexts.GetString(MyCommonTexts.LoadScreenButtonSaveAs))
					button9 = (MyGuiControlButton)c;
			}
		}
		if (button9 != null)
		{
			int index = Controls.IndexOf(button9);
			MyGuiControlButton newGameButton = MakeButton(MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, -1000, -1000), MyCommonTexts.ScreenMenuButtonCampaign, NullButtonAction);
			Controls.Add(newGameButton);
			m_elemtents.Add(newGameButton);
			newGameButton.Name = button9.Name;
			Controls[index] = newGameButton;
			newGameButton.SetToolTip(button9.Tooltips);
		}
		MyGuiControlButton button10 = null;
		foreach (var c in Controls)
		{
			if (c is MyGuiControlButton)
			{
				m_elemtents.Add(c);
				if (((MyGuiControlButton)c).Text == MyTexts.GetString(MyCommonTexts.ScreenMenuButtonPlayers))
					button10 = (MyGuiControlButton)c;
			}
		}
		if (button10 != null)
		{
			int index = Controls.IndexOf(button10);
			MyGuiControlButton newGameButton = MakeButton(MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, -1000, -1000), MyCommonTexts.ScreenMenuButtonCampaign, NullButtonAction);
			Controls.Add(newGameButton);
			m_elemtents.Add(newGameButton);
			newGameButton.Name = button10.Name;
			Controls[index] = newGameButton;
			newGameButton.SetToolTip(button10.Tooltips);
		}
		MyGuiControlButton button11 = null;
		foreach (var c in Controls)
		{
			if (c is MyGuiControlButton)
			{
				m_elemtents.Add(c);
				if (((MyGuiControlButton)c).Text == MyTexts.GetString(MyCommonTexts.ScreenMenuButtonOptions))
					button11 = (MyGuiControlButton)c;
			}
		}
		if (button11 != null)
		{
			int index = Controls.IndexOf(button11);
			MyGuiControlButton newGameButton = MakeButton(MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, -1000, -1000), MyCommonTexts.ScreenMenuButtonCampaign, NullButtonAction);
			Controls.Add(newGameButton);
			m_elemtents.Add(newGameButton);
			newGameButton.Name = button11.Name;
			Controls[index] = newGameButton;
			newGameButton.SetToolTip(button11.Tooltips);
		}
		MyGuiControlButton button12 = null;
		foreach (var c in Controls)
		{
			if (c is MyGuiControlButton)
			{
				m_elemtents.Add(c);
				if (((MyGuiControlButton)c).Text == MyTexts.GetString(MyCommonTexts.ScreenMenuButtonExitToMainMenu))
					button12 = (MyGuiControlButton)c;
			}
		}
		if (button12 != null)
		{
			int index = Controls.IndexOf(button12);
			MyGuiControlButton newGameButton = MakeButton(MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, -1000, -1000), MyCommonTexts.ScreenMenuButtonCampaign, NullButtonAction);
			Controls.Add(newGameButton);
			m_elemtents.Add(newGameButton);
			newGameButton.Name = button12.Name;
			Controls[index] = newGameButton;
			newGameButton.SetToolTip(button12.Tooltips);
		}
		


		m_elementGroup = new MyGuiControlElementGroup();
		Vector2 minSizeGui = MyGuiControlButton.GetVisualStyle(MyGuiControlButtonStyleEnum.Default).NormalTexture.MinSizeGui;
		Vector2 leftButtonPositionOrigin = MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM) + new Vector2(minSizeGui.X / 2f, 0f) + new Vector2(15f, 0f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
		leftButtonPositionOrigin.Y += 0.043f;
		_ = MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM) + new Vector2((0f - minSizeGui.X) / 2f, 0f);
		Vector2 lastButtonPosition = Vector2.Zero;
		if (MyGuiScreenGamePlay.Static == null)
		{
			CreateMainMenu();
		}
		else
		{
			CreateInGameMenu();
		}
		//Draws the controllor hints under the buttons.
		MyGuiControlLabel myGuiControlLabel = new MyGuiControlLabel(MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, 49, 82));
		myGuiControlLabel.Name = MyGuiScreenBase.GAMEPAD_HELP_LABEL_NAME;
		Controls.Add(myGuiControlLabel);
		//Draws the Keen Software House Logo in the top righthand corner.
		MyGuiControlPanel myGuiControlPanel = new MyGuiControlPanel(MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, 49, 82), MyGuiConstants.TEXTURE_KEEN_LOGO.MinSizeGui, null, null, null, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
		myGuiControlPanel.BackgroundTexture = MyGuiConstants.TEXTURE_KEEN_LOGO;
		Controls.Add(myGuiControlPanel);
		//Refreshes the badges under the game logo.
		m_myBadgeHelper.RefreshGameLogo();
		CheckLowMemSwitchToLow();
		if (MyGuiScreenGamePlay.Static == null && !MyPlatformGameSettings.FEEDBACK_ON_EXIT && !string.IsNullOrEmpty(MyPlatformGameSettings.FEEDBACK_URL))
		{
			MyGuiSandbox.OpenUrl(MyPlatformGameSettings.FEEDBACK_URL, UrlOpenMode.ExternalWithConfirm, MyTexts.Get(MyCommonTexts.MessageBoxTextBetaFeedback), MyTexts.Get(MyCommonTexts.MessageBoxCaptionBetaFeedback));
		}
	}

    private void NullButtonAction(MyGuiControlButton obj)
    {
        
    }

    private void CreateInGameMenu()
	{
		base.GamepadHelpTextId = MySpaceTexts.MainMenuScreen_Help_ScreenIngame;
		base.EnabledBackgroundFade = true;
		MyGuiControlButton myGuiControlButton = MakeButton(MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, 0), MyCustomTexts.PauseMenu, OnClickPauseMenu);
		MyGuiControlButton myGuiControlButton2 = MakeButton(MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, 196, 84), MyCustomTexts.OldButtonsAndMenus, OnClickOldMenus);
		Controls.Add(myGuiControlButton);
		m_elementGroup.Add(myGuiControlButton);
		Controls.Add(myGuiControlButton2);
		m_elementGroup.Add(myGuiControlButton2);
		

	}

    private void OnClickOldMenus(MyGuiControlButton obj)
    {
		MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen<MyOldMenusAccessMenu>(false));
	}

    private void OnClickPauseMenu(MyGuiControlButton obj)
    {
		MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen<MyCustomMainMenu>(true));
	}

	private void OnHighlightChange(MyGuiControlElementGroup obj)
	{
		foreach (var c in m_elemtents)
		{
			if (c.HasFocus && m_elemtents.SelectedIndex != obj.SelectedIndex)
			{
				FocusedControl = c;
				break;
			}
		}
	}

	private void CreateMainMenu()
	{
		base.GamepadHelpTextId = MySpaceTexts.MainMenuScreen_Help_Screen;
		base.EnabledBackgroundFade = false;
		MyGuiControlButton myGuiControlButton = MakeButton(MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, 0, 0), MyCustomTexts.StartGame, OnClickStartGame);
		MyGuiControlButton myGuiControlButton2 = MakeButton(MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM, 196, 84), MyCustomTexts.OldButtonsAndMenus, OnClickOldMenus);
		Controls.Add(myGuiControlButton);
		m_elementGroup.Add(myGuiControlButton);
		Controls.Add(myGuiControlButton2);
		m_elementGroup.Add(myGuiControlButton2);
		

	}

    

    private void OnClickStartGame(MyGuiControlButton obj)
    {
		MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen<MyCustomMainMenu>(false));
	}



    public override void OpenUserRelatedScreens()
	{
	}

	public override void CloseUserRelatedScreens()
	{
		m_newsControl?.CloseNewVersionScreen();
		
	}
}
