using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using InternalModding.Mods;
using UnityEngine;

namespace InternalModding.Assemblies
{
    public interface IComponentProvider
    {
        // Token: 0x1700013F RID: 319
        // (get) Token: 0x06000F35 RID: 3893
        bool ActiveInSingleplayer { get; }

        // Token: 0x06000F36 RID: 3894
        bool LoadMod(ModContainer mod);

        // Token: 0x06000F37 RID: 3895
        bool ActivateMod(ModContainer mod);

        // Token: 0x06000F38 RID: 3896
        void RegisterPrefabs(ModContainer mod);

        // Token: 0x06000F39 RID: 3897
        void UnregisterPrefabs(ModContainer mod);

        // Token: 0x06000F3A RID: 3898
        void PostRegisterPrefabs();
    }
    // Token: 0x02000242 RID: 578
    public class AssemblyLoader : SingleInstanceFindOnly<AssemblyLoader>
    {
        // Token: 0x170000CD RID: 205
        // (get) Token: 0x06000D5A RID: 3418 RVA: 0x0005B194 File Offset: 0x00059394
        public override string Name
        {
            get
            {
                return "ModMaster";
            }
        }

        // Token: 0x170000CE RID: 206
        // (get) Token: 0x06000D5B RID: 3419 RVA: 0x0005B19C File Offset: 0x0005939C
        public bool ActiveInSingleplayer
        {
            get
            {
                return true;
            }
        }

        // Token: 0x06000D5C RID: 3420 RVA: 0x0005B1A0 File Offset: 0x000593A0
        public static bool LoadMod(ModContainer mod)
        {
            bool result = true;
            foreach (ModInfo.AssemblyInfo assemblyInfo in mod.Info.Assemblies)
            {
                assemblyInfo.Mod = mod;
                assemblyInfo.Resolve();
                ModAssembly item;
                if (!ValidateAssembly(assemblyInfo, out item))
                {
                    MLog.Error(string.Concat(new string[]
                    {
                        "Assembly for ",
                        mod.Info.Name,
                        " did not validate, not loading it! (",
                        assemblyInfo.Path,
                        ")"
                    }));
                    result = false;
                }
                else
                {
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine("[SUCSESS] Mod Loaded :D");
                }
            }
            return result;
        }

        // Token: 0x06000D5D RID: 3421 RVA: 0x0005B280 File Offset: 0x00059480
        public bool ActivateMod(ModContainer mod)
        {
            bool result = true;
            foreach (ModAssembly modAssembly in mod.Assemblies)
            {
                if (!this.LoadAssembly(modAssembly))
                {
                    result = false;
                }
                else if (modAssembly.HasModEntryPoint)
                {
                    modAssembly.ModEntryPoint.ModContainer = mod;
                    if (!this.InitializeAssembly(modAssembly))
                    {
                        result = false;
                        MLog.Error("There was an error activating assembly: " + modAssembly.Assembly.GetName().Name);
                    }
                }
            }
            return result;
        }

        // Token: 0x06000D5E RID: 3422 RVA: 0x0005B33C File Offset: 0x0005953C
        private static bool ValidateAssembly(ModInfo.AssemblyInfo info, out ModAssembly mAssembly)
        {
            List<Assembly> list = new List<Assembly>();
            if (!AssemblyScanner.Scan(info, list))
            {
                mAssembly = null;
                return false;
            }
            mAssembly = new ModAssembly(info, list);
            if (info.Mod.Assemblies.Any((ModAssembly a) => a.Info.Path == info.Path))
            {
                MLog.Error("The same assmbly is listed in the manifest more than once!");
                mAssembly = null;
                return false;
            }
            return true;
        }

