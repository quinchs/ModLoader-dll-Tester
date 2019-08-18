using System;
using System.Collections.Generic;
using System.Linq;
using InternalModding.Assemblies;
using UnityEngine;

namespace InternalModding.Mods
{
    // Token: 0x0200027C RID: 636
    public class ModContainer
    {
        // Token: 0x06000FC0 RID: 4032 RVA: 0x0006813C File Offset: 0x0006633C
        public ModContainer(ModInfo data)
        {
            this.Info = data;
            this.Assemblies = new List<ModAssembly>();
            this.HadLoadOrActivateErrors = false;
        }

        // Token: 0x17000146 RID: 326
        // (get) Token: 0x06000FC1 RID: 4033 RVA: 0x000681A0 File Offset: 0x000663A0
        // (set) Token: 0x06000FC2 RID: 4034 RVA: 0x000681A8 File Offset: 0x000663A8
        public ModInfo Info { get; private set; }

        // Token: 0x17000147 RID: 327
        // (get) Token: 0x06000FC3 RID: 4035 RVA: 0x000681B4 File Offset: 0x000663B4
        // (set) Token: 0x06000FC4 RID: 4036 RVA: 0x000681BC File Offset: 0x000663BC
        public Texture2D SmallIcon { get; set; }

        // Token: 0x17000148 RID: 328
        // (get) Token: 0x06000FC5 RID: 4037 RVA: 0x000681C8 File Offset: 0x000663C8
        // (set) Token: 0x06000FC6 RID: 4038 RVA: 0x000681D0 File Offset: 0x000663D0
        public ModContainer.State CurrentState { get; set; }

        // Token: 0x17000149 RID: 329
        // (get) Token: 0x06000FC7 RID: 4039 RVA: 0x000681DC File Offset: 0x000663DC
        public bool IsActive
        {
            get
            {
                return this.CurrentState != ModContainer.State.Loaded;
            }
        }

        // Token: 0x1700014A RID: 330
        // (get) Token: 0x06000FC8 RID: 4040 RVA: 0x000681EC File Offset: 0x000663EC
        // (set) Token: 0x06000FC9 RID: 4041 RVA: 0x000681F4 File Offset: 0x000663F4
        public bool IsEnabled { get; set; }

        // Token: 0x1700014B RID: 331
        // (get) Token: 0x06000FCA RID: 4042 RVA: 0x00068200 File Offset: 0x00066400
        // (set) Token: 0x06000FCB RID: 4043 RVA: 0x00068208 File Offset: 0x00066408
        public bool HadLoadOrActivateErrors { get; set; }

        // Token: 0x06000FCC RID: 4044 RVA: 0x00068214 File Offset: 0x00066414
        public List<Type> GetTypesByName(string name)
        {
            List<Type> result;
            try
            {
                result = this.Assemblies.SelectMany((ModAssembly a) => from t in a.Assembly.GetTypes()
                                                                       where t.FullName == name
                                                                       select t).ToList<Type>();
            }
            catch (Exception ex)
            {
                InternalModding.Assemblies.MLog.Error(string.Concat(new object[]
                {
                    "Error searching for type ",
                    name,
                    ": ",
                    ex
                }));
                result = new List<Type>();
            }
            return result;
        }

        // Token: 0x06000FCD RID: 4045 RVA: 0x000682B0 File Offset: 0x000664B0


        // Token: 0x06000FCF RID: 4047 RVA: 0x00068318 File Offset: 0x00066518

        // Token: 0x06000FD1 RID: 4049 RVA: 0x00068490 File Offset: 0x00066690
        public override bool Equals(object obj)
        {
            ModContainer modContainer = obj as ModContainer;
            return modContainer != null && modContainer.Info.Id == this.Info.Id;
        }

        // Token: 0x06000FD2 RID: 4050 RVA: 0x000684C8 File Offset: 0x000666C8
        public override int GetHashCode()
        {
            return (this.Info == null) ? 0 : this.Info.GetHashCode();
        }

        // Token: 0x04001057 RID: 4183
        public List<ModAssembly> Assemblies;

        // Token: 0x0200027D RID: 637
        public enum State
        {
            // Token: 0x04001065 RID: 4197
            Loaded,
            // Token: 0x04001066 RID: 4198
            Active,
            // Token: 0x04001067 RID: 4199
            ActiveUnregistered
        }
    }
}
