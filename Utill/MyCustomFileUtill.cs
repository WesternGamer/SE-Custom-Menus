using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using ProtoBuf;
using Sandbox;
using Sandbox.Engine.Multiplayer;
using Sandbox.Engine.Platform;
using Sandbox.Game.Gui;
using Sandbox.Game.Screens;
using Sandbox.Game.World;
using Sandbox.Graphics.GUI;
using Sandbox.ModAPI;
using VRage.FileSystem;
using VRage.Game.ModAPI;

public class MyCustomFileUtilities : IMyUtilities, IMyGamePaths
{
	private const string STORAGE_FOLDER = "Storage";

	public static readonly MyCustomFileUtilities Static;

	private Dictionary<long, List<Action<object>>> m_registeredListeners = new Dictionary<long, List<Action<object>>>();

	public Dictionary<string, object> Variables = new Dictionary<string, object>();

	IMyConfigDedicated IMyUtilities.ConfigDedicated => MySandboxGame.ConfigDedicated;

	string IMyGamePaths.ContentPath => MyFileSystem.ContentPath;

	string IMyGamePaths.ModsPath => MyFileSystem.ModsPath;

	string IMyGamePaths.UserDataPath => MyFileSystem.UserDataPath;

	string IMyGamePaths.SavesPath => MyFileSystem.SavesPath;

	string IMyGamePaths.ModScopeName => StripDllExtIfNecessary(Assembly.GetCallingAssembly().ManifestModule.ScopeName);

	IMyGamePaths IMyUtilities.GamePaths => this;

	bool IMyUtilities.IsDedicated => Game.IsDedicated;

	public event MessageEnteredDel MessageEntered;

	public event MessageEnteredSenderDel MessageEnteredSender;

	public event Action<ulong, string> MessageRecieved;

	event MessageEnteredDel IMyUtilities.MessageEntered
	{
		add
		{
			MessageEntered += value;
		}
		remove
		{
			MessageEntered -= value;
		}
	}

	event Action<ulong, string> IMyUtilities.MessageRecieved
	{
		add
		{
			MessageRecieved += value;
		}
		remove
		{
			MessageRecieved -= value;
		}
	}

	static MyCustomFileUtilities()
	{
		Static = new MyCustomFileUtilities();
	}

	string IMyUtilities.GetTypeName(Type type)
	{
		return type.Name;
	}

	void IMyUtilities.ShowNotification(string message, int disappearTimeMs, string font)
	{
		MyHudNotification myHudNotification = new MyHudNotification(MyCommonTexts.CustomText, disappearTimeMs, font);
		myHudNotification.SetTextFormatArguments(message);
		MyHud.Notifications.Add(myHudNotification);
	}

	IMyHudNotification IMyUtilities.CreateNotification(string message, int disappearTimeMs, string font)
	{
		MyHudNotification myHudNotification = new MyHudNotification(MyCommonTexts.CustomText, disappearTimeMs, font);
		myHudNotification.SetTextFormatArguments(message);
		return myHudNotification;
	}

	void IMyUtilities.ShowMessage(string sender, string messageText)
	{
		MyHud.Chat.ShowMessage(sender, messageText);
	}

	void IMyUtilities.SendMessage(string messageText)
	{
		if (MyMultiplayer.Static != null)
		{
			MyMultiplayer.Static.SendChatMessage(messageText, ChatChannel.Global, 0L);
		}
	}

	public void EnterMessage(ulong sender, string messageText, ref bool sendToOthers)
	{
		this.MessageEntered?.Invoke(messageText, ref sendToOthers);
		this.MessageEnteredSender?.Invoke(sender, messageText, ref sendToOthers);
	}

	public void EnterMessageSender(ulong sender, string messageText, ref bool sendToOthers)
	{
		this.MessageEnteredSender?.Invoke(sender, messageText, ref sendToOthers);
	}

	public void RecieveMessage(ulong senderSteamId, string message)
	{
		this.MessageRecieved?.Invoke(senderSteamId, message);
	}

	private string StripDllExtIfNecessary(string name)
	{
		string text = ".dll";
		if (name.EndsWith(text, StringComparison.InvariantCultureIgnoreCase))
		{
			return name.Substring(0, name.Length - text.Length);
		}
		return name;
	}

	TextReader IMyUtilities.ReadFileInGlobalStorage(string file)
	{
		if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
		{
			throw new FileNotFoundException();
		}
		Stream stream = MyFileSystem.OpenRead(Path.Combine(MyFileSystem.UserDataPath, "Storage", file));
		if (stream != null)
		{
			return new StreamReader(stream);
		}
		throw new FileNotFoundException();
	}

