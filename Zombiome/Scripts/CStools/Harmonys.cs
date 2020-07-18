using Harmony;
using System.Reflection;
using UnityEngine;
using DMT;
using System;
using System.Collections.Generic;


namespace CSutils {

public class Harmonys {
    private static bool Applied = false;
    public static void ApplyOnce(HarmonyInstance harmony, Assembly assembly, Type patch) {
        /* Important: the call to Assembly.GetExecutingAssembly() need be done by the caller
        TODO: check method is patched when skipping ?
        */
        Debug.Log(String.Format("Harmonys.ApplyOnce {0} -> {1}", patch.ToString(), Applied));
        if (Applied) return;
        Debug.Log(String.Format("Harmonys.Applying {0}", patch.ToString()));
        harmony.PatchAll(assembly);
        Applied = true;
    }

}

////////////////////
} // End Namespace
////////////////////