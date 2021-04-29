// Sandbox.Game.Screens.MyGuiScreenMods
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using ParallelTasks;
using Sandbox;
using Sandbox.Engine.Networking;
using Sandbox.Engine.Utils;
using Sandbox.Game.Gui;
using Sandbox.Game.GUI;
using Sandbox.Game.Localization;
using Sandbox.Game.Multiplayer;
using Sandbox.Game.Screens;
using Sandbox.Game.Screens.Helpers;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using VRage;
using VRage.Collections;
using VRage.FileSystem;
using VRage.Game;
using VRage.GameServices;
using VRage.Input;
using VRage.Utils;
using VRageMath;

public class MyMenuPacksMenu : MyGuiScreenBase
{
	private class ModDependenciesWorkData : WorkData
	{
		public ulong ParentId;

		public string ServiceName;

		public MyGuiControlTable.Row ParentModRow;

		public List<MyWorkshopItem> Dependencies;

		public bool HasReferenceIssue;
	}

	private MyGuiControlTable m_modsTableEnabled;

	private MyGuiControlTable m_modsTableDisabled;

	private MyGuiControlButton m_moveUpButton;

	private MyGuiControlButton m_moveDownButton;

	private MyGuiControlButton m_moveTopButton;

	private MyGuiControlButton m_moveBottomButton;

	private MyGuiControlButton m_moveLeftButton;

	private MyGuiControlButton m_moveLeftAllButton;

	private MyGuiControlButton m_moveRightButton;

	private MyGuiControlButton m_moveRightAllButton;

	private MyGuiControlButton m_openInWorkshopButton;

	private MyGuiControlButton m_refreshButton;

	private MyGuiControlButton m_browseWorkshopButton;

	private MyGuiControlButton m_publishModButton;

	private MyGuiControlButton m_okButton;

	private MyGuiControlTable.Row m_selectedRow;

	private MyGuiControlTable m_selectedTable;

	private bool m_listNeedsReload;

	private bool m_keepActiveMods;

	private List<MyWorkshopItem> m_subscribedMods;

	private List<MyWorkshopItem> m_worldMods;

	private List<MyObjectBuilder_Checkpoint.ModItem> m_modListToEdit;

	private MyGuiControlLabel m_workshopError;

	private bool m_workshopPermitted;

	private bool m_refreshWhenInFocusNext;

	private MyObjectBuilder_Checkpoint.ModItem m_selectedMod;

	private HashSet<string> m_worldLocalMods = new HashSet<string>();

	private HashSet<WorkshopId> m_worldWorkshopMods = new HashSet<WorkshopId>();

	private List<MyGuiControlButton> m_categoryButtonList = new List<MyGuiControlButton>();

	private MyGuiControlSearchBox m_searchBox;

	private MyGuiControlButton m_searchClear;

	private List<string> m_tmpSearch = new List<string>();

	private List<string> m_selectedCategories = new List<string>();

	private List<string> m_selectedServiceNames = new List<string>();

	private Dictionary<ulong, StringBuilder> m_modsToolTips = new Dictionary<ulong, StringBuilder>();

	private WorkshopId[] m_selectedModWorkshopIds;

	private int m_consentUpdateFrameTimer = 30;

	private int m_consentUpdateFrameTimer_current;

	public MyMenuPacksMenu()
		: base(new Vector2(0.5f, 0.5f), MyGuiConstants.SCREEN_BACKGROUND_COLOR, new Vector2(1.015f, 0.934f), isTopMostScreen: false, null, MySandboxGame.Config.UIBkOpacity, MySandboxGame.Config.UIOpacity)
	{
		RecreateControls(constructor: true);
	}

	private void RefreshWorldMods(ListReader<MyObjectBuilder_Checkpoint.ModItem> mods)
	{
		m_worldLocalMods.Clear();
		m_worldWorkshopMods.Clear();
		foreach (MyObjectBuilder_Checkpoint.ModItem item in mods)
		{
			if (item.PublishedFileId == 0L)
			{
				m_worldLocalMods.Add(item.Name);
			}
			else
			{
				m_worldWorkshopMods.Add(new WorkshopId(item.PublishedFileId, item.PublishedServiceName));
			}
		}
	}

