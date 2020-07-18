using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Linq;
using System.Reflection.Emit;
using CSutils;
using Iterating;

using ZBActivity;
using SdtdUtils;

namespace ZBActivity.Deco {

/* IF MB mobes by 1, I think is is the mb-corrected... I need Blockvalue here, and possibly
check mb when dequeuing
unless I use a dict, mblocks are discovered multiple times */
public class BlockPos {
    public BlockValue Value;
    public Vector3i Pos;
    public BlockPos(BlockValue Value, Vector3i Pos) : this() {
        this.Value = Value;
        this.Pos = Pos;
    }
    public BlockPos() {}

    public override string ToString() {return String.Format("{0}<{1}>", Value.Block, Pos);}

    public bool SameBlock(BlockPos other) {
        Block tblock = this.Value.Block;
        Block oblock = other.Value.Block;

        if (! tblock.isMultiBlock) return false;
        if (! oblock.isMultiBlock) return false;
        return tblock.multiBlockPos.GetParentPos(this.Pos, this.Value)
                == oblock.multiBlockPos.GetParentPos(other.Pos, other.Value);
    }
    public BlockPos Parent() { // in place, requires validation first ?
        if (! Value.ischild) return this;
        // if (! Value.Block.isMultiBlock) return this;
        int current_type = Value.Block.blockID;
        Vector3i ppos = Value.Block.multiBlockPos.GetParentPos(Pos, Value);
        BlockValue pexisting = GameManager.Instance.World.GetBlock(ppos);
        if (pexisting.Block.blockID != current_type) {
            Printer.Log(85, "Parent: changed blockID !", Pos, Value.Block,Value, "=>", ppos, pexisting, pexisting.Block);
            // throw new Exception();
            return null;
        }
        if (pexisting.ischild) {
            Printer.Log(85, "Parent is child !", Pos, Value.Block,Value, "=>", ppos, pexisting, pexisting.Block);
            return null;
        }
        Value = pexisting;
        Pos = ppos;
        return this;
    }
}


public class DecoSearch {
    /* Lets store Vector3i only, content at pos may have changed before dequeue

    I am creating duplicates (even on 1XyX1 block), due to asynchron:
    - not destroy previous ?
    - from/to added and in between callback (async may cause duplicates)
            -> add a min timer before dequeue (make sure callback exec)

    cela dit c marrant aussi ... mais risque de faire bg le jeu
    */
    public Func<Block,bool> Selector = null;
    private Geo3D.SurfaceNeighbourhood Searcher = new Geo3D.SurfaceNeighbourhood(new Vector3i());
    public Queue<Vector3i> Positions = new Queue<Vector3i>();
    public int radius = 30;
    private int maxSize = 50;
    public DecoSearch() {}
    public DecoSearch(Func<Block,bool> Selector) : this() {
        this.Selector = Selector;
        Searcher.IsGround = Geo3D.IsGroundOrBuilding;
    }
    public Vector3i center = new Vector3i();

    public int Count{get{return Positions.Count;}}
    public void Enqueue(Vector3i pos) {
        if (Positions.Count >= maxSize) return;
        if (Vectors.D1(pos, center) > radius) return;
        if (Positions.Count >= maxSize / 2 
                && Vectors.D1(pos, center) > radius / 3) return;
        Positions.Enqueue(pos);
    }
    public BlockPos Dequeue() {
        if (Positions.Count == 0) return null;
        Vector3i pos = Positions.Dequeue();
        BlockValue existing = GameManager.Instance.World.GetBlock(pos);
        if (Selector != null && !Selector(existing.Block)) return null;
        return new BlockPos(existing, pos).Parent();
    }


