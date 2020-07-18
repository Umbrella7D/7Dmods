using System;
using System.Xml;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Reflection;

using Harmony;
using System;
using System.Collections.Generic;

using Harmony;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

using CSutils;
using SdtdUtils;

public class MinEventActionImpactFragment : MinEventActionTargetedBaseCatcher {
    /** Throw (rep) new projectile (item) at impact point (after dt) */
   public string item;
   public int rep = 1;
   public int dt = 1000; // millis
   public bool atEntity = false;
   public GameRandom random;

    public MinEventActionImpactFragment() : base() {
        random = GameRandomManager.Instance.CreateGameRandom();
    }
    public override bool ParseXmlAttribute(XmlAttribute _attribute) {
        bool based = base.ParseXmlAttribute(_attribute);
        string name=  _attribute.Name;
        bool used = true;
        if (name == "item") this.item = _attribute.Value;
        else if (name == "rep") this.rep = int.Parse(_attribute.Value);
        else if (name == "dt") this.dt = int.Parse(_attribute.Value); 
        else if (name == "at_entity") this.atEntity = bool.Parse(_attribute.Value);
        else used = false;
        return used || based;
    }


    public override void ExecuteUncatch(MinEventParams _params) {
        Printer.Log(81, "MinEventActionImpactFragment Execute", this.targets.Count);
        foreach (EntityAlive Self in this.targets) { // Expect ghost only if Self
            if (atEntity) {
                Execute(Self, Self.GetPosition()); // Expects Position at ProjectileImpact
            } else {
                Execute(Self, _params.Position); // Expects Position at ProjectileImpact
                break;
            }

        }
    }
    public void Execute(EntityAlive ctrl, Vector3 pos) {
        // has controler the good cvar ?
        Printer.Log(81, "MinEventActionImpactFragment Execute", ctrl, pos);
        Zombiome.Routines.Start(Fragments(ctrl, pos), "MEAImpactFragment.Fragments({0})", ctrl);
    }

    public IEnumerator Fragments(EntityAlive ctrl, Vector3 pos) {
        Printer.Log(81, "MinEventActionImpactFragment", ctrl, pos, this.item);
        string item = this.item;
        if (item.StartsWith("$")) item = StringMap.Get(ctrl, item);
        if (item=="") yield break;
        Printer.Log(30, "MinEventActionImpactFragment", ctrl, pos, ctrl.GetPosition());
        YieldInstruction dt = new WaitForSeconds((float) this.dt / 1000f);
        for (int replicate=0; replicate<rep; replicate++) {
            yield return dt;
            Vector3 offset = Vectors.Float.Randomize(random, 0.5f, Vectors.Float.UnitY);
            // Vector3 motion = Vectors.Float.Randomize(random, 1f, Vectors.Float.UnitY);
            Vector3 motion = offset.normalized * (1f + 5 * random.RandomFloat);
            motion = motion.normalized * 3;
            yield return SdtdUtils.EffectsItem.spawnItemGhost(ctrl, item, pos + offset + Vectors.Float.UnitY, motion);
        }
    }
}