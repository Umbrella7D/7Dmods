using System;
using System.Collections.Generic;
using UnityEngine;

using CSutils;

// Token: 0x020001CE RID: 462
public class BlockWaterTransitionSimple : Block {
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

	/* NB
	Used with liquidv2, we interfere with the scheduled updates (faster, more water ?)
	*/


	private TransitionBlock Transition;
	public BlockWaterTransitionSimple() {
		base.IsNotifyOnLoadUnload = true;
		this.IsRandomlyTick = true; // false; //
		Transition = new TransitionBlock(this);
	}
	public static System.Random Random = new System.Random();

	public override void Init() {
		base.Init();
		Transition.Init(this.Properties);
	}
	public static bool IsEmpty(Block block) {
		if (TransitionBlock.IsWater(block)) return false;
		if (block.IsPathSolid) return false;
		if (block.shape.IsTerrain()) return false;
		return true;
	}


	public override bool UpdateTick(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd) {
		bool ret = base.UpdateTick(_world, _clrIdx, _blockPos, _blockValue, _bRandomTick, _ticksIfLoaded, _rnd);
		return Transition.UpdateTick(_world, _clrIdx, _blockPos, _blockValue, _bRandomTick, _ticksIfLoaded, _rnd);
	}

	public override void OnNeighborBlockChange(WorldBase world, int _clrIdx, Vector3i _myBlockPos, BlockValue _myBlockValue,
					Vector3i _blockPosThatChanged, BlockValue _newNeighborBlockValue, BlockValue _oldNeighborBlockValue) {
		base.OnNeighborBlockChange(world, _clrIdx, _myBlockPos, _myBlockValue,
					_blockPosThatChanged, _newNeighborBlockValue, _oldNeighborBlockValue);
		Transition.OnNeighborBlockChange(world, _clrIdx, _myBlockPos, _myBlockValue,
					_blockPosThatChanged, _newNeighborBlockValue, _oldNeighborBlockValue);
	}

	
	public override void OnBlockAdded(WorldBase _world, Chunk _chunk, Vector3i _blockPos, BlockValue _blockValue) {
		base.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
		Transition.OnBlockAdded(_world, _chunk, _blockPos, _blockValue);
	}
}