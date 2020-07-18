using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using Harmony;


using System.Linq;
using System.Reflection.Emit;

using CSutils;

/*

Est (>0), Hauteur (>0), Nord (>0)

PLayer + 2N seems to move 1 block
due to splint -> no ??
it seems +3N give 1 block ... pareil en hauteur ...

Z:
+2y is 1,5 block ... (slightly more)
+3y is 2.3 block

FIXME: lateral shift is an issue of V3 <=> V3i ?

Why Z requires a slight >y to move horizontally ?

*/

namespace SdtdUtils {

public class EntityMover {
    // FIXME : CheckEntityCollisionWithBlocks NPE (seen once)
    /// TODO: clean command test
    // TODO: use GameManager.Instance.World.CheckEntityCollisionWithBlocks(target);
    /** 
    NB: block setter uses teleport, I need a condition in teleport

    TODO:
    - check collision. check next block is easy, but what if speed goes beyond ?
        => check multiple, can we forecast the next pos when setting speed ?

    -- how to check the multiple block (of the entity bounding rect) ?

    - EntityVehicle or driveable do not fall (gravity) after moving

    - Allow for condition (vector3->bool) in move (eg until y> 3)
    **/ 
    // Velocity corresponds to twice more block per second, so I divide to align with SetPos
    // TODO: adjust correction with interleave ...

    /* may unstick?

    either private methd with refl, either copy past and ignore controler !

    NB: ca affect motion pour corriger ! ca ne marchera pas tel quel

    this.IsStuck = this.pushOutOfBlocks(this.position.x - base.width * 0.3f, this.boundingBox.min.y + 0.5f, this.position.z + base.depth * 0.3f);
			this.IsStuck = (this.pushOutOfBlocks(this.position.x - base.width * 0.3f, this.boundingBox.min.y + 0.5f, this.position.z - base.depth * 0.3f) || this.IsStuck);
			this.IsStuck = (this.pushOutOfBlocks(this.position.x + base.width * 0.3f, this.boundingBox.min.y + 0.5f, this.position.z - base.depth * 0.3f) || this.IsStuck);
			this.IsStuck = (this.pushOutOfBlocks(this.position.x + base.width * 0.3f, this.boundingBox.min.y + 0.5f, this.position.z + base.depth * 0.3f) || this.IsStuck);

    */

    public static class SlowFall {
        private static float[] SlowParams = new float[]{-0.2f, 0.2f, 0f}; // motiony cond, speed, interleave
        // -0.5f, 0.3f, 0f is slowed down, but still fast
        // -0.3f, 0.3f, 0f is slower and nice, still paced
        // -0.2f, 0.2f, 0f is even slower
        public static int InAir(Entity ent) { // called with entity.GetPosition
            // Looks like motion is the external motion (gravity, push, explosion), not the "action"
            Vector3i where = Vectors.ToInt(ent.GetPosition());
            if( GameManager.Instance.World.GetBlock(where).type != BlockValue.Air.type) return 2;
            if( GameManager.Instance.World.GetBlock(where - Vectors.UnitY).type != BlockValue.Air.type) return 2;
            // if (ent.motion.y >= SlowParams[0]) return 1;
            if (ent.motion.y >= -0.3) return 1;
            return 0;
        }
        public static IEnumerator Apply(Entity entity, float slowby, int interleave) {
            // TODO: a buff ? but the buff needs be responsible for iteration ?
            // it would not flatten nested returne iterators
            // let just the buff start a routine ? How to finish it on entity death or landing ?
            EntityMover mover = new EntityMover(100000, slowby, interleave); // randomize + haut
            mover.StopCond = InAir;
            return mover.Move(entity, Vectors.Float.UnitY);
        }
        public static Coroutine Start(Entity entity, float slowby, int interleave) {
            // return GameManager.Instance.StartCoroutine(Apply(entity, slowby, interleave));
            return Zombiome.Routines.Start(Apply(entity, slowby, interleave), "SlowFall");
        }
    }

    private static bool UseTP(Entity entity) {
        /* Select implementation

        Different entity classes require different implementations:
        - Players, Vehicle, Item : use _MoveSetPos
          Can only be moved via teleport (absolute position, because of controler)
        - Zombie, Animals : use _MoveVelocity
          Can only be moved via speed (relative position, because of IA) 

        FIXME: vehicules dont fall after motion, I need apply gravity myself
        */
        if (entity as EntityPlayer != null) return true;
        if (entity as EntityVehicle != null) return true;
        if (entity as EntityItem != null) return true;
        return false;
    }

