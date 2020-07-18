using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using Harmony;


using System.Linq;
using System.Reflection.Emit;
using CSutils;

namespace SdtdUtils.Blocks {

public class BlockSetter {
    /**
    Setting block at position is easy, but then you need to manage manually:

    * Entity collision
      Avoid stuck player and 'falling off the world' Zombies, especially when setting multiple blocks at once.
      We push colliding entities up (after unicizing their set), (class EntityMover)

    * Multiblock
      Nothing done yet. Upper part of previous mblock may subsist when moving elastic down

    * Previous block support
      Not 100% sure here, but I have seen trees floating on water. Inserting air seems to be ok

    * Elasticity
      If you want to insert block (not replace), you need push up all blocks above

    * TileEntities
      They are destroyed and re-created on elastic motion
      This is why container lose content. I could at least drop it on ground,
      or store it an re-assign (tedious and bad performance, unless I can assign the list pointer ?)



    FIXME:
    - multiblock, block a travers arbre (push multiblock ne suffit pas ? est ce qu'on pousse le parent ? ...)
    - multipush : use moveto condition (y>target)

    - Manage entity collision: Avoid or Push
      Must be managed, otherwise "zombie felt out of the world", or player stuck (even after "trying to unstick them")

    - Manage blocks!
        - Elastic : Push or attract, elasticursively
                    For y axis only (other axis possible, but less guaranteed vertical support for moved blocks)
        - Avoid
        - TODO: small decoration only, terrain only ...
        - TODO: water
        - TODO: elastic down

    **/


    /* FIXME: I need unicize the above blocks and entities when applying push up, just before any yield 

    Block.HasTileEntity, OnBlockRemoved(), PlaceBlock()

    chunkSync3.StopStabilityCalculation = false;

    TileEntity.public static TileEntity Instantiate(TileEntityType type, Chunk _chunk)


    if (this.isMultiBlock && _blockValue.ischild)
	{
		Vector3i parentPos = this.multiBlockPos.GetParentPos(_blockPos, _blockValue);
		BlockValue block = _world.GetBlock(parentPos);

    */

    // static bool ProtectLCB = true;
    // public static BlockValue GenBlockValue(Block block) {
    //     /// Return a new BlockValue, or the "singleton" Air instance.
    //     if (block == null) return BlockValue.Air;
    //     return new BlockValue((uint) block.blockID);
    // }

    /*

    terrSnow
    terrBrownGrassDiagnoal (decoration !)
    terrBurntForestGround
    terrGravel (sentier)
    terrForestGround
    terrTallGrassDiagonal (c'estt un bloc - empeche de plaser - mais non collidant)
    terrrAsphalt (Diresville), cconcretePlate, concretePillar100
    cntBirdNest
    terrDestroyedStone (wasteland
    treeBurntMaple02
    cinderBlock02
    terrDesertGround
    terrStone (dans le desert, un peu partout...)
    terrDesertShrub (decoration vegatale)
    rockResource
    mushroom01 (grosses pierres, à spawn pour rouler depuis falaise)
    flagstoneBlock
    treeDeadPineLeaf
    driftWood (bois desert)
    orePotassiumNitrateBoulder
    plantedAloe3Harvest
    plantedYucca3Harvest
    treeCactus04 : un petit
    */

    // GameManager.ShowTooltipWithAlert(_data.holdingEntity as EntityPlayerLocal, "You cannot use that at this time.", "ui_denied");


    public static void SetBlockAt(Vector3i where, Block block, OptionEffect options) {
        BlockSetter setter = new BlockSetter(options.OptionBlock);
        Block[] previous = setter.options.blocks;
        setter.options.block = block;
        setter.Apply(where);
        setter.Push();
        setter.options.blocks = previous;
    }

    public class Options {
        public static System.Random Random = new System.Random();
        public Options() {
            blocks = new Block[]{Block.GetBlockByName("air", false)};
        }
        public Options Copy() {
            Options clone = this.MemberwiseClone() as Options;
            clone.blocks = (Block[]) this.blocks.Clone();
            return clone;
        }
        // public Block block;
        public Block block { // need string input ?
            get {
                if (blocks==null) return null; // Should I return Air instead ?
                if (blocks.Length==0) return null; 
                if (blocks.Length==1) return blocks[0];
                return blocks[Random.Next(0,blocks.Length)];
            }
            set {
                blocks = new Block[1]{value};
            }
        }
        public void SetBlocks(String names) {
            String[] pars = names.Split(',');
            this.blocks = pars.Select(name => Block.GetBlockByName(name, false)).ToArray();
        }
        public Block[] blocks; // do I need a dict ?  base:, top: ... // TODO: default to air ?
        public int LCBradius = 20;
        public bool avoidEntity = false;
        public bool avoidBlock = false; // old erase
        public int elastic = 0;
        public Action<Vector3i> CallBack;
        public float RateCallBack = -1f;
    }

