using System;
using System.Xml;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Reflection;

using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

using CSutils;
using SdtdUtils;


public class MinEventActionEjectSlime : MinEventActionTargetedBaseCatcher {
    /** Try to find a slime block near Self, and try to move it
    Done: propagate to adjacent slime blocks (but may be detected as empty by BlockWaterTransitionSimple.IsEmpty)
    */

    public override void ExecuteUncatch(MinEventParams _params) {
        if (_params.Self == null) return;
        // Vector3i pos = Vectors.ToInt(_params.Self.GetPosition());
        Vector3i pos = World.worldToBlockPos(_params.Self.GetPosition());
        BlockValue block = GameManager.Instance.World.GetBlock(pos);
        if (block.Block.GetBlockName() == "waterSlime") Move(pos, block);
        pos = pos - Vectors.UnitY;
        block = GameManager.Instance.World.GetBlock(pos);
        if (block.Block.GetBlockName() == "waterSlime") Move(pos, block);
    }

    public void Move(Vector3i pos, BlockValue block) {
        int[] count = new int[]{0};
        bool[] done = new bool[]{false};
        Zombiome.Routines.Start(_Move(pos, block, count, done), "MinEventActionEjectSlime");
    }

    private System.Random Random = new System.Random();
    public IEnumerator _Move(Vector3i pos, BlockValue block, int[] count, bool[] done) {
        while (true) {
            int idx = Random.Next(Vector3i.AllDirections.Length);
            Vector3i dir = Vector3i.AllDirections[idx];
            dir = dir + Random.Next(3) * Vectors.UnitY;
            BlockValue target = GameManager.Instance.World.GetBlock(pos + dir);
            count[0] = count[0] + 1;
            if (BlockWaterTransitionSimple.IsEmpty(target.Block)) {
                GameManager.Instance.World.SetBlockRPC(pos + dir, block);
                GameManager.Instance.World.SetBlockRPC(pos, BlockValue.Air);
                done[0] = true;
                yield break;
            } else if (block.Block.GetBlockName() == "waterSlime") {
                yield return _Move(pos + dir, block, count, done); // propagate
            }
            if (done[0]) yield break;
            if (count[0] > 10) yield break;
            yield return Routines.WaitFrame;
        }
    }
}