        // Token: 0x06000D5F RID: 3423 RVA: 0x0005B3B8 File Offset: 0x000595B8
        private bool LoadAssembly(ModAssembly mAssembly)
        {
            bool result;
            try
            {
                Assembly assembly = Assembly.LoadFrom(mAssembly.Info.Path);
                mAssembly.Assembly = assembly;
                List<Type> list = (from t in assembly.GetTypes()
                                   where t.IsSubclassOf(typeof(ModEntryPoint)) && !t.IsAbstract
                                   select t).ToList<Type>();
                if (list.Count > 1)
                {
                    throw new Exception("Too many types extending ModEntryPoint!");
                }
                if (list.Count == 1)
                {
                    Type type = list[0];
                    ModEntryPoint modEntryPoint = (ModEntryPoint)Activator.CreateInstance(type);
                    mAssembly.HasModEntryPoint = true;
                    mAssembly.ModEntryPoint = modEntryPoint;
                }
                else
                {
                    mAssembly.HasModEntryPoint = false;
                }
                result = true;
            }
            catch (Exception ex)
            {
                MLog.Error("Error loading assembly: " + mAssembly.Info.Path);
                MLog.Error(ex.ToString());
                result = false;
            }
            return result;
        }

        // Token: 0x06000D60 RID: 3424 RVA: 0x0005B4BC File Offset: 0x000596BC
        private bool InitializeAssembly(ModAssembly assembly)
        {
            return ModdingUtil.PerformCallback(new Action(assembly.ModEntryPoint.OnLoad));
        }

        // Token: 0x06000D61 RID: 3425 RVA: 0x0005B4D8 File Offset: 0x000596D8
        

        // Token: 0x06000D62 RID: 3426 RVA: 0x0005B650 File Offset: 0x00059850
        //public static ModContainer GetModByAssembly(Assembly assembly)
        //{
        //    return ModManager.Mods.FirstOrDefault((ModContainer c) => c.Assemblies.Any((ModAssembly mA) => mA.Assembly == assembly));
        //}

        // Token: 0x06000D63 RID: 3427 RVA: 0x0005B680 File Offset: 0x00059880
        public void RegisterPrefabs(ModContainer mod)
        {
        }

        // Token: 0x06000D64 RID: 3428 RVA: 0x0005B684 File Offset: 0x00059884
        public void PostRegisterPrefabs()
        {
        }

        // Token: 0x06000D65 RID: 3429 RVA: 0x0005B688 File Offset: 0x00059888
        public void UnregisterPrefabs(ModContainer mod)
        {
        }

        // Token: 0x04000F10 RID: 3856
        //private List<ModAssembly> LoadedAssemblies = new List<ModAssembly>();
    }

    public abstract class ModEntryPoint
    {
        // Token: 0x0600249A RID: 9370 RVA: 0x000CBBB4 File Offset: 0x000C9DB4
        public virtual void OnLoad()
        {
        }

        // Token: 0x0600249B RID: 9371 RVA: 0x000CBBB8 File Offset: 0x000C9DB8

        // Token: 0x0600249D RID: 9373 RVA: 0x000CBBC0 File Offset: 0x000C9DC0
        public virtual void OnBlockPrefabCreation(int blockId, GameObject prefab, GameObject ghost)
        {
        }

        // Token: 0x0600249E RID: 9374 RVA: 0x000CBBC4 File Offset: 0x000C9DC4
        public virtual void OnEntityPrefabCreation(int entityId, GameObject prefab)
        {
        }

        // Token: 0x0400221B RID: 8731
        internal ModContainer ModContainer;
    }

    public abstract class SingleInstanceFindOnly<T> : MonoBehaviour where T : SingleInstanceFindOnly<T>
    {
        // Token: 0x170005E8 RID: 1512
        // (get) Token: 0x0600392D RID: 14637
        public abstract string Name { get; }

        // Token: 0x170005E9 RID: 1513
        // (get) Token: 0x0600392E RID: 14638 RVA: 0x0013E92C File Offset: 0x0013CB2C
        public static T Instance
        {
            get
            {
                if (!SingleInstanceFindOnly<T>.hasInstance())
                {
                    if (SingleInstanceFindOnly<T>.Find() && !SingleInstanceFindOnly<T>.instance.setUp)
                    {
                        SingleInstanceFindOnly<T>.instance.setUp = true;
                        SingleInstanceFindOnly<T>.instance.SetUp();
                    }
                }
                else if (!SingleInstanceFindOnly<T>.instance.setUp)
                {
                    SingleInstanceFindOnly<T>.instance.setUp = true;
                    SingleInstanceFindOnly<T>.instance.SetUp();
                }
                return SingleInstanceFindOnly<T>.instance;
            }
        }

