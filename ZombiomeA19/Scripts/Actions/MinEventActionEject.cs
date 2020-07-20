using System;
using System.Xml;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Reflection;

using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

using CSutils;
using SdtdUtils;


public class MinEventActionEject : MinEventActionTargetedBaseCatcher {
    /*
    TODO: parse strength etc

    */

    /* CheckOthers:
    - OnAttacked needs check it (prevents eject loop on bleeding)
    - OnProjectileImpact must not check it (weirdly, it is null)
    - OnSelfAttackedOther (item modidifier), no need ??
    - Ghost self jump must not check it ?
    */
    public bool CheckOthers = true;
    public float dirrandom = 0f;
    public float strength = 1f;
    public Vector3 dsi = new Vector3(4f, 1f, 1f); // duration, speed, interleave
    // NB:
    // - depends on entity mass ...
    // - (ghost) (4f, 1f, 1f) is very small (4f, 1f, 0f) is way too strong
    //  - (4,2,1): very strong, but remains alive most of the time
    // todo: random duration / trigger on duration

    public override bool ParseXmlAttribute(XmlAttribute _attribute) {
        bool based = base.ParseXmlAttribute(_attribute);
        string name=  _attribute.Name;
        bool used = true;
        if (name == "checkothers") this.CheckOthers = bool.Parse(_attribute.Value);
        if (name == "dirrandom") this.dirrandom = float.Parse(_attribute.Value);
        if (name == "strength") this.strength = float.Parse(_attribute.Value);
        if (name == "dsi") this.dsi = StringParsers.ParseVector3(_attribute.Value);
        else used = false;
        return used || based;
    }

    public override void ExecuteUncatch(MinEventParams _params) {
        // Printer.Print("MinEventActionEject", _params.Self, _params.Other);
        if (CheckOthers && null == _params.Other) return; // bleeding would cause jumping infinitely
        EntityMover mover;
        Vector3 dir;
        if (null != this.targets) {
            // Printer.Print("MinEventActionEject has targets", targets.Count);
            mover = new EntityMover((int) dsi[0], dsi[1], (int) dsi[2]);
            foreach(EntityAlive ent in this.targets) {
                dir = strength * Vectors.Float.Randomize(Zombiome.rand, dirrandom, Vectors.Float.UnitY);
                if (ent != null) mover.Apply(ent, dir);
            }
            return;
        }
        mover = new EntityMover((int) dsi[0], dsi[1], (int) dsi[2]);
        dir = strength * Vectors.Float.Randomize(Zombiome.rand, dirrandom, Vectors.Float.UnitY);
        if (_params.Self != null) mover.Apply(_params.Self, dir);
    }
}