using System;
using System.Collections.Generic;
using UnityEngine;

using CSutils;

// Token: 0x020001CE RID: 462
public class TransitionBlock {
	/**
	NB: the same block instance manages multiple BlockValue
	-> cant implement lifetime

	if I had blockvalue based state (meta ?)
	- UpdateTick 1st call
		- stable : set state, skip next UpdateTick
		- otherwise, move, add Scheduled change, dont skip next
		- UpdateTick would restore unstable state (for the next call to UpdateTick * or add scheduled direct!)

	TODO: into from biome ( snow / des/burnt=> air )
	*/

	private static Dictionary<string,string> BiomeWaters = new Dictionary<string,string>();
	static TransitionBlock() {
		BiomeWaters["desert"] = "air";
		BiomeWaters["burnt_forest"] = "air";
		BiomeWaters["pine_forest"] = "";
		BiomeWaters["snow"] = "terrSnow";
		BiomeWaters["wasteland"] = "waterSlime";
	}

	public static bool IsWater(BlockValue bv) {return bv.Block.blockMaterial.IsLiquid;}
		// {return bv.Block.blockMaterial == MaterialBlock.water;}
	public static bool IsWater(Block blk) {return blk.blockMaterial.IsLiquid;}
	public static System.Random Random = new System.Random();

	public TransitionBlock(Block Block) {
		this.Block = Block;
	}
	public Block Block;
	public bool belowTo = false;
	public int gravity = 0;
	public ulong dt = 0UL;
	public string to = "";
	public bool toBiomeWater = false; // the buff also depends on biome, so not that a gd idea ...
	public float rto = 1f;
	public int damage = 0;
	public int damtime = 0;
	public string particle = ""; // still todo
	public float rparticle = 1f;
	public bool onb = false; // can I use damage as state ?
	public bool autostart = false;
	public TransitionBlock() {
		// base.IsNotifyOnLoadUnload = true;
		// this.IsRandomlyTick = true; // false; //
	}

	public void Init(DynamicProperties props) {
		if (props.Values.ContainsKey("dt")) this.dt = (ulong) int.Parse(props.Values["dt"]);
		if (props.Values.ContainsKey("gravity")) this.gravity = int.Parse(props.Values["gravity"]);
		if (props.Values.ContainsKey("to")) this.to = props.Values["to"];
		if (props.Values.ContainsKey("rto")) this.rto = float.Parse(props.Values["rto"]);
		if (props.Values.ContainsKey("damage")) this.damage = int.Parse(props.Values["damage"]);
		if (props.Values.ContainsKey("damtime")) this.damtime = int.Parse(props.Values["damtime"]);
		if (props.Values.ContainsKey("particle")) this.particle = props.Values["particle"];
		if (props.Values.ContainsKey("rparticle")) this.rparticle = float.Parse(props.Values["rparticle"]);
		if (props.Values.ContainsKey("onb")) this.onb = bool.Parse(props.Values["onb"]);
		if (props.Values.ContainsKey("autostart")) this.autostart = bool.Parse(props.Values["autostart"]);
		if (props.Values.ContainsKey("belowTo")) this.belowTo = bool.Parse(props.Values["belowTo"]);


		if (to == "__water__") toBiomeWater = true;
		lastUT = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
	}
	public static Vector3i[] belows = new Vector3i[]{new Vector3i(0,-1,0)};

