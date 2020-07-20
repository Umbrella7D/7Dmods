using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Linq;
using System.Reflection.Emit;



public class ConsoleCmdBuffSelf : ConsoleCmdAbstract {
   // Token: 0x060012FC RID: 4860 RVA: 0x00077EB4 File Offset: 0x000760B4
    public override string GetDescription() {return "ConsoleCmdBuffSelf";}
    public override string[] GetCommands() {
        return new string[] {"sbuff"};
    }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
        EntityPlayerLocal player = GameManager.Instance.World.GetLocalPlayers()[0]; 
        string buff = _params[0];
        bool force = false;
        int eid = -1;
        if (_params.Count > 1) eid = int.Parse(_params[1]);
        if (_params.Count > 2) force = true;

        if (eid == -1) eid = player.entityId;
        Entity ent = GameManager.Instance.World.GetEntity(eid);
        EntityAlive ea = ent as EntityAlive;
        Printer.Print("ConsoleCmdBuffSelf", _params, buff);
        if (buff.StartsWith("-")) {
            ea.Buffs.RemoveBuff(buff.Substring(1)); // only if Has ?
            return;
        }
        if(force || ! ea.Buffs.HasBuff(buff)) ea.Buffs.AddBuff(buff);
        // NETWORK ?
   }

}