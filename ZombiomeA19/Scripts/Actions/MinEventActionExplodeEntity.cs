using System;
using System.Xml;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using CSutils;
using SdtdUtils;

public class MinEventActionExplodeEntity : MinEventActionTargetedBaseCatcher {
    /** 
    NB: Entity.Kill() will print a message on killed ghosts. Instead, SetDead() does not,
    but it does not trigger onSelfKilled either.
    So we need to link the exploding item here too (for both ) 

    3 explosion cases:
    - DropFall
    - Sursis (buffZBRespise)
    - Explode on contact

    NB: Expose a static func for manual implementation

    */

    public static GameRandom random = null;
    public MinEventActionExplodeEntity() : base() {
        if (random==null) random = GameRandomManager.Instance.CreateGameRandom();
    }

    public static string cvarKey = "ZB_ExplodeEntityCvar";
    public override void ExecuteUncatch(MinEventParams _params) {
        /** Force trigger (<target="self">) */
        if (this.targetType == MinEventActionTargetedBase.TargetTypes.self) {
            Trigger(_params.Self);
            return;
        }
        /** Detects collision with area effect <target="selfAOE">*/
        Printer.Print("ContactKill", _params.Self, this.targets.Count);
        foreach (Entity target in this.targets) {
            if (target !=  _params.Self) {
                Trigger(_params.Self);
                // Printer.Print("ContactKill", _params.Self, "KILLED");
                return;
            }
        }
	}


    public static void Trigger(EntityAlive entity) {
        Zombiome.Routines.Start(_Trigger(entity), "MEAExplodeEntity._Trigger({0})", entity);
    }

    private static YieldInstruction dt = new WaitForSeconds(0.03f);
    private static IEnumerator _Trigger(EntityAlive entity) {
        if (entity==null) yield break;
        Vector3 pos = entity.GetPosition();
        string item = StringMap.Get(entity, cvarKey);
        // Printer.Print("MinEventActionExplodeEntity", entity, pos, item);
        if (item == "") Printer.Print("StringMap no cvar");
        else {
            Vector3 offset = 0.3f * Vectors.Float.UnitY;
            Vector3 motion = -Vectors.Float.UnitY;
            motion = motion.normalized * 3;
            yield return SdtdUtils.EffectsItem.spawnItemGhost(item, pos + offset + Vectors.Float.UnitY, motion); 
            yield return dt;
        }
        if (entity==null) yield break;
        entity.SetDead();
    }
}