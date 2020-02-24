using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;

namespace ServerTools.WebAPI
{
	// Token: 0x02000017 RID: 23
	public class WebPermissions
	{
		// Token: 0x06000075 RID: 117 RVA: 0x00007EE4 File Offset: 0x000060E4
		public WebPermissions()
		{
			this.allModulesList = new List<WebPermissions.WebModulePermission>();
			this.allModulesListRO = new ReadOnlyCollection<WebPermissions.WebModulePermission>(this.allModulesList);
			Directory.CreateDirectory(this.GetFilePath());
			this.InitFileWatcher();
			this.Load();
		}

		// Token: 0x17000009 RID: 9
		// (get) Token: 0x06000076 RID: 118 RVA: 0x00007F60 File Offset: 0x00006160
		public static WebPermissions Instance
		{
			get
			{
				object typeFromHandle = typeof(WebPermissions);
				WebPermissions result;
				lock (typeFromHandle)
				{
					if (WebPermissions.instance == null)
					{
						WebPermissions.instance = new WebPermissions();
					}
					result = WebPermissions.instance;
				}
				return result;
			}
		}

		// Token: 0x06000077 RID: 119 RVA: 0x00007FB8 File Offset: 0x000061B8
		public bool ModuleAllowedWithLevel(string _module, int _level)
		{
			return this.GetModulePermission(_module).permissionLevel >= _level;
		}

		// Token: 0x06000078 RID: 120 RVA: 0x00002309 File Offset: 0x00000509
		public WebPermissions.AdminToken GetWebAdmin(string _name, string _token)
		{
			if (this.IsAdmin(_name) && this.admintokens[_name].token == _token)
			{
				return this.admintokens[_name];
			}
			return null;
		}

		// Token: 0x06000079 RID: 121 RVA: 0x00007FDC File Offset: 0x000061DC
		public WebPermissions.WebModulePermission GetModulePermission(string _module)
		{
			if (this.modules.TryGetValue(_module, out WebPermissions.WebModulePermission result))
			{
				return result;
			}
			if (this.knownModules.TryGetValue(_module, out result))
			{
				return result;
			}
			return this.defaultModulePermission;
		}

		// Token: 0x0600007A RID: 122 RVA: 0x00008014 File Offset: 0x00006214
		public void AddAdmin(string _name, string _token, int _permissionLevel, bool _save = true)
		{
			WebPermissions.AdminToken value = new WebPermissions.AdminToken(_name, _token, _permissionLevel);
			lock (this)
			{
				this.admintokens[_name] = value;
				if (_save)
				{
					this.Save();
				}
			}
		}

		// Token: 0x0600007B RID: 123 RVA: 0x0000806C File Offset: 0x0000626C
		public void RemoveAdmin(string _name, bool _save = true)
		{
			lock (this)
			{
				this.admintokens.Remove(_name);
				if (_save)
				{
					this.Save();
				}
			}
		}

		// Token: 0x0600007C RID: 124 RVA: 0x0000233B File Offset: 0x0000053B
		public bool IsAdmin(string _name)
		{
			return this.admintokens.ContainsKey(_name);
		}

		// Token: 0x0600007D RID: 125 RVA: 0x000080B8 File Offset: 0x000062B8
		public WebPermissions.AdminToken[] GetAdmins()
		{
			WebPermissions.AdminToken[] array = new WebPermissions.AdminToken[this.admintokens.Count];
			this.admintokens.CopyValuesTo(array);
			return array;
		}

		// Token: 0x0600007E RID: 126 RVA: 0x000080E4 File Offset: 0x000062E4
		public void AddModulePermission(string _module, int _permissionLevel, bool _save = true)
		{
			WebPermissions.WebModulePermission value = new WebPermissions.WebModulePermission(_module, _permissionLevel);
			lock (this)
			{
				this.allModulesList.Clear();
				this.modules[_module] = value;
				if (_save)
				{
					this.Save();
				}
			}
		}

		// Token: 0x0600007F RID: 127 RVA: 0x00008144 File Offset: 0x00006344
		public void AddKnownModule(string _module, int _defaultPermission)
		{
			if (string.IsNullOrEmpty(_module))
			{
				return;
			}
			WebPermissions.WebModulePermission value = new WebPermissions.WebModulePermission(_module, _defaultPermission);
			lock (this)
			{
				this.allModulesList.Clear();
				this.knownModules[_module] = value;
			}
		}

