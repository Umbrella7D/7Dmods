using System;
using System.Linq;
using System.Collections.Generic;

using Unity;
using UnityEngine;


namespace CSutils {
// Token: 0x02000126 RID: 294
public static partial class Geo3D {
    /* I have seen GameManager.Instance.World.GetTerrainHeight(start.x, start.z) fail and return 0 !

	*/

	public static Vector3 intersectLook(EntityPlayer player) { //  'Entity' does not contain a definition for 'GetLookRay', so not for Zs ?
        // EntityPlayer player = GameManager.Instance.World.GetLocalPlayer(); // : 'World' does not contain a definition for 'GetLocalPlayer' 
        Ray lookRay = player.GetLookRay();
        if (Voxel.Raycast(GameManager.Instance.World, lookRay, 5000f, 65536, 64, 0f)) return Voxel.voxelRayHitInfo.hit.pos;
        else return player.position;
    }
    public static Vector3 directionLook(EntityPlayer player) {
        // Vector3 intersect = intersectLook(player);
        // return (intersect - player.position).normalized;
        Ray lookRay = player.GetLookRay();
        Vector3 dx = lookRay.direction;
        return dx.normalized;
    }

	
	public static bool IsAboveSurface(Block block) {
		// if (bv.Equals(BlockValue.Air)) return true;
		// Debug.Log(String.Format("BlockValue air: {0} {1} ", BlockValue.Air, BlockValue.Air.Block.blockID)); // BlockValue.Air.Block.blockID == 0
		if (block.blockID == 0) return true; // should be equivalent to air
		if (block.IsDecoration) return true;
		if (block.Properties.Values.ContainsKey("Shape")) {
			if (block.Properties.Values["Shape"] =="Terrain") return false;
		}
		return true; // buildings 
	}
	public static bool IsGround(Block block) {
		if (block.Properties.Values.ContainsKey("Shape") && block.Properties.Values["Shape"] =="Terrain") return true;
		return false;
	}

    public static bool IsGroundOrBuilding(Block block) {
		if (block.Properties.Values.ContainsKey("Shape") && block.Properties.Values["Shape"] =="Terrain") return true;
		if (block.FilterTags == null) return false;
        if (block.FilterTags.Contains("fconstruction")) return true;
        if (block.FilterTags.Contains("fbuilding")) return true;
        return false;
	}
	
    // <property name="FilterTags" value="fbuilding,fwood,fconstruction,fframes"/>

	public static Vector3 Surface(Vector3 start, int iniy = -1, Func<Block,bool> _IsGround = null) {
		return Vectors.ToFloat(Surface(Vectors.ToInt(start), iniy, _IsGround));
	}
	public static Vector3i Surface(Vector3i start, int iniy = -1, Func<Block,bool> _IsGround = null) {
		// TODO ? protéger par chunk not loaded => no ZB Effect
		// FIXME: return y, instead of ip modif ...
		Vector3i clone = new Vector3i(start.x, start.y, start.z);
		int y0 = clone.y;
		if (iniy >= 0) clone.y = iniy; //dej afait dedan ...
		Vector3i surf = _Surface(clone, iniy, _IsGround);
		if (surf.y <= 1) {
			int gth = GameManager.Instance.World.GetTerrainHeight(start.x, start.z);
			Printer.Log(20, "Surface Weirdy:", surf.y, "/", surf, " From ", start, ". Options (y0,iniy,gth)= ", y0, iniy, gth);
		}
		return surf;
	}
    public static Vector3i _Surface(Vector3i start, int iniy = -1, Func<Block,bool> _IsGround = null) {
        if (_IsGround == null) _IsGround = IsGround;
		/// Get heigth of the surface, return top occupied block
        if (iniy >= 0) start.y = iniy; // starting at 0 does -1 + break
		if (iniy == -2) start.y = GameManager.Instance.World.GetTerrainHeight(start.x, start.z);
        start.y = Math.Max(start.y, 1);

		int y = start.y;
        bool looking = true;
	
		start.y = y;
        BlockValue current = GameManager.Instance.World.GetBlock(start);
		int dy = (_IsGround(current.Block)) ? 1 : -1;
		y = y + dy;
        while(looking) {
			// Debug.Log("Surface Looking " + start.ToString());
            start.y = y;
            current = GameManager.Instance.World.GetBlock(start);
			bool ig = _IsGround(current.Block);
			if (ig && dy == -1) return start;
			// Otherwise, air  -> keep down (dy=-1)
			else if (! ig && dy == 1) {
				start.y = y-1;
				return start;
			}
			// Otherwise, ground -> keep up (dy=1)
			y = y + dy;
            if (y<=0) {start.y = 0; return start;}
            if (y>=254) {start.y = 254; return start;}
        }
        return start; /// Should never come here
    }

	
    public class SurfaceNeighbourhood : IntNeighbourhood {

        public static Vector3i[] adjacent = new Vector3i[]{
            new Vector3i(1,0,0),
            new Vector3i(1,0,1),
            new Vector3i(0,0,1),
            new Vector3i(-1,0,1),
            new Vector3i(-1,0,0),
            new Vector3i(-1,0,-1),
            new Vector3i(0,0,-1),
            new Vector3i(1,0,-1),
        };
        public SurfaceNeighbourhood(Vector3i center) {
            xcenter = center.x;
            ycenter = center.z;
        }
        public Vector3i Position;
        private int heigth; // at the current point
        public bool ok = true;
        public Func<Block,bool> IsGround= null;
        public override void Next() {
            base.Next();
            Position.x = x;
            Position.z = y;
            int gth = GameManager.Instance.World.GetTerrainHeight(x, y);
            if (gth == 0) ok = false; // dont set y to keep neighboor memory
            else {
                ok = true;
                Position = Geo3D.Surface(Position, heigth, this.IsGround);
                Position.y = Position.y + 1;
                heigth = Position.y; // right above surface
            }
        }
		public virtual void Reset(int xcenter, int ycenter, int h=0) {
            base.Reset(xcenter, ycenter);
			heigth = h;
        }
    }


} // end static class Geo3D;

} // end namespace CSutils;