    // // Search
    private long _printDistLast;
    private bool[] LockSearch = new bool[]{false};
    private static YieldInstruction SearchYield = new WaitForSeconds(0.1f);
    public void Search(EntityPlayer player, int cycle=1, int repeat=1) {
        center = Vectors.ToInt(player.GetPosition());
        Zombiome.Routines.Start(Routines.IfNotRunning(LockSearch, _Search(player, 20, 10)), "MovingDeco.Search");
    }
    private void _ResetTooFar(EntityPlayer player) {
        Vector3i ppos = Vectors.ToInt(player.GetPosition());
        int pdist = Math.Abs(ppos.x - Searcher.x) + Math.Abs(ppos.z - Searcher.y);
        // Iter.EverySeconds(ref _printDistLast, 10, (_x) => Printer.Print(_x), "Search pdist=" + pdist.ToString());
        if (pdist > radius) {
            Printer.Log(85, "Searcher.Reset !", ppos, Searcher.x, Searcher.y);
            Searcher.Reset(ppos.x, ppos.z, ppos.y);
        }
    }
    private IEnumerator _Search(EntityPlayer player, int cycle=1, int repeat=1) {
        // TODO: check player motion since last reset !
        World World = GameManager.Instance.World;
        for (int k=0; k<cycle; k++) {
            yield return SearchYield;
            _ResetTooFar(player);
            for (int q=0; q<repeat; q++) {
                Searcher.Next();
                if (! Searcher.ok) {Printer.Log(85, "Searcher not ok !", Searcher.x, Searcher.y); yield break;}
                BlockValue existing = World.GetBlock(Searcher.Position);
                if (existing.ischild) continue; // inserts a single will help !
                if (Positions.Count >= maxSize) continue;
                if (Selector != null && !Selector(existing.Block)) continue;
                Vector3i pos = Vectors.Copy(Searcher.Position);
                Printer.Log(85, "Found decoration:", existing.Block, pos, existing);
                Positions.Enqueue(pos);
            }
        }
    }


    /** Look around player and find decoration

    Make sure: only a parent gets set by SetBlockRPC
    */
    public static bool CanSwap(Block block) {
        // return true; // destructive and fun
        // return block.blockID == BlockValue.Air.Block.blockID; // air only
        if (block.blockID == BlockValue.Air.Block.blockID) return true; 
        return ! block.isMultiBlock; 
        // return (block.IsDecoration || block.IsTerrainDecoration) && ! block.isMultiBlock; // small decos
    }
    public static bool CanSwap(Block block, ref string blockers) {
        bool res = CanSwap(block);
        if (! res) blockers = String.Format("{0},{1}", blockers, block);
        return res;
    }
    public static bool CanSwap(BlockPos block, ref string blockers) {
        bool res = CanSwap(block.Value.Block);
        if (! res) blockers = String.Format("{0},{1}", blockers, block);
        return res;
    }
    public static bool IsDeco(Block block) {
        if (block.blockID == BlockValue.Air.Block.blockID) return false; 
        return (block.IsDecoration || block.IsTerrainDecoration) && block.isMultiBlock;
    }
    public static bool IsDeco(Block block, bool usemb=true) {
        if (block.blockID == BlockValue.Air.Block.blockID) return false; 
        return (block.IsDecoration || block.IsTerrainDecoration) && (block.isMultiBlock || ! usemb);
    }

}

public class MovingDeco : MultiEffect {
    /** Instead of multiple moves at once, use routines ? */
    /* DONE enqueue small deco if size < tresh


    si src != dest, le multiblock a pas l'air de poser pb ...

    when src==dest:
    - replace does nothing (game prolly check src==dest on block ids)
    - air, yield, replace works
    - air,replace does not (breaks the block and prevents the override), but airFake,replace does the trick

    en terme de stabilité, il faut au moins 2 block porteurs, pas forcément le centre ...
    */

    private DecoSearch DecoSearch = new DecoSearch(DecoSearch.IsDeco);
    public MovingDeco(Zone zone) : base(zone) {
        EffectType = EffectType.Ground;
        BiomeDefinition biomeDef = Zone.GetBiomeProvider(centerx, centerz);
        Printer.Print("MovingDeco init");
        DecoSearch.Selector = block => DecoSearch.IsDeco(block, DecoSearch.Count > 5); // take small deco while size < 5
    }

    private bool[] LockRegen = new bool[]{false};
    public override void Effect1(EntityPlayer player, Emplacement place,  OptionEffect opt) {
        Zombiome.Routines.Start(
            Routines.IfNotRunning(LockRegen, Regen(player)),
            "MovingDeco.Regen"
        );
    }
    private static YieldInstruction YieldRegen = new WaitForSeconds(0.1f);
    // public override IEnumerator Regen(EntityPlayer player, Bounds bounds, int iniguess) {
    //     return Regen(player);
    // }
    public IEnumerator Regen(EntityPlayer player) {
        /** Consumes Decorations, check distance */
        Printer.Log(85, "MovingDeco:", DecoSearch.Count, " Decorations");
        World World = GameManager.Instance.World;
        DecoSearch.Search(player, 20, 3);
        long now = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        /** Size before new pos are added: let time for callback */
        int nsteps = Math.Min(10, DecoSearch.Count);
        for (int k=0; k<nsteps; k++) {
            yield return YieldRegen;
            BlockPos deco = DecoSearch.Dequeue();
            if (deco == null) continue;
            Vector3i ppos = Vectors.ToInt(player.GetPosition());
            if (Math.Abs(ppos.x - deco.Pos.x) + Math.Abs(ppos.z - deco.Pos.z) > DecoSearch.radius){
                Printer.Log(85, "MovingDeco: too far !", deco, ppos);
                continue;
            }
            TryMove(deco);
        }

    }

