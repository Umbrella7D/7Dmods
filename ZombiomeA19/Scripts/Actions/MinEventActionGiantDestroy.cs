using System;
using System.Xml;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Reflection;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UnityEngine;

using CSutils;

      /* 
        MinEventParams. CachedEventParam=MinEventParams, TileEntity=,
        Self=[type=EntityPlayerLocal, name=s_h_a_i_o, id=1298],
        Other=, Others=,
        ItemValue=item=853 m=0 ut=0,
        ItemActionData=ItemActionEat+MyInventoryData, ItemInventoryData=ItemInventoryData, Position=(-331.7, 61.1, -1752.9),
        Transform=Player_1298 (UnityEngine.Transform),
        Buff=BuffValue, // acces au buf !
        BlockValue=id=18 r=0 d=0 m=0 m2=0 m3=0, POI=,
        Area=Center: (-331.7, 61.9, -1752.9), Extents: (0.3, 0.9, 0.3),
        Biome=desert,
        Tags=standing, idle, player, entity, human,
        DamageResponse=DamageResponse, ProgressionValue=,
        Seed=683647262, IsLocal=False,
        */

public class MinEventActionGiantDestroy : MinEventActionTargetedBaseCatcher {
    /** Aura around giant that damages block and entities
    Why? 1) it's fun 2) when a giant spawns inside building, he will remain trapped (maybe changing the
    boundingBox may help understand where he hits when resized)

    */


    private World World;
    public override void ExecuteUncatch(MinEventParams _params) {
        if (running) return;
        foreach (EntityAlive Self in this.targets) {
            Execute(Self);
            break;
        }
    }
    public void Execute(EntityAlive ctrl) {
        World = GameManager.Instance.World;
        Zombiome.Routines.Start(DestroyAround(ctrl), "MEAGiantDestroy.DestroyAround({0})", ctrl);
    }
    // TODO: set of giant to not restart until finished

    private bool running = false;
    public IEnumerator DestroyAround(EntityAlive ctrl) {
        running = true; // Actions are static, is this fine ? FIXME: There should be one per giant !
        Printer.Log(35, "MinEventActionGiantDestroy", ctrl, ctrl.GetPosition());

        Bounds Bounds = ctrl.boundingBox; // check: not affected by rescale !
        Vector3 dir = SdtdUtils.EffectsEntity.MoveDir(ctrl);
        Vector3 pos = ctrl.GetPosition();

        int ymax= (int) Math.Floor(ctrl.boundingBox.size.y * ctrl.gameObject.transform.localScale.y);
        // how to enumerate x,z  wrt direction ?? h is ok
        foreach(int p in SdtdUtils.EffectsGround.LR(4)) { 
            foreach(int q in SdtdUtils.EffectsGround.LR(4)) {
                Vector3 where = new Vector3(pos.x + p, pos.y, pos.z + q);
                if (! aigu(dir.x, dir.z, where.x - pos.x, where.z - pos.z)) continue; // test not depending on y
                for (int y=0; y< ymax; y++) { // TODO adjust by current giant ratio
                    Printer.Log(30, "MinEventActionGiantDestroy at", p, q, y);
                    where.y = pos.y + y;
                    Vector3i iwhere = Vectors.ToInt(where);
                    BlockValue block = World.GetBlock(iwhere);
                    if (block.type != 0) DamageBlock(ctrl, iwhere);
                    // todo: passer block et bv a DamageBlock
                }
                DamageEnts(ctrl);
                yield return new WaitForSeconds(0.2f);
                if (ctrl==null || ctrl.IsDead()) yield break;
            }
        }
        running = false;
    }
    public bool aigu(float dxr, float dzr, float dx, float dz) {
        return dxr * dx + dzr * dz > 0;
    }
    public bool aigu(float x0, float z0, float xr, float zr, float x, float z) {
        return (xr-x0) * (x-x0) + (zr-z0) * (z-z0) > 0;
    }

    private void DamageBlock(EntityAlive ctrl, Vector3i pos) {
        // TODO entities too
        Printer.Log(30, "MinEventActionGiantDestroy DamageBlock", pos);
        BlockValue block = World.GetBlock(pos);
        Printer.Log(30, "MinEventActionGiantDestroy DamageBlock2", pos);
        block.Block.DamageBlock(World, 0, pos, block, 1000, ctrl.entityId); //clrIdx at - => array[-1]

        if (true) return;
        if (Zombiome.rand.RandomFloat > 0.05f) return;

        Printer.Log(30, "MinEventActionGiantDestroy ParticleEffect", pos);
        float lightValue = World.GetLightBrightness(pos) / 2f;
        string pen = Zombiome.rand.RandomFloat < 0.5 ? "smoke" : "big_smoke";
        // TODO: use ZBiomeInfo.groundParticle
        ParticleEffect pe = new ParticleEffect(
            pen, Vectors.ToFloat(pos), lightValue,
            new Color(1f, 1f, 0f, 0.3f), "", null, false
        ); //2 e string is sound
        GameManager.Instance.SpawnParticleEffectServer(pe, -1);
    }

    private void DamageEnts(EntityAlive ctrl) {
        // TODO: command test
        Vector3 pos = ctrl.GetPosition();
        Bounds Bounds = new Bounds(pos, new Vector3(4, 4, 4));
        List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(typeof(Entity),
                Bounds,
                new List<Entity>());
        //DamageSource DamageSource= new DamageSource(EnumDamageSource.External, EnumDamageTypes.Crushing);// TODO: DamageSourceEntity ?
        DamageSource DamageSource= new DamageSourceEntity(EnumDamageSource.External, EnumDamageTypes.Crushing,
            ctrl.entityId); // direction ? can it be used to push back ?
        foreach(Entity ent in entitiesInBounds) {
            if (ent.entityId == ctrl.entityId) continue;
            ent.DamageEntity(DamageSource, 3, false);
        }
    }
}