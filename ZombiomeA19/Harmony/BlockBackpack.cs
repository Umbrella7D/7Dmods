using Harmony;
using System.Reflection;
using UnityEngine;
using DMT;
using System;
using System.Collections.Generic;

// XUiC_WindowSelector.public static void OpenSelectorAndWindow(EntityPlayerLocal _localPlayer, string selectedPage)
// XUiC_WindowSelector.public void OpenSelectedWindow()

[HarmonyPatch(typeof(XUiC_WindowSelector))]
[HarmonyPatch("OpenSelectorAndWindow")]
public class BlockBackpack : IHarmony {
    public void Start() {
        Debug.Log(" Loading Patch: " + GetType().ToString());
        HarmonyInstance harmony = HarmonyInstance.Create(GetType().ToString());
        CSutils.Harmonys.ApplyOnce(harmony, Assembly.GetExecutingAssembly(), GetType());
        // harmony.PatchAll(Assembly.GetExecutingAssembly());
    }
 
    static bool Prefix(EntityPlayerLocal _localPlayer, string selectedPage) {
        Debug.Log("BlockBackpack Prefix hook");
        if (selectedPage != "crafting") return true;
        if (_localPlayer.Buffs != null && _localPlayer.Buffs.HasBuff("buffZBNosack")) {
            GameManager.ShowTooltip(_localPlayer, "Your bag's zip is stuck, it won't open.");
            return false;
        }
        return true;
        // could also call windowManager.Close("backpack"); in postfix  ?
    }

}