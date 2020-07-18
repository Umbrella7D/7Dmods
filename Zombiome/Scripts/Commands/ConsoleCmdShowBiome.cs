using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using Harmony;


using System.Linq;
using System.Reflection.Emit;

using CSutils;


public class ConsoleCmdShowBiome : ConsoleCmdAbstract {
   // Token: 0x060012FC RID: 4860 RVA: 0x00077EB4 File Offset: 0x000760B4
   public override string GetDescription() {return "Show Biome";}
   public override string[] GetCommands() {
       return new string[] {"showbiome", "sb"};
   }
    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
        /*
        Subbiomes seem always equal to main main biome
        Subbiomes have no subbiomes
        */
        List<EntityPlayerLocal> players = GameManager.Instance.World.GetLocalPlayers();
        EntityPlayerLocal player = players[0];

        foreach (string b in new string[]{"snow","pine_forest","desert","water","radiated","wasteland"}) {
            BiomeDefinition bd = WorldBiomes.Instance.GetBiome(b); // or World.Biomes.GetBiome(b) !
            ShowBiome(bd);
        }


        if (false) {
            // BiomeDefinition biome = this.world.GetBiome(this.blockPosStandingOn.x, this.blockPosStandingOn.z);
            BiomeDefinition bd = player.biomeStandingOn;
            ShowBiome(bd);

            ShowOthers();
        }
   }



   public static void ShowBiome(BiomeDefinition bd) {
        Printer.Print("=========");
        Printer.Print("Biome", bd, bd.m_Id, bd.m_sBiomeName, bd.m_SpectrumName, bd.TotalLayerDepth, bd.m_TopSoilBlock);
        // snow 1 snow snow 6 terrSnow
        //foreach(KeyValuePair<string,Byte> entry in BiomeDefinition.nameToId) Printer.Print("nameToId:", entry.Key, entry.Value);
        // snow:1
        foreach(BiomeLayer layer in bd.m_Layers) Printer.Print("nameToId:", layer);
        foreach(BiomeBlockDecoration bbd in bd.m_DecoBlocks) {
            //Printer.Print("m_DecoBlocks:", bbd);
            Printer.Print("m_DecoBlocks:", bbd.m_sBlockName, bbd.m_BlockValue, bbd.m_Prob);
        }
        foreach(BiomeBlockDecoration bbd in bd.m_DistantDecoBlocks) Printer.Print("m_DistantDecoBlocks:", bbd.m_sBlockName, bbd.m_BlockValue, bbd.m_Prob);
        foreach(BiomeDefinition layer in bd.subbiomes) Printer.Print("subbiomes:", layer);
        // 6 * BiomeBlockDecoration, 6 * snow, 6 µ BiomeLayer

        BiomeDefinition sub = bd.subbiomes[0];
        Printer.Print("subbiome", sub, sub.m_Id, sub.m_sBiomeName, sub.m_SpectrumName, sub.TotalLayerDepth, sub.m_TopSoilBlock);
        foreach(BiomeDefinition layer in sub.subbiomes) Printer.Print("sub subbiomes: ?", layer);
   }

    public static void ShowOthers() {
        List<EntityPlayerLocal> players = GameManager.Instance.World.GetLocalPlayers();
        EntityPlayerLocal player = players[0];

        World world = GameManager.Instance.World;
        Vector3i where = Vectors.ToInt(player.GetPosition());

        BiomeIntensity bi = new BiomeIntensity();
        world.GetBiomeIntensity(where, out bi);
        Printer.Print("BiomeIntensity:", bi);


        bool osa = world.IsOpenSkyAbove(0, where.x, where.y, where.z);
        Printer.Print("IsOpenSkyAbove:", osa);

        Printer.Print("player (stuck, prevPos, taregtPos):", player, player.IsStuck, player.prevPos, "private");// player.targetPos);
        Printer.Print("player (motion, dwalked, dclimbed):", player, player.motion, player.distanceWalked, player.distanceClimbed);

        ulong wt = world.GetWorldTime();
        int day = GameUtils.WorldTimeToDays(wt);
        int hou = GameUtils.WorldTimeToHours(wt);
        int min = GameUtils.WorldTimeToMinutes(wt);
        Printer.Print("world time", wt, day, hou, min);
   }


}