using System;
using System.Reflection;
using BepInEx;
using BepInEx.Logging;
using HarmonyLib;

namespace CommonPlugin
{
	// Token: 0x02000002 RID: 2
	public abstract class BasePlugin : BaseUnityPlugin
	{
		// Token: 0x17000001 RID: 1
		// (get) Token: 0x06000001 RID: 1 RVA: 0x00002050 File Offset: 0x00000250
		public static ManualLogSource Log
		{
			get
			{
				return BasePlugin.instance.Logger;
			}
		}

		// Token: 0x06000002 RID: 2 RVA: 0x0000205C File Offset: 0x0000025C
		protected virtual void Awake()
		{
			BasePlugin.instance = this;
			this.OnAwake();
			Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
		}

		// Token: 0x06000003 RID: 3
		protected abstract void OnAwake();

		// Token: 0x04000001 RID: 1
		internal static BasePlugin instance;
	}
}