		// Token: 0x06000080 RID: 128 RVA: 0x000081A4 File Offset: 0x000063A4
		public bool IsKnownModule(string _module)
		{
			if (string.IsNullOrEmpty(_module))
			{
				return false;
			}
			bool result;
			lock (this)
			{
				result = this.knownModules.ContainsKey(_module);
			}
			return result;
		}

		// Token: 0x06000081 RID: 129 RVA: 0x000081F4 File Offset: 0x000063F4
		public void RemoveModulePermission(string _module, bool _save = true)
		{
			lock (this)
			{
				this.allModulesList.Clear();
				this.modules.Remove(_module);
				if (_save)
				{
					this.Save();
				}
			}
		}

		// Token: 0x06000082 RID: 130 RVA: 0x0000824C File Offset: 0x0000644C
		public IList<WebPermissions.WebModulePermission> GetModules()
		{
			if (this.allModulesList.Count == 0)
			{
				foreach (KeyValuePair<string, WebPermissions.WebModulePermission> keyValuePair in this.knownModules)
				{
					if (this.modules.ContainsKey(keyValuePair.Key))
					{
						this.allModulesList.Add(this.modules[keyValuePair.Key]);
					}
					else
					{
						this.allModulesList.Add(keyValuePair.Value);
					}
				}
			}
			return this.allModulesListRO;
		}

		// Token: 0x06000083 RID: 131 RVA: 0x000082F4 File Offset: 0x000064F4
		private void InitFileWatcher()
		{
			this.fileWatcher = new FileSystemWatcher(this.GetFilePath(), this.GetFileName());
			this.fileWatcher.Changed += this.OnFileChanged;
			this.fileWatcher.Created += this.OnFileChanged;
			this.fileWatcher.Deleted += this.OnFileChanged;
			this.fileWatcher.EnableRaisingEvents = true;
		}

		// Token: 0x06000084 RID: 132 RVA: 0x00002349 File Offset: 0x00000549
		private void OnFileChanged(object sender, FileSystemEventArgs e)
		{
			Log.Out("Reloading webpermissions.xml");
			this.Load();
		}

		// Token: 0x06000085 RID: 133 RVA: 0x0000235B File Offset: 0x0000055B
		private string GetFilePath()
		{
			return GamePrefs.GetString(EnumUtils.Parse<EnumGamePrefs>("SaveGameFolder", false));
		}

		// Token: 0x06000086 RID: 134 RVA: 0x0000236D File Offset: 0x0000056D
		private string GetFileName()
		{
			return "webpermissions.xml";
		}

		// Token: 0x06000087 RID: 135 RVA: 0x00002374 File Offset: 0x00000574
		private string GetFullPath()
		{
			return this.GetFilePath() + "/" + this.GetFileName();
		}

