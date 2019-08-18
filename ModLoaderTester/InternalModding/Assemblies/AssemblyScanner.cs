using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using InternalModding.Mods;
using Mono.Cecil;
using Mono.Cecil.Cil;
using UnityEngine;

namespace InternalModding.Assemblies
{
    // Token: 0x02000243 RID: 579
    public class BesiegeLogFilter
    {
        public static bool logDev = true;
    }
    public static class Debug
    {
        public static void LogFormat(string format, params object[] args)
        {
            Console.WriteLine(format, args);
        }
        public static void LogError(object message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Error]: {message}");
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void LogException(Exception exception)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"[Exeption] {exception}");
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void Log(object message)
        {
            Console.WriteLine("[LOG]" + message);
        }
        public static void LogWarning(object message)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine($"[WARNING] -> {message}");
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void LogWarningFormat(string message, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.DarkYellow;
            Console.WriteLine(message, args);
            Console.ForegroundColor = ConsoleColor.White;
        }
        public static void LogErrorFormat(string message, params object[] args)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message, args);
            Console.ForegroundColor = ConsoleColor.White;
        }

    }
    public static class MLog
    {
        // Token: 0x06000F7E RID: 3966 RVA: 0x00066BC8 File Offset: 0x00064DC8
        public static void Info(string message)
        {
            Debug.Log("[Mods] " + message);
        }

        // Token: 0x06000F7F RID: 3967 RVA: 0x00066BDC File Offset: 0x00064DDC
        public static void InfoFormat(string format, params object[] args)
        {
            Debug.LogFormat("[Mods] " + format, args);
        }

        // Token: 0x06000F80 RID: 3968 RVA: 0x00066BF0 File Offset: 0x00064DF0
        public static void Warn(string message)
        {
            Debug.LogWarning("[Mods] " + message);
        }

        // Token: 0x06000F81 RID: 3969 RVA: 0x00066C04 File Offset: 0x00064E04
        public static void WarnFormat(string format, params object[] args)
        {
            Debug.LogWarningFormat("[Mods] " + format, args);
        }

        // Token: 0x06000F82 RID: 3970 RVA: 0x00066C18 File Offset: 0x00064E18
        public static void Error(string message)
        {
            Debug.LogError("[Mods] " + message);
        }

        // Token: 0x06000F83 RID: 3971 RVA: 0x00066C2C File Offset: 0x00064E2C
        public static void ErrorFormat(string format, params object[] args)
        {
            Debug.LogErrorFormat("[Mods] " + format, args);
        }

        // Token: 0x04001033 RID: 4147
        public const string LogTag = "[Mods] ";
    }



    public class AssemblyScanner
    {
        // Token: 0x06000D69 RID: 3433 RVA: 0x0005B7EC File Offset: 0x000599EC
        public static bool Scan(ModInfo.AssemblyInfo info, List<Assembly> additionalRefs)
        {
            if (BesiegeLogFilter.logDev)
            {
                Console.WriteLine("[AssemblyScanner] Scanning {0}.", new object[]
                {
                    info.Path
                });
            }
            bool result;
            try
            {
                AssemblyScanner assemblyScanner = new AssemblyScanner();
                assemblyScanner.CurrentlyScanning = info;
                AssemblyDefinition assemblyDefinition = AssemblyDefinition.ReadAssembly(info.Path);
                Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
                if (BesiegeLogFilter.logDev)
                {
                    string format = "[AssemblyScanner] Got loaded assembly list:\n{0}.";
                    object[] array = new object[1];
                    array[0] = string.Join("\n", (from a in assemblies
                                                  select a.FullName + "(" + a.CodeBase + ")").ToArray<string>());
                    Debug.LogFormat(format, array);
                }
                foreach (ModuleDefinition moduleDefinition in assemblyDefinition.Modules)
                {
                    AssemblyNameReference reference;
                    foreach (AssemblyNameReference reference2 in moduleDefinition.AssemblyReferences)
                    {
                        reference = reference2;
                        if (BesiegeLogFilter.logDev)
                        {
                            Debug.LogFormat("[AssemblyScanner] Checking if reference {0} is loaded.", new object[]
                            {
                                reference.FullName
                            });
                        }
                        if (assemblies.Any((Assembly a) => a.GetName().Name == reference.Name))
                        {
                            if (BesiegeLogFilter.logDev)
                            {
                                Debug.LogFormat("[AssemblyScanner] Found matching loaded assembly!", new object[0]);
                            }
                        }
                        else
                        {
                            if (BesiegeLogFilter.logDev)
                            {
                                Debug.LogFormat("[AssemblyScanner] Did not find matching loaded assembly!", new object[0]);
                            }
                            Assembly assembly;
                            try
                            {
                                assembly = Assembly.ReflectionOnlyLoad(reference.FullName);
                            }
                            catch (FileNotFoundException)
                            {
                                string text = Path.Combine(new FileInfo(info.Path).DirectoryName, reference.Name + ".dll");
                                if (!File.Exists(text))
                                {
                                    continue;
                                }
                                assembly = Assembly.ReflectionOnlyLoadFrom(text);
                            }
                            additionalRefs.Add(assembly);
                            ModInfo.AssemblyInfo info2 = new ModInfo.AssemblyInfo
                            {
                                Path = assembly.Location
                            };
                            if (!AssemblyScanner.Scan(info2, additionalRefs))
                            {
                                MLog.Error("Referenced assembly did not validate: " + reference.Name);
                                MLog.Error("Not loading " + assemblyDefinition.Name);
                                return false;
                            }
                        }
                    }
                    foreach (TypeDefinition type in moduleDefinition.Types)
                    {
                        if (!assemblyScanner.ScanType(type))
                        {
                            return false;
                        }
                    }
                }
                result = true;
            }
            catch (Exception exception)
            {
                Debug.LogError("Error scanning assembly, not loading!");
                Debug.LogException(exception);
                result = false;
            }
            return result;
        }

        // Token: 0x06000D6A RID: 3434 RVA: 0x0005BB54 File Offset: 0x00059D54
        private bool ScanType(TypeDefinition type)
        {
            foreach (FieldDefinition fieldDefinition in type.Fields)
            {
                if (AssemblyScanner.IsForbidden(fieldDefinition.FieldType))
                {
                    this.LogError(fieldDefinition, fieldDefinition.FieldType);
                    return false;
                }
            }
            foreach (MethodDefinition methodDefinition in type.Methods)
            {
                if (methodDefinition.HasPInvokeInfo)
                {
                    this.LogError("You are not allowed to use PInvoke!");
                    return false;
                }
                if (methodDefinition.HasBody)
                {
                    foreach (VariableDefinition variableDefinition in methodDefinition.Body.Variables)
                    {
                        if (AssemblyScanner.IsForbidden(variableDefinition.VariableType))
                        {
                            this.LogError(methodDefinition, variableDefinition.VariableType);
                            return false;
                        }
                    }
                    foreach (Instruction instruction in methodDefinition.Body.Instructions)
                    {
                        if (instruction.OpCode == OpCodes.Ldfld)
                        {
                            FieldReference fieldReference = (FieldReference)instruction.Operand;
                            if (AssemblyScanner.IsForbidden(fieldReference.FieldType))
                            {
                                this.LogError(methodDefinition, fieldReference.FieldType);
                                return false;
                            }
                        }
                        else if (instruction.OpCode == OpCodes.Call || instruction.OpCode == OpCodes.Callvirt || instruction.OpCode == OpCodes.Calli || instruction.OpCode == OpCodes.Newobj)
                        {
                            MethodReference methodReference = (MethodReference)instruction.Operand;
                            if (AssemblyScanner.IsForbidden(methodReference))
                            {
                                this.LogError(methodDefinition, methodReference);
                                return false;
                            }
                        }
                        else if (instruction.OpCode == OpCodes.Newarr)
                        {
                            TypeReference typeReference = (TypeReference)instruction.Operand;
                            if (AssemblyScanner.IsForbidden(typeReference))
                            {
                                this.LogError(methodDefinition, typeReference);
                                return false;
                            }
                        }
                    }
                }
            }
            foreach (TypeDefinition type2 in type.NestedTypes)
            {
                if (!this.ScanType(type2))
                {
                    return false;
                }
            }
            return true;
        }

        // Token: 0x06000D6B RID: 3435 RVA: 0x0005BEAC File Offset: 0x0005A0AC
        private static bool IsForbidden(TypeReference type)
        {
            return AssemblyScanner.IsForbiddenType(type.Namespace, type.Name);
        }

        // Token: 0x06000D6C RID: 3436 RVA: 0x0005BEC0 File Offset: 0x0005A0C0
        private static bool IsForbidden(MethodReference method)
        {
            return AssemblyScanner.IsForbiddenMethod(method.DeclaringType.Namespace, method.DeclaringType.Name, method.Name);
        }

        // Token: 0x06000D6D RID: 3437 RVA: 0x0005BEF0 File Offset: 0x0005A0F0
        private static bool IsForbiddenType(string nspace, string name)
        {
            return AssemblyScanner.BlacklistNamespaces.Any(new Func<string, bool>((nspace + "." + name).StartsWith)) && !AssemblyScanner.WhitelistedTypes.Contains(nspace + "." + name);
        }

        // Token: 0x06000D6E RID: 3438 RVA: 0x0005BF40 File Offset: 0x0005A140
        private static bool IsForbiddenMethod(string nspace, string type, string method)
        {
            return AssemblyScanner.IsForbiddenType(nspace, type) || AssemblyScanner.BlacklistMethods.Contains(string.Concat(new string[]
            {
                nspace,
                ".",
                type,
                ".",
                method
            }));
        }

        // Token: 0x06000D6F RID: 3439 RVA: 0x0005BF94 File Offset: 0x0005A194
        private void LogError(string msg)
        {
            MLog.Error("[Security] " + msg);
            MLog.Error("[Security] Not loading " + this.CurrentlyScanning.Path);
        }

        // Token: 0x06000D70 RID: 3440 RVA: 0x0005BFCC File Offset: 0x0005A1CC
        private void LogError(string location, string forbiddenType)
        {
            this.LogError(string.Concat(new string[]
            {
                "You are not allowed to use ",
                forbiddenType,
                " (used in ",
                location,
                ")."
            }));
        }

        // Token: 0x06000D71 RID: 3441 RVA: 0x0005C000 File Offset: 0x0005A200
        private void LogError(MemberReference location, TypeReference forbiddenType)
        {
            this.LogError(location.FullName, forbiddenType.FullName);
        }

        // Token: 0x06000D72 RID: 3442 RVA: 0x0005C014 File Offset: 0x0005A214
        private void LogError(MemberReference location, MethodReference forbiddenMethod)
        {
            this.LogError(location.FullName, forbiddenMethod.FullName);
        }

        // Token: 0x04000F12 RID: 3858
        private static readonly string[] BlacklistNamespaces = new string[]
        {
            "System.IO",
            "System.Net",
            "System.Xml",
            "System.Reflection",
            "System.Runtime.InteropServices",
            "System.Diagnostics",
            "System.Security",
            "Mono.CSharp",
            "Mono.Cecil",
            "System.CodeDom.Compiler",
            "CSharpCompiler",
            "IKVM",
            "Mono.CompilerServices",
            "UnityEngine.WWW",
            "UnityEngine.MasterServer",
            "Steamworks",
            "GameGrind",
            "InternalModding"
        };

        // Token: 0x04000F13 RID: 3859
        private static readonly string[] BlacklistMethods = new string[]
        {
            "XmlSaver.Save",
            "LevelXMLSaver.Create",
            "UnityEngine.AssetBundle.LoadFromFile",
            "UnityEngine.AssetBundle.LoadFromFileAsync"
        };

        // Token: 0x04000F14 RID: 3860
        private static readonly string[] WhitelistedTypes = new string[]
        {
            "System.IO.Stream",
            "System.IO.TextWriter",
            "System.IO.TextReader",
            "System.IO.BinaryWriter",
            "System.IO.BinaryReader",
            "System.IO.MemoryStream",
            "System.IO.Path",
            "System.Diagnostics.Stopwatch",
            "System.Security.Cryptography"
        };

        // Token: 0x04000F15 RID: 3861
        private ModInfo.AssemblyInfo CurrentlyScanning;
    }
}
