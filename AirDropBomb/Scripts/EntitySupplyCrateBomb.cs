using System;
using System.IO;
using UnityEngine;
using System.Collections;

public class EntitySupplyCrateBomb : EntitySupplyCrate {

    private static float explodeChance = 0.5f;
    private int closestId = -1; // = world.GetClosestPlayer(position.x, position.y, position.z, 0, -1.0);
    protected override void fallHitGround(float _v, Vector3 _fallMotion) {  
        /* Probably called once upon impact. I should avoid testing below block myself */
        base.fallHitGround(Mathf.Min(_v, 5f), new Vector3(_fallMotion.x, Mathf.Max(-0.75f, _fallMotion.y), _fallMotion.z)); 

        Vector3 pos = this.GetPosition();
        World world = this.world; // GameManager.Instance.World;
        if (this.closestId == -1) {
            EntityPlayer _p = world.GetClosestPlayer(this.GetPosition(), 100f, false);
            if (_p != null) this.closestId = _p.entityId;
        }
        float u;
        for (int k=0; k<1; k++) {            
            Vector3i where = new Vector3i(Utils.Fastfloor(pos.x), Utils.Fastfloor(pos.y) - 1 - k,  Utils.Fastfloor(pos.z));  
            BlockValue bv = world.GetBlock(where);
            if (bv.type != BlockValue.Air.type) {
                u = AIAirDrop.controller.Random.RandomRange(0f,1f);
                if (u < explodeChance) GameManager.Instance.StartCoroutine(this.Explode());
                return;
            }
        }
	}

    
    private IEnumerator Explode() {		
        Vector3 pos = this.GetPosition();
        Vector3i where = V3i(pos);

        World world = GameManager.Instance.World;  
        SpawnItem(pos, "thrownAmmoMolotovCocktail", new Vector3(0f,-0.1f,0f)); 
        
        // world.SetBlockRPC(0, where, BlockValue.Air);  /// Fails to oestroy crate - matter of V3/V3i conversion ?       
        this.Kill(DamageResponse.New(true));

        GameRandom random = AIAirDrop.controller.Random;
        for (int k=0; k< 10; k++) {
            SpawnItem(
                pos + new Vector3(random.RandomRange(-3f, 3f), random.RandomRange(1f, 3f), random.RandomRange(-3f, 3f)),
                "rockBomb",
                new Vector3(random.RandomRange(-5f, 5f), random.RandomRange(-2f, 2f), random.RandomRange(-5f, 5f))
            );
            yield return new WaitForSeconds(0.2f);
            SpawnItem(
                pos + new Vector3(random.RandomRange(-3f, 3f), random.RandomRange(1f, 3f), random.RandomRange(-3f, 3f)),
                "thrownAmmoMolotovCocktail",
                new Vector3(random.RandomRange(-5f, 5f), random.RandomRange(2f, 4f), random.RandomRange(-5f, 5f))
            );
            float dt = AIAirDrop.controller.Random.RandomRange(0.1f,1f);
            yield return new WaitForSeconds(dt);
        }
	}

    private static Vector3i V3i(Vector3 v) {return new Vector3i(Utils.Fastfloor(v.x), Utils.Fastfloor(v.y),  Utils.Fastfloor(v.z));}
    private static Vector3 V3(Vector3i v) {return new Vector3(1f*v.x, 1f*v.y, 1f*v.z);}
    private static void SpawnItem(Vector3 position, String item="thrownAmmoMolotovCocktail", Vector3 motion=new Vector3()) {        
        World world = GameManager.Instance.World;
        EntityPlayer closest = world.GetClosestPlayer(position.x, position.y, position.z, 0, -1.0);
        Vector3 where = position; // closest.GetPosition();
        Debug.Log(String.Format("Closest {0} {1}", closest, where));
        ItemStack itemStack = new ItemStack(ItemClass.GetItem(item, false), 1); // this.CreateItemCount
        world.gameManager.ItemDropServer(
            itemStack,
            where, // initial pos
            Vector3.zero, /// rdm
            motion, ///  _initialMotion
            closest.entityId, // -1 plante (pour gagner XP ?)
            1f,
            false,
            0);
    }

}