using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Linq;
using System.Reflection.Emit;



public class ConsoleCmdZombiome : ConsoleCmdAbstract {
    /* TODO: 
    start, stop , pause, force code or position, show

    */
    public override string GetDescription(){return "Manage Zombiome activity";}
    public override string GetHelp(){return
        "zb stop / zb pause / zb start / zb start name / zb log n / zb nz 1-4 / zb intens float";
    }
    public override string[] GetCommands() {
        return new string[] {"zombiome", "zb"};
    }
    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
        // Debug.Log("ConsoleCmdZombiome");
        // foreach (string p in _params) Debug.Log(p);
        // GameManager.Instance.World.aiDirector.GetComponent<AIDirectorWanderingHordeComponent>().SpawnWanderingHorde(false);

        // PersistentPlayerData x =_world.GetGameManager().GetPersistentLocalPlayer()
        List<EntityPlayerLocal> players = GameManager.Instance.World.GetLocalPlayers(); // multiple on console only?
        EntityPlayerLocal player = players[0];
        Vector3 pos = player.GetPosition();

        if (_params[0] == "select" || _params[0] == "sel") {
            ZBActivity.ZombiomeActivitySelector.SelectDebug(_params[1]);
            Zone.ClearCache();
            return;
        }

        if (_params[0] == "log") {// Set log level
            Printer.level = int.Parse(_params[1]);
            return;
        }
        if (_params[0] == "f") { // Debug: force activity
            Zombiome.IgnoreScale = (_params.Count>=2) ? bool.Parse(_params[1]) : !Zombiome.IgnoreScale ;
            return;
        }

        if (_params[0] == "g") { // Debug: select a ghost
            ZBActivity.Entities.Ghost.ghost_type = (_params.Count>=2) ? _params[1] : "";
            return;
        }

        if (_params[0] == "w") { // Debug: select a water block
            ZBiomeInfo.force_water_block =  (_params.Count>=2) ? _params[1] : "";
            return;
        }

        if (_params[0] == "nz") { // Debug: restrict one zone
            int nz = int.Parse(_params[1]);
            if (nz == 1) Zone.Get = position => new Zone[]{Zone.GetSingle(position)};
            if (nz == 4) Zone.Get = Zone.GetFour;
            return;
        }


        if (_params[0] == "start") {
            string res = (_params.Count > 1) ? _params[1] : "";
            ZombiomeManager.Reset(player.entityId, res);
        }
        else if (_params[0] == "stop") ZombiomeManager.Stop(player.entityId, true); // do not use this, it is definitive !
        else if (_params[0] == "pause") ZombiomeManager.Stop(player.entityId, false);
   }


}