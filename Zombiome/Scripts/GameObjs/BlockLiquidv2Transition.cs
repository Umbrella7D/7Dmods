using System;
using System.Collections.Generic;
using UnityEngine;

using CSutils;

// Token: 0x020001CE RID: 462
public class BlockLiquidv2Transition : BlockLiquidv2 {
	private TransitionBlock Transition;
	public BlockLiquidv2Transition() {
		base.IsNotifyOnLoadUnload = true;
		this.IsRandomlyTick = true; // false; //
		Transition = new TransitionBlock(this);
	}
	public override void Init() {
		base.Init();
		Transition.Init(this.Properties);
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