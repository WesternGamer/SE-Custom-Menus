using ParallelTasks;
using Sandbox;
using Sandbox.Engine.Networking;
using Sandbox.Engine.Utils;
using Sandbox.Game;
using Sandbox.Game.Gui;
using Sandbox.Game.Localization;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Screens;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using SpaceEngineers.Game.GUI;
using System;
using System.IO;
using System.Text;
using VRage;
using VRage.Audio;
using VRage.Game;
using VRage.GameServices;
using VRage.Utils;
using VRageMath;

internal class MyOldMenusAccessMenu : MyGuiScreenBase
{
	private MyGuiControlElementGroup m_elementGroup;

	private bool m_parallelLoadIsRunning;

	private bool m_isLimitedMenu;

	public MyOldMenusAccessMenu(bool isLimitedMenu = false)
		: base(new Vector2(0.5f, 0.5f), MyGuiConstants.SCREEN_BACKGROUND_COLOR, isLimitedMenu ? new Vector2(9f / 28f, 0.405534357f) : new Vector2(9f / 28f, 0.5200382f), isTopMostScreen: false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
	{
		m_isLimitedMenu = isLimitedMenu;
		base.EnabledBackgroundFade = true;
		RecreateControls(constructor: true);
	}

	public override void RecreateControls(bool constructor)
	{
		base.RecreateControls(constructor);
		m_elementGroup = new MyGuiControlElementGroup();
		AddCaption(MyCustomTexts.OldButtonsAndMenus, null, new Vector2(0f, 0.003f));
		m_backgroundTransition = MySandboxGame.Config.UIBkOpacity;
		m_guiTransition = MySandboxGame.Config.UIOpacity;
		MyGuiControlSeparatorList myGuiControlSeparatorList = new MyGuiControlSeparatorList();
		myGuiControlSeparatorList.AddHorizontal(-new Vector2(m_size.Value.X * 0.83f / 2f, m_size.Value.Y / 2f - 0.075f), m_size.Value.X * 0.83f);
		Controls.Add(myGuiControlSeparatorList);
		MyGuiControlSeparatorList myGuiControlSeparatorList2 = new MyGuiControlSeparatorList();
		Vector2 start = -new Vector2(m_size.Value.X * 0.83f / 2f, (0f - m_size.Value.Y) / 2f + 0.05f);
		myGuiControlSeparatorList2.AddHorizontal(start, m_size.Value.X * 0.83f);
		Controls.Add(myGuiControlSeparatorList2);
		if (MyGuiScreenGamePlay.Static == null)
		{
			CreateMainMenuControls();
		}
		else
		{
			CreateInGameMenuControls();
		}
		Vector2 minSizeGui = MyGuiControlButton.GetVisualStyle(MyGuiControlButtonStyleEnum.Default).NormalTexture.MinSizeGui;
		MyGuiControlLabel myGuiControlLabel = new MyGuiControlLabel(new Vector2(0f, start.Y + minSizeGui.Y / 2f), null, null, null, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
		myGuiControlLabel.Name = MyGuiScreenBase.GAMEPAD_HELP_LABEL_NAME;
		Controls.Add(myGuiControlLabel);
		base.GamepadHelpTextId = MySpaceTexts.Gamepad_Help_Back;
		base.CloseButtonEnabled = true;
	}

    private void CreateInGameMenuControls()
    {
		MyStringId optionsScreen_Help_Menu = MySpaceTexts.OptionsScreen_Help_Menu;
		Vector2 vector = new Vector2(0.001f, (0f - m_size.Value.Y) / 2f + 0.126f);
		int num = 0;

		MyGuiControlButton myGuiControlButton = new MyGuiControlButton(vector + num++ * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyGuiControlButtonStyleEnum.Default, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ScreenMenuButtonSave), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, delegate
		{
			base.CanBeHidden = false;
			MyGuiScreenMessageBox myGuiScreenMessageBox = ((!MyAsyncSaving.InProgress) ? MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextDoYouWantToSaveYourProgress), MyTexts.Get(MyCommonTexts.MessageBoxCaptionPleaseConfirm), null, null, null, null, OnSaveWorldMessageBoxCallback) : MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.OK, MyTexts.Get(MyCommonTexts.MessageBoxTextSavingInProgress), MyTexts.Get(MyCommonTexts.MessageBoxCaptionError)));
			myGuiScreenMessageBox.SkipTransition = true;
			myGuiScreenMessageBox.InstantClose = false;
			MyGuiSandbox.AddScreen(myGuiScreenMessageBox);
		});
		myGuiControlButton.GamepadHelpTextId = optionsScreen_Help_Menu;
		

		MyGuiControlButton myGuiControlButton2 = new MyGuiControlButton(vector + num++ * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyGuiControlButtonStyleEnum.Default, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.LoadScreenButtonSaveAs), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, delegate
		{
			MyGuiSandbox.AddScreen(new MyGuiScreenSaveAs(MySession.Static.Name));
		});
		myGuiControlButton2.GamepadHelpTextId = optionsScreen_Help_Menu;
		

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
			myGuiControlButton3 = new MyGuiControlButton(vector + num++ * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyGuiControlButtonStyleEnum.Default, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ScreenMenuButtonPlayers), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, delegate
			{
				MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen<MyGuiScreenPlayers>(Array.Empty<object>()));
			});
			myGuiControlButton3.GamepadHelpTextId = optionsScreen_Help_Menu;
			Controls.Add(myGuiControlButton3);
			m_elementGroup.Add(myGuiControlButton3);
		}


		MyGuiControlButton myGuiControlButton4 = new MyGuiControlButton(vector + num++ * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyGuiControlButtonStyleEnum.Default, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ScreenMenuButtonOptions), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, delegate
		{
			MyGuiSandbox.AddScreen(new MyGuiScreenOptionsAudio(m_isLimitedMenu));
		});
		myGuiControlButton4.GamepadHelpTextId = optionsScreen_Help_Menu;
		Controls.Add(myGuiControlButton4);
		m_elementGroup.Add(myGuiControlButton4);

		MyGuiControlButton myGuiControlButton5 = new MyGuiControlButton(vector + num++ * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyGuiControlButtonStyleEnum.Default, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ScreenMenuButtonExitToMainMenu), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, delegate
		{
			base.CanBeHidden = false;
			MyGuiScreenMessageBox myGuiScreenMessageBox = ((!Sync.IsServer) ? MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextAnyWorldBeforeExit), MyTexts.Get(MyCommonTexts.MessageBoxCaptionExit), null, null, null, null, OnExitToMainMenuFromCampaignMessageBoxCallback) : ((MySession.Static.Settings.EnableSaving && Sync.IsServer) ? MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Error, MyMessageBoxButtonsType.YES_NO_CANCEL, MyTexts.Get(MyCommonTexts.MessageBoxTextSaveChangesBeforeExit), MyTexts.Get(MyCommonTexts.MessageBoxCaptionExit), null, null, null, null, OnExitToMainMenuMessageBoxCallback) : MyGuiSandbox.CreateMessageBox(MyMessageBoxStyleEnum.Info, MyMessageBoxButtonsType.YES_NO, MyTexts.Get(MyCommonTexts.MessageBoxTextCampaignBeforeExit), MyTexts.Get(MyCommonTexts.MessageBoxCaptionExit), null, null, null, null, OnExitToMainMenuFromCampaignMessageBoxCallback)));
			myGuiScreenMessageBox.SkipTransition = true;
			myGuiScreenMessageBox.InstantClose = false;
			MyGuiSandbox.AddScreen(myGuiScreenMessageBox);
		});
		
		myGuiControlButton4.GamepadHelpTextId = optionsScreen_Help_Menu;
		Controls.Add(myGuiControlButton5);
		m_elementGroup.Add(myGuiControlButton5);
		
	}

    private void CreateMainMenuControls()
    {
		MyStringId optionsScreen_Help_Menu = MySpaceTexts.OptionsScreen_Help_Menu;
		Vector2 vector = new Vector2(0.001f, (0f - m_size.Value.Y) / 2f + 0.126f);
		int num = 0;
		MyObjectBuilder_LastSession lastSession = MyLocalCache.GetLastSession();
		if (lastSession != null && (lastSession.Path == null || MyPlatformGameSettings.GAME_SAVES_TO_CLOUD || Directory.Exists(lastSession.Path)) && (!lastSession.IsLobby || MyGameService.LobbyDiscovery.ContinueToLobbySupported))
		{
			MyGuiControlButton myGuiControlButton = new MyGuiControlButton(vector + num++ * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyGuiControlButtonStyleEnum.Default, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ScreenMenuButtonContinueGame), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, delegate
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
		});

			myGuiControlButton.GamepadHelpTextId = optionsScreen_Help_Menu;
			Controls.Add(myGuiControlButton);
			m_elementGroup.Add(myGuiControlButton);
		}
		else
		{
			num--;
		}

		MyGuiControlButton myGuiControlButton2 = new MyGuiControlButton(vector + num++ * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyGuiControlButtonStyleEnum.Default, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ScreenMenuButtonCampaign), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, delegate
		{
			MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen<MyGuiScreenNewGame>(new object[3] { true, true, true }));
		});
		myGuiControlButton2.GamepadHelpTextId = optionsScreen_Help_Menu;
		Controls.Add(myGuiControlButton2);
		m_elementGroup.Add(myGuiControlButton2);


		
		MyGuiControlButton myGuiControlButton3 = new MyGuiControlButton(vector + num++ * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyGuiControlButtonStyleEnum.Default, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ScreenMenuButtonLoadGame), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, delegate
		{
			MyGuiSandbox.AddScreen(new MyGuiScreenLoadSandbox());
		});
		myGuiControlButton3.GamepadHelpTextId = optionsScreen_Help_Menu;
		Controls.Add(myGuiControlButton3);
		m_elementGroup.Add(myGuiControlButton3);


		if (MyPerGameSettings.MultiplayerEnabled)
		{
			MyGuiControlButton myGuiControlButton4 = new MyGuiControlButton(vector + num++ * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyGuiControlButtonStyleEnum.Default, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ScreenMenuButtonJoinGame), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, delegate
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
			myGuiControlButton4.GamepadHelpTextId = optionsScreen_Help_Menu;
			Controls.Add(myGuiControlButton4);
			m_elementGroup.Add(myGuiControlButton4);
		}

		MyGuiControlButton myGuiControlButton5 = new MyGuiControlButton(vector + num++ * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyGuiControlButtonStyleEnum.Default, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ScreenMenuButtonOptions), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, delegate
		{
			bool flag = !MyPlatformGameSettings.LIMITED_MAIN_MENU;
			            
			MyGuiSandbox.AddScreen(MyGuiSandbox.CreateScreen<MyOldOptionsAccessMenu>(new object[1] { !flag }));
		});
		myGuiControlButton5.GamepadHelpTextId = optionsScreen_Help_Menu;
		Controls.Add(myGuiControlButton5);
		m_elementGroup.Add(myGuiControlButton5);

		if (MyFakes.ENABLE_MAIN_MENU_INVENTORY_SCENE)
		{
			MyGuiControlButton myGuiControlButton6 = new MyGuiControlButton(vector + num++ * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyGuiControlButtonStyleEnum.Default, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ScreenMenuButtonInventory), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, delegate
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
		});
			myGuiControlButton6.GamepadHelpTextId = optionsScreen_Help_Menu;
			Controls.Add(myGuiControlButton6);
			m_elementGroup.Add(myGuiControlButton6);
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

	private void joinGameScreen_Closed(MyGuiScreenBase source, bool isUnloading)
	{
		if (source.Cancelled)
		{
			base.State = MyGuiScreenState.OPENING;
			source.Closed -= joinGameScreen_Closed;
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
	protected override void OnShow()
	{
		base.OnShow();
		m_backgroundTransition = MySandboxGame.Config.UIBkOpacity;
		m_guiTransition = MySandboxGame.Config.UIOpacity;
	}

	public override string GetFriendlyName()
	{
		return "MyOldMenusAccessMenu";
	}

	public void OnBackClick(MyGuiControlButton sender)
	{
		CloseScreen();
	}
}
