using System.IO;

namespace ServerTools
{
    class HowToSetup
    {
        private const string file = "HowToSetup.xml";
        private static string filePath = string.Format("{0}/{1}", API.ConfigPath, file);
        private const double version = 6.4;

        public static void HowToSeup()
        {
            using (StreamWriter sw = new StreamWriter(filePath))
            {
                sw.WriteLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
                sw.WriteLine("ServerTools - How to setup ServerTools");
                sw.WriteLine(string.Format("Version = \"{0}\"/>", version));
                sw.WriteLine();
                sw.WriteLine();
                sw.WriteLine("ServerTools was coded and tested under a Windows operating system. It should operate on other major O.S. but if you find bugs, report them to");
                sw.WriteLine("https://github.com/dmustanger/7dtd-ServerTools/issues");
                sw.WriteLine();
                sw.WriteLine();
                sw.WriteLine("This version is compatible with 7 Days to Die version Alpha 16.");
                sw.WriteLine();
                sw.WriteLine();
                sw.WriteLine("Tool Name=AdminChatCommands Enable=False");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.WriteLine("");
                sw.Flush();
                sw.Close();
            }
        }
    }
}