    private static float speedNormalizer = 2f;
    public static Vector3 direction(Vector3 from, Vector3 target) {
        return target - from;
    }

    /* StopCond -> 0 (go) , 1 (pause/skip iteration), 2 (break, true stop) */
    public Func<Entity,int> StopCond = null; // v => false;
    public int OnCollide = 0; // 0:do nothing, 1:stop, +: damage, rebound, move obstacle ...
    public bool checkCollision = true; // 0:do nothing, 1:stop, +: damage, rebound, move obstacle ...
    // CHECK: should it be done before or after callback ?
    int interleave = 0;
    float speed; // Actual speed is speed * direction.norm, per frame or second
    // float sduration = -1; // in second
    int fduration = -1; // in frame



    public EntityMover(int fduration=1, float speed = 1f, int interleave = 0) {
        this.fduration = fduration;
        this.speed = speed;
        this.interleave = interleave;
    }
    public EntityMover Limits(int OnCollide = 0, Func<Entity,int> StopCond = null) {
        this.OnCollide = OnCollide; // 0 do nothing, 1: stop motion, 2: normalize and continue
        this.StopCond = StopCond;
        return this;
    }

    private static BlockFace FaceFrom(Vector3 dir) {
        // coming with direction dir => reverse
        int imx = 0;
        if (Math.Abs(dir.y) >= Math.Max(Math.Abs(dir.x), Math.Abs(dir.z))) imx = 1;
        if (Math.Abs(dir.z) >= Math.Max(Math.Abs(dir.x), Math.Abs(dir.y))) imx = 2;
        if (imx == 0) return (dir.x >= 0) ? BlockFace.Bottom : BlockFace.Top;
        if (imx == 0) return (dir.x >= 0) ? BlockFace.West : BlockFace.East;
        if (imx == 0) return (dir.x >= 0) ? BlockFace.South : BlockFace.North;
        return BlockFace.None;
    }
    private bool WillCollide(Entity entity, Vector3 toward) {
        // I should return block or entity ?
        // TODO: I should check the whole body boundingRect
        // only input position, not entity ?
        World world = GameManager.Instance.World;
        Vector3 pos = entity.GetPosition() + toward;
        Vector3i vector3i = World.worldToBlockPos(pos);
        BlockValue block = world.GetBlock(vector3i);
        int type = block.type;
        if (Block.list[type].IsMovementBlocked(world, vector3i, block, FaceFrom(toward))) {
            // Debug.Log("WillCollide !");
            return true;
            // EntityplayerLocal.pushOutOfBlocks()
        }
        return false;

    }

    private static YieldInstruction WaitFrame = new WaitForEndOfFrame();
    private IEnumerator _MoveVelocity(Entity entity, Vector3 toward) {
        speed = this.speed / speedNormalizer; /// Correctif
        toward = toward * this.speed/ (1 + this.interleave); // toward = toward.normalized * speed;
        for (int f=0; f< this.fduration; f++) {
            if (null == entity) yield break;
            if (this.OnCollide == 1 && this.WillCollide(entity, toward)) yield break;
            if (this.StopCond != null) {
                int stopped = this.StopCond(entity);
                if (stopped==2) yield break;
                if (stopped==1) {yield return WaitFrame; continue;}
            }
            // if (this.StopCond != null && this.StopCond(entity.GetPosition()) ) yield break;
            GameManager.Instance.AddVelocityToEntityServer​(entity.entityId, toward);
            if (checkCollision) GameManager.Instance.World.CheckEntityCollisionWithBlocks(entity); // todo only if collision
            yield return WaitFrame;
            if (checkCollision) GameManager.Instance.World.CheckEntityCollisionWithBlocks(entity);
            for (int k=0; k< this.interleave; k++) {
                yield return WaitFrame;
                if (checkCollision) GameManager.Instance.World.CheckEntityCollisionWithBlocks(entity);
            }
        }
        if (checkCollision) GameManager.Instance.World.CheckEntityCollisionWithBlocks(entity);
    }

