using System;
using System.CodeDom.Compiler;
using System.IO;
using System.Linq;
using System.Reflection;
using CSharpCompiler;
using InternalModding.Misc;
using InternalModding.Mods;

namespace InternalModding.Assemblies
{
    // Token: 0x02000241 RID: 577
    public static class AssemblyCompiler
    {
        // Token: 0x06000D55 RID: 3413 RVA: 0x0005AF34 File Offset: 0x00059134
        public static string ResolveScriptAssembly(string codeDir, ModContainer mod)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(codeDir);
            if (!directoryInfo.Exists)
            {
                MLog.Error("Code directory " + codeDir + " does not exist!");
                return string.Empty;
            }
            string assemblyPath = ModPaths.GetAssemblyPath(mod, directoryInfo.Name);
            if (File.Exists(assemblyPath))
            {
                return assemblyPath;
            }
            CompilerParameters compilerParameters = new CompilerParameters
            {
                GenerateExecutable = false,
                GenerateInMemory = false,
                OutputAssembly = assemblyPath
            };
            compilerParameters.ReferencedAssemblies.AddRange((from a in AppDomain.CurrentDomain.GetAssemblies().Where(delegate (Assembly a)
            {
                bool result;
                try
                {
                    result = !string.IsNullOrEmpty(a.Location);
                }
                catch (NotSupportedException)
                {
                    result = false;
                }
                return result;
            })
                                                              select a.Location).ToArray<string>());
            string[] array = (from f in directoryInfo.GetFiles("*.cs", SearchOption.AllDirectories)
                              select f.FullName).ToArray<string>();
            if (array.Length == 0)
            {
                MLog.Error("Code directory " + codeDir + " does not contain any source files!");
            }
            CSharpCompiler.CodeCompiler codeCompiler = new CSharpCompiler.CodeCompiler();
            CompilerResults compilerResults = codeCompiler.CompileAssemblyFromFileBatch(compilerParameters, array);
            foreach (object obj in compilerResults.Errors)
            {
                CompilerError compilerError = (CompilerError)obj;
                string message = compilerError.ToString();
                if (compilerError.IsWarning)
                {
                    MLog.Warn(message);
                }
                else
                {
                    MLog.Error(message);
                }
            }
            if (compilerResults.Errors.HasErrors)
            {
                MLog.Error("There were errors compiling the ScriptAssembly at " + codeDir + "!");
            }
            return assemblyPath;
        }
    }
}