	TextReader IMyUtilities.ReadFileInLocalStorage(string file, Type callingType)
	{
		if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
		{
			throw new FileNotFoundException();
		}
		Stream stream = MyFileSystem.OpenRead(Path.Combine(MyFileSystem.UserDataPath, "Storage", StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file));
		if (stream != null)
		{
			return new StreamReader(stream);
		}
		throw new FileNotFoundException();
	}

	TextReader IMyUtilities.ReadFileInWorldStorage(string file, Type callingType)
	{
		if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
		{
			throw new FileNotFoundException();
		}
		Stream stream = MyFileSystem.OpenRead(Path.Combine(MySession.Static.CurrentPath, "Storage", StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file));
		if (stream != null)
		{
			return new StreamReader(stream);
		}
		throw new FileNotFoundException();
	}

	TextWriter IMyUtilities.WriteFileInGlobalStorage(string file)
	{
		if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
		{
			throw new FileNotFoundException();
		}
		Stream stream = MyFileSystem.OpenWrite(Path.Combine(MyFileSystem.UserDataPath, "Storage", file));
		if (stream != null)
		{
			return new StreamWriter(stream);
		}
		throw new FileNotFoundException();
	}

	TextWriter IMyUtilities.WriteFileInLocalStorage(string file, Type callingType)
	{
		if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
		{
			throw new FileNotFoundException();
		}
		Stream stream = MyFileSystem.OpenWrite(Path.Combine(MyFileSystem.UserDataPath, "Storage", StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file));
		if (stream != null)
		{
			return new StreamWriter(stream);
		}
		throw new FileNotFoundException();
	}

	TextWriter IMyUtilities.WriteFileInWorldStorage(string file, Type callingType)
	{
		if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
		{
			throw new FileNotFoundException();
		}
		Stream stream = MyFileSystem.OpenWrite(Path.Combine(MySession.Static.CurrentPath, "Storage", StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file));
		if (stream != null)
		{
			return new StreamWriter(stream);
		}
		throw new FileNotFoundException();
	}

	string IMyUtilities.SerializeToXML<T>(T objToSerialize)
	{
		XmlSerializer xmlSerializer = new XmlSerializer(objToSerialize.GetType());
		StringWriter stringWriter = new StringWriter();
		xmlSerializer.Serialize(stringWriter, objToSerialize);
		return stringWriter.ToString();
	}

	T IMyUtilities.SerializeFromXML<T>(string xml)
	{
		if (string.IsNullOrEmpty(xml))
		{
			return default(T);
		}
		XmlSerializer xmlSerializer = new XmlSerializer(typeof(T));
		using (StringReader input = new StringReader(xml))
		{
			using (XmlReader xmlReader = XmlReader.Create(input))
			{
				return (T)xmlSerializer.Deserialize(xmlReader);
			}
		}
	}

	byte[] IMyUtilities.SerializeToBinary<T>(T obj)
	{
		using (MemoryStream memoryStream = new MemoryStream())
		{
			Serializer.Serialize(memoryStream, obj);
			return memoryStream.ToArray();
		}
	}

	T IMyUtilities.SerializeFromBinary<T>(byte[] data)
	{
		using (MemoryStream source = new MemoryStream(data))
		{
			return Serializer.Deserialize<T>(source);
		}
	}

	void IMyUtilities.InvokeOnGameThread(Action action, string invokerName)
	{
		if (MySandboxGame.Static != null)
		{
			MySandboxGame.Static.Invoke(action, invokerName);
		}
	}

	bool IMyUtilities.FileExistsInGlobalStorage(string file)
	{
		if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
		{
			return false;
		}
		return File.Exists(Path.Combine(MyFileSystem.UserDataPath, "Storage", file));
	}

	bool IMyUtilities.FileExistsInLocalStorage(string file, Type callingType)
	{
		if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
		{
			return false;
		}
		return File.Exists(Path.Combine(MyFileSystem.UserDataPath, "Storage", StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file));
	}

	bool IMyUtilities.FileExistsInWorldStorage(string file, Type callingType)
	{
		if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
		{
			return false;
		}
		return File.Exists(Path.Combine(MySession.Static.CurrentPath, "Storage", StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file));
	}

	void IMyUtilities.DeleteFileInLocalStorage(string file, Type callingType)
	{
		if (((IMyUtilities)this).FileExistsInLocalStorage(file, callingType))
		{
			File.Delete(Path.Combine(MyFileSystem.UserDataPath, "Storage", StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file));
		}
	}

	void IMyUtilities.DeleteFileInWorldStorage(string file, Type callingType)
	{
		if (((IMyUtilities)this).FileExistsInLocalStorage(file, callingType))
		{
			File.Delete(Path.Combine(MySession.Static.CurrentPath, "Storage", StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file));
		}
	}

