using System;
using BepInEx;
using BepInEx.Configuration;
using CommonPlugin;
using HarmonyLib;
using SpaceCraft;
using UnityEngine;
using UnityEngine.InputSystem;

namespace HoveringJetPack
{
	[BepInPlugin("com.lukev.hoveringjetpack", "Hovering Jetpack", "2.0.0")]
	public class Plugin : BasePlugin
	{
		protected override void OnAwake()
		{
			Plugin.toggleButton = base.Config.Bind<string>("General", "ToggleButton", "<Keyboard>/c", "Button to toggle hovering");
			Plugin.enableOnStart = base.Config.Bind<bool>("General", "EnableOnStart", true, "Enable hovering when the game starts");
			Plugin.hoveringEnabled = Plugin.enableOnStart.Value;
			Plugin.actionToggle = new InputAction(null, InputActionType.Value, Plugin.toggleButton.Value, null, null, null);
			Plugin.actionToggle.Enable();
		}

		private void Update()
		{
			bool flag = Plugin.actionToggle.WasPressedThisFrame();
			if (flag)
			{
				Plugin.hoveringEnabled = !Plugin.hoveringEnabled;
				BasePlugin.Log.LogInfo("Hovering toggled: " + Plugin.hoveringEnabled.ToString());
			}
		}


		private static ConfigEntry<bool> enableOnStart;
		private static ConfigEntry<string> toggleButton;
		private static bool hoveringEnabled;
		private static InputAction actionToggle;


		[HarmonyPatch(typeof(PlayerMovable), "UpdatePlayerMovement")]
		private static class PlayerMovable_UpdatePlayerMovement_Patch
		{
			private static void Prefix(ref float ___jumpActionValue, int ___jumpStatusInAir, float ___jetpackFactor, CharacterController ___m_Controller, out float? __state)
			{
				__state = null;
				bool hoveringNow =
				Plugin.hoveringEnabled &&
				___jetpackFactor > 0f &&
				___jumpStatusInAir == 2;

				if (!hoveringNow)
				{
					return;
				}
				// Remember the Y position to lock to
				__state = ___m_Controller.transform.position.y;

				// Keep the game thinking "jump is held" so jetpack FX/sound stay on,
				// even if the player has released the Space key.
				if (___jumpActionValue <= 0f)
				{
					___jumpActionValue = 1f;
				}

			}

			private static void Postfix(float ___jumpActionValue, int ___jumpStatusInAir, float ___jetpackFactor, CharacterController ___m_Controller, ref float ___m_Fall, float? __state)
			{
				bool stillHovering =
				__state != null &&
				Plugin.hoveringEnabled &&
				___jetpackFactor > 0f &&
				___jumpStatusInAir == 2;

				if (!stillHovering)
				{
					return;
				}

				// Cancel any vertical motion this frame
				float deltaY = ___m_Controller.transform.position.y - __state.Value;
				___m_Controller.Move(Vector3.up * -deltaY);

				// Reset vertical velocity so we don't "store up" a huge downward speed
				// that would fire the moment hover is disabled.
				___m_Fall = 0f;
			}
		}
	}
}