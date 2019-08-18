using System;
using UnityEngine;

namespace InternalModding.Misc
{
	// Token: 0x02000276 RID: 630
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
}