    private bool SeeToward(BlockPos deco, BlockPos Target, int idir) {
        /** -> isSame */
        Vector3i dir = Geo3D.SurfaceNeighbourhood.adjacent[idir];
        Target.Pos = Geo3D.Surface(deco.Pos + dir) + Vectors.UnitY;
        Target.Value = World.GetBlock(Target.Pos);
        Target.Parent(); // should not be needed, but CanSwap does
        // return deco.SameBlock(Target);
        if (! deco.SameBlock(Target)) return false;
        Target.Pos = Geo3D.Surface(deco.Pos + dir) + Vectors.UnitY; // same block, dont point to the same parent, or not motion...
        Target.Value = BlockValue.Air;
        return true;
    }
    private void TryMove(BlockPos deco) {
        int ndirs = Geo3D.SurfaceNeighbourhood.adjacent.Length;
        int baseDir = Hashes.Rand(0,ndirs, "moveableDir", deco.Value.Block.blockID.ToString()) % ndirs;
        Vector3i dir = Geo3D.SurfaceNeighbourhood.adjacent[baseDir];
        bool sameBlock = false; 
        BlockPos Target = new BlockPos();

        string blockers = "";
        sameBlock = SeeToward(deco, Target, baseDir);
        if (!sameBlock && ! DecoSearch.CanSwap(Target, ref blockers)) // Valid <=> same OR canswap
                sameBlock = SeeToward(deco, Target, (baseDir + 1) % ndirs);
        if (!sameBlock && ! DecoSearch.CanSwap(Target, ref blockers))
                sameBlock = SeeToward(deco, Target, (baseDir - 1 + ndirs) % ndirs); // "+ ndirs" => remains > 0 before %
        if (!sameBlock && ! DecoSearch.CanSwap(Target, ref blockers)) {
            Printer.Log(85, "Block cant move: ", deco, blockers);
            DecoSearch.Enqueue(deco.Pos); // try again in the future, and dont lose track !
            return;
        }
        Target.Parent(); 
        SwapNow(deco, Target);
    }
    private void SwapNow(BlockPos from, BlockPos to) {
        /** Assumes validated args */
        // FIXME: very careful with multiblock (eg to=from)
        // -> always check parent ! 
        /** Mulitdim: setblock air => Block.OnBlockRemoved
        - if parent, kill childs
        - if child, kill parent

        maybe try insert a new BlockValue ?

        it it "to" or to.parent => yes, SeeToward goes to parent.
        */
        bool issame = from.SameBlock(to);
        BlockValue restored = issame ? Block.GetBlockValue("airFake") : new BlockValue((uint) to.Value.Block.blockID); // to.Value;
        restored.damage = to.Value.damage;
        BlockValue ins = new BlockValue((uint) from.Value.Block.blockID);
        ins.damage = from.Value.damage;
        // si ce n'est pas le mm, ca peut toujours etre le mm block id ...
        Printer.Log(85, "Swap", from.Value.Block, "->", to.Value.Block, issame, "i=", ins, "r=", restored.Block);
        List<BlockChangeInfo> changes = new List<BlockChangeInfo>();
        if (true) {
            if (!issame) // creates parent problem by not deleting ?
            // if (restored.Block.blockID != BlockValue.Air.Block.blockID)
                // GameManager.Instance.World.SetBlockRPC(0, from.Pos, restored); // try without air
                changes.Add(new BlockChangeInfo(0, from.Pos, Block.GetBlockValue("airFake")));
                changes.Add(new BlockChangeInfo(0, from.Pos, restored));
            // GameManager.Instance.World.SetBlockRPC(0, to.Pos, from.Value);
            // GameManager.Instance.World.SetBlockRPC(0, to.Pos, ins); 
            // else // NB: etre le meme est peut etre mal detecté, et si c le mm blockid ca gene aussi ...
            // if (to.Value.Block.blockID == ins.Block.blockID) changes.Add(new BlockChangeInfo(0, to.Pos, Block.GetBlockValue("airFake")));
            changes.Add(new BlockChangeInfo(0, to.Pos, Block.GetBlockValue("airFake")));
            changes.Add(new BlockChangeInfo(0, to.Pos, ins));
        } else {
            bool sameType = to.Value.Block.blockID == from.Value.Block.blockID;
            if (sameType) changes.Add(new BlockChangeInfo(0, from.Pos, Block.GetBlockValue("airFake")));
            changes.Add(new BlockChangeInfo(0, from.Pos, restored));
            if (sameType) changes.Add(new BlockChangeInfo(0, to.Pos, Block.GetBlockValue("airFake")));
            changes.Add(new BlockChangeInfo(0, to.Pos, ins));
        }
        GameManager.Instance.World.SetBlocksRPC(changes);
        // DecoSearch.Enqueue(from.Pos); // only if there was deco ?
        DecoSearch.Enqueue(to.Pos); // forcing a delay since enqueue time might be a gd idea ?
        // if (bto.Block.IsTerrainDecoration && bto.Block.isMultiBlock)
        //         Decorations.Enqueue(new BlockPos(bto.Block, from.Pos));
    }