    private IEnumerator _MoveSetPos(Entity entity, Vector3 toward) {
        toward = toward * this.speed / (1+ this.interleave);
        for (int f=0; f< this.fduration; f++) {
            if (null == entity) yield break;
            if (this.OnCollide == 1 && this.WillCollide(entity, toward)) yield break;
            // if (this.StopCond != null && this.StopCond(entity.GetPosition()) ) yield break;
            if (this.StopCond != null) {
                int stopped = this.StopCond(entity);
                if (stopped==2) yield break;
                if (stopped==1) {yield return WaitFrame; continue;}
            }
            entity.SetPosition(entity.GetPosition() + toward, true); // TODO test true
            if (checkCollision) GameManager.Instance.World.CheckEntityCollisionWithBlocks(entity); // todo only if collision
            yield return WaitFrame;
            if (checkCollision) GameManager.Instance.World.CheckEntityCollisionWithBlocks(entity);
            for (int k=0; k< this.interleave; k++) {
                yield return WaitFrame;
                if (checkCollision) GameManager.Instance.World.CheckEntityCollisionWithBlocks(entity);
            }
            if (checkCollision) GameManager.Instance.World.CheckEntityCollisionWithBlocks(entity);
        }
    }

/*     private IEnumerator _MoveVelocitySecond(Entity entity, Vector3 toward) {
        /// duration is in second
        /// speed is block per frame
        speed = this.speed / speedNormalizer; /// Correctif
        toward = toward * this.speed / FPS / (1 + this.interleave); // toward = toward.normalized * speed;
        DateTime t0 = DateTime.UtcNow;
        while(DateTime.UtcNow - t0 < TimeSpan.FromSeconds(this.fduration)) {
            GameManager.Instance.AddVelocityToEntityServer​(entity.entityId, toward) ;
            yield return new WaitForEndOfFrame();
            for (int k=0; k<this.interleave; k++) yield return new WaitForEndOfFrame();
        }
    }
    private IEnumerator _MoveSetPosSecond(Entity entity, Vector3 toward) {
        /// duration is in second
        /// speed is block per frame
        DateTime t0 = DateTime.UtcNow;
        toward = toward * this.speed / FPS / (1+ this.interleave);
        while(DateTime.UtcNow - t0 < TimeSpan.FromSeconds(this.fduration)) {
            entity.SetPosition(entity.GetPosition() + toward, true); // TODO test true
            yield return new WaitForEndOfFrame();
            for (int k=0; k< this.interleave; k++) yield return new WaitForEndOfFrame();
        }
    }

    public static IEnumerator Move(Entity entity, Vector3 toward, float speed=1f, float duration=1f, int interleave=0) {
        if (UseTP(entity)) return _MoveSetPos(entity, toward, speed, duration, interleave);
        else return _MoveVelocity(entity, toward, speed, duration, interleave);
    }
    */


    public IEnumerator Move(Entity entity, Vector3 toward) {
        if (UseTP(entity)) return this._MoveSetPos(entity, toward);
        else return this._MoveVelocity(entity, toward);
    }
    public Coroutine Apply(Entity entity, Vector3 toward) {
        return Zombiome.Routines.Start(this.Move(entity, toward), "EntityMover");
    }


    public static void Teleport(Entity entity, Vector3 at, bool relative=false) {
        /// FIXME: pour Z, ca saute bcp plus !
        bool tp = UseTP(entity);
        Vector3 current = entity.GetPosition();
        Vector3 dest = relative ? (current + at) : at;
        if (tp) entity.SetPosition(dest, false); // TODO test false
        else {
            float speed = 1f / speedNormalizer; /// Correctif
            Vector3 delta = dest - current;
            // Debug.Log(String.Format("TeleportVelo: {0} {1} delta={2} at={3}", entity.entityId, entity.name, delta, at));
            GameManager.Instance.AddVelocityToEntityServer​(entity.entityId, delta * speed); // for 1 frame
        }
    }


}


public class DetectIntersect {
    // Ray lookRay = player.GetLookRay();
    // if (Voxel.Raycast(GameManager.Instance.World, lookRay, 5000f, 65536, 64, 0f)) return Voxel.voxelRayHitInfo.hit.pos;
    // else return player.position;

    // new Ray(this.position + new Vector3(0f, this.GetEyeHeight(), 0f), this.GetLookVector());
    // Vector3i OneVoxelStep(Vector3i _voxelPos, Vector3 _origin, Vector3 _direction, out Vector3 hitPos, out BlockFace blockFace)
}

////////////////////
} // End Namespace
////////////////////