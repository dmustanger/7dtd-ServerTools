using ServerTools.Website;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Text;
using System.Xml;

namespace ServerTools
{
    public class WindowedResponse
    {
        public static bool IsEnabled = false;
        public static string Server_Response_Name = "ServerTools";
        public static string LastFileCreated = "", LastIP = "";
        public static Dictionary<string, List<string>> Responses = new Dictionary<string, List<string>>();

        public static void SetupXUiWindow()
        {
            try
            {
                string _file = Directory.GetCurrentDirectory() + "/Data/Config/XUi/windows.xml";
                if (Utils.FileExists(_file))
                {
                    XmlDocument _xmlDoc = new XmlDocument();
                    try
                    {
                        _xmlDoc.Load(_file);
                    }
                    catch (XmlException e)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", _file, e.Message));
                        return;
                    }
                    bool _found = false, _changed = false;
                    int _count = 0;
                    XmlNode _rootNode = _xmlDoc.DocumentElement;
                    XmlNodeList _nodes = _xmlDoc.DocumentElement.GetElementsByTagName("window");
                    if (_nodes != null && _nodes.Count > 0)
                    {
                        for (int i = 0; i < _nodes.Count; i++)
                        {
                            if (_nodes[i].Attributes != null && _nodes[i].Attributes.Count > 0 && _nodes[i].Attributes[0].Value.Contains("servertoolswindow"))
                            {
                                _found = true;
                                _count += 1;
                                XmlNode _subNode = _nodes[i].ChildNodes[0];
                                if (_subNode != null)
                                {
                                    XmlNode _subChildNode = _subNode.ChildNodes[1];
                                    if (_subChildNode != null)
                                    {
                                        if (_subChildNode.Attributes != null && _subChildNode.Attributes.Count > 0 && !_subChildNode.Attributes[3].Value.Contains(WindowedResponse.Server_Response_Name))
                                        {
                                            _changed = true;
                                            _subChildNode.Attributes[3].Value = WindowedResponse.Server_Response_Name;
                                        }
                                    }
                                }
                                _subNode = _nodes[i].ChildNodes[1];
                                if (_subNode != null)
                                {
                                    XmlNode _subChildNode = _subNode.ChildNodes[2];
                                    if (_subChildNode != null)
                                    {
                                        if (_subChildNode.Attributes != null && _subChildNode.Attributes.Count > 0 && !_subChildNode.Attributes[6].Value.Contains(WebPanel.ExternalIp))
                                        {
                                            _changed = true;
                                            _subChildNode.Attributes[6].Value = "@http://" + WebPanel.ExternalIp + ":" + WebPanel.Port + "/WindowedResponse/windowedResponse" + _count + ".png";
                                        }
                                    }
                                }
                            }
                        }
                    }
                    if (_found)
                    {
                        if (_changed)
                        {
                            using (StreamWriter sw = new StreamWriter(_file, true, Encoding.UTF8))
                            {
                                _xmlDoc.Save(sw);
                                sw.Flush();
                                sw.Close();
                            }
                            WorldStaticData.ReloadAllXmlsSync();
                        }
                    }
                    else
                    {
                        for (int i = 1; i < 101; i++)
                        {
                            XmlElement _window = _rootNode.AddXmlElement("window");
                            _window.SetAttribute("name", "servertoolswindow" + i);
                            _window.SetAttribute("anchor", "CenterCenter");
                            _window.SetAttribute("pos", "-450,300");
                            _window.SetAttribute("width", "900");
                            _window.SetAttribute("height", "600");
                            _window.SetAttribute("cursor_area", "true");
                            XmlElement _panel1 = _window.AddXmlElement("panel");
                            _panel1.SetAttribute("name", "header");
                            _panel1.SetAttribute("pos", "0,0");
                            _panel1.SetAttribute("height", "43");
                            _panel1.SetAttribute("depth", "1");
                            _panel1.SetAttribute("disableautobackground", "true");
                            XmlElement _sprite1 = _panel1.AddXmlElement("sprite");
                            _sprite1.SetAttribute("depth", "1");
                            _sprite1.SetAttribute("name", "backgroundMain");
                            _sprite1.SetAttribute("sprite", "menu_empty3px");
                            _sprite1.SetAttribute("pos", "0,0");
                            _sprite1.SetAttribute("height", "43");
                            _sprite1.SetAttribute("color", "[black]");
                            _sprite1.SetAttribute("type", "sliced");
                            _sprite1.SetAttribute("fillcenter", "true");
                            _sprite1.SetAttribute("globalopacity", "true");
                            XmlElement _label1 = _panel1.AddXmlElement("label");
                            _label1.SetAttribute("name", "stheader");
                            _label1.SetAttribute("style", "header.name");
                            _label1.SetAttribute("pos", "6,-6");
                            _label1.SetAttribute("text_key", WindowedResponse.Server_Response_Name);
                            XmlElement _panel2 = _window.AddXmlElement("panel");
                            _panel2.SetAttribute("name", "content");
                            _panel2.SetAttribute("pos", "0,-46");
                            _panel2.SetAttribute("height", "500");
                            _panel2.SetAttribute("depth", "1");
                            _panel2.SetAttribute("pivot", "center");
                            _panel2.SetAttribute("disableautobackground", "true");
                            XmlElement _sprite2 = _panel2.AddXmlElement("sprite");
                            _sprite2.SetAttribute("depth", "6");
                            _sprite2.SetAttribute("name", "border");
                            _sprite2.SetAttribute("sprite", "menu_empty3px");
                            _sprite2.SetAttribute("pos", "0,0");
                            _sprite2.SetAttribute("color", "[black]");
                            _sprite2.SetAttribute("type", "sliced");
                            _sprite2.SetAttribute("fillcenter", "false");
                            _sprite2.SetAttribute("globalopacity", "true");
                            _sprite2.SetAttribute("globalopacitymod", "0.9");
                            XmlElement _sprite3 = _panel2.AddXmlElement("sprite");
                            _sprite3.SetAttribute("depth", "1");
                            _sprite3.SetAttribute("name", "backgroundMain");
                            _sprite3.SetAttribute("sprite", "menu_empty3px");
                            _sprite3.SetAttribute("pos", "2,-2");
                            _sprite3.SetAttribute("height", "496");
                            _sprite3.SetAttribute("width", "896");
                            _sprite3.SetAttribute("color", "[darkGrey]");
                            _sprite3.SetAttribute("type", "sliced");
                            _sprite3.SetAttribute("fillcenter", "true");
                            _sprite3.SetAttribute("globalopacity", "true");
                            _sprite3.SetAttribute("globalopacitymod", "0.65");
                            XmlElement _texture = _panel2.AddXmlElement("texture");
                            _texture.SetAttribute("name", "response");
                            _texture.SetAttribute("pos", "8,-6");
                            _texture.SetAttribute("width", "880");
                            _texture.SetAttribute("height", "480");
                            _texture.SetAttribute("depth", "2");
                            _texture.SetAttribute("material", "Materials/Transparent Colored");
                            _texture.SetAttribute("texture", "@http://" + WebPanel.ExternalIp + ":" + WebPanel.Port + "/WindowedResponse/windowedResponse" + i + ".png");
                            _xmlDoc.DocumentElement.AppendChild(_window);
                        }
                        using (StreamWriter sw = new StreamWriter(_file, true, Encoding.UTF8))
                        {
                            _xmlDoc.Save(sw);
                            sw.Flush();
                            sw.Close();
                        }
                        WorldStaticData.ReloadAllXmlsSync();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WindowedResponse.SetupXUiWindow: {0}", e.Message));
            }
        }

        public static void SetupXUiXui()
        {
            try
            {
                string _file = Directory.GetCurrentDirectory() + "/Data/Config/XUi/xui.xml";
                if (Utils.FileExists(_file))
                {
                    XmlDocument _xmlDoc = new XmlDocument();
                    try
                    {
                        _xmlDoc.Load(_file);
                    }
                    catch (XmlException e)
                    {
                        Log.Out(string.Format("[SERVERTOOLS] Failed loading {0}: {1}", _file, e.Message));
                        return;
                    }
                    XmlNode _rootNode = _xmlDoc.DocumentElement;
                    XmlNodeList _nodes = _xmlDoc.DocumentElement.GetElementsByTagName("ruleset");
                    if (_nodes != null && _nodes.Count > 0)
                    {
                        XmlNode _subNode = _nodes[0];
                        _nodes = _subNode.ChildNodes;
                        if (_nodes != null && _nodes.Count > 0)
                        {
                            for (int i = 0; i < _nodes.Count; i++)
                            {
                                if (_nodes[i].Attributes != null && _nodes[i].Attributes.Count > 0)
                                {
                                    if (_nodes[i].Attributes[0].Value.Contains("servertoolswindow"))
                                    {
                                        return;
                                    }
                                }
                            }
                        }
                        for (int i = 1; i < 101; i++)
                        {
                            XmlElement _window_group = _xmlDoc.CreateElement("window_group");
                            _window_group.SetAttribute("name", "servertoolswindow" + i);
                            XmlElement _window = _window_group.AddXmlElement("window");
                            _window.SetAttribute("name", "servertoolswindow" + i);
                            _subNode.AppendChild(_window_group);
                        }
                        using (StreamWriter sw = new StreamWriter(_file, true, Encoding.UTF8))
                        {
                            _xmlDoc.Save(sw);
                            sw.Flush();
                            sw.Close();
                        }
                        WorldStaticData.ReloadAllXmlsSync();
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WindowedResponse.SetupXUiXui: {0}", e.Message));
            }
        }

        public static bool HasResponse(ClientInfo _cInfo, string _response)
        {
            try
            {
                if (Responses.ContainsKey(_cInfo.playerId))
                {
                    Responses.TryGetValue(_cInfo.playerId, out List<string> _oldResponses);
                    if (_oldResponses.Contains(_response))
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WindowedResponse.HasResponse: {0}", e.Message));
            }
            return false;
        }

        public static bool IsFull(ClientInfo _cInfo)
        {
            try
            {
                if (Responses.ContainsKey(_cInfo.playerId))
                {
                    Responses.TryGetValue(_cInfo.playerId, out List<string> _oldResponses);
                    if (_oldResponses.Count == 100)
                    {
                        return true;
                    }
                }
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WindowedResponse.IsFull: {0}", e.Message));
            }
            return false;
        }

        public static void Exec(ClientInfo _cInfo, string _response)
        {
            try
            {
                List<string> _responses = new List<string>();
                if (Responses.ContainsKey(_cInfo.playerId))
                {
                    Responses.TryGetValue(_cInfo.playerId, out _responses);
                    if (_responses.Contains(_response))
                    {
                        _cInfo.SendPackage(new NetPackageConsoleCmdClient().Setup(string.Format("xui open servertoolswindow{0}", _responses.IndexOf("_response") + 1), true));
                        return;
                    }
                }
                string _filePath = string.Format(API.GamePath + "/Mods/ServerTools/WebPanel/WindowedResponse/windowedResponse{0}.png", _responses.Count + 1);
                if (File.Exists(_filePath))
                {
                    File.Delete(_filePath);
                }
                Image img = new Bitmap(480, 880);
                Graphics drawing = Graphics.FromImage(img);
                Font _font = new Font("Arial", 18);
                SizeF _textSize = drawing.MeasureString(_response, _font, 480);
                StringFormat sf = new StringFormat();
                sf.Trimming = StringTrimming.Word;
                img.Dispose();
                drawing.Dispose();
                img = new Bitmap((int)_textSize.Width, (int)_textSize.Height);
                drawing = Graphics.FromImage(img);
                drawing.CompositingQuality = CompositingQuality.HighQuality;
                drawing.InterpolationMode = InterpolationMode.HighQualityBilinear;
                drawing.PixelOffsetMode = PixelOffsetMode.HighQuality;
                drawing.SmoothingMode = SmoothingMode.HighQuality;
                drawing.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                drawing.Clear(Color.Transparent);
                Brush textBrush = new SolidBrush(Color.White);
                drawing.DrawString(_response, _font, textBrush, new RectangleF(0, 0, _textSize.Width, _textSize.Height), sf);
                drawing.Save();
                textBrush.Dispose();
                drawing.Dispose();
                img.Save(_filePath, ImageFormat.Png);
                img.Dispose();
                LastFileCreated = _filePath.Remove(0, _filePath.IndexOf("windowedResponse"));
                LastIP = _cInfo.ip;
                _responses.Add(_response);
                Responses[_cInfo.playerId] = _responses;
                _cInfo.SendPackage(new NetPackageConsoleCmdClient().Setup(string.Format("xui open servertoolswindow{0}", _responses.Count), true));
            }
            catch (Exception e)
            {
                Log.Out(string.Format("[SERVERTOOLS] Error in WindowedResponse.Exec: {0}", e.Message));
            }
        }
    }
}
