using System;
using System.IO;
using System.Text;

namespace ServerTools
{
    class OversizedTraps
    {
        public static bool IsEnabled = false, IsRunning = false;

        public static void CreateXPath()
        {
            if (!string.IsNullOrEmpty(GeneralFunction.XPathDir))
            {
                if (!File.Exists(GeneralFunction.XPathDir + "blocks.xml"))
                {
                    using (StreamWriter sw = new StreamWriter(GeneralFunction.XPathDir + "blocks.xml", false, Encoding.UTF8))
                    {
                        sw.WriteLine("<configs>");
                        sw.WriteLine();
                        sw.WriteLine("<append xpath=\"/blocks/block[@name='dartTrap']\">");
                        sw.WriteLine("  <property name=\"MultiBlockDim\" value=\"3,1,3\"/>");
                        sw.WriteLine("</append>");
                        sw.WriteLine();
                        sw.WriteLine("<set xpath=\"/blocks/block[@name='flamethrowerTrap']/property[@name='MultiBlockDim']/@value\">3,2,4</set>");
                        sw.WriteLine();
                        sw.WriteLine("<set xpath=\"/blocks/block[@name='bladeTrap']/property[@name='MultiBlockDim']/@value\">5,1,5</set>");
                        sw.WriteLine();
                        sw.WriteLine("<append xpath=\"/blocks/block[@name='autoTurret']\">");
                        sw.WriteLine("  <property name=\"MultiBlockDim\" value=\"3,1,3\"/>");
                        sw.WriteLine("</append>");
                        sw.WriteLine();
                        sw.WriteLine("</configs>");
                        sw.Flush();
                        sw.Close();
                    }
                    Log.Out(string.Format("[SERVERTOOLS] Oversized_Traps has created a file named blocks.xml upon activation. This will only take affect after the server restarts"));
                }
            }
            IsRunning = true;
        }

        public static void RemoveXPath()
        {
            if (!string.IsNullOrEmpty(GeneralFunction.XPathDir))
            {
                if (File.Exists(GeneralFunction.XPathDir + "blocks.xml"))
                {
                    File.Delete(GeneralFunction.XPathDir + "blocks.xml");
                    Log.Out(string.Format("[SERVERTOOLS] Oversized_Traps has deleted a file named blocks.xml upon deactivation. This will only take affect after the server restarts"));
                }
            }
            IsRunning = false;
        }
    }
}