        // Token: 0x0600392F RID: 14639 RVA: 0x0013E9C0 File Offset: 0x0013CBC0
        protected virtual void Awake()
        {
            SingleInstanceFindOnly<T>.instance = (this as T);
            if (!SingleInstanceFindOnly<T>.instance.setUp)
            {
                SingleInstanceFindOnly<T>.instance.SetUp();
                SingleInstanceFindOnly<T>.instance.setUp = true;
            }
        }

        // Token: 0x06003930 RID: 14640 RVA: 0x0013EA14 File Offset: 0x0013CC14
        public static void Initialize()
        {
            if (object.ReferenceEquals(SingleInstanceFindOnly<T>.instance, null))
            {
                SingleInstanceFindOnly<T>.Find();
            }
        }

        // Token: 0x06003931 RID: 14641 RVA: 0x0013EA34 File Offset: 0x0013CC34
        public static bool hasInstance()
        {
            return SingleInstanceFindOnly<T>.instance != null;
        }

        // Token: 0x06003932 RID: 14642 RVA: 0x0013EA48 File Offset: 0x0013CC48
        private static bool Find()
        {
            T[] array = UnityEngine.Object.FindObjectsOfType<T>();
            if (array.Length > 1)
            {
                Debug.LogWarning("Too many instances of " + typeof(T).Name + ".");
            }
            if (array.Length > 0)
            {
                SingleInstanceFindOnly<T>.instance = array[0];
                return true;
            }
            return false;
        }

        // Token: 0x06003933 RID: 14643 RVA: 0x0013EAA0 File Offset: 0x0013CCA0
        public virtual void SetUp()
        {
        }

        // Token: 0x0400330D RID: 13069
        private static T instance;

        // Token: 0x0400330E RID: 13070
        protected bool setUp;
    }
    public static class ModdingUtil
    {
        // Token: 0x06000F8B RID: 3979 RVA: 0x00066EF4 File Offset: 0x000650F4
        public static void PerformCallback(MulticastDelegate callback, params object[] args)
        {
            if (callback == null)
            {
                return;
            }
            foreach (Delegate @delegate in callback.GetInvocationList())
            {
                try
                {
                    @delegate.DynamicInvoke(args);
                }
                catch (Exception arg)
                {
                    Debug.LogError("[Callback Exception] " + arg);
                }
            }
        }

        // Token: 0x06000F8C RID: 3980 RVA: 0x00066F68 File Offset: 0x00065168
        public static bool PerformCallback(Action action)
        {
            if (action == null)
            {
                return false;
            }
            bool result;
            try
            {
                action();
                result = true;
            }
            catch (Exception arg)
            {
                Debug.LogError("[Callback Exception] " + arg);
                result = false;
            }
            return result;
        }

        // Token: 0x06000F8D RID: 3981 RVA: 0x00066FCC File Offset: 0x000651CC
        public static bool PerformCallback<TResult>(Func<TResult> func, out TResult result)
        {
            if (func == null)
            {
                result = default(TResult);
                return false;
            }
            bool result2;
            try
            {
                result = func();
                result2 = true;
            }
            catch (Exception arg)
            {
                Debug.LogError("[Callback Exception] " + arg);
                result = default(TResult);
                result2 = false;
            }
            return result2;
        }

        // Token: 0x06000F8E RID: 3982 RVA: 0x00067054 File Offset: 0x00065254
        public static bool IsInGame()
        {
            return true;
        }

        // Token: 0x06000F8F RID: 3983 RVA: 0x00067098 File Offset: 0x00065298
        public static bool TryParseGuid(string s, out Guid g)
        {
            bool result;
            try
            {
                g = new Guid(s);
                result = true;
            }
            catch (Exception)
            {
                g = Guid.Empty;
                result = false;
            }
            return result;
        }

        // Token: 0x04001034 RID: 4148
        public const string CallbackErrorTag = "[Callback Exception] ";
    }
}
