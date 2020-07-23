using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Linq;
using System.Reflection.Emit;

using CSutils;
using SdtdUtils;

public class ConsoleCmdBlockMarker : ConsoleCmdAbstract {
    // Token: 0x060012FC RID: 4860 RVA: 0x00077EB4 File Offset: 0x000760B4
    public override string GetDescription() {return "ConsoleCmdPositions";}
    public override string[] GetCommands() {
        return new string[] {"blockmark", "bm"};
    }
    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
        Printer.Print("ConsoleCmdBlockMarker");   
        int val = int.Parse(_params[1]);
        int h = int.Parse(_params[2]);
        Entity player = GameManager.Instance.World.GetLocalPlayers()[0];
        int read = SdtdUtils.Blocks.BlockSetter.Marker(player.GetPosition(), h, val);
        Printer.Print("Marker", h, val, "=>", read);
    }

}