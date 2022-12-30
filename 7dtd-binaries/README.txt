When compiling your own version of ServerTools, direct your references to these required libraries.


0Harmony
Assembly-CSharp
ICSharpCode.SharpZipLib
LogLibrary
Microsoft.CSharp
Microsoft.VisualBasic
System
System.IO.Compression.FileSystem
System.Net.Http
System.Numerics
System.Web
System.Xml
System.Xml.Linq
UnityEngine
UnityEngine.CoreModule


Most of the required files are provided in the dedicated server directory called Managed. Example: C:\DedicatedServer\7DaysToDieServer_Data\Managed.
The remainder are provided by .net framework and your operating system. It is recommended to install and utilize .net framework 4.8.

You may load other libraries if you require it for your project. Be careful to pay attention at server startup for errors reported in the output log. Adding addition libraries can trigger errors due to compatibility issues.

If you are unable to locate the required files to compile your own ServerTools, please contact the development team on Discord or Github.