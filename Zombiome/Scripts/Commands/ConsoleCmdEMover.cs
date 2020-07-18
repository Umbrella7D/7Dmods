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

public class ConsoleCmdEMover : ConsoleCmdAbstract {
   // Token: 0x060012FC RID: 4860 RVA: 0x00077EB4 File Offset: 0x000760B4
    public override string GetDescription() {return "ConsoleCmdEMover eid check dir";}
    public override string[] GetCommands() {
        return new string[] {"emove"};
    }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
        /*
        Direction not perfect, always moving up, jumping diagonally (issue of not waiting callback ?)

        */
        Entity target = GameManager.Instance.World.GetLocalPlayers()[0];
        if (_params.Count > 0 && _params[0] == "sg") {
            Vector3 pos = target.GetPosition();
            Vector3i ipos = Vectors.ToInt(pos);
            Printer.Print("pos: ", pos, "=>", ipos, "=>", Vectors.ToFloat(ipos));
            Printer.Print("pos: ", ipos, "=>", Vectors.ToFloat(ipos), "=>", Vectors.ToInt(Vectors.ToFloat(ipos)));
            target.SetPosition(pos); // _MoveSetPos only
            return;
            // also check conversion in Emplacement
        }


        int check = 0;
        int eid = -1;
        Vector3 dir = Vectors.Float.UnitY;

        if (_params.Count > 0) eid = int.Parse(_params[0]);
        if (_params.Count > 1) check = int.Parse(_params[1]);
        if (_params.Count > 2) dir = StringParsers.ParseVector3(_params[2]);

        if (eid > 0) target = GameManager.Instance.World.GetEntity(eid);
        if (target == null) {Printer.Print("Entity not there ", eid); return;}

        Printer.Print("ConsoleCmdEMover", eid, check, dir);
        EntityMover mover = new EntityMover(3, 0.2f); // .Config(1);
        mover.checkCollision = check==1;
        mover.Apply(target, dir); // This is a routine... so check not executed ...

   }

}