    public Options options;
    public Action<BlockValue> OnCreation = null;

    public BlockSetter(Options options=null) : base() {
        this.options = options;
    }

    // public bool avoidSmallDeco = false;
    // private IDictionary<Entity,Integer> entities = new Dictionary<Entity,Integer>();
    // private IDictionary<Tuple<Integer>,Integer> hauteurs = new Dictionary<Entity,Integer>(); // last non air block

    private IDictionary<Entity,int> CollideH = new Dictionary<Entity,int>();

    public int ApplyAndRec(Vector3i where, BlockValue existing) {
        /*
        Vectorized SetBlockRPC (does it avoid receiving messages 1/1 and having an inserted air fumble the above
        before they are replaced ?)
        */
        List<BlockChangeInfo> Changes = new List<BlockChangeInfo>();
        IDictionary<Entity,int> collide = new Dictionary<Entity,int>();
        World world = GameManager.Instance.World;
        Vector3i whereIni = new Vector3i(where.x, where.y, where.z);
        // GameManager.Instance.World.SetBlockRPC(0, where, GenBlockValue(options.block, OnCreation));
        Changes.Add(new BlockChangeInfo(0, where, GenBlockValue(options.block, OnCreation)));
        int k=0;
        for (k=0; k< options.elastic; k++) {
            where = where + Vectors.Up;
            BlockValue next = world.GetBlock(where); // get before set !
            Changes.Add(new BlockChangeInfo(0, where, existing));
            existing = next;
            /// FIXME: not just air ? if not ground dont push ? (wht does set at surface does not go on surface ???)
            if (next.type == BlockValue.Air.type) break;
        }
        world.SetBlocksRPC(Changes);
        // for (int r=0; r<Changes.Count; r++) world.SetBlockRPC(0, Changes[r].pos, Changes[r].blockValue);
        return k;
    }


    public int ApplyDownAndRec(Vector3i where) {
        List<BlockChangeInfo> Changes = new List<BlockChangeInfo>();
        IDictionary<Entity,int> collide = new Dictionary<Entity,int>();
        World world = GameManager.Instance.World;
        if (options.elastic == 0) {
            GameManager.Instance.World.SetBlockRPC(0, where, GenBlockValue(options.block, OnCreation));
            return 0;
        }
        int k=0;
        for (k=0; k< options.elastic; k++) {
            BlockValue next = world.GetBlock(where + Vectors.Up);
            Printer.Log(20, "ApplyDownAndRec {0} -> {1} {2} {3} =>{4}",
                            k, where, next.Block.GetBlockName(), next, (where + Vectors.Down).y);
            Changes.Add(new BlockChangeInfo(0, where, next));
            where = where + Vectors.Up;
            if (next.type == BlockValue.Air.type) {k=k+1; break;} // Breaks => k = k-1 after loop !
        }
        Changes.Add(new BlockChangeInfo(0, where + Vectors.Down, GenBlockValue(options.block, OnCreation)));
        // for (int r=Changes.Count-1; r>=0; r--) world.SetBlockRPC(0, Changes[r].pos, Changes[r].blockValue);
        world.SetBlocksRPC(Changes);
        if (options.block.blockID == BlockValue.Air.type) return 0; // todo: water
        return k;
    }
    private static int LCBradius = 20;
    private static ISet<Entity> NoEntities = new HashSet<Entity>();
    public ISet<Entity> Apply(Vector3i where) {
        // TODO: a 2nd method "Apply(where, block)"
        // entities a cheval will be pushed to the max only (same for blocks)
        World world = GameManager.Instance.World;
        BlockValue existing = world.GetBlock(where);

        if (this.options.avoidBlock && existing.type != BlockValue.Air.type) return NoEntities;
        if (this.options.LCBradius > 0) {
            Vector3i down = new Vector3i(where.x - LCBradius, 0, where.z - LCBradius);
            Vector3i up = new Vector3i(where.x + LCBradius, 254, where.z + LCBradius);
            GameUtils.EPlayerHomeType hy = GameUtils.CheckForAnyPlayerHome(world, down, up);
            if (hy != GameUtils.EPlayerHomeType.None) return NoEntities;
        }
        List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(typeof(Entity),
                                        GetBounds(where, options.block, 0), new List<Entity>());
        if (this.options.avoidEntity && entitiesInBounds.Count > 0) return NoEntities;
        // careful is elastic + avoidEntity, and there are entities above 
        // entities must always be managed : either avoid or push

        // int extraPush = ApplyAndRec(where, existing);
        int extraPush ;
        if (this.options.block.blockID == BlockValue.Air.type) { // TODO or water, fake air ... plus general ? || 
            extraPush = ApplyDownAndRec(where);
            // I should return and not push ? but if I want the set, i should continue and not push
        } else {
            extraPush = ApplyAndRec(where, existing);
        }
        if (extraPush > 0) entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(
                                typeof(Entity), GetBounds(where, options.block, extraPush), new List<Entity>()); // Update

        ISet<Entity> uniques = new HashSet<Entity>(entitiesInBounds);
        foreach (Entity entity in uniques) {
            int previous_h;
            if (this.CollideH.TryGetValue(entity, out previous_h)) {
                this.CollideH[entity] = Math.Max(previous_h, where.y + extraPush + 1);
            } else {
                this.CollideH[entity] = where.y + extraPush + 1;
            }
            // Debug.Log(String.Format(" BLockSetter.CollideH {0} {1} <= {2}", entity, this.CollideH[entity], previous_h));
        }
        if (options.RateCallBack > 0f && Zombiome.rand.RandomFloat< options.RateCallBack) options.CallBack(where);
        return uniques;
    }

