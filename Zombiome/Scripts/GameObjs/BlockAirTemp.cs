using System;
using System.Collections.Generic;
using UnityEngine;

// Token: 0x020001CE RID: 462
public class BlockAirTemp : Block {
	/** Automatically replaced by true air block. Using Set(BlockAirTemp) does not trigger
	block destruction, when set("air") would.
	*/
	public BlockAirTemp() {
		base.IsNotifyOnLoadUnload = true;
		this.IsRandomlyTick = false;
	}
	public override ulong GetTickRate() {return 20UL;}
	public override bool UpdateTick(WorldBase _world, int _clrIdx, Vector3i _blockPos, BlockValue _blockValue, bool _bRandomTick, ulong _ticksIfLoaded, GameRandom _rnd) {
		GameManager.Instance.World.SetBlockRPC(_clrIdx, _blockPos, BlockValue.Air);
		return true;
	}	
}