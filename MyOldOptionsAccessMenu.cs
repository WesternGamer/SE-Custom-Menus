using Sandbox;
using Sandbox.Game.Gui;
using Sandbox.Game.Localization;
using Sandbox.Graphics.GUI;
using SpaceEngineers.Game.GUI;
using VRage;
using VRage.Game;
using VRage.Utils;
using VRageMath;

internal class MyOldOptionsAccessMenu : MyGuiScreenBase
{
	private MyGuiControlElementGroup m_elementGroup;

	private bool m_isLimitedMenu;

	public MyOldOptionsAccessMenu(bool isLimitedMenu = false)
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
		AddCaption(MyCommonTexts.ScreenCaptionOptions, null, new Vector2(0f, 0.003f));
		m_backgroundTransition = MySandboxGame.Config.UIBkOpacity;
		m_guiTransition = MySandboxGame.Config.UIOpacity;
		MyGuiControlSeparatorList myGuiControlSeparatorList = new MyGuiControlSeparatorList();
		myGuiControlSeparatorList.AddHorizontal(-new Vector2(m_size.Value.X * 0.83f / 2f, m_size.Value.Y / 2f - 0.075f), m_size.Value.X * 0.83f);
		Controls.Add(myGuiControlSeparatorList);
		MyGuiControlSeparatorList myGuiControlSeparatorList2 = new MyGuiControlSeparatorList();
		Vector2 start = -new Vector2(m_size.Value.X * 0.83f / 2f, (0f - m_size.Value.Y) / 2f + 0.05f);
		myGuiControlSeparatorList2.AddHorizontal(start, m_size.Value.X * 0.83f);
		Controls.Add(myGuiControlSeparatorList2);
		MyStringId optionsScreen_Help_Menu = MySpaceTexts.OptionsScreen_Help_Menu;
		Vector2 vector = new Vector2(0.001f, (0f - m_size.Value.Y) / 2f + 0.126f);
		int num = 0;
		MyGuiControlButton myGuiControlButton = new MyGuiControlButton(vector + num++ * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyGuiControlButtonStyleEnum.Default, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ScreenOptionsButtonGame), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, delegate
		{
			MyGuiSandbox.AddScreen(new MyGuiScreenOptionsGame(m_isLimitedMenu));
		});
		myGuiControlButton.GamepadHelpTextId = optionsScreen_Help_Menu;
		Controls.Add(myGuiControlButton);
		m_elementGroup.Add(myGuiControlButton);
		if (!m_isLimitedMenu)
		{
			myGuiControlButton = new MyGuiControlButton(vector + num++ * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyGuiControlButtonStyleEnum.Default, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ScreenOptionsButtonDisplay), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, delegate
			{
				MyGuiSandbox.AddScreen(new MyGuiScreenOptionsDisplay());
			});
			myGuiControlButton.GamepadHelpTextId = optionsScreen_Help_Menu;
			Controls.Add(myGuiControlButton);
			m_elementGroup.Add(myGuiControlButton);
			myGuiControlButton = new MyGuiControlButton(vector + num++ * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyGuiControlButtonStyleEnum.Default, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ScreenOptionsButtonGraphics), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, delegate
			{
				MyGuiSandbox.AddScreen(new MyGuiScreenOptionsGraphics());
			});
			myGuiControlButton.GamepadHelpTextId = optionsScreen_Help_Menu;
			Controls.Add(myGuiControlButton);
			m_elementGroup.Add(myGuiControlButton);
		}
		myGuiControlButton = new MyGuiControlButton(vector + num++ * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyGuiControlButtonStyleEnum.Default, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ScreenOptionsButtonAudio), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, delegate
		{
			MyGuiSandbox.AddScreen(new MyGuiScreenOptionsAudio(m_isLimitedMenu));
		});
		myGuiControlButton.GamepadHelpTextId = optionsScreen_Help_Menu;
		Controls.Add(myGuiControlButton);
		m_elementGroup.Add(myGuiControlButton);
		myGuiControlButton = new MyGuiControlButton(vector + num++ * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyGuiControlButtonStyleEnum.Default, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ScreenOptionsButtonControls), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, delegate
		{
			MyGuiSandbox.AddScreen(new MyGuiScreenOptionsControls());
		});
		myGuiControlButton.GamepadHelpTextId = optionsScreen_Help_Menu;
		Controls.Add(myGuiControlButton);
		m_elementGroup.Add(myGuiControlButton);
		myGuiControlButton = new MyGuiControlButton(vector + num++ * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, MyGuiControlButtonStyleEnum.Default, null, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, null, MyTexts.Get(MyCommonTexts.ScreenMenuButtonCredits), 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, delegate (MyGuiControlButton sender)
		{
			OnClickCredits(sender);
		});
		myGuiControlButton.GamepadHelpTextId = optionsScreen_Help_Menu;
		Controls.Add(myGuiControlButton);
		m_elementGroup.Add(myGuiControlButton);
		Vector2 minSizeGui = MyGuiControlButton.GetVisualStyle(MyGuiControlButtonStyleEnum.Default).NormalTexture.MinSizeGui;
		MyGuiControlLabel myGuiControlLabel = new MyGuiControlLabel(new Vector2(0f, start.Y + minSizeGui.Y / 2f), null, null, null, 0.8f, "Blue", MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER);
		myGuiControlLabel.Name = MyGuiScreenBase.GAMEPAD_HELP_LABEL_NAME;
		Controls.Add(myGuiControlLabel);
		base.GamepadHelpTextId = MySpaceTexts.Gamepad_Help_Back;
		base.CloseButtonEnabled = true;
	}

	private void OnClickCredits(MyGuiControlButton sender)
	{
		MyGuiSandbox.AddScreen(new MyGuiScreenGameCredits());
	}

	protected override void OnShow()
	{
		base.OnShow();
		m_backgroundTransition = MySandboxGame.Config.UIBkOpacity;
		m_guiTransition = MySandboxGame.Config.UIOpacity;
	} 

	public override string GetFriendlyName()
	{
		return "MyCustomOptionsMenu";
	}

	public void OnBackClick(MyGuiControlButton sender)
	{
		CloseScreen();
	}
}
