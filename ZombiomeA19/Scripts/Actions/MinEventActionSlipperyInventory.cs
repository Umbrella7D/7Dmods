using System;
using System.Xml;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Reflection;


using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;




public class MinEventActionSlipperyInventory : MinEventActionTargetedBaseCatcher {
    /** Randomly drop item from hand/belt */
    public override void ExecuteUncatch(MinEventParams _params) {
        foreach (EntityAlive target in this.targets) { // Expect ghost only if Self
            EntityPlayer player = target as EntityPlayer;
            if (null != player) {
                Execute(player); // Expects Position at ProjectileImpact
                break;
            }
        }
    }
    public void Execute(EntityPlayer ctrl) {
        bool drop = Zombiome.rand.RandomFloat < 0.2;
        if (drop) {
            Printer.Log(30, "MinEventActionSlipperyInventory drops", ctrl);
            SdtdUtils.EffectsInventory.Drop(ctrl);
        }
    }
}