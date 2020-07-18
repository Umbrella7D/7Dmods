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

using SdtdUtils;
using SdtdUtils.Blocks;

using CSutils;


public class MinEventActionImpactSpawn : MinEventActionTargetedBaseCatcher {
    /** Spawn block at impact point */
   public string block;
   public int dt = 1000; // millis

    public override bool ParseXmlAttribute(XmlAttribute _attribute) {
        bool based = base.ParseXmlAttribute(_attribute);
        string name=  _attribute.Name;
        bool used = true;
        if (name == "block") this.block = _attribute.Value;
        // else if (name == "rep") this.rep = int.Parse(_attribute.Value);
        // else
        else if (name == "dt") this.dt = int.Parse(_attribute.Value); 
        else used = false;
        return used || based;
    }

    public override void ExecuteUncatch(MinEventParams _params) {
        foreach (EntityAlive Self in this.targets) { // Expect ghost only if Self
            Execute(Self, _params.Position); // Expects Position at ProjectileImpact
            break;
        }
    }
    public void Execute(EntityAlive ctrl, Vector3 pos) {
        Zombiome.Routines.Start(SpawnBlock(ctrl, pos), "MEAImpactSpawn.SpawnBlock({0})", ctrl);
    }

    public IEnumerator SpawnBlock(EntityAlive ctrl, Vector3 pos) {
        Printer.Log(30, "MinEventActionImpactSpawn", ctrl, pos, ctrl.GetPosition());
        yield return new WaitForSeconds((float) this.dt / 1000f);

        BlockSetter.Options opt = new BlockSetter.Options();
        opt.avoidBlock = false;
        opt.avoidEntity = false;
        opt.elastic = 0;
        opt.SetBlocks(this.block);

        BlockSetter setter = new BlockSetter(opt);
        setter.OnCreation = BlockSetter.Rotate;
        Vector3i ipos = Vectors.ToInt(pos);
        Vector3i surf = Geo3D.Surface(ipos, ipos.y) + Vectors.Up;
        // setter.Apply(ipos); // is it pos +1 ? do we need surface ?
        setter.Apply(surf);
        setter.Push();

    }
}