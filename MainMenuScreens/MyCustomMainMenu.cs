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

public class MyCustomMainMenu : MyCustomMainMenuBase
{
	private const int CONTROLS_PER_BANNER = 3;
	private readonly int DLC_UPDATE_INTERVAL = 5000;
	private MyGuiControlNews m_newsControl;
	private MyGuiControlDLCBanners m_dlcBannersControl;
	private MyGuiControlBase m_continueTooltipcontrol;
	private MyGuiControlButton m_continueButton;
	private MyGuiControlElementGroup m_elementGroup;
	private int m_currentDLCcounter;
	private MyBadgeHelper m_myBadgeHelper;
	private MyGuiControlButton m_exitGameButton;
	private MyGuiControlImageButton m_lastClickedBanner;
	private MyGuiScreenIntroVideo m_backgroundScreen;
	private bool m_parallelLoadIsRunning;

	public MyCustomMainMenu()
		: this(pauseGame: false)
	{
	}

	public MyCustomMainMenu(bool pauseGame)
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
	/// <summary>
	/// Draws the buttons in the main menu or pause menu. Also draws the Keen Software House logo and controllor hints.
	/// </summary>
	/// <param name="constructor"></param>
	public override void RecreateControls(bool constructor)
	{
		base.RecreateControls(constructor);
		m_elementGroup = new MyGuiControlElementGroup();
		Vector2 minSizeGui = MyGuiControlButton.GetVisualStyle(MyGuiControlButtonStyleEnum.Default).NormalTexture.MinSizeGui;
		Vector2 leftButtonPositionOrigin = MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM) + new Vector2(minSizeGui.X / 2f, 0f) + new Vector2(15f, 0f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
		leftButtonPositionOrigin.Y += 0.043f;
		_ = MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM) + new Vector2((0f - minSizeGui.X) / 2f, 0f);
		Vector2 lastButtonPosition = Vector2.Zero;
		if (MyGuiScreenGamePlay.Static == null)
		{
			CreateMainMenu(leftButtonPositionOrigin, out lastButtonPosition);
		}
		else
		{
			CreateInGameMenu(leftButtonPositionOrigin, out lastButtonPosition);
		}
		//Draws the controllor hints under the buttons.
		MyGuiControlLabel myGuiControlLabel = new MyGuiControlLabel(lastButtonPosition + new Vector2((0f - minSizeGui.X) / 2f, minSizeGui.Y / 2f));
		myGuiControlLabel.Name = MyGuiScreenBase.GAMEPAD_HELP_LABEL_NAME;
		Controls.Add(myGuiControlLabel);
		//Draws the Keen Software House Logo in the top righthand corner.
		MyGuiControlPanel myGuiControlPanel = new MyGuiControlPanel(MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP, 49, 82), MyGuiConstants.TEXTURE_KEEN_LOGO.MinSizeGui, null, null, null, MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP);
		myGuiControlPanel.BackgroundTexture = MyGuiConstants.TEXTURE_KEEN_LOGO;
		Controls.Add(myGuiControlPanel);
		//Refreshes the badges under the game logo.
		m_myBadgeHelper.RefreshGameLogo();
		//Creates the news and Dlc banners on the right of the screen.
		CreateRightSection(minSizeGui);
		CheckLowMemSwitchToLow();
		if (MyGuiScreenGamePlay.Static == null && !MyPlatformGameSettings.FEEDBACK_ON_EXIT && !string.IsNullOrEmpty(MyPlatformGameSettings.FEEDBACK_URL))
		{
			MyGuiSandbox.OpenUrl(MyPlatformGameSettings.FEEDBACK_URL, UrlOpenMode.ExternalWithConfirm, MyTexts.Get(MyCommonTexts.MessageBoxTextBetaFeedback), MyTexts.Get(MyCommonTexts.MessageBoxCaptionBetaFeedback));
		}
	}

	/// <summary>
	/// Creates the news and Dlc banners on the right of the screen.
	/// </summary>
	private void CreateRightSection(Vector2 buttonSize)
	{
		m_newsControl = new MyGuiControlNews
		{
			Position = MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_BOTTOM) - 5f * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA,
			Size = new Vector2(0.4f, 0.28f),
			OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP
		};
		Controls.Add(m_newsControl);
		float num = m_newsControl.Size.X - 0.004f;
		float num2 = 0.407226563f;
		float num3 = num * num2 * 1.33333337f;
		Vector2 size = new Vector2(m_newsControl.Size.X, num3 + 0.052f);

		m_dlcBannersControl = new MyGuiControlDLCBanners
		{
			Position = new Vector2(m_newsControl.Position.X, 0.26f),
			OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_RIGHT_AND_VERTICAL_TOP
		};
		m_dlcBannersControl.Size = size;
		m_dlcBannersControl.Visible = false;
		Vector2 vector = MyGuiManager.ComputeFullscreenGuiCoordinate(MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_BOTTOM) + new Vector2(buttonSize.X / 2f, 0f) + new Vector2(15f, 0f) / MyGuiConstants.GUI_OPTIMAL_SIZE;
		vector.Y += 0.043f;
		Controls.Add(m_dlcBannersControl);
	}

	private bool IsControllerHelpGoingToFitInMiddleBottomOfScreen()
	{
		_ = MySandboxGame.Config;
		if ((float)MyVideoSettingsManager.CurrentDeviceSettings.BackBufferWidth / (float)MyVideoSettingsManager.CurrentDeviceSettings.BackBufferHeight < 1.77777779f)
		{
			return false;
		}
		return true;
	}

	private void CreateInGameMenu(Vector2 leftButtonPositionOrigin, out Vector2 lastButtonPosition)
	{
		base.GamepadHelpTextId = MySpaceTexts.MainMenuScreen_Help_ScreenIngame;
		base.EnabledBackgroundFade = true;
		int num = (Sync.MultiplayerActive ? 6 : 5);
		MyGuiControlButton myGuiControlButton = MakeButton(leftButtonPositionOrigin - --num * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyCommonTexts.ScreenMenuButtonSave, OnClickSaveWorld);
		MyGuiControlButton myGuiControlButton2 = MakeButton(leftButtonPositionOrigin - --num * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyCommonTexts.LoadScreenButtonSaveAs, OnClickSaveAs);
		if (!Sync.IsServer || (MySession.Static != null && !MySession.Static.Settings.EnableSaving))
		{
			MyStringId toolTip = ((!Sync.IsServer) ? MyCommonTexts.NotificationClientCannotSave : MyCommonTexts.NotificationSavingDisabled);
			myGuiControlButton.Enabled = false;
			myGuiControlButton.ShowTooltipWhenDisabled = true;
			myGuiControlButton.SetToolTip(toolTip);
			myGuiControlButton2.Enabled = false;
			myGuiControlButton2.ShowTooltipWhenDisabled = true;
			myGuiControlButton2.SetToolTip(toolTip);
		}
		Controls.Add(myGuiControlButton);
		m_elementGroup.Add(myGuiControlButton);
		Controls.Add(myGuiControlButton2);
		m_elementGroup.Add(myGuiControlButton2);
		MyGuiControlButton myGuiControlButton3;
		if (Sync.MultiplayerActive)
		{
			myGuiControlButton3 = MakeButton(leftButtonPositionOrigin - --num * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyCommonTexts.ScreenMenuButtonPlayers, OnClickPlayers);
			Controls.Add(myGuiControlButton3);
			m_elementGroup.Add(myGuiControlButton3);
		}
		myGuiControlButton3 = MakeButton(leftButtonPositionOrigin - --num * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyCommonTexts.ScreenMenuButtonOptions, OnClickOptions);
		Controls.Add(myGuiControlButton3);
		m_elementGroup.Add(myGuiControlButton3);
		m_exitGameButton = MakeButton(leftButtonPositionOrigin - --num * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyCommonTexts.ScreenMenuButtonExitToMainMenu, OnExitToMainMenuClick);
		Controls.Add(m_exitGameButton);
		m_elementGroup.Add(m_exitGameButton);
		lastButtonPosition = m_exitGameButton.Position;
	}

	private void CreateMainMenu(Vector2 leftButtonPositionOrigin, out Vector2 lastButtonPosition)
	{
		base.GamepadHelpTextId = MySpaceTexts.MainMenuScreen_Help_Screen;
		base.EnabledBackgroundFade = false;
		MyGuiControlButton myGuiControlButton = null;
		int num = (MyPerGameSettings.MultiplayerEnabled ? 7 : 6);
		MyObjectBuilder_LastSession lastSession = MyLocalCache.GetLastSession();
		if (lastSession != null && (lastSession.Path == null || MyPlatformGameSettings.GAME_SAVES_TO_CLOUD || Directory.Exists(lastSession.Path)) && (!lastSession.IsLobby || MyGameService.LobbyDiscovery.ContinueToLobbySupported))
		{
			m_continueButton = MakeButton(leftButtonPositionOrigin - num-- * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA - MyGuiConstants.MENU_BUTTONS_POSITION_DELTA / 2f, MyCommonTexts.ScreenMenuButtonContinueGame, OnContinueGameClicked);
			Controls.Add(m_continueButton);
			m_elementGroup.Add(m_continueButton);
			GenerateContinueTooltip(lastSession, m_continueButton, new Vector2(0.003f, -0.0025f));
			m_continueButton.FocusChanged += FocusChangedContinue;
		}
		else
		{
			num--;
		}
		myGuiControlButton = MakeButton(leftButtonPositionOrigin - num-- * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyCommonTexts.ScreenMenuButtonCampaign, OnClickNewGame);
		Controls.Add(myGuiControlButton);
		m_elementGroup.Add(myGuiControlButton);
		myGuiControlButton = MakeButton(leftButtonPositionOrigin - num-- * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyCommonTexts.ScreenMenuButtonLoadGame, OnClickLoad);
		Controls.Add(myGuiControlButton);
		m_elementGroup.Add(myGuiControlButton);
		if (MyPerGameSettings.MultiplayerEnabled)
		{
			myGuiControlButton = MakeButton(leftButtonPositionOrigin - num-- * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyCommonTexts.ScreenMenuButtonJoinGame, OnJoinWorld);
			Controls.Add(myGuiControlButton);
			m_elementGroup.Add(myGuiControlButton);
		}
		myGuiControlButton = MakeButton(leftButtonPositionOrigin - num-- * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyCommonTexts.ScreenMenuButtonOptions, OnClickOptions);
		Controls.Add(myGuiControlButton);
		m_elementGroup.Add(myGuiControlButton);
		lastButtonPosition = myGuiControlButton.Position;

		myGuiControlButton = MakeButton(leftButtonPositionOrigin - num-- * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyCommonTexts.ScreenMenuButtonOptions, OnClickMenuPacks);
		Controls.Add(myGuiControlButton);
		m_elementGroup.Add(myGuiControlButton);
		lastButtonPosition = myGuiControlButton.Position;

		if (MyFakes.ENABLE_MAIN_MENU_INVENTORY_SCENE)
		{
			myGuiControlButton = MakeButton(leftButtonPositionOrigin - num-- * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyCommonTexts.ScreenMenuButtonInventory, OnClickInventory);
			Controls.Add(myGuiControlButton);
			m_elementGroup.Add(myGuiControlButton);
			lastButtonPosition = myGuiControlButton.Position;
		}
		if (!MyPlatformGameSettings.LIMITED_MAIN_MENU)
		{
			m_exitGameButton = MakeButton(leftButtonPositionOrigin - num-- * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyCommonTexts.ScreenMenuButtonExitToWindows, OnClickExitToWindows);
			Controls.Add(m_exitGameButton);
			m_elementGroup.Add(m_exitGameButton);
			lastButtonPosition = m_exitGameButton.Position;
		}
	}

    private void OnClickMenuPacks(MyGuiControlButton obj)
    {
		MyGuiSandbox.AddScreen(new MyMenuPacksMenu());

	}

    private void GenerateContinueTooltip(MyObjectBuilder_LastSession lastSession, MyGuiControlButton button, Vector2 correction)
	{
		string thumbnail = GetThumbnail(lastSession);
		string text = ((!lastSession.IsOnline) ? $"{MyTexts.GetString(MyCommonTexts.ToolTipContinueGame)}{Environment.NewLine}{lastSession.GameName}" : $"{MyTexts.GetString(MyCommonTexts.ToolTipContinueGame)}{Environment.NewLine}{lastSession.GameName} - {lastSession.GetConnectionString()}");
		MyGuiControlBase myGuiControlBase = null;
		if (thumbnail != null)
		{
			MyRenderProxy.PreloadTextures(new List<string> { thumbnail }, TextureType.GUIWithoutPremultiplyAlpha);
		}
		myGuiControlBase = CreateImageTooltip(thumbnail, text);
		myGuiControlBase.Visible = false;
		myGuiControlBase.Position = button.Position + new Vector2(0.5f * button.Size.X, -1f * button.Size.Y) + correction;
		m_continueTooltipcontrol = myGuiControlBase;
		Controls.Add(m_continueTooltipcontrol);
	}

	private void FocusChangedContinue(MyGuiControlBase controls, bool focused)
	{
		m_continueTooltipcontrol.Visible = focused;
	}

	private string GetThumbnail(MyObjectBuilder_LastSession session)
	{
		string text = session?.Path;
		if (text == null)
		{
			return null;
		}
		if (Directory.Exists(text + MyGuiScreenLoadSandbox.CONST_BACKUP))
		{
			string[] directories = Directory.GetDirectories(text + MyGuiScreenLoadSandbox.CONST_BACKUP);
			if (Enumerable.Any(directories))
			{
				string text2 = Path.Combine(Enumerable.Last(directories), MyTextConstants.SESSION_THUMB_NAME_AND_EXTENSION);
				if (File.Exists(text2) && new FileInfo(text2).Length > 0)
				{
					return text2;
				}
			}
		}
		string text3 = Path.Combine(text, MyTextConstants.SESSION_THUMB_NAME_AND_EXTENSION);
		if (File.Exists(text3) && new FileInfo(text3).Length > 0)
		{
			return text3;
		}
		if (MyPlatformGameSettings.GAME_SAVES_TO_CLOUD)
		{
			byte[] array = MyGameService.LoadFromCloud(MyCloudHelper.Combine(MyCloudHelper.LocalToCloudWorldPath(text + "/"), MyTextConstants.SESSION_THUMB_NAME_AND_EXTENSION));
			if (array != null)
			{
				try
				{
					string text4 = Path.Combine(text, MyTextConstants.SESSION_THUMB_NAME_AND_EXTENSION);
					Directory.CreateDirectory(text);
					File.WriteAllBytes(text4, array);
					MyRenderProxy.UnloadTexture(text4);
					return text4;
				}
				catch
				{
				}
			}
		}
		return null;
	}

	private MyGuiControlBase CreateImageTooltip(string path, string text)
	{
		MyGuiControlParent myGuiControlParent = new MyGuiControlParent
		{
			OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP,
			BackgroundTexture = new MyGuiCompositeTexture("Textures\\GUI\\Blank.dds"),
			ColorMask = MyGuiConstants.THEMED_GUI_BACKGROUND_COLOR
		};
		myGuiControlParent.CanHaveFocus = false;
		myGuiControlParent.HighlightType = MyGuiControlHighlightType.NEVER;
		myGuiControlParent.BorderEnabled = true;
		Vector2 vector = new Vector2(0.005f, 0.002f);
		MyGuiControlLabel myGuiControlLabel = new MyGuiControlLabel(Vector2.Zero, null, text)
		{
			OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP
		};
		myGuiControlLabel.CanHaveFocus = false;
		myGuiControlLabel.HighlightType = MyGuiControlHighlightType.NEVER;
		MyGuiControlImage myGuiControlImage = null;
		if (!string.IsNullOrEmpty(path))
		{
			myGuiControlImage = new MyGuiControlImage(Vector2.Zero, new Vector2(0.175625f, 0.131718755f))
			{
				OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM
			};
			myGuiControlImage.SetTexture(path);
			myGuiControlImage.CanHaveFocus = false;
			myGuiControlImage.HighlightType = MyGuiControlHighlightType.NEVER;
		}
		else
		{
			myGuiControlImage = new MyGuiControlImage(Vector2.Zero, Vector2.Zero)
			{
				OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_BOTTOM
			};
		}
		myGuiControlParent.Size = new Vector2(Math.Max(myGuiControlLabel.Size.X, myGuiControlImage.Size.X) + vector.X * 2f, myGuiControlLabel.Size.Y + myGuiControlImage.Size.Y + vector.Y * 4f);
		myGuiControlParent.Controls.Add(myGuiControlImage);
		myGuiControlParent.Controls.Add(myGuiControlLabel);
		myGuiControlLabel.Position = -myGuiControlParent.Size / 2f + vector;
		myGuiControlImage.Position = new Vector2(0f, myGuiControlParent.Size.Y / 2f - vector.Y);
		return myGuiControlParent;
	}

	private void MenuRefocusImageButton(MyGuiControlImageButton sender)
	{
		m_lastClickedBanner = sender;
	}

	private void OnClickBack(MyGuiControlButton obj)
	{
		RecreateControls(constructor: false);
	}

	private void OnPlayClicked(MyGuiControlButton obj)
	{
		RecreateControls(constructor: false);
	}

	private void OnClickInventory(MyGuiControlButton obj)
	{
		if (MyGameService.IsActive)
		{
			if (MyGameService.Service.GetInstallStatus(out var _))
			{
				if (MySession.Static == null)
				{
					MyGuiScreenLoadInventory inventory = MyGuiSandbox.CreateScreen<MyGuiScreenLoadInventory>(Array.Empty<object>());
					MyGuiScreenLoading screen = new MyGuiScreenLoading(inventory, null);
					MyGuiScreenLoadInventory myGuiScreenLoadInventory = inventory;
					myGuiScreenLoadInventory.OnLoadingAction = (Action)Delegate.Combine(myGuiScreenLoadInventory.OnLoadingAction, (Action)delegate
					{
						MySessionLoader.LoadInventoryScene();
						MySandboxGame.IsUpdateReady = true;
						inventory.Initialize(inGame: false, null);
					});
					MyGuiSandbox.AddScreen(screen);
				}
				else
				{
					MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen<MyGuiScreenLoadInventory>(new object[2] { false, null }));
				}
			}
			else
			{
				MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, messageCaption: MyTexts.Get(MyCommonTexts.MessageBoxCaptionInfo), messageText: MyTexts.Get(MyCommonTexts.InventoryScreen_InstallInProgress)));
			}
		}
		else
		{
			MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, messageCaption: MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), messageText: MyTexts.Get(MyCommonTexts.SteamIsOfflinePleaseRestart)));
		}
	}

	private void OnContinueGameClicked(MyGuiControlButton myGuiControlButton)
	{
		RunWithTutorialCheck(delegate
		{
			ContinueGameInternal();
		});
	}

	private void ContinueGameInternal()
	{
		MyObjectBuilder_LastSession mySession = MyLocalCache.GetLastSession();
		if (mySession == null)
		{
			return;
		}
		if (mySession.IsOnline)
		{
			if (mySession.IsLobby)
			{
				MyJoinGameHelper.JoinGame(ulong.Parse(mySession.ServerIP));
				return;
			}
			MyGameService.Service.RequestPermissions(Permissions.Multiplayer, attemptResolution: true, delegate (PermissionResult granted)
			{
				switch (granted)
				{
					case PermissionResult.Granted:
						MyGameService.Service.RequestPermissions(Permissions.CrossMultiplayer, attemptResolution: true, delegate (PermissionResult crossGranted)
						{
							switch (crossGranted)
							{
								case PermissionResult.Granted:
									MyGameService.Service.RequestPermissions(Permissions.UGC, attemptResolution: true, delegate (PermissionResult ugcGranted)
									{
										switch (ugcGranted)
										{
											case PermissionResult.Granted:
												JoinServer(mySession);
												break;
											case PermissionResult.Error:
												MySandboxGame.Static.Invoke(delegate
												{
													MyGuiSandbox.Show(MyCommonTexts.XBoxPermission_MultiplayerError, default(MyStringId), MyMessageBoxStyleEnum.Info);
												}, "New Game screen");
												break;
										}
									});
									break;
								case PermissionResult.Error:
									MySandboxGame.Static.Invoke(delegate
									{
										MyGuiSandbox.Show(MyCommonTexts.XBoxPermission_MultiplayerError, default(MyStringId), MyMessageBoxStyleEnum.Info);
									}, "New Game screen");
									break;
							}
						});
						break;
					case PermissionResult.Error:
						MySandboxGame.Static.Invoke(delegate
						{
							MyGuiSandbox.Show(MyCommonTexts.XBoxPermission_MultiplayerError, default(MyStringId), MyMessageBoxStyleEnum.Info);
						}, "New Game screen");
						break;
				}
			});
		}
		else if (!m_parallelLoadIsRunning)
		{
			m_parallelLoadIsRunning = true;
			MyGuiScreenProgress progressScreen = new MyGuiScreenProgress(MyTexts.Get(MySpaceTexts.ProgressScreen_LoadingWorld));
			MyScreenManager.AddScreen(progressScreen);
			Parallel.StartBackground(delegate
			{
				MySessionLoader.LoadLastSession();
			}, delegate
			{
				progressScreen.CloseScreen();
				m_parallelLoadIsRunning = false;
			});
		}
	}

	private void JoinServer(MyObjectBuilder_LastSession mySession)
	{
		try
		{
			MyGuiScreenProgress prog = new MyGuiScreenProgress(MyTexts.Get(MyCommonTexts.DialogTextCheckServerStatus));
			MyGuiSandbox.AddScreen(prog);
			MyGameService.OnPingServerResponded += OnPingSuccess;
			MyGameService.OnPingServerFailedToRespond += OnPingFailure;
			MyGameService.PingServer(mySession.GetConnectionString());
			void OnPingFailure(object sender, object data)
			{
				MyGuiSandbox.RemoveScreen(prog);
				MySandboxGame.Static.ServerFailedToRespond(sender, data);
				MyGameService.OnPingServerResponded -= OnPingSuccess;
				MyGameService.OnPingServerFailedToRespond -= OnPingFailure;
			}
			void OnPingSuccess(object sender, MyGameServerItem item)
			{
				MyGuiSandbox.RemoveScreen(prog);
				MySandboxGame.Static.ServerResponded(sender, item);
				MyGameService.OnPingServerResponded -= OnPingSuccess;
				MyGameService.OnPingServerFailedToRespond -= OnPingFailure;
			}
		}
		catch (Exception ex)
		{
			MyLog.Default.WriteLine(ex);
			MyGuiSandbox.Show(MyTexts.Get(MyCommonTexts.MultiplayerJoinIPError), MyCommonTexts.MessageBoxCaptionError);
		}
	}

	private void OnCustomGameClicked(MyGuiControlButton myGuiControlButton)
	{
		MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen<MyGuiScreenWorldSettings>(Array.Empty<object>()));
	}

	private void OnClickReportBug(MyGuiControlButton obj)
	{
		MyGuiSandbox.OpenUrl(MyPerGameSettings.BugReportUrl, UrlOpenMode.SteamOrExternalWithConfirm, new StringBuilder().AppendFormat(MyCommonTexts.MessageBoxTextOpenBrowser, "forums.keenswh.com"));
	}

	private void OnJoinWorld(MyGuiControlButton sender)
	{
		RunWithTutorialCheck(delegate
		{
			if (MyGameService.IsOnline)
			{
				MyGameService.Service.RequestPermissions(Permissions.Multiplayer, attemptResolution: true, delegate (PermissionResult granted)
				{
					switch (granted)
					{
						case PermissionResult.Granted:
							MyGameService.Service.RequestPermissions(Permissions.UGC, attemptResolution: true, delegate (PermissionResult ugcGranted)
							{
								switch (ugcGranted)
								{
									case PermissionResult.Granted:
										MyGameService.Service.RequestPermissions(Permissions.CrossMultiplayer, attemptResolution: true, delegate (PermissionResult crossGranted)
										{
											MyGuiScreenJoinGame myGuiScreenJoinGame = new MyGuiScreenJoinGame(crossGranted == PermissionResult.Granted);
											myGuiScreenJoinGame.Closed += joinGameScreen_Closed;
											MyGuiSandbox.AddScreen(myGuiScreenJoinGame);
										});
										break;
									case PermissionResult.Error:
										MySandboxGame.Static.Invoke(delegate
										{
											MyGuiSandbox.Show(MyCommonTexts.XBoxPermission_MultiplayerError, default(MyStringId), MyMessageBoxStyleEnum.Info);
										}, "New Game screen");
										break;
								}
							});
							break;
						case PermissionResult.Error:
							MyGuiSandbox.Show(MyCommonTexts.XBoxPermission_MultiplayerError, default(MyStringId), MyMessageBoxStyleEnum.Info);
							break;
					}
				});
			}
			else
			{
				MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, messageCaption: MyTexts.Get(MyCommonTexts.MessageBoxCaptionError), messageText: new StringBuilder().AppendFormat(MyTexts.GetString(MyGameService.IsActive ? MyCommonTexts.SteamIsOfflinePleaseRestart : MyCommonTexts.ErrorJoinSessionNoUser), MySession.GameServiceName)));
			}
		});
	}

	private void joinGameScreen_Closed(MyGuiScreenBase source, bool isUnloading)
	{
		if (source.Cancelled)
		{
			base.State = MyGuiScreenState.OPENING;
			source.Closed -= joinGameScreen_Closed;
		}
	}

	private void RunWithTutorialCheck(Action afterTutorial)
	{
		if (MySandboxGame.Config.FirstTimeTutorials)
		{
			MyGuiSandbox.AddScreen(new MyGuiScreenTutorialsScreen(afterTutorial));
		}
		else
		{
			afterTutorial();
		}
	}

	private void OnClickNewGame(MyGuiControlButton sender)
	{
		if (MySandboxGame.Config.EnableNewNewGameScreen)
		{
			RunWithTutorialCheck(delegate
			{
				MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen<MyGuiScreenSimpleNewGame>(Array.Empty<object>()));
			});
			return;
		}
		RunWithTutorialCheck(delegate
		{
			MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen<MyGuiScreenNewGame>(new object[3] { true, true, true }));
		});
	}

	private void OnClickLoad(MyGuiControlBase sender)
	{
		RunWithTutorialCheck(delegate
		{
			MyGuiSandbox.AddScreen(new MyGuiScreenLoadSandbox());
		});
	}

	private void OnClickPlayers(MyGuiControlButton obj)
	{
		MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen<MyGuiScreenPlayers>(Array.Empty<object>()));
	}

	private void OnExitToMainMenuClick(MyGuiControlButton sender)
	{
		base.CanBeHidden = false;
		MyGuiScreenMessageBox myGuiScreenMessageBox = ((!Sync.IsServer) ? MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextAnyWorldBeforeExit), MyTexts.Get(MyCommonTexts.MessageBoxCaptionExit), null, null, null, null, OnExitToMainMenuFromCampaignMessageBoxCallback) : ((MySession.Static.Settings.EnableSaving && Sync.IsServer) ? MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO_CANCEL, MyTexts.Get(MyCommonTexts.MessageBoxTextSaveChangesBeforeExit), MyTexts.Get(MyCommonTexts.MessageBoxCaptionExit), null, null, null, null, OnExitToMainMenuMessageBoxCallback) : MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextCampaignBeforeExit), MyTexts.Get(MyCommonTexts.MessageBoxCaptionExit), null, null, null, null, OnExitToMainMenuFromCampaignMessageBoxCallback)));
		myGuiScreenMessageBox.SkipTransition = true;
		myGuiScreenMessageBox.InstantClose = false;
		MyGuiSandbox.AddScreen(myGuiScreenMessageBox);
	}

	private void OnExitToMainMenuMessageBoxCallback(MyGuiScreenMessageBox.ResultEnum callbackReturn)
	{
		switch (callbackReturn)
		{
			case MyGuiScreenMessageBox.ResultEnum.YES:
				MyAudio.Static.Mute = true;
				MyAudio.Static.StopMusic();
				MyAsyncSaving.Start(delegate
				{
					MySandboxGame.Static.OnScreenshotTaken += UnloadAndExitAfterScreeshotWasTaken;
				});
				break;
			case MyGuiScreenMessageBox.ResultEnum.NO:
				MyAudio.Static.Mute = true;
				MyAudio.Static.StopMusic();
				MySessionLoader.UnloadAndExitToMenu();
				break;
			case MyGuiScreenMessageBox.ResultEnum.CANCEL:
				base.CanBeHidden = true;
				break;
		}
	}

	private void OnExitToMainMenuFromCampaignMessageBoxCallback(MyGuiScreenMessageBox.ResultEnum callbackReturn)
	{
		if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES)
		{
			MyAudio.Static.Mute = true;
			MyAudio.Static.StopMusic();
			MySessionLoader.UnloadAndExitToMenu();
		}
		else
		{
			base.CanBeHidden = true;
		}
	}

	private void UnloadAndExitAfterScreeshotWasTaken(object sender, EventArgs e)
	{
		MySandboxGame.Static.OnScreenshotTaken -= UnloadAndExitAfterScreeshotWasTaken;
		MySessionLoader.UnloadAndExitToMenu();
	}

	private void OnClickOptions(MyGuiControlButton sender)
	{
		bool flag = !MyPlatformGameSettings.LIMITED_MAIN_MENU;
		MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen<MyCustomOptionsMenu>(new object[1] { !flag }));
	}

	private void OnClickExitToWindows(MyGuiControlButton sender)
	{
		MyGuiSandbox.AddScreen(MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextAreYouSureYouWantToExit), MyTexts.Get(MyCommonTexts.MessageBoxCaptionExit), null, null, null, null, OnExitToWindowsMessageBoxCallback));
	}

	private void OnExitToWindowsMessageBoxCallback(MyGuiScreenMessageBox.ResultEnum callbackReturn)
	{
		if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES)
		{
			OnLogoutProgressClosed();
		}
		else if (m_exitGameButton != null && m_exitGameButton.Visible)
		{
			base.FocusedControl = m_exitGameButton;
			m_exitGameButton.Selected = true;
		}
	}

	private void OnLogoutProgressClosed()
	{
		MySandboxGame.Config.ControllerDefaultOnStart = MyInput.Static.IsJoystickLastUsed;
		MySandboxGame.Config.Save();
		MySandboxGame.Log.WriteLine("Application closed by user");
		if (MySpaceAnalytics.Instance != null)
		{
			MySpaceAnalytics.Instance.ReportSessionEnd("Exit to Windows");
		}
		MyScreenManager.CloseAllScreensNowExcept(null);
		MySandboxGame.ExitThreadSafe();
	}

	private void OnClickSaveWorld(MyGuiControlButton sender)
	{
		base.CanBeHidden = false;
		MyGuiScreenMessageBox myGuiScreenMessageBox = ((!MyAsyncSaving.InProgress) ? MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextDoYouWantToSaveYourProgress), MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm), null, null, null, null, OnSaveWorldMessageBoxCallback) : MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.MessageBoxTextSavingInProgress), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError)));
		myGuiScreenMessageBox.SkipTransition = true;
		myGuiScreenMessageBox.InstantClose = false;
		MyGuiSandbox.AddScreen(myGuiScreenMessageBox);
	}

	private void OnSaveWorldMessageBoxCallback(MyGuiScreenMessageBox.ResultEnum callbackReturn)
	{
		if (callbackReturn == MyGuiScreenMessageBox.ResultEnum.YES)
		{
			MyAsyncSaving.Start();
		}
		else
		{
			base.CanBeHidden = true;
		}
	}

	private void OnClickSaveAs(MyGuiControlButton sender)
	{
		MyGuiSandbox.AddScreen(new MyGuiScreenSaveAs(MySession.Static.Name));
	}

	public override bool Update(bool hasFocus)
	{
		base.Update(hasFocus);
		if (MySession.Static == null)
		{
			Parallel.RunCallbacks();
		}
		m_currentDLCcounter += 16;
		if (m_currentDLCcounter > DLC_UPDATE_INTERVAL)
		{
			m_currentDLCcounter = 0;
			m_myBadgeHelper.RefreshGameLogo();
		}
		if (hasFocus && MyGuiScreenGamePlay.Static == null && MyInput.Static.IsNewKeyPressed(MyKeys.Escape))
		{
			OnClickExitToWindows(null);
		}
		if (hasFocus && m_lastClickedBanner != null)
		{
			base.FocusedControl = null;
			m_lastClickedBanner = null;
		}
		if (m_newsControl != null)
		{
			MyNewsLink[] newsLinks = m_newsControl.NewsLinks;
			if (newsLinks != null && newsLinks.Length != 0)
			{
				if (MyGuiScreenGamePlay.Static == null)
				{
					base.GamepadHelpTextId = MySpaceTexts.MainMenuScreen_Help_ScreenWithLink;
				}
				else
				{
					base.GamepadHelpTextId = MySpaceTexts.MainMenuScreen_Help_ScreenInGameWithLink;
				}
				goto IL_00ef;
			}
		}
		if (MyGuiScreenGamePlay.Static == null)
		{
			base.GamepadHelpTextId = MySpaceTexts.MainMenuScreen_Help_Screen;
		}
		else
		{
			base.GamepadHelpTextId = MySpaceTexts.MainMenuScreen_Help_ScreenIngame;
		}
		goto IL_00ef;
	IL_00ef:
		return true;
	}

	public override void HandleInput(bool receivedFocusInThisUpdate)
	{
		base.HandleInput(receivedFocusInThisUpdate);
		if (!receivedFocusInThisUpdate && MyGuiScreenGamePlay.Static != null && MyControllerHelper.IsControl(MySpaceBindingCreator.CX_GUI, MyControlsGUI.MAIN_MENU))
		{
			CloseScreen();
		}
		if (MyControllerHelper.IsControl(MySpaceBindingCreator.CX_GUI, MyControlsGUI.BUTTON_X))
		{
			ShowCurrentNews();
		}
	}

	private void ShowCurrentNews()
	{
		if (m_newsControl == null)
		{
			return;
		}
		MyNewsLink[] newsLinks = m_newsControl.NewsLinks;
		if (newsLinks != null && newsLinks.Length != 0)
		{
			MyNewsLink myNewsLink = m_newsControl.NewsLinks[0];
			if (!string.IsNullOrEmpty(myNewsLink.Link))
			{
				MyGuiSandbox.OpenUrlWithFallback(myNewsLink.Link, myNewsLink.Text);
			}
		}
	}

	private void CreateModIoConsentScreen(Action onConsentAgree = null, Action onConsentOptOut = null)
	{
		MyModIoConsentViewModel viewModel = new MyModIoConsentViewModel(onConsentAgree, onConsentOptOut);
		ServiceManager.Instance.GetService<IMyGuiScreenFactoryService>().CreateScreen(viewModel);
	}

	protected override void OnShow()
	{
		base.OnShow();
		m_backgroundTransition = MySandboxGame.Config.UIBkOpacity;
		m_guiTransition = MySandboxGame.Config.UIOpacity;
	}

	public override void CloseScreenNow(bool isUnloading = false)
	{
		base.CloseScreenNow(isUnloading);
		if (m_backgroundScreen != null)
		{
			m_backgroundScreen.CloseScreenNow(isUnloading);
		}
		m_backgroundScreen = null;
	}

	public override void OpenUserRelatedScreens()
	{
	}

	public override void CloseUserRelatedScreens()
	{
		m_newsControl?.CloseNewVersionScreen();
	}

	public override void OnScreenOrderChanged(MyGuiScreenBase oldLast, MyGuiScreenBase newLast)
	{
		base.OnScreenOrderChanged(oldLast, newLast);
		if (newLast == this)
		{
			CheckContinueButtonVisibility();
		}
	}

	private void CheckContinueButtonVisibility()
	{
		if (m_continueButton != null)
		{
			MyObjectBuilder_LastSession lastSession = MyLocalCache.GetLastSession();
			bool visible = lastSession != null && (lastSession.Path == null || Directory.Exists(lastSession.Path)) && (!lastSession.IsLobby || MyGameService.LobbyDiscovery.ContinueToLobbySupported);
			m_continueButton.Visible = visible;
		}
	}
}
