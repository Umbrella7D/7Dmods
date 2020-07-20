using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Linq;
using System.Reflection.Emit;



public class ConsoleCmdShowZChunk : ConsoleCmdAbstract {
   // Token: 0x060012FC RID: 4860 RVA: 0x00077EB4 File Offset: 0x000760B4
   public override string GetDescription() {return "ConsoleCmdExplShowExplo";}
   public override string[] GetCommands() {
       return new string[] {"shzc"};
   }

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
        Printer.Print("ConsoleCmdShowZChunk");
        EntityPlayerLocal player = GameManager.Instance.World.GetLocalPlayers()[0];
   }







}