	public override void RecreateControls(bool constructor)
	{
		base.RecreateControls(constructor);
		AddCaption(MyCommonTexts.ScreenCaptionWorkshop, null, new Vector2(0f, 0.003f));
		MyGuiControlSeparatorList myGuiControlSeparatorList = new MyGuiControlSeparatorList();
		myGuiControlSeparatorList.AddHorizontal(new Vector2(0f, 0f) - new Vector2(m_size.Value.X * 0.895f / 2f, m_size.Value.Y / 2f - 0.075f), m_size.Value.X * 0.895f);
		Controls.Add(myGuiControlSeparatorList);
		MyGuiControlSeparatorList myGuiControlSeparatorList2 = new MyGuiControlSeparatorList();
		Vector2 start = new Vector2(0f, 0f) - new Vector2(m_size.Value.X * 0.895f / 2f, (0f - m_size.Value.Y) / 2f + 0.123f);
		myGuiControlSeparatorList2.AddHorizontal(start, m_size.Value.X * 0.895f);
		Controls.Add(myGuiControlSeparatorList2);
		Vector2 vector = new Vector2(-0.454f, -0.417f);
		Vector2 vector2 = new Vector2(-0f, -4.75f * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA.Y);
		m_modsTableDisabled = new MyGuiControlTable();
		m_modsTableEnabled = new MyGuiControlTable();
		if (MyFakes.ENABLE_MOD_CATEGORIES)
		{
			m_modsTableDisabled.Position = vector + new Vector2(0f, 0.1f);
			m_modsTableDisabled.VisibleRowsCount = 18;
			m_modsTableEnabled.VisibleRowsCount = 18;
		}
		else
		{
			m_modsTableDisabled.Position = vector;
			m_modsTableDisabled.VisibleRowsCount = 20;
			m_modsTableEnabled.VisibleRowsCount = 20;
		}
		m_modsTableDisabled.Size = new Vector2(m_size.Value.X * 0.428f, 1f);
		m_modsTableDisabled.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
		m_modsTableDisabled.ColumnsCount = 2;
		m_modsTableDisabled.ItemSelected += OnTableItemSelected;
		m_modsTableDisabled.ItemDoubleClicked += OnTableItemConfirmedOrDoubleClick;
		m_modsTableDisabled.ItemConfirmed += OnTableItemConfirmedOrDoubleClick;
		float[] customColumnWidths = new float[2] { 0.065f, 0.945f };
		m_modsTableDisabled.SetCustomColumnWidths(customColumnWidths);
		m_modsTableDisabled.SetColumnComparison(1, (MyGuiControlTable.Cell a, MyGuiControlTable.Cell b) => a.Text.CompareToIgnoreCase(b.Text));
		Controls.Add(m_modsTableDisabled);
		m_modsTableEnabled.Position = vector + new Vector2(m_modsTableDisabled.Size.X + 0.04f, 0.1f);
		m_modsTableEnabled.Size = new Vector2(m_size.Value.X * 0.428f, 1f);
		m_modsTableEnabled.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP;
		m_modsTableEnabled.ColumnsCount = 2;
		m_modsTableEnabled.ItemSelected += OnTableItemSelected;
		m_modsTableEnabled.ItemDoubleClicked += OnTableItemConfirmedOrDoubleClick;
		m_modsTableEnabled.ItemConfirmed += OnTableItemConfirmedOrDoubleClick;
		m_modsTableEnabled.SetCustomColumnWidths(customColumnWidths);
		m_modsTableEnabled.SetColumnComparison(1, (MyGuiControlTable.Cell a, MyGuiControlTable.Cell b) => a.Text.CompareToIgnoreCase(b.Text));
		Controls.Add(m_modsTableEnabled);
		Controls.Add(m_moveRightAllButton = MakeButtonTiny(vector2 + 0f * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, 0f, MyCommonTexts.ToolTipScreenMods_MoveRightAll, MyGuiConstants.TEXTURE_BUTTON_ARROW_DOUBLE, OnMoveRightAllClick));
		Controls.Add(m_moveRightButton = MakeButtonTiny(vector2 + 1f * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, 0f, MyCommonTexts.ToolTipScreenMods_MoveRight, MyGuiConstants.TEXTURE_BUTTON_ARROW_SINGLE, OnMoveRightClick));
		Controls.Add(m_moveUpButton = MakeButtonTiny(vector2 + 2.5f * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, (float)Math.E * -449f / 777f, MyCommonTexts.ToolTipScreenMods_MoveUp, MyGuiConstants.TEXTURE_BUTTON_ARROW_SINGLE, OnMoveUpClick));
		Controls.Add(m_moveTopButton = MakeButtonTiny(vector2 + 3.5f * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, (float)Math.E * -449f / 777f, MyCommonTexts.ToolTipScreenMods_MoveTop, MyGuiConstants.TEXTURE_BUTTON_ARROW_DOUBLE, OnMoveTopClick));
		Controls.Add(m_moveBottomButton = MakeButtonTiny(vector2 + 4.5f * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, (float)Math.E * 449f / 777f, MyCommonTexts.ToolTipScreenMods_MoveBottom, MyGuiConstants.TEXTURE_BUTTON_ARROW_DOUBLE, OnMoveBottomClick));
		Controls.Add(m_moveDownButton = MakeButtonTiny(vector2 + 5.5f * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, (float)Math.E * 449f / 777f, MyCommonTexts.ToolTipScreenMods_MoveDown, MyGuiConstants.TEXTURE_BUTTON_ARROW_SINGLE, OnMoveDownClick));
		Controls.Add(m_moveLeftButton = MakeButtonTiny(vector2 + 7f * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, 3.141593f, MyCommonTexts.ToolTipScreenMods_MoveLeft, MyGuiConstants.TEXTURE_BUTTON_ARROW_SINGLE, OnMoveLeftClick));
		Controls.Add(m_moveLeftAllButton = MakeButtonTiny(vector2 + 8f * MyGuiConstants.MENU_BUTTONS_POSITION_DELTA, 3.141593f, MyCommonTexts.ToolTipScreenMods_MoveLeftAll, MyGuiConstants.TEXTURE_BUTTON_ARROW_DOUBLE, OnMoveLeftAllClick));
		float num = 0.0075f;
		m_okButton = MakeButton(new Vector2(m_modsTableDisabled.Position.X + 0.002f, 0f) - new Vector2(0f, (0f - m_size.Value.Y) / 2f + 0.097f), MyCommonTexts.Ok, MyCommonTexts.Ok, OnOkClick);
		m_okButton.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipMods_Ok));
		m_okButton.Visible = !MyInput.Static.IsJoystickLastUsed;
		Controls.Add(m_okButton);
		m_refreshButton = MakeButton(m_okButton.Position + new Vector2(m_okButton.Size.X + num, 0f), MyCommonTexts.ScreenLoadSubscribedWorldRefresh, MyCommonTexts.ToolTipWorkshopRefreshMod, OnRefreshClick);
		m_refreshButton.Visible = !MyInput.Static.IsJoystickLastUsed;
		Controls.Add(m_refreshButton);
		m_browseWorkshopButton = MakeButton(m_okButton.Position + new Vector2(m_okButton.Size.X * 2f + num * 2f, 0f), MyCommonTexts.ScreenLoadSubscribedWorldBrowseWorkshop, MyCommonTexts.ToolTipWorkshopBrowseWorkshop, OnBrowseWorkshopClick);
		m_browseWorkshopButton.Visible = !MyInput.Static.IsJoystickLastUsed;
		Controls.Add(m_browseWorkshopButton);
		m_openInWorkshopButton = MakeButton(m_okButton.Position + new Vector2(m_okButton.Size.X * 3f + num * 3f, 0f), MyCommonTexts.ScreenLoadSubscribedWorldOpenInWorkshop, MyCommonTexts.ToolTipWorkshopPublish, OnOpenInWorkshopClick);
		Controls.Add(m_openInWorkshopButton);
		m_publishModButton = MakeButton(m_okButton.Position + new Vector2(m_okButton.Size.X * 4f + num * 4f, 0f), MyCommonTexts.LoadScreenButtonPublish, MyCommonTexts.LoadScreenButtonPublish, OnPublishModClick);
		Controls.Add(m_publishModButton);
		m_workshopError = new MyGuiControlLabel(null, null, null, Color.Red);
		m_workshopError.Position = new Vector2(-0.454f, m_okButton.Position.Y + 0.07f);
		m_workshopError.Visible = false;
		Controls.Add(m_workshopError);
		if (MyFakes.ENABLE_MOD_CATEGORIES)
		{
			Vector2 vector3 = m_modsTableDisabled.Position + new Vector2(0.015f, -0.036f);
			Vector2 vector4 = new Vector2(0.0335f, 0f);
			MyWorkshop.Category[] modCategories = MyWorkshop.ModCategories;
			for (int i = 0; i < modCategories.Length; i++)
			{
				if (modCategories[i].IsVisibleForFilter)
				{
					Controls.Add(MakeButtonCategory(vector3 + vector4 * i, modCategories[i]));
				}
			}
			Vector2 vector5 = new Vector2(m_modsTableEnabled.Position.X, 0f) - new Vector2(0f, m_size.Value.Y / 2f - 0.099f);
			m_searchBox = new MyGuiControlSearchBox(vector5 + new Vector2(0f, 0.013f));
			m_searchBox.SetToolTip(MyTexts.GetString(MySpaceTexts.ToolTipMods_Search));
			m_searchBox.OriginAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_CENTER;
			m_searchBox.Size = new Vector2(m_modsTableEnabled.Size.X, 0.2f);
			m_searchBox.OnTextChanged += OnSearchTextChanged;
			Vector2 vector6 = new Vector2(0f, 0.05f);
			m_moveUpButton.Position += vector6;
			m_moveTopButton.Position += vector6;
			m_moveBottomButton.Position += vector6;
			m_moveDownButton.Position += vector6;
			m_moveLeftButton.Position += vector6;
			m_moveLeftAllButton.Position += vector6;
			m_moveRightAllButton.Position += vector6;
			m_moveRightButton.Position += vector6;
			Controls.Add(m_searchBox);
		}
		Vector2 minSizeGui = MyGuiControlButton.GetVisualStyle(MyGuiControlButtonStyleEnum.Default).NormalTexture.MinSizeGui;
		MyGuiControlLabel myGuiControlLabel = new MyGuiControlLabel(new Vector2(start.X, m_okButton.Position.Y + minSizeGui.Y / 2f));
		myGuiControlLabel.Name = MyGuiScreenBase.GAMEPAD_HELP_LABEL_NAME;
		Controls.Add(myGuiControlLabel);
		base.CloseButtonEnabled = true;
		if ((float)MySandboxGame.ScreenSize.X / (float)MySandboxGame.ScreenSize.Y == 1.25f)
		{
			SetCloseButtonOffset_5_to_4();
		}
		else
		{
			SetDefaultCloseButtonOffset();
		}
		base.GamepadHelpTextId = MySpaceTexts.ModsScreen_Help_Screen;
		if (!MyInput.Static.IsJoystickLastUsed)
		{
			base.FocusedControl = m_searchBox.TextBox;
		}
		
	}

	private void OnSearchTextChanged(string text)
	{
		if (!string.IsNullOrEmpty(text))
		{
			string[] source = text.Split(' ');
			m_tmpSearch = Enumerable.ToList(source);
		}
		else
		{
			m_tmpSearch.Clear();
		}
		RefreshModList();
	}

	private void OnPublishModClick(MyGuiControlButton obj)
    {
        
    }

    private void OnTableItemSelected(MyGuiControlTable sender, MyGuiControlTable.EventArgs eventArgs)
	{
		sender.CanHaveFocus = true;
		base.FocusedControl = sender;
		m_selectedRow = sender.SelectedRow;
		m_selectedTable = sender;
		if (sender == m_modsTableEnabled)
		{
			m_modsTableDisabled.SelectedRowIndex = null;
		}
		if (sender == m_modsTableDisabled)
		{
			m_modsTableEnabled.SelectedRowIndex = null;
		}
		if (MyInput.Static.IsAnyCtrlKeyPressed())
		{
			OnTableItemConfirmedOrDoubleClick(sender, eventArgs);
		}
	}

	private MyGuiControlButton MakeButtonCategory(Vector2 position, MyWorkshop.Category category)
	{
		string arg = category.Id.Replace(" ", "");
		MyGuiControlButton myGuiControlButton = MakeButtonCategoryTiny(position, 0f, category.LocalizableName, new MyGuiHighlightTexture
		{
			Normal = $"Textures\\GUI\\Icons\\buttons\\small_variant\\{arg}.dds",
			Highlight = $"Textures\\GUI\\Icons\\buttons\\small_variant\\{arg}.dds",
			Focus = $"Textures\\GUI\\Icons\\buttons\\small_variant\\{arg}_focus.dds",
			SizePx = new Vector2(48f, 48f)
		}, OnCategoryButtonClick);
		myGuiControlButton.UserData = category.Id;
		m_categoryButtonList.Add(myGuiControlButton);
		myGuiControlButton.Size = new Vector2(0.005f, 0.005f);
		return myGuiControlButton;
	}

	private void OnCategoryButtonClick(MyGuiControlButton sender)
	{
		if (sender.UserData != null && sender.UserData is string)
		{
			string item = (string)sender.UserData;
			if (m_selectedCategories.Contains(item))
			{
				m_selectedCategories.Remove(item);
				sender.Selected = false;
			}
			else
			{
				m_selectedCategories.Add(item);
				sender.Selected = true;
			}
			RefreshModList();
		}
	}

	private MyGuiControlButton MakeButtonCategoryTiny(Vector2 position, float rotation, MyStringId toolTip, MyGuiHighlightTexture icon, Action<MyGuiControlButton> onClick, Vector2? size = null)
	{
		Vector2? position2 = position;
		string @string = MyTexts.GetString(toolTip);
		MyGuiControlButton myGuiControlButton = new MyGuiControlButton(position2, MyGuiControlButtonStyleEnum.Square48, size, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, @string, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, onClick);
		icon.SizePx = new Vector2(48f, 48f);
		myGuiControlButton.Icon = icon;
		myGuiControlButton.IconRotation = rotation;
		myGuiControlButton.IconOriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
		return myGuiControlButton;
	}

	private void OnTableItemConfirmedOrDoubleClick(MyGuiControlTable sender, MyGuiControlTable.EventArgs eventArgs)
	{
		if (sender.SelectedRow != null)
		{
			MyGuiControlTable.Row selectedRow = sender.SelectedRow;
			MyObjectBuilder_Checkpoint.ModItem selectedMod = (MyObjectBuilder_Checkpoint.ModItem)selectedRow.UserData;
			MyGuiControlTable myGuiControlTable = ((sender == m_modsTableEnabled) ? m_modsTableDisabled : m_modsTableEnabled);
			MoveSelectedItem(sender, myGuiControlTable);
		}
	}

	private void MoveSelectedItem(MyGuiControlTable from, MyGuiControlTable to)
	{
		to.Add(from.SelectedRow);
		from.RemoveSelectedRow();
		m_selectedRow = from.SelectedRow;
	}

	private MyGuiControlButton MakeButtonTiny(Vector2 position, float rotation, MyStringId toolTip, MyGuiHighlightTexture icon, Action<MyGuiControlButton> onClick, Vector2? size = null)
	{
		Vector2? position2 = position;
		string @string = MyTexts.GetString(toolTip);
		MyGuiControlButton myGuiControlButton = new MyGuiControlButton(position2, MyGuiControlButtonStyleEnum.Square, size, null, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, @string, null, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, onClick);
		icon.SizePx = new Vector2(64f, 64f);
		myGuiControlButton.Icon = icon;
		myGuiControlButton.IconRotation = rotation;
		myGuiControlButton.IconOriginAlign = MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER;
		return myGuiControlButton;
	}

	private void OnMoveUpClick(MyGuiControlButton sender)
	{
		m_selectedTable.MoveSelectedRowUp();
	}

	private void OnMoveDownClick(MyGuiControlButton sender)
	{
		m_selectedTable.MoveSelectedRowDown();
	}

	private void OnMoveTopClick(MyGuiControlButton sender)
	{
		m_selectedTable.MoveSelectedRowTop();
	}

	private void OnMoveBottomClick(MyGuiControlButton sender)
	{
		m_selectedTable.MoveSelectedRowBottom();
	}

	private void OnMoveLeftClick(MyGuiControlButton sender)
	{
		MoveSelectedItem(m_modsTableEnabled, m_modsTableDisabled);
	}

	private void OnMoveRightClick(MyGuiControlButton sender)
	{
		MoveSelectedItem(m_modsTableDisabled, m_modsTableEnabled);
	}

	private void OnMoveLeftAllClick(MyGuiControlButton sender)
	{
		while (m_modsTableEnabled.RowsCount > 0)
		{
			m_modsTableEnabled.SelectedRowIndex = 0;
			MoveSelectedItem(m_modsTableEnabled, m_modsTableDisabled);
		}
	}

	private void OnMoveRightAllClick(MyGuiControlButton sender)
	{
		while (m_modsTableDisabled.RowsCount > 0)
		{
			m_modsTableDisabled.SelectedRowIndex = 0;
			MoveSelectedItem(m_modsTableDisabled, m_modsTableEnabled);
		}
	}

	private MyGuiControlButton MakeButton(Vector2 position, MyStringId text, MyStringId toolTip, Action<MyGuiControlButton> onClick, MyGuiDrawAlignEnum originAlign = MyGuiDrawAlignEnum.HORISONTAL_LEFT_AND_VERTICAL_TOP)
	{
		Vector2? position2 = position;
		StringBuilder text2 = MyTexts.Get(text);
		string @string = MyTexts.GetString(toolTip);
		return new MyGuiControlButton(position2, MyGuiControlButtonStyleEnum.Default, null, null, originAlign, @string, text2, 0.8f, MyGuiDrawAlignEnum.HORISONTAL_CENTER_AND_VERTICAL_CENTER, MyGuiControlHighlightType.WHEN_CURSOR_OVER, onClick);
	}

	private void OnOpenInWorkshopClick(MyGuiControlButton obj)
	{
		///if (m_selectedRow == null)
		//{
		//	return;
		//}
		//object userData = m_selectedRow.UserData;
		//if (userData is MyObjectBuilder_Checkpoint.ModItem)
		//{
		//	MyObjectBuilder_Checkpoint.ModItem modItem = (MyObjectBuilder_Checkpoint.ModItem)userData;
		//	MyWorkshopItem subscribedItem = GetSubscribedItem(modItem.PublishedFileId, modItem.PublishedServiceName);
		//	if (subscribedItem != null)
		//	{
		//		MyGuiSandbox.OpenUrlWithFallback(subscribedItem.GetItemUrl(), subscribedItem.ServiceName + " Workshop");
		//	}
		//}
		///
	}

	private void OnBrowseWorkshopClick(MyGuiControlButton obj)
	{
		MyWorkshop.OpenWorkshopBrowser(MySteamConstants.TAG_MODS, delegate
		{
			m_refreshWhenInFocusNext = true;
		});
	}

	private void OnRefreshClick(MyGuiControlButton obj)
	{
		if (!m_listNeedsReload)
		{
			m_listNeedsReload = true;
			FillList();
		}
	}

	private void OnOkClick(MyGuiControlButton obj)
	{
		m_modListToEdit.Clear();
		GetActiveMods(m_modListToEdit);
		CloseScreen();
	}

	private void OnCancelClick(MyGuiControlButton obj)
	{
		CloseScreen();
	}

	private void GetActiveMods(List<MyObjectBuilder_Checkpoint.ModItem> outputList)
	{
		for (int num = m_modsTableEnabled.RowsCount - 1; num >= 0; num--)
		{
			outputList.Add((MyObjectBuilder_Checkpoint.ModItem)m_modsTableEnabled.GetRow(num).UserData);
		}
	}

	private void FillList()
	{
		MyGuiSandbox.AddScreen(new MyGuiScreenProgressAsync(MyCommonTexts.LoadingPleaseWait, null, beginAction, endAction));
	}

	private IMyAsyncResult beginAction()
	{
		return new MyModsLoadListResult(m_worldWorkshopMods);
	}

	private void endAction(IMyAsyncResult result, MyGuiScreenProgressAsync screen)
	{
		m_listNeedsReload = false;
		MyModsLoadListResult myModsLoadListResult = result as MyModsLoadListResult;
		if (myModsLoadListResult != null)
		{
			m_subscribedMods = myModsLoadListResult.SubscribedMods;
			m_worldMods = myModsLoadListResult.SetMods;
			RefreshModList();
			screen.CloseScreen();
		}
	}

	private void RefreshModList()
	{
		
	}

	public override string GetFriendlyName()
    {
		return "MyMenuPacksMenu";
	}
}