    public void Push() {
        foreach(KeyValuePair<Entity,int> item in CollideH) {
            Vector3 dest = item.Key.GetPosition();
            dest = new Vector3(dest.x, dest.y, dest.z);
            dest.y = Math.Max(dest.y, 1f* item.Value + 0.05f);
            EntityMover.Teleport(item.Key, dest, false);
        }
        CollideH.Clear();
    }

    public static BlockValue GenBlockValue(Block block, Action<BlockValue> OnCreation = null) {
        /// Return a new BlockValue, or the "singleton" Air instance.
        if (block == null) return BlockValue.Air;
        BlockValue gen = new BlockValue((uint) block.blockID);
        if (OnCreation != null) OnCreation(gen);
        return gen;
    }
    private static System.Random rand = new System.Random();
    public static void Rotate(BlockValue block) {
        /* Randomy rotate */
        // block.rotation = (byte) ((block.rotation + rand.Next()) % 7);
        block.rotation = (byte) ((block.rotation + rand.Next()) % 24);
    }

    public static Bounds GetBounds(Vector3i where, Block block, int extray = 0, float enlarge=0.05f) {
        /// enlarge > 1 maybe prevents the falling mode to work
        Vector3 delta;
        if (block.isMultiBlock) {
            // BoundsUtils.BoundsForMinMax(float mnx, float mny, float mnz, float mxx, float mxy, float mxz)
            Vector3i mbshape = new Vector3i(1,1,1);
            if (block.multiBlockPos == null) Printer.Print("WARNING: isMultiBlock but null multiBlockPos", block);
            else {
                if (block.multiBlockPos.dim == null) Printer.Print("WARNING: isMultiBlock but null dim", block);
                else mbshape = block.multiBlockPos.dim;
            }
            delta = new Vector3(mbshape.x + enlarge, mbshape.y + enlarge + extray, mbshape.z + enlarge);
        } else {
            delta = new Vector3(1f + enlarge, 1f + enlarge + extray, 1f + enlarge);
            // Bounds bounds =  new Bounds(Vectors..ToFloat(where) + new Vector3(0f,1f,0f), Vector3.one * 2f);
        }
        Bounds bounds = new Bounds();
        if (delta==null) Printer.Print("deta null!");
        bounds.SetMinMax(Vectors.ToFloat(where) - new Vector3(enlarge, enlarge, enlarge), Vectors.ToFloat(where) + delta);
        return bounds;
    }

    // public static Bounds GetBounds(Vector3i where, Block block, int extray = 0, float enlarge=0.05f) {
    //     // probleme en (x,0) ???
    //     Bounds b = new Bounds(Vectors.ToFloat(where), Vectors.Float.One);
    //     try {b = _GetBounds(where, block, extray, enlarge);}
    //     catch(NullReferenceException e) {
    //         Printer.Print("GetBounds null !", where, block, extray, enlarge, block.isMultiBlock);
    //         // throw e;
    //     }
    //     return b;
    // }


}



////////////////////
} // End Namespace
////////////////////