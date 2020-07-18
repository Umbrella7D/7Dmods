using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using Harmony;


using System.Linq;
using System.Reflection.Emit;

using CSutils;
using SdtdUtils;

public class ConsoleCmdExplBuff : ConsoleCmdAbstract {
   // Token: 0x060012FC RID: 4860 RVA: 0x00077EB4 File Offset: 0x000760B4
    public override string GetDescription() {return "ConsoleCmdExplBuff";}
    public override string[] GetCommands() {
        return new string[] {"shb"};
    }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
        GameManager.Instance.StartCoroutine(_Execute(_params, _senderInfo));
    }

    private string[] buffs = new string[]{
        "buffZBRadiating", "buffZBShoking", "buffRingOfFire_ZB", "buffZbesRFireEst", "buffZBFiring",
        "buffZbesRFireTri", "buffZbesRFireTriTO"
    };


    public IEnumerator _Execute(List<string> _params, CommandSenderInfo _senderInfo) {

        EntityPlayerLocal player = GameManager.Instance.World.GetLocalPlayers()[0]; 
        int eb = int.Parse(_params[0]);
        string buffname = buffs[eb];

        Printer.Print("ConsoleCmdExplShowExplo", _params, buffname);
        // todo: surface

        Emplacement place = Emplacement.At(player.GetPosition() + new Vector3(0,1,10), Vectors.Float.Zero); // N

        Entity Requested = SdtdUtils.EntityCreation.Spawn(place.position, "zombieMoe");
        int eid = Requested.entityId;
        yield return SdtdUtils.EntityCreation.WaitEntity(Requested);
        Entity ent = GameManager.Instance.World.GetEntity(eid);
        if (ent == null) {
            Debug.Log(String.Format("SpawnAndbuff failed entity {0} {1}", eid, Requested));
            yield break;
        }
        EntityAlive entity = ent as EntityAlive;
        if (entity != null) {
            if(! entity.Buffs.HasBuff(buffname)) entity.Buffs.AddBuff(buffname);
        }

   }







}