using System;
using System.Collections.Generic;
using System.Reflection;
using InternalModding.Mods;

namespace InternalModding.Assemblies
{
    // Token: 0x02000244 RID: 580
    public class ModAssembly
    {
        // Token: 0x06000D74 RID: 3444 RVA: 0x0005C050 File Offset: 0x0005A250
        public ModAssembly(ModInfo.AssemblyInfo info, List<Assembly> additionalReferences)
        {
            this.Info = info;
            this.AdditionalReferences = additionalReferences;
        }

        // Token: 0x170000CF RID: 207
        // (get) Token: 0x06000D75 RID: 3445 RVA: 0x0005C068 File Offset: 0x0005A268
        // (set) Token: 0x06000D76 RID: 3446 RVA: 0x0005C070 File Offset: 0x0005A270
        public ModInfo.AssemblyInfo Info { get; private set; }

        // Token: 0x170000D0 RID: 208
        // (get) Token: 0x06000D77 RID: 3447 RVA: 0x0005C07C File Offset: 0x0005A27C
        // (set) Token: 0x06000D78 RID: 3448 RVA: 0x0005C084 File Offset: 0x0005A284
        public Assembly Assembly { get; set; }

        // Token: 0x170000D1 RID: 209
        // (get) Token: 0x06000D79 RID: 3449 RVA: 0x0005C090 File Offset: 0x0005A290
        // (set) Token: 0x06000D7A RID: 3450 RVA: 0x0005C098 File Offset: 0x0005A298
        public List<Assembly> AdditionalReferences { get; private set; }

        // Token: 0x170000D2 RID: 210
        // (get) Token: 0x06000D7B RID: 3451 RVA: 0x0005C0A4 File Offset: 0x0005A2A4
        // (set) Token: 0x06000D7C RID: 3452 RVA: 0x0005C0AC File Offset: 0x0005A2AC
        public bool HasModEntryPoint { get; set; }

        // Token: 0x170000D3 RID: 211
        // (get) Token: 0x06000D7D RID: 3453 RVA: 0x0005C0B8 File Offset: 0x0005A2B8
        // (set) Token: 0x06000D7E RID: 3454 RVA: 0x0005C0C0 File Offset: 0x0005A2C0
        public ModEntryPoint ModEntryPoint { get; set; }
    }
}
