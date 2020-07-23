using Harmony;
using System.Reflection;
using UnityEngine;
using DMT;
using System;
using System.Collections.Generic;

// XUiCWindowSelector.public void OpenSelectedWindow()
// 

[HarmonyPatch(typeof(XUiC_WindowSelector))]
[HarmonyPatch("OpenSelectedWindow")]
public class BlockBackpack3 : IHarmony {
    public void Start() {
        Debug.Log(" Loading Patch: " + GetType().ToString());
        HarmonyInstance harmony = HarmonyInstance.Create(GetType().ToString());
        CSutils.Harmonys.ApplyOnce(harmony, Assembly.GetExecutingAssembly(), GetType());
    }
    static bool Prefix(XUiC_WindowSelector __instance) {        
        Printer.Print("BlockBackpack3 Prefix hook", __instance.GetType());

        if (__instance.Selected == null) return true; // not my problem

        if (__instance.Selected.ID.EqualsCaseInsensitive("crafting")) {
            EntityPlayerLocal _localPlayer = GameManager.Instance.World.GetLocalPlayers()[0];
            if (_localPlayer.Buffs != null && _localPlayer.Buffs.HasBuff("buffZBNosack")) {
                GameManager.ShowTooltip(_localPlayer, "Your bag's zip is stuck, it won't open.");
                return false;
            }
        }
		return true;
    }

}