    public override void Configure() {
        opt.OptionShape.pace = Hashes.Rand(new float[]{0.5f, 1f, 3f, 5f}, seed,"pace"); // slower bc reverse
        opt.OptionBlock.avoidBlock = true;
        opt.OptionBlock.avoidEntity = true;
        opt.OptionBlock.elastic = 0;
        Printer.Log(60, "MovingDeco Configure: Physics done");
    }
    protected override void Randomize(OptionEffect opt) {
        opt.OptionShape.pace = opt.OptionShape.pace + this.rand.RandomRange(0f, 1f);
    }
    public override void ConfigurePlacesRepeat() {
        base.ConfigurePlacesRepeat();
        Cycler = new Repeater(3).Set(0.5f);
        Repeater = new Repeater(1);
    }
}



public class FlyDeco : MultiEffect {
    /* TODO enqueue small deco if size < tresh
    how to speedup search ? 1 local + 1 far ?
    Could speed up by linking search to action

    */

    /*
    ZBProj_treeWinterPine28m
    ZBProjGrass ? fails
    ZBProj_treeMountainPine12m
    ZBProj_treePlainsTree
    ZBProj_treePlantedOak41m
    ZBProj_treeOakSml01

    ZBProj_treeDeadTree01
    ZBProj_driftwood
    ZBProj_treeCactus03 : make 2 sizes
    ZBProj_treeShrub :souche

    */

    /* See ItemActionProjectile, but I am relying on dropped item ! action is only here for debug

    */

    public static bool IsDeco(Block block) {
        if (block.blockID.Equals(BlockValue.Air)) return false;
        if (! block.IsDecoration && ! block.IsTerrainDecoration) return false;
        return block.isMultiBlock || block.GetBlockName().Equals("treeStump");
    }
    public static string Deco2Proj(Block block) {
        /** See treeSmallRandomHelper

        treeJuniper: small sapin/pin
        */
        int y = (block.isMultiBlock) ? block.multiBlockPos.dim.y : 1;
        string name = block.GetBlockName();
        // Big trees
        if (name.StartsWith("treeWinterPine")) return y < 20 ? "ZBProj_treeWinterPine13m" : "ZBProj_treeWinterPine28m";
        if (name.StartsWith("treeDead")) return "ZBProj_treeDeadTree01"; // treeDeadPineLeaf
        if (name.StartsWith("treeOak")) return y < 20 ? "ZBProj_treeOakSml01" : "ZBProj_treeOakLrg01"; // ZBProj_treePlantedOak41m
        if (name.StartsWith("treePlantedOak")) return "ZBProj_treePlantedOak41m"; // celui la est enorme...
        if (name.StartsWith("treeMountain")) return "ZBProj_treeMountainPine12m"; //and PineDry
        if (name.StartsWith("treeBurntMaple")) return y <=5 ? "ZBProj_treeBurntMaple01" : "ZBProj_treeBurntMaple03";
        // Boulder
        if (name.StartsWith("rockResource")) return "ZBProj_rockResource";
        // Small trees
        if (name.StartsWith("treeCactus")) return "ZBProj_treeCactus03";
        if (name.StartsWith("treeStump")) return "ZBProj_treeShrub";  // souche
        if (name.StartsWith("driftwood")) return "ZBProj_driftwood"; // projectile looks like nothing

        if (name.StartsWith("treePlains")) return "ZBProj_treePlainsTree";  // bosquet

        Printer.Log(85, "Deco2Proj, unmatched ", name);
        return "";
    }

