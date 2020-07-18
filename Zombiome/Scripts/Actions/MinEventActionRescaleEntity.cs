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
public class MinEventActionRescaleEntity : MinEventActionTargetedBaseCatcher {
    /*
    CHECK: Changing localScale attribute is probably a local effect, I need a buff to network it

    Restoring initial scale is vital (mostly for players), a buff does that too

    NB: Initial player scale assumed to be 1,1,1
    (wouldnt work for general entity, might be enough for EntityAlive)
    (depends on the various blockCollider selected when creating the Bouding Box. In the worse case, a cvar could
    store the initial size before the first alteration)
    */
   public Vector3 scale;

    public override bool ParseXmlAttribute(XmlAttribute _attribute) {
        bool based = base.ParseXmlAttribute(_attribute);
        string name=  _attribute.Name;
        bool used = true;
        if (name == "scale") {
            string val = _attribute.Value;
            if (val.Contains(',')) scale = StringParsers.ParseVector3(val);
            else {
                float v = float.Parse(val);
                scale = v * Vectors.Float.One;
            }
        }
        else used = false;
        return used || based;
    }

    public override void ExecuteUncatch(MinEventParams _params) {
        foreach (EntityAlive target in this.targets) {
            Execute(target);
        }
    }
    public void Execute(EntityAlive entity) {
        // SetScale(entity, this.scale);
        Zombiome.Routines.Start(Progressively(entity, this.scale), "MEARescaleEntity.Progressively({0})", entity);
    }

    public static void SetScale(EntityAlive entity, Vector3 target) {
        if (null == entity) return;
        if (null == entity.gameObject) return;
        if (null == entity.gameObject.transform) return;
        entity.gameObject.transform.localScale = target;
    }

    public static IEnumerator Progressively(EntityAlive target, Vector3 size) {
        // how would it interact if many occurs. I need a single OnUpdate, rather than many coroutines..
        if (null == target) yield break;
        if (null == target.gameObject) yield break;
        if (null == target.gameObject.transform) yield break;
        Vector3 from = target.gameObject.transform.localScale; // Created by Entity.Awake()
        int steps = 10;
        for(int k=0; k<steps; k++){
            float w = (k+1) / (1f * steps);
            if (null == target) yield break;
            if (null == target.gameObject) yield break;
            if (null == target.gameObject.transform) yield break;
            target.gameObject.transform.localScale = (1-w) * from + w * size;
            yield return new WaitForSeconds(0.1f); 
        }
        if (null == target) yield break;
        if (null == target.gameObject) yield break;
        if (null == target.gameObject.transform) yield break;
        target.gameObject.transform.localScale = size;
    }


}