	void IMyUtilities.DeleteFileInGlobalStorage(string file)
	{
		if (((IMyUtilities)this).FileExistsInGlobalStorage(file))
		{
			File.Delete(Path.Combine(MyFileSystem.UserDataPath, "Storage", file));
		}
	}

	void IMyUtilities.ShowMissionScreen(string screenTitle, string currentObjectivePrefix, string currentObjective, string screenDescription, Action<ResultEnum> callback = null, string okButtonCaption = null)
	{
		MyScreenManager.AddScreen(new MyGuiScreenMission(screenTitle, currentObjectivePrefix, currentObjective, screenDescription, callback, okButtonCaption));
	}

	IMyHudObjectiveLine IMyUtilities.GetObjectiveLine()
	{
		return MyHud.ObjectiveLine;
	}

	BinaryReader IMyUtilities.ReadBinaryFileInGlobalStorage(string file)
	{
		if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
		{
			throw new FileNotFoundException();
		}
		Stream stream = MyFileSystem.OpenRead(Path.Combine(MyFileSystem.UserDataPath, "Storage", file));
		if (stream != null)
		{
			return new BinaryReader(stream);
		}
		throw new FileNotFoundException();
	}

	BinaryReader IMyUtilities.ReadBinaryFileInLocalStorage(string file, Type callingType)
	{
		if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
		{
			throw new FileNotFoundException();
		}
		Stream stream = MyFileSystem.OpenRead(Path.Combine(MyFileSystem.UserDataPath, "Storage", StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file));
		if (stream != null)
		{
			return new BinaryReader(stream);
		}
		throw new FileNotFoundException();
	}

	BinaryReader IMyUtilities.ReadBinaryFileInWorldStorage(string file, Type callingType)
	{
		if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
		{
			throw new FileNotFoundException();
		}
		Stream stream = MyFileSystem.OpenRead(Path.Combine(MySession.Static.CurrentPath, "Storage", StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file));
		if (stream != null)
		{
			return new BinaryReader(stream);
		}
		throw new FileNotFoundException();
	}

	BinaryWriter IMyUtilities.WriteBinaryFileInGlobalStorage(string file)
	{
		if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
		{
			throw new FileNotFoundException();
		}
		Stream stream = MyFileSystem.OpenWrite(Path.Combine(MyFileSystem.UserDataPath, "Storage", file));
		if (stream != null)
		{
			return new BinaryWriter(stream);
		}
		throw new FileNotFoundException();
	}

	BinaryWriter IMyUtilities.WriteBinaryFileInLocalStorage(string file, Type callingType)
	{
		if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
		{
			throw new FileNotFoundException();
		}
		Stream stream = MyFileSystem.OpenWrite(Path.Combine(MyFileSystem.UserDataPath, "Storage", StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file));
		if (stream != null)
		{
			return new BinaryWriter(stream);
		}
		throw new FileNotFoundException();
	}

	BinaryWriter IMyUtilities.WriteBinaryFileInWorldStorage(string file, Type callingType)
	{
		if (file.IndexOfAny(Path.GetInvalidFileNameChars()) != -1)
		{
			throw new FileNotFoundException();
		}
		Stream stream = MyFileSystem.OpenWrite(Path.Combine(MySession.Static.CurrentPath, "Storage", StripDllExtIfNecessary(callingType.Assembly.ManifestModule.ScopeName), file));
		if (stream != null)
		{
			return new BinaryWriter(stream);
		}
		throw new FileNotFoundException();
	}

	void IMyUtilities.SetVariable<T>(string name, T value)
	{
		Variables.Remove(name);
		Variables.Add(name, value);
	}

	bool IMyUtilities.GetVariable<T>(string name, out T value)
	{
		value = default(T);
		if (Variables.TryGetValue(name, out var value2) && value2 is T)
		{
			value = (T)value2;
			return true;
		}
		return false;
	}

	bool IMyUtilities.RemoveVariable(string name)
	{
		return Variables.Remove(name);
	}

	public void RegisterMessageHandler(long id, Action<object> messageHandler)
	{
		if (m_registeredListeners.TryGetValue(id, out var value))
		{
			value.Add(messageHandler);
			return;
		}
		m_registeredListeners[id] = new List<Action<object>> { messageHandler };
	}

	public void UnregisterMessageHandler(long id, Action<object> messageHandler)
	{
		if (m_registeredListeners.TryGetValue(id, out var value))
		{
			value.Remove(messageHandler);
		}
	}

	public void SendModMessage(long id, object payload)
	{
		if (!m_registeredListeners.TryGetValue(id, out var value))
		{
			return;
		}
		foreach (Action<object> item in value)
		{
			item(payload);
		}
	}
}