    private DecoSearch DecoSearch = new DecoSearch(FlyDeco.IsDeco);
    public FlyDeco(Zone zone) : base(zone) {
        EffectType = EffectType.Ground;
        Printer.Print("FlyDeco init");
    }
    private static YieldInstruction WaitRepop = new WaitForSeconds(5f);
    public override void Effect1(EntityPlayer player, Emplacement place,  OptionEffect opt) {
        DecoSearch.Search(player, 20, 3);

        int nsteps = Math.Min(3, DecoSearch.Count);
        for (int k=0; k<nsteps; k++) {
            BlockPos deco = DecoSearch.Dequeue();
            if (deco == null) continue;
            Vector3i ppos = Vectors.ToInt(player.GetPosition());
            if (Math.Abs(ppos.x - deco.Pos.x) + Math.Abs(ppos.z - deco.Pos.z) > DecoSearch.radius){
                Printer.Log(85, "MovingDeco: too far !", deco, ppos);
                continue;
            }
            string item = Deco2Proj(deco.Value.Block);
            if (item == "") continue;
            Zombiome.Routines.Start(Fly(deco, item), "FlyDeco");
        }
    }

    private static YieldInstruction WaitFrame = new WaitForEndOfFrame();
    private static YieldInstruction afterParticle = new WaitForSeconds(0.5f);
    public IEnumerator Fly(BlockPos pos, string item) {
        int my = (pos.Value.Block.isMultiBlock) ? pos.Value.Block.multiBlockPos.dim.y : 1;
        // GameManager.Instance.World.SetBlockRPC(pos.Pos, BlockValue.Air);
        // yield return WaitFrame;
        int wgt = ItemClass.GetItem(item, false).ItemClass.Weight.Value; // TODO: quicker access string->ItemValue->ItemClass
        Vector3 dir = Upward(this.rand, wgt);
        float offset = (1f + my/2f -1f);
        offset = offset = Math.Max(offset, 2);
        if (item.Equals("ZBProj_treeShrub")) offset = offset = Math.Max(offset, 3);

        SdtdUtils.EffectsItem.SpawnParticle(Vectors.ToFloat(pos.Pos), "wire_tool_sparks", biome.groundColor, "electric_fence_impact");
        yield return afterParticle;

        yield return Zombiome.Routines.Start(EffectsItem.spawnItemGhost(
            item,
            Vectors.ToFloat(pos.Pos) + offset * Vectors.Float.UnitY,
            dir// throw dir 
        ), "FlyDeco-Item");
        // yield return WaitFrame; // o ther way around, byt careful not to spawn on existing (ghost proj will delay it though)
        GameManager.Instance.World.SetBlockRPC(pos.Pos, BlockValue.Air);
        // Do not repop because the projectile does at impact !
    }


    private static Vector3 Upward(GameRandom rand, int wgt) {
        // gen is 1.1 Up + Rdm 1
        Vector3 dir = Vectors.Float.Randomize(rand, 0.5f, Vectors.Float.UnitY);
        dir = dir.normalized;
        // dir.y = (0.5f + Math.Abs(dir.y)) / 2;
        dir = dir.normalized;
        // dir = dir * wgt * 0.5f;    // mass dependant !  still make a bit random and wgt dependant ?
        dir = dir * 10;
        // dir = dir * (float) Math.Sqrt(wgt); 
        dir = dir * 2.5f; // too small for tree (m=20-50), too large for boulder (m=100) ?
        dir = dir * (1f + rand.RandomFloat);
        return dir;
    }


    public override void Configure() {
        Printer.Log(60, "FlyDeco Configure: Physics done");
    }
    protected override void Randomize(OptionEffect opt) {
    }
    public override void ConfigurePlacesRepeat() {
        base.ConfigurePlacesRepeat();
        Cycler = new Repeater(3).Set(0.5f);
        Repeater = new Repeater(2);
    }
}



// // 
}  // end namespace
// //
