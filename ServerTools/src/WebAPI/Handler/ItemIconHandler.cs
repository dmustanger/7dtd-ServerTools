using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using UnityEngine;

namespace ServerTools.WebAPI
{
	// Token: 0x0200001B RID: 27
	public class ItemIconHandler : PathHandler
	{
		// Token: 0x06000091 RID: 145 RVA: 0x00002403 File Offset: 0x00000603
		public ItemIconHandler(string _staticPart, bool _logMissingFiles, string _moduleName = null) : base(_moduleName, 0)
		{
			this.staticPart = _staticPart;
			this.logMissingFiles = _logMissingFiles;
			ItemIconHandler.Instance = this;
		}

		// Token: 0x1700000A RID: 10
		// (get) Token: 0x06000092 RID: 146 RVA: 0x0000242C File Offset: 0x0000062C
		// (set) Token: 0x06000093 RID: 147 RVA: 0x00002433 File Offset: 0x00000633
		public static ItemIconHandler Instance { get; private set; } = null;

		// Token: 0x06000094 RID: 148 RVA: 0x00008B5C File Offset: 0x00006D5C
		public override void HandleRequest(HttpListenerRequest _req, HttpListenerResponse _resp, WebConnect _user, int _permissionLevel)
		{
			if (!this.loaded)
			{
				_resp.StatusCode = 500;
				Log.Out("Web:IconHandler: Icons not loaded");
				return;
			}
			string text = _req.Url.AbsolutePath.Remove(0, this.staticPart.Length);
			text = text.Remove(text.LastIndexOf('.'));
			if (this.icons.ContainsKey(text) && _req.Url.AbsolutePath.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
			{
				_resp.ContentType = MimeType.GetMimeType(".png");
				byte[] array = this.icons[text];
				_resp.ContentLength64 = (long)array.Length;
				_resp.OutputStream.Write(array, 0, array.Length);
			}
			else
			{
				_resp.StatusCode = 404;
				if (this.logMissingFiles)
				{
					Log.Out("Web:IconHandler:FileNotFound: \"" + _req.Url.AbsolutePath + "\" ");
				}
			}
		}

		// Token: 0x06000095 RID: 149 RVA: 0x00008C44 File Offset: 0x00006E44
		public bool LoadIcons()
		{
			object obj = this.icons;
			bool result;
			lock (obj)
			{
				if (this.loaded)
				{
					result = true;
				}
				else
				{
					MicroStopwatch microStopwatch = new MicroStopwatch();
					Dictionary<string, List<Color>> dictionary = new Dictionary<string, List<Color>>();
					foreach (ItemClass itemClass in ItemClass.list)
					{
						if (itemClass != null)
						{
							Color iconTint = itemClass.GetIconTint(null);
							if (iconTint != Color.white)
							{
								string iconName = itemClass.GetIconName();
								if (!dictionary.ContainsKey(iconName))
								{
									dictionary.Add(iconName, new List<Color>());
								}
								List<Color> list2 = dictionary[iconName];
								list2.Add(iconTint);
							}
						}
					}
					try
					{
						this.LoadIconsFromFolder(Utils.GetGameDir("Data/ItemIcons"), dictionary);
					}
					catch (Exception ex)
					{
						Log.Error("Failed loading icons from base game");
						Log.Exception(ex);
					}
					foreach (Mod mod in ModManager.GetLoadedMods())
					{
						try
						{
							string path = mod.Path + "/ItemIcons";
							this.LoadIconsFromFolder(path, dictionary);
						}
						catch (Exception ex2)
						{
							Log.Error("Failed loading icons from mod " + mod.ModInfo.Name.Value);
							Log.Exception(ex2);
						}
					}
					this.loaded = true;
					Log.Out("Web:IconHandler: Icons loaded - {0} ms", new object[]
					{
						microStopwatch.ElapsedMilliseconds
					});
					result = true;
				}
			}
			return result;
		}

		// Token: 0x06000096 RID: 150 RVA: 0x00008E30 File Offset: 0x00007030
		private void LoadIconsFromFolder(string _path, Dictionary<string, List<Color>> _tintedIcons)
		{
			if (Directory.Exists(_path))
			{
				foreach (string text in Directory.GetFiles(_path))
				{
					try
					{
						if (text.EndsWith(".png", StringComparison.OrdinalIgnoreCase))
						{
							string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(text);
							Texture2D texture2D = new Texture2D(1, 1, UnityEngine.Experimental.Rendering.DefaultFormat.LDR, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
							if (ImageConversion.LoadImage(texture2D, File.ReadAllBytes(text)))
							{
								this.AddIcon(fileNameWithoutExtension, texture2D, _tintedIcons);
								UnityEngine.Object.Destroy(texture2D);
							}
						}
					}
					catch (Exception ex)
					{
						Log.Exception(ex);
					}
				}
			}
		}

		// Token: 0x06000097 RID: 151 RVA: 0x00008EBC File Offset: 0x000070BC
		private void AddIcon(string _name, Texture2D _tex, Dictionary<string, List<Color>> _tintedIcons)
		{
			this.icons[_name + "__FFFFFF"] = ImageConversion.EncodeToPNG(_tex);
			if (_tintedIcons.ContainsKey(_name))
			{
				foreach (Color color in _tintedIcons[_name])
				{
					string key = _name + "__" + ColorToHex(color);
					if (!this.icons.ContainsKey(key))
					{
						Texture2D texture2D = new Texture2D(_tex.width, _tex.height, UnityEngine.Experimental.Rendering.DefaultFormat.LDR, UnityEngine.Experimental.Rendering.TextureCreationFlags.None);
						for (int i = 0; i < _tex.width; i++)
						{
							for (int j = 0; j < _tex.height; j++)
							{
								texture2D.SetPixel(i, j, _tex.GetPixel(i, j) * color);
							}
						}
						this.icons[key] = ImageConversion.EncodeToPNG(texture2D);
						UnityEngine.Object.Destroy(texture2D);
					}
				}
			}
		}

		public static string ColorToHex(Color _color)
		{
			return string.Format("{0:X02}{1:X02}{2:X02}", (int)(_color.r * 255f), (int)(_color.g * 255f), (int)(_color.b * 255f));
		}

		// Token: 0x04000048 RID: 72
		private readonly Dictionary<string, byte[]> icons = new Dictionary<string, byte[]>();

		// Token: 0x04000049 RID: 73
		private readonly bool logMissingFiles;

		// Token: 0x0400004A RID: 74
		private readonly string staticPart;

		// Token: 0x0400004B RID: 75
		private bool loaded;
	}
}
