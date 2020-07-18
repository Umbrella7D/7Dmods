using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using Harmony;


using System.Linq;
using System.Reflection.Emit;

using Iterating;
using CSutils;
using SdtdUtils;

public class ConsoleCmdZBTest : ConsoleCmdAbstract {
    // Token: 0x060012FC RID: 4860 RVA: 0x00077EB4 File Offset: 0x000760B4
    public override string GetDescription() {return "Test ZBiome";}
    public override string[] GetCommands() {
        return new string[] {"zbtest"};
    }
    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
        Printer.Print("ConsoleCmdZBTest");
        // GameManager.Instance.StartCoroutine(TestRouts());
        // testGetBiome(_params);
        //GetZBiome(_params);
        // ShowBlockMap(_params);
        //ShowInventory(_params);
        // ShowSurface(_params);
        //ShowInitialLocalScale(_params);

        // ShowSpeed(_params);
        // TestDamage(_params);
        // TestCoordAlign(_params);
        // TestEveryS(_params);
        //TestWriter(_params);
        Zombiome.Routines.Start(TestMB(_params));
    }

    private static IEnumerator TestMB(List<string> _params) {
        int mpos = int.Parse(_params[0]);
        int mblk = int.Parse(_params[1]);
        int mmid = int.Parse(_params[2]);
        int my = int.Parse(_params[3]);

        EntityPlayerLocal player = GameManager.Instance.World.GetLocalPlayers()[0]; 
        Vector3 pos = player.GetPosition();
        Vector3i ipos = Vectors.ToInt(pos) + 5 * Vectors.North;
        BlockValue block = Block.GetBlockValue("rockResource");
        GameManager.Instance.World.SetBlockRPC(0, ipos, block);

        yield return new WaitForSeconds(1f);

        /*
        scrapIronFrameMaster: ok efface tout (pas de downgraded rockResourceBroke !)
        rockResource : ne marche pas. car mb, ou car identique ?
        chemistryStation: marche bien
        */
        // scrapIronFrameMaster
        Vector3i at = ipos + mpos * Vectors.North;
        string[] blocks = new string[]{"air", "scrapIronFrameMaster", "rockResource", "chemistryStation", "airFake"};

        // both air work. do I need intermediate yield ?
        /* Sans yield, air détuit le block et empeche le nv, alor que airFake fonctionne ...

        */
        if (mmid == -1) {
            List<BlockChangeInfo> changes = new List<BlockChangeInfo>();
            changes.Add(new BlockChangeInfo(0, at, Block.GetBlockValue("airFake")));
            changes.Add(new BlockChangeInfo(0, at, Block.GetBlockValue(blocks[mblk])));
            GameManager.Instance.World.SetBlocksRPC(changes);
        } else {
            if (mmid == 1) {
                GameManager.Instance.World.SetBlockRPC(0, at, BlockValue.Air);
                if (my> 0) yield return new WaitForSeconds(my * 0.1f);
            } else if (mmid == 2) {
                GameManager.Instance.World.SetBlockRPC(0, at, Block.GetBlockValue("airFake"));
                if (my> 0) yield return new WaitForSeconds(my * 0.1f);
            }
            GameManager.Instance.World.SetBlockRPC(0, at, Block.GetBlockValue(blocks[mblk]));
        }
    }

    private static void TestWriter(List<string> _params) {
        // Printer.Write("TestWriter");
        Printer.Print("SGF", GamePrefs.GetString(EnumGamePrefs.SaveGameFolder)); // C:\Users\N4TH\AppData\Roaming/7DaysToDie/Saves
        Printer.Print("USF", GamePrefs.GetString(EnumGamePrefs.UserDataFolder)); // C:\Users\N4TH\AppData\Roaming/7DaysToDie
        string modpth = (Application.platform == RuntimePlatform.OSXPlayer) ? (Application.dataPath + "/../../Mods") : (Application.dataPath + "/../Mods");
        Printer.Print("MF", modpth); // ModManager.MOD_PATH);  //
    }

    private static void TestEveryS(List<string> _params) {
        Zombiome.Routines.Start(TestEveryS_E());
    }
    private static void TestEveryS_C() {
        Printer.Print("EveryS", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
    }
    private static IEnumerator TestEveryS_E() {
        long last = 0;
        for (int k=0; k< 20; k++) {
            Iter.EverySeconds(ref last, 3, TestEveryS_C);
            yield return new WaitForSeconds(1);
        }
    }

    private static void TestCoordAlign(List<string> _params) {
        EntityPlayerLocal player = GameManager.Instance.World.GetLocalPlayers()[0]; 
        Vector3 pos = player.GetPosition();

        Vector3i p0 = Vectors.ToInt(pos);
        Block blk = GameManager.Instance.World.GetBlock(p0).Block;

        Vector3i p1 = Vectors.ToInt(pos) - Vectors.Up;
        Block b1 = GameManager.Instance.World.GetBlock(p1).Block;

        Printer.Print("Player at ", pos);
        Printer.Print("   block ", blk);
        Printer.Print("   below ", b1);
    }
    private static void TestDamage(List<string> _params) {
        EntityPlayerLocal player = GameManager.Instance.World.GetLocalPlayers()[0]; 

        Vector3 pos = player.GetPosition();
        Bounds Bounds = new Bounds(pos, 1f* Vectors.Float.One);
        List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(typeof(Entity),
                Bounds,
                new List<Entity>());
        DamageSource DamageSource= new DamageSource(EnumDamageSource.External, EnumDamageTypes.Crushing);// TODO: DamageSourceEntity ?
        // DamageSource DamageSource= new DamageSourceEntity(EnumDamageSource.External, EnumDamageTypes.Crushing, player.entityId); // direction ? can it be used to push back ?
        foreach(Entity ent in entitiesInBounds) {
            // if (ent.entityId == player.entityId) continue;
            Printer.Print("Damaging", ent);
            ent.DamageEntity(DamageSource, 20, false);
        }
    }

    private static void ShowSpeed(List<string> _params) {
        GameManager.Instance.StartCoroutine(_GoShowSpeed(_params));
    }
    private static IEnumerator _GoShowSpeed(List<string> _params) {
        // fails for player, but what about Z ??
        /*
        Z: always has motion.y = -0.1 (gravity), except when falling, its << 
        P: motion always = 0 
        */ 

        EntityAlive target = GameManager.Instance.World.GetLocalPlayers()[0]; 
        if (_params.Count > 0) {
            int eid = int.Parse(_params[0]);
            target = GameManager.Instance.World.GetEntity(eid) as EntityAlive;
        }

        // EntityPlayerLocal player = GameManager.Instance.World.GetLocalPlayers()[0]; 
        for (int k=0; k<20; k++) {
            // Vector3 dir = player.moveDirection; // relative to o rientation
            // Vector3 rhs = EffectsEntity.MoveDir(player);
            // Printer.Print("ShowSpeed", player, "=> ", dir, " rot=", player.rotation, "=>", rhs);
            Vector3 speed = EffectsEntity.MoveDir(target);
            Printer.Print("ShowSpeed", target, "=> ", speed.magnitude, speed, target.motion, target.motion.magnitude);
            yield return new WaitForSeconds(1f);
        }
        /*
        moveDirection is relative to orientation
        (0,0,1) is forward

        rotation : (x,y,0). 0 maybe not when eg ragdoll, but remain 0 at looking up/down


        groud: water, terrDirt, terrForestGround...
        */
    }

    private static void ShowInitialLocalScale(List<string> _params) {
        EntityPlayerLocal player = GameManager.Instance.World.GetLocalPlayers()[0]; 
        Vector3 ils = EffectsEntity.initialLocalScale(player);
        Printer.Print(player, ils);

        GameManager.Instance.StartCoroutine(EffectsEntity.TestDynamicSizeVis(player));
        if (_params.Count > 0) {
            int eid = int.Parse(_params[0]);
            Entity target = GameManager.Instance.World.GetEntity(eid);
            Vector3 elles = EffectsEntity.initialLocalScale(player);
            Printer.Print(target, elles);
            GameManager.Instance.StartCoroutine(EffectsEntity.TestDynamicSizeVis(target));
        }



    }


    private static void ShowSurface(List<string> _params) {
        EntityPlayerLocal player = GameManager.Instance.World.GetLocalPlayers()[0];
        Vector3i pos = Vectors.ToInt(player.GetPosition());
        Vector3i s;

        if (_params.Count > 0) {
            pos.x = int.Parse(_params[0]);
            pos.y = int.Parse(_params[1]);
            pos.z = int.Parse(_params[2]);
        }

        // int dx = 100;
        // pos.x = pos.x + dx;

        Printer.Print("GetTerrainHeight", GameManager.Instance.World.GetTerrainHeight(pos.x, pos.z));
        s = Geo3D.Surface(pos, -1); Printer.Print("pos", pos, "@", -1, "->", s);
        s = Geo3D.Surface(pos, -2); Printer.Print("pos", pos, "@", -2, "->", s);

        pos.y = pos.y+1;
        s = Geo3D.Surface(pos, -1); Printer.Print("pos", pos, "@", -1, "->", s);
        s = Geo3D.Surface(pos, -2); Printer.Print("pos", pos, "@", -2, "->", s);

        pos.y = pos.y+1;
        s = Geo3D.Surface(pos, -1); Printer.Print("pos", pos, "@", -1, "->", s);
        s = Geo3D.Surface(pos, -2); Printer.Print("pos", pos, "@", -2, "->", s);

        pos.y = pos.y-3;
        s = Geo3D.Surface(pos, -1); Printer.Print("pos", pos, "@", -1, "->", s);
        s = Geo3D.Surface(pos, -2); Printer.Print("pos", pos, "@", -2, "->", s);

        pos.y = pos.y-1;
        s = Geo3D.Surface(pos, -1); Printer.Print("pos", pos, "@", -1, "->", s);
        s = Geo3D.Surface(pos, -2); Printer.Print("pos", pos, "@", -2, "->", s);

        pos.y = 1;
        s = Geo3D.Surface(pos, -1); Printer.Print("pos", pos, "@", -1, "->", s);
        s = Geo3D.Surface(pos, -2); Printer.Print("pos", pos, "@", -2, "->", s);

        pos.y = 0;
        s = Geo3D.Surface(pos, -1); Printer.Print("pos", pos, "@", -1, "->", s);
        s = Geo3D.Surface(pos, -2); Printer.Print("pos", pos, "@", -2, "->", s);
    }

    private static void ShowInventory(List<string> _params) {
        EntityPlayerLocal player = GameManager.Instance.World.GetLocalPlayers()[0];
        Inventory inventory = player.inventory;
        for (int k=0; k<9; k++) Printer.Print("Inventory", k, inventory.GetItemInSlot(k), inventory[k]);
        // CanStack/ TryStackItem / CanStackNoEmpty

        // Bag bag = player.bag;
        // ItemStack adding = new ItemStack(ItemClass.GetItem("resourceSnowBall", false), 1);
        // bag.AddItem(adding);
        for (int k=0; k<5; k++) EffectsInventory.AddToBag(player, "resourceSnowBall", false, 1);

        string item;
        item = "drinkJarYuccaJuice"; Printer.Print(item, EffectsBag.IsGroup(item));
        item = "drinkJarGoldenRodTea"; Printer.Print(item, EffectsBag.IsGroup(item));
        item = "foodEggBoiled"; Printer.Print(item, EffectsBag.IsGroup(item));
    }

    private static void ShowBlockMap(List<string> _params) {
        EntityPlayerLocal player = GameManager.Instance.World.GetLocalPlayers()[0];
        Vector3i pos = Vectors.ToInt(player.GetPosition());
        BlockValue current;

        pos.y = 0;
        current = GameManager.Instance.World.GetBlock(pos);
        Printer.Print(pos, "->", current.Block, current);
        pos.y = 1;
        current = GameManager.Instance.World.GetBlock(pos);
        Printer.Print(pos, "->", current.Block, current);
        pos.y = 254;
        current = GameManager.Instance.World.GetBlock(pos);
        Printer.Print(pos, "->", current.Block, current);
        pos.y = 255;
        current = GameManager.Instance.World.GetBlock(pos);
        Printer.Print(pos, "->", current.Block, current);

        pos.y = 256;
        current = GameManager.Instance.World.GetBlock(pos);
        Printer.Print(pos, "->", current.Block, current);
        pos.y = -1;
        current = GameManager.Instance.World.GetBlock(pos);
        Printer.Print(pos, "->", current.Block, current);
    }

    private static void GetZBiome(List<string> _params) {
        int x = int.Parse(_params[0]);
        int y = int.Parse(_params[1]);
        int nb = -1;
        if (_params.Count>2) nb = int.Parse(_params[2]);
        Zone z = new Zone(x, y);
        z.Log("test");

        EntityPlayerLocal player = GameManager.Instance.World.GetLocalPlayers()[0];
        Printer.Print("GetZBiome update", player, z);
        z.Next(player, player.GetPosition());
    }
    private static void testGetBiome(List<string> _params) {
        /* Using World.ChunkCache.ChunkProvider.GetBiomeProvider().GetBiomeAt
            Instead of World.getbiome */
        int x = int.Parse(_params[0]);
        int y = int.Parse(_params[1]);
        Printer.Print("testGetBiome at", x, y);
        World w = GameManager.Instance.World;
        ChunkCluster cc = w.ChunkCache;
        Printer.Print("testGetBiome ChunkCluster", cc);
        IChunkProvider icp = w.ChunkCache.ChunkProvider;
        Printer.Print("testGetBiome IChunkProvider", icp);
        IBiomeProvider bp = w.ChunkCache.ChunkProvider.GetBiomeProvider();
        Printer.Print("testGetBiome IBiomeProvider", bp);
        BiomeDefinition bd = w.ChunkCache.ChunkProvider.GetBiomeProvider().GetBiomeAt(x,y);
        Printer.Print("testGetBiome BiomeDefinition", bd);
    }

    private static IEnumerator TestRouts() {
        Printer.Print("TestRouts started");
        Routines Group = new Routines();
        for (int p=0; p<5;p++) Group.Start(rout(p));
        for (int k=0; k<10; k++) {
            Printer.Print("Not yet stopping", k);
            yield return new WaitForSeconds(1f);
        }
        Printer.Print("Stopping Group");
        Group.Stop();
    }
    private static IEnumerator rout(int p) {
        for (int k=0; k<10; k++) {
            Printer.Print("rout", p, k);
            yield return new WaitForSeconds(1f);
        }
    }




}