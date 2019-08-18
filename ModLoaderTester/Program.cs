using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InternalModding.Mods;
using InternalModding.Assemblies;

namespace ModLoaderTester
{
    class Program
    {
        static void Main(string[] args)
        {
            if(args.Length == 1)//started the exe with a dir
            {
                testFile(args[0]);
            }
            else
            {
                Console.WriteLine("Enter Mod path to test");
                string fn = Console.ReadLine();
                testFile(fn);
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
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("Path doesnt exist!");
            }
        }
        public static ModContainer LoadModInfoFrom(DirectoryInfo dir)
        {
            FileInfo fileInfo = new FileInfo(Path.Combine(dir.FullName, "Mod.xml"));
            if (!fileInfo.Exists)
            {
                return null;
            }
            ModInfo info = ModInfo.LoadFromFile(fileInfo.FullName, false);
            if (info == null)
            {
                MLog.Warn("Not loading " + dir.Name);
                return null;
            }

            ModContainer modContainer2 = new ModContainer(info);

            //modContainer2.IsEnabled = !ModStatus.IsModDisabled(info);
            //this.LoadResourcesAsync(modContainer2);
            return modContainer2;
        }
    }
}
