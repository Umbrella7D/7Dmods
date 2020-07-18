using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using Harmony;


using System.Linq;
using System.Reflection.Emit;

using CSutils;

public class ConsoleCmdPositions : ConsoleCmdAbstract {
    // Token: 0x060012FC RID: 4860 RVA: 0x00077EB4 File Offset: 0x000760B4
    public override string GetDescription() {return "ConsoleCmdPositions";}
    public override string[] GetCommands() {
        return new string[] {"sp"};
    }
    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
        Printer.Print("ConsoleCmdPositions");
        // GameManager.Instance.StartCoroutine(TestRouts());
        // testGetBiome(_params);
        //GetZBiome(_params);
        // ShowBlockMap(_params);
        //ShowInventory(_params);
        ShowPos(_params);
    }

    private static void ShowPos(List<string> _params) {
        EntityPlayerLocal player = GameManager.Instance.World.GetLocalPlayers()[0];
        Vector3i pos = Vectors.ToInt(player.GetPosition());
        Vector3i c1 = ZChunk.TL1(pos.x, pos.z);
        Vector3i c4 = ZChunk.TL4(pos);
        Printer.FPrint("Pos {0} -> {1}. C4={2}", pos, c1, c4); 
    }

}