		// Token: 0x06000088 RID: 136 RVA: 0x0000836C File Offset: 0x0000656C
		public void Load()
		{
			this.admintokens.Clear();
			this.modules.Clear();
			if (!Utils.FileExists(this.GetFullPath()))
			{
				Log.Out(string.Format("Permissions file '{0}' not found, creating.", this.GetFileName()));
				this.Save();
				return;
			}
			Log.Out(string.Format("Loading permissions file at '{0}'", this.GetFullPath()));
			XmlDocument xmlDocument = new XmlDocument();
			try
			{
				xmlDocument.Load(this.GetFullPath());
			}
			catch (XmlException ex)
			{
				Log.Error("Failed loading permissions file: " + ex.Message);
				return;
			}
			XmlNode documentElement = xmlDocument.DocumentElement;
			if (documentElement == null)
			{
				Log.Error("Failed loading permissions file: No DocumentElement found");
				return;
			}
			IEnumerator enumerator = documentElement.ChildNodes.GetEnumerator();
			try
			{
				while (enumerator.MoveNext())
				{
					object obj = enumerator.Current;
					XmlNode xmlNode = (XmlNode)obj;
					if (xmlNode.Name == "admintokens")
					{
						IEnumerator enumerator2 = xmlNode.ChildNodes.GetEnumerator();
						try
						{
							while (enumerator2.MoveNext())
							{
								object obj2 = enumerator2.Current;
								XmlNode xmlNode2 = (XmlNode)obj2;
								if (xmlNode2.NodeType != XmlNodeType.Comment)
								{
									if (xmlNode2.NodeType != XmlNodeType.Element)
									{
										Log.Warning("Unexpected XML node found in 'admintokens' section: " + xmlNode2.OuterXml);
									}
									else
									{
										XmlElement xmlElement = (XmlElement)xmlNode2;
										if (!xmlElement.HasAttribute("name"))
										{
											Log.Warning("Ignoring admintoken-entry because of missing 'name' attribute: " + xmlNode2.OuterXml);
										}
										else if (!xmlElement.HasAttribute("token"))
										{
											Log.Warning("Ignoring admintoken-entry because of missing 'token' attribute: " + xmlNode2.OuterXml);
										}
										else if (!xmlElement.HasAttribute("permission_level"))
										{
											Log.Warning("Ignoring admintoken-entry because of missing 'permission_level' attribute: " + xmlNode2.OuterXml);
										}
										else
										{
											string attribute = xmlElement.GetAttribute("name");
											string attribute2 = xmlElement.GetAttribute("token");
											if (!int.TryParse(xmlElement.GetAttribute("permission_level"), out int permissionLevel))
											{
												Log.Warning("Ignoring admintoken-entry because of invalid (non-numeric) value for 'permission_level' attribute: " + xmlNode2.OuterXml);
											}
											else
											{
												this.AddAdmin(attribute, attribute2, permissionLevel, false);
											}
										}
									}
								}
							}
						}
						finally
						{
							IDisposable disposable;
							if ((disposable = (enumerator2 as IDisposable)) != null)
							{
								disposable.Dispose();
							}
						}
					}
					if (xmlNode.Name == "permissions")
					{
						IEnumerator enumerator3 = xmlNode.ChildNodes.GetEnumerator();
						try
						{
							while (enumerator3.MoveNext())
							{
								object obj3 = enumerator3.Current;
								XmlNode xmlNode3 = (XmlNode)obj3;
								if (xmlNode3.NodeType != XmlNodeType.Comment)
								{
									if (xmlNode3.NodeType != XmlNodeType.Element)
									{
										Log.Warning("Unexpected XML node found in 'permissions' section: " + xmlNode3.OuterXml);
									}
									else
									{
										XmlElement xmlElement2 = (XmlElement)xmlNode3;
										if (!xmlElement2.HasAttribute("module"))
										{
											Log.Warning("Ignoring permission-entry because of missing 'module' attribute: " + xmlNode3.OuterXml);
										}
										else if (!xmlElement2.HasAttribute("permission_level"))
										{
											Log.Warning("Ignoring permission-entry because of missing 'permission_level' attribute: " + xmlNode3.OuterXml);
										}
										else if (!int.TryParse(xmlElement2.GetAttribute("permission_level"), out int permissionLevel2))
										{
											Log.Warning("Ignoring permission-entry because of invalid (non-numeric) value for 'permission_level' attribute: " + xmlNode3.OuterXml);
										}
										else
										{
											this.AddModulePermission(xmlElement2.GetAttribute("module"), permissionLevel2, false);
										}
									}
								}
							}
						}
						finally
						{
							IDisposable disposable2;
							if ((disposable2 = (enumerator3 as IDisposable)) != null)
							{
								disposable2.Dispose();
							}
						}
					}
				}
			}
			finally
			{
				IDisposable disposable3;
				if ((disposable3 = (enumerator as IDisposable)) != null)
				{
					disposable3.Dispose();
				}
			}
			Log.Out("Loading permissions file done.");
		}

		// Token: 0x06000089 RID: 137 RVA: 0x00008744 File Offset: 0x00006944
		public void Save()
		{
			this.fileWatcher.EnableRaisingEvents = false;
			using (StreamWriter streamWriter = new StreamWriter(this.GetFullPath()))
			{
				streamWriter.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
				streamWriter.WriteLine("<webpermissions>");
				streamWriter.WriteLine();
				streamWriter.WriteLine("\t<admintokens>");
				streamWriter.WriteLine("\t\t<!-- <token name=\"adminuser1\" token=\"supersecrettoken\" permission_level=\"0\" /> -->");
				foreach (KeyValuePair<string, WebPermissions.AdminToken> keyValuePair in this.admintokens)
				{
					streamWriter.WriteLine("\t\t<token name=\"{0}\" token=\"{1}\" permission_level=\"{2}\" />", keyValuePair.Value.name, keyValuePair.Value.token, keyValuePair.Value.permissionLevel);
				}
				streamWriter.WriteLine("\t</admintokens>");
				streamWriter.WriteLine();
				streamWriter.WriteLine("\t<permissions>");
				foreach (KeyValuePair<string, WebPermissions.WebModulePermission> keyValuePair2 in this.modules)
				{
					streamWriter.WriteLine("\t\t<permission module=\"{0}\" permission_level=\"{1}\" />", keyValuePair2.Value.module, keyValuePair2.Value.permissionLevel);
				}
				streamWriter.WriteLine("\t\t<!-- <permission module=\"web.map\" permission_level=\"1000\" /> -->");
				streamWriter.WriteLine();
				streamWriter.WriteLine("\t\t<!-- <permission module=\"webapi.getlog\" permission_level=\"0\" /> -->");
				streamWriter.WriteLine("\t\t<!-- <permission module=\"webapi.executeconsolecommand\" permission_level=\"0\" /> -->");
				streamWriter.WriteLine();
				streamWriter.WriteLine("\t\t<!-- <permission module=\"webapi.getstats\" permission_level=\"1000\" /> -->");
				streamWriter.WriteLine("\t\t<!-- <permission module=\"webapi.getplayersonline\" permission_level=\"1000\" /> -->");
				streamWriter.WriteLine();
				streamWriter.WriteLine("\t\t<!-- <permission module=\"webapi.getplayerslocation\" permission_level=\"1000\" /> -->");
				streamWriter.WriteLine("\t\t<!-- <permission module=\"webapi.viewallplayers\" permission_level=\"1\" /> -->");
				streamWriter.WriteLine();
				streamWriter.WriteLine("\t\t<!-- <permission module=\"webapi.getlandclaims\" permission_level=\"1000\" /> -->");
				streamWriter.WriteLine("\t\t<!-- <permission module=\"webapi.viewallclaims\" permission_level=\"1\" /> -->");
				streamWriter.WriteLine();
				streamWriter.WriteLine("\t\t<!-- <permission module=\"webapi.getplayerinventory\" permission_level=\"1\" /> -->");
				streamWriter.WriteLine();
				streamWriter.WriteLine("\t\t<!-- <permission module=\"webapi.gethostilelocation\" permission_level=\"1\" /> -->");
				streamWriter.WriteLine("\t\t<!-- <permission module=\"webapi.getanimalslocation\" permission_level=\"1\" /> -->");
				streamWriter.WriteLine("\t</permissions>");
				streamWriter.WriteLine();
				streamWriter.WriteLine("</webpermissions>");
				streamWriter.Flush();
				streamWriter.Close();
			}
			this.fileWatcher.EnableRaisingEvents = true;
		}

		// Token: 0x04000039 RID: 57
		private static WebPermissions instance;

		// Token: 0x0400003A RID: 58
		private readonly WebPermissions.WebModulePermission defaultModulePermission = new WebPermissions.WebModulePermission(string.Empty, 0);

		// Token: 0x0400003B RID: 59
		private readonly Dictionary<string, WebPermissions.WebModulePermission> knownModules = new CaseInsensitiveStringDictionary<WebPermissions.WebModulePermission>();

		// Token: 0x0400003C RID: 60
		private readonly Dictionary<string, WebPermissions.AdminToken> admintokens = new CaseInsensitiveStringDictionary<WebPermissions.AdminToken>();

		// Token: 0x0400003D RID: 61
		private FileSystemWatcher fileWatcher;

		// Token: 0x0400003E RID: 62
		private readonly Dictionary<string, WebPermissions.WebModulePermission> modules = new CaseInsensitiveStringDictionary<WebPermissions.WebModulePermission>();

		// Token: 0x0400003F RID: 63
		private readonly List<WebPermissions.WebModulePermission> allModulesList;

		// Token: 0x04000040 RID: 64
		private readonly ReadOnlyCollection<WebPermissions.WebModulePermission> allModulesListRO;

		// Token: 0x02000018 RID: 24
		public class AdminToken
		{
			// Token: 0x0600008A RID: 138 RVA: 0x0000238C File Offset: 0x0000058C
			public AdminToken(string _name, string _token, int _permissionLevel)
			{
				this.name = _name;
				this.token = _token;
				this.permissionLevel = _permissionLevel;
			}

			// Token: 0x04000041 RID: 65
			public string name;

			// Token: 0x04000042 RID: 66
			public int permissionLevel;

			// Token: 0x04000043 RID: 67
			public string token;
		}

		// Token: 0x02000019 RID: 25
		public struct WebModulePermission
		{
			// Token: 0x0600008B RID: 139 RVA: 0x000023A9 File Offset: 0x000005A9
			public WebModulePermission(string _module, int _permissionLevel)
			{
				this.module = _module;
				this.permissionLevel = _permissionLevel;
			}

			// Token: 0x04000044 RID: 68
			public string module;

			// Token: 0x04000045 RID: 69
			public int permissionLevel;
		}
	}
}