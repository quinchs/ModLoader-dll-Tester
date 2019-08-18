using System;
using System.IO;
using InternalModding.Mods;

namespace InternalModding.Misc
{
	// Token: 0x02000277 RID: 631
	public static class ModPaths
	{
		// Token: 0x06000F84 RID: 3972 RVA: 0x00066C40 File Offset: 0x00064E40
		public static string GetFilePath(ModInfo mod, string path, bool relativeToResources = false)
		{
			return ModPaths.GetFilePath(Path.Combine(mod.Directory, (!relativeToResources) ? string.Empty : "Resources"), path);
		}

		// Token: 0x06000F85 RID: 3973 RVA: 0x00066C74 File Offset: 0x00064E74
		public static string GetFilePathData(ModInfo mod, string path)
		{
			return ModPaths.GetFilePath(Path.Combine(ModPaths.GetDataDirectory(), string.Concat(new object[]
			{
				mod.Name.Replace(" ", string.Empty),
				"_",
				mod.Id,
				"/"
			})), path);
		}

		// Token: 0x06000F86 RID: 3974 RVA: 0x00066CD4 File Offset: 0x00064ED4
		public static string GetDataDirectoryMod(ModInfo mod)
		{
			return Path.Combine(ModPaths.GetDataDirectory(), string.Concat(new object[]
			{
				mod.Name.Replace(" ", string.Empty),
				"_",
				mod.Id,
				"/"
			}));
		}

		// Token: 0x06000F87 RID: 3975 RVA: 0x00066D2C File Offset: 0x00064F2C
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

		// Token: 0x06000F88 RID: 3976 RVA: 0x00066E58 File Offset: 0x00065058
		public static string GetDataDirectory()
		{
			string text = Path.Combine(@"C:\Program Files (x86)\Steam\steamapps\common\Besiege\Besiege_Data\Mods", "Data/");
			Directory.CreateDirectory(text);
			return text;
		}

		// Token: 0x06000F89 RID: 3977 RVA: 0x00066E80 File Offset: 0x00065080
		public static string GetAssemblyDirectory()
		{
			string text = Path.Combine(@"C:\Program Files (x86)\Steam\steamapps\common\Besiege\Besiege_Data\Mods", ".CompiledAssemblies/");
			Directory.CreateDirectory(text);
			return text;
		}

		// Token: 0x06000F8A RID: 3978 RVA: 0x00066EA8 File Offset: 0x000650A8
		public static string GetAssemblyPath(ModContainer mod, string assemblyName)
		{
			return Path.Combine(ModPaths.GetAssemblyDirectory(), string.Concat(new object[]
			{
				mod.Info.Id,
				"_",
				assemblyName,
				".dll"
			}));
		}
	}
}