	private BlockValue NewValue(Vector3i pos) {
		string block = to;
		if (toBiomeWater) {
			string biome = GameManager.Instance.World.ChunkCache.ChunkProvider
								.GetBiomeProvider().GetBiomeAt(pos.x, pos.z).ToString();
			block = BiomeWaters[biome];
		}
		return Block.GetBlockValue(block);
	}
	private bool Transition(Vector3i _blockPos, BlockValue bval, bool force=false) {	
		Printer.Log(71, "TransitionBlock", _blockPos, bval.Block, bval.damage);
		World World = GameManager.Instance.World; // as arg it is available above !	- in init !
		if (to != "" && Random.NextDouble() <= rto) {
			BlockValue transformed = NewValue(_blockPos);
			World.SetBlockRPC(0, _blockPos, transformed);
			return true;	
		}
		if (belowTo && World.GetBlock(_blockPos - Vectors.UnitY).type == BlockValue.Air.type) {
			BlockValue transformed = (to=="") ? BlockValue.Air : NewValue(_blockPos);
			World.SetBlockRPC(0, _blockPos, transformed);
			return true;	
		}
		if (particle != "" && Random.NextDouble() <= rparticle) {
			if (World.GetBlock(_blockPos + Vectors.UnitY).type == BlockValue.Air.type)
				SdtdUtils.EffectsItem.SpawnParticle(Vectors.ToFloat(_blockPos), particle); // todo color+sound
		}
		if (damtime > 0) {
			if (bval.damage < 0) {
				Printer.Log(71, "DamTime: damage < 0", _blockPos, bval.damage, bval.Block.MaxDamage);
				Block.DamageBlock(World, 0, _blockPos, bval, - bval.damage - bval.Block.MaxDamage - 1, -1, false, false);
			}
			else if (bval.damage >= bval.Block.MaxDamage - 1) {
			    // cant use UTC (GWT knows game start)
				ulong wt = World.GetWorldTime();
				int unitm = 1; // in game minute
				int now = GameUtils.WorldTimeToMinutes(wt) + 60 * (GameUtils.WorldTimeToHours(wt) + 24 * (GameUtils.WorldTimeToDays(wt) - 1));
				Printer.Log(71, "DamTime: damage = max", _blockPos, bval.damage, "=>", now);
				Block.DamageBlock(World, 0, _blockPos, bval, - bval.damage - now, -1, false, false);
			}
			else if (bval.damage > 0) {
				ulong wt = World.GetWorldTime();
				int unitm = 1; // in game minute
				int now = GameUtils.WorldTimeToMinutes(wt) + 60 * (GameUtils.WorldTimeToHours(wt) + 24 * (GameUtils.WorldTimeToDays(wt) - 1));
				Printer.Log(71, "DamTime: checking", _blockPos, now, ">", bval.damage, "+", damtime, "?");
				if (now - bval.damage > damtime) {
					Printer.Log(71, "DamTime: Death !", _blockPos);
					BlockValue transformed = (to=="") ? BlockValue.Air : NewValue(_blockPos);
					World.SetBlockRPC(0, _blockPos, transformed);
					return true;
				}
			}

		}
		else if (damage > 0) {
			if (bval.damage < -10)
				Block.DamageBlock(GameManager.Instance.World, 0, _blockPos, bval, - bval.damage - bval.Block.MaxDamage, -1, false, false);
			else
				Block.DamageBlock(GameManager.Instance.World, 0, _blockPos, bval, damage, -1, false, false);
		}
		return false;
	}

	private int TryFall(Vector3i _blockPos, BlockValue _blockValue) {
		int k = 0;
		Vector3i pos = Vectors.Copy(_blockPos);
		int y0 = pos.y;
		for (k=1; k<= gravity; k++) {
			pos.y = y0 - k;
			BlockValue below = GameManager.Instance.World.GetBlock(pos);
			if (below.Equals(BlockValue.Air) || IsEmpty(below.Block)) {}
			else {
				k = k -1;
				break;
			}
		}
		pos.y = y0 - k;
		// There was a gap and < len : insert at new pos
		if (k > 0 && k < gravity) GameManager.Instance.World.SetBlockRPC(pos, _blockValue);
		// There was a gap : delete initial
		if (k > 0) GameManager.Instance.World.SetBlockRPC(_blockPos, BlockValue.Air);
		return k; // -> true if if it felt
		// int mdom = _myBlockValue.damage; 
		// this.DamageBlock(GameManager.Instance.World, 0, _myBlockPos, _myBlockValue, 10, -1, false, false);	
	}





	public static bool IsEmpty(Block block) {
		if (IsWater(block)) return false;
		if (block.IsPathSolid) return false;
		if (block.shape.IsTerrain()) return false;
		return true;
	}

	private long lastUT;	
	/* FIXME: slow bc all blocks treated at once.
	the game has a quick callback that add scheduled block update (run more smoothly - should I do it ?)

	*/
	public bool UpdateTick(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd) {
		if (_blockValue.type != Block.blockID) return true; // changed block type
		int felt = 0;
		bool changed = Transition(_blockPos, _blockValue);
		if (changed) return true;	// transformed	
		if (gravity > 0) felt = TryFall(_blockPos, _blockValue);
		if (changed) return true; // felt

		// on liquidV2, don't add any more SD.
		if (dt > 0 && felt <= gravity)
			_world.GetWBT().AddScheduledBlockUpdate(0, _blockPos - felt * Vectors.UnitY, Block.blockID, dt);
		return true;
	}

	public void OnNeighborBlockChange(WorldBase world, int _clrIdx, Vector3i _myBlockPos, BlockValue _myBlockValue,
					Vector3i _blockPosThatChanged, BlockValue _newNeighborBlockValue, BlockValue _oldNeighborBlockValue) {
		// If I was inserting a new BV wo rotation, the slime would stick below ?
		if (! this.onb) return;
		Printer.Log(71, "OnNeighborBlockChange", _myBlockPos, _blockPosThatChanged, this.GetHashCode(), "damage", _myBlockValue.damage);
		if (_myBlockPos.y != _blockPosThatChanged.y + 1) return; // only react to below block
		if (! IsEmpty(_newNeighborBlockValue.Block)) return; // only when below is empty
		int felt = TryFall(_myBlockPos, _myBlockValue);
		if (dt > 0 && felt <= gravity)
			world.GetWBT().AddScheduledBlockUpdate(0, _myBlockPos - felt * Vectors.UnitY, Block.blockID, dt);
	}

	public void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue) {
		if (! autostart) return;
		if (dt > 0)
			if (_blockValue.type == this.Block.blockID && !_world.IsRemote())
				_world.GetWBT().AddScheduledBlockUpdate(0, _blockPos, Block.blockID, dt);

	}
}