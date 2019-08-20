using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InternalModding.Mods;
using InternalModding.Assemblies;
using System.Diagnostics;

namespace ModLoaderTester
{
    class Program
    {
        internal static string currentCMD = "";
        static void Main(string[] args)
        {
            while (true)
            {
                string fn = "";
                if (currentCMD == "")
                {
                    Console.WriteLine("Enter Command: \n 1) Load dll \n 2) test Proc strings");
                    fn = Console.ReadLine();
                }
                else
                {
                    fn = currentCMD;
                }
                if(fn == "1")
                {
                    Console.WriteLine("Enter Path: ");
                    string stuff = Console.ReadLine();
                    if (stuff == "return") { currentCMD = ""; return; }
                    testFile(stuff);
                    currentCMD = "1";
                }
                if(fn == "2")
                {
                    Console.WriteLine("Enter Proc string: ");
                    string procS = Console.ReadLine();
                    if(procS == "return") { currentCMD = ""; return; }
                    ModInfo mi = new ModInfo();
                    mi.Author = "Testing";
                    mi.Assemblies = new List<ModInfo.AssemblyInfo>();
                    mi.Assemblies.Add(new ModInfo.AssemblyInfo()
                    {
                        FileName = fn.Split('\\').Last(),
                        Path = fn,
                    });
                    mi.DebugEnabled = true;
                    mi.FromWorkshop = false;
                    mi.Name = fn.Split('\\').Last().Replace(".dll", "");
                    mi.MultiplayerCompatible = true;
                    mi.Directory = @"C:\Program Files (x86)\Steam\steamapps\common\Besiege\Besiege_Data\Mods\testtings Project";
                    Guid guid;
                    Guid.TryParse("df4de75b-fa7d-42ca-aa6c-fa512ffb5729", out guid);
                    mi.Id = guid;
                    testProcLoading(procS, mi);
                    currentCMD = "2";
                }
                if (fn == "return")
                {
                    currentCMD = "";
                }
                
            }
        }
        static void testFile(string fn)
        {
            if (File.Exists(fn))
            {
                //var mc = LoadModInfoFrom(new DirectoryInfo(fn));
                //Console.WriteLine(mc.Info);
                //Console.WriteLine(mc.HadLoadOrActivateErrors);
                //Console.WriteLine(mc.IsActive.ToString());
                //Console.WriteLine(mc.CurrentState);
                //foreach (var assembly in mc.Assemblies)
                //{
                //    Console.WriteLine("[AssemblyInfo] " + assembly.Info);
                //    Console.WriteLine("[AssemblyInfo] " + assembly.HasModEntryPoint.ToString());
                //    Console.WriteLine("[AssemblyInfo] " + assembly.Assembly.FullName);
                //}
                //Console.ReadLine();
                
                ModInfo mi = new ModInfo();
                mi.Author = "Testing";
                mi.Assemblies = new List<ModInfo.AssemblyInfo>();
                mi.Assemblies.Add(new ModInfo.AssemblyInfo() {
                    FileName = fn.Split('\\').Last(),
                    Path = fn,
                });
                mi.DebugEnabled = true;
                mi.FromWorkshop = false;
                mi.Name = fn.Split('\\').Last().Replace(".dll", "");
                mi.MultiplayerCompatible = true;
                Guid guid;
                Guid.TryParse("df4de75b-fa7d-42ca-aa6c-fa512ffb5729", out guid);
                mi.Id = guid;

                ModContainer mc = new ModContainer(mi);
                mc.IsEnabled = true;
                

                AssemblyLoader.LoadMod(mc); //C:\Program Files (x86)\Steam\steamapps\common\Besiege\Besiege_Data\Mods\testtings Project\testtings\testingtings.dll
                Console.ForegroundColor = ConsoleColor.Magenta;
                //Console.WriteLine($"[INFO] ModIO Path: {GetFilePath(mi.Directory,  )}");

                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Path doesnt exist!");
            }
        }
        private static void testProcLoading(string path, ModInfo mi)
        {
            try
            {
                string text = GetFilePath(mi, path);
                text = text.TrimEnd(new char[]
                {
                '\\',
                '/'
                });
                Console.WriteLine($"[TESTING PROC] Process.Start({text});");
                Process.Start(text);
            }
            catch(Exception ex)
            {
                Console.WriteLine($"[EXEPTION] {ex.Message}");
            }
            
        }
        public static string GetFilePath(ModInfo mod, string path, bool relativeToResources = false)
        {
            return GetFilePath(Path.Combine(mod.Directory, (!relativeToResources) ? string.Empty : "Resources"), path);
        }
        public static string GetFilePath(string baseDir, string path)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(baseDir);
            directoryInfo = directoryInfo.Parent.CreateSubdirectory(directoryInfo.Name);
            baseDir = baseDir.Replace("\\", "/");
            path = path.Replace("\\", "/");
            string text = (!(path == "/")) ? Path.Combine(baseDir, path) : (baseDir + "/");
            DirectoryInfo directoryInfo2;
            if (!text.EndsWith("/") && !text.EndsWith("\\"))
            {
                FileInfo fileInfo = new FileInfo(text);
                directoryInfo2 = fileInfo.Directory;
            }
            else
            {
                DirectoryInfo directoryInfo3 = new DirectoryInfo(text);
                directoryInfo2 = directoryInfo3;
            }
            DirectoryInfo directoryInfo4;
            for (directoryInfo4 = directoryInfo2; directoryInfo4 != null; directoryInfo4 = directoryInfo4.Parent)
            {
                if (directoryInfo4.FullName.Equals(directoryInfo.FullName, StringComparison.OrdinalIgnoreCase) || directoryInfo4.FullName.Equals(directoryInfo.FullName + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }
            }
            if (directoryInfo4 == null)
            {
                throw new Exception("Path is not in mod directory! (" + path + ")");
            }
            return text;
        }
    }
}
