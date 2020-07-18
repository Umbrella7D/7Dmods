using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using Harmony;


using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

using CSutils;
using SdtdUtils;

namespace SdtdUtils {

public class EffectsEntity {

    public static void AddBuffToRadius(String strBuff, Vector3 position, int Radius) {
        // If there's no radius, pick 30 blocks.
        if(Radius <= 0  ) Radius = 30;

        World world = GameManager.Instance.World;
        List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(null, new Bounds(position, Vector3.one * Radius));
        if(entitiesInBounds.Count > 0) {
            for(int i = 0; i < entitiesInBounds.Count; i++) {
                EntityAlive entity = entitiesInBounds[i] as EntityAlive;
                if(entity != null) {
                    if( ! entity.Buffs.HasBuff(strBuff)) entity.Buffs.AddBuff(strBuff);
                }
            }
        }
    }

    public static void DynamicSizeVis(EntityPlayer player, Emplacement place, OptionEffect options) {
        // TODO: use size option
        List<Entity> entitiesInBounds = GameManager.Instance.World.GetEntitiesInBounds(typeof(EntityZombie),
                new Bounds(place.position, Vector3.one * 2f * 10f),
                new List<Entity>());
        foreach(Entity entity in entitiesInBounds) {
            Zombiome.Routines.Start(TestDynamicSizeVis(entity));
        }
    }
    public static IEnumerator TestDynamicSizeVis_V0(Entity entity) {
        // Should be connected to speed/hp/damage increase ? also proportionnal jump strength ?
        yield return inflate(entity, new Vector3(1,2,1), 10);
        yield return new WaitForSeconds(10f); 
        yield return inflate(entity, new Vector3(1,1,1), 10);
        yield return new WaitForSeconds(5f); 

        yield return inflate(entity, new Vector3(0.5f,0.8f,0.5f), 10);
        yield return new WaitForSeconds(10f); 
        yield return inflate(entity, new Vector3(1,1,1), 10);
    }

    public static IEnumerator TestDynamicSizeVis(Entity entity) {
        Vector3 based = initialLocalScale(entity);
        Printer.Log(20, "TestDynamicSizeVis, initial/actual", based, entity.gameObject.transform);
        entity.gameObject.transform.localScale = 2 * entity.gameObject.transform.localScale;
        yield return new WaitForSeconds(10f); 
        based = initialLocalScale(entity);
        Printer.Log(20, "TestDynamicSizeVis, initial/actual", based, entity.gameObject.transform);
        entity.gameObject.transform.localScale = based;
    }

    public static IEnumerator inflate(Entity entity, Vector3 factor, int steps=0) {
        /*
        Should I make a buff of this, for better restoration (mainly for the player)

        in Entity.Awake() - is it called multiple times ? -
            this.scaledExtent = new Vector3(
                component.size.x / 2f * base.transform.localScale.x,
                component.size.y / 2f * base.transform.localScale.y,
                component.size.z / 2f * base.transform.localScale.z
            );
        is used for Entity.Width, and BoundingBox, that's all
        So Bounding box are not updated when altering localScale after
        but this means the base value is recoverable from scaledExtent?
        */
        Vector3 current = entity.gameObject.transform.localScale; // Created by Entity.Awake()
        steps = steps + 1;
        for(int k=0; k<steps; k++){
            float w = (k+1) / (1f * steps);
            entity.gameObject.transform.localScale = (1-w) * current + w * factor;
            yield return new WaitForSeconds(0.1f); 
        }
        entity.gameObject.transform.localScale = factor;
        // entity.emodel.SetVisible(false, false);
        // yield return new WaitForSeconds(20f);
        // entity.emodel.SetVisible(true, true);
    }

    public static IEnumerator inflate(Entity entity, float factor, int steps=0) {
        return inflate(entity, factor * Vectors.Float.One, steps);
    }

    public static Vector3 initialLocalScale(Entity entity) {
        Vector3 scaledExtent = new Vector3(entity.width, entity.height, entity.depth) / 2f; // Protected
        Vector3 correct = Vectors.Float.One;

        BoxCollider component = ((MonoBehaviour) entity).gameObject.GetComponent<BoxCollider>();
        if (component != null) {
            correct = component.size / 2f;
            Printer.Print("initialLocalScale BoxCollider", scaledExtent, "/", correct);
            // ils = Vectors.Float.Divide(scaledExtent, component.size) * 2f;
        } //else {
        Printer.Print("component1 done");
        CharacterController component2 = ((MonoBehaviour) entity).gameObject.GetComponent<CharacterController>();
        if (component2 != null) {
            correct = new Vector3(component2.radius, component2.height, component2.radius);
            Printer.Print("initialLocalScale CharacterController", scaledExtent, "/", correct);
            // if (component == null) ils = Vectors.Float.Divide(scaledExtent, ccrh);
        }
        Printer.Print("component2 done"); // player in the 2nd case

        correct.y = correct.y / 2f;
        Vector3 ils = Vectors.Float.Divide(scaledExtent, correct);
        Printer.Print("ils", ils); // 1,1,1 for the player ...
        return ils;
    }
    public static Vector3 _initialLocalScaleExpl(Entity entity) {
        /*
         width is eg " this.scaledExtent.x * 2f; "

        BoxCollider component = base.gameObject.GetComponent<BoxCollider>();
        scaledExtent.x = component.size.x / 2f * base.transform.localScale.x

        CharacterController component2 = base.gameObject.GetComponent<CharacterController>();
        this.scaledExtent = new Vector3(component2.radius * base.transform.localScale.x, component2.height * base.transform.localScale.y * 0.5f, component2.radius * base.transform.localScale.z);


        base is monobehav

        FIXME ther is a nother way l360 !

        Is awake ever called ?
        */
        Vector3 scaledExtent = new Vector3(entity.width, entity.height, entity.depth) / 2f; // Protected
        Vector3 ils = new Vector3();
        // TODO correctif and in the end, divide

        BoxCollider component = ((MonoBehaviour) entity).gameObject.GetComponent<BoxCollider>();
        if (component != null) {
            Printer.Print("initialLocalScale BoxCollider", scaledExtent, "/", component.size);
            ils = Vectors.Float.Divide(scaledExtent, component.size) * 2f;
        } //else {
        Printer.Print("component1 done");
        CharacterController component2 = ((MonoBehaviour) entity).gameObject.GetComponent<CharacterController>();
        if (component2 != null) {
            Vector3 ccrh = new Vector3(component2.radius, component2.height, component2.radius);
            Printer.Print("initialLocalScale CharacterController", scaledExtent, "/", ccrh);
            if (component == null) ils = Vectors.Float.Divide(scaledExtent, ccrh);
        }
        Printer.Print("component2 done");
        //}
        CharacterController component3 = (CharacterController) entity.PhysicsTransform.gameObject.GetComponent<CharacterController>();
		if (component3 != null) {
            Printer.Print("component3 not null");
            float num = 0.08f;
            if (! (entity is EntityPlayer)) num = 0f;
            float num2 = component3.height;
            float num3 = component3.radius;
            Printer.Print("component3 before boxCollider");
            //is it on base class ?

            PropertyInfo _nativeCol = null;
            _nativeCol = typeof(Entity).GetProperty("nativeCollider", BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic);
            Printer.Print("_nativeCol", _nativeCol);
            _nativeCol = typeof(Entity).GetProperty("nativeCollider", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
            Printer.Print("_nativeCol", _nativeCol);



            BoxCollider boxCollider = _nativeCol.GetValue(entity) as BoxCollider;
            if (boxCollider != null) {
                Printer.Print("component3 boxCollide non null");
                bool linkCapsuleSizeToBoundingBox = (bool) (typeof(Entity).GetProperty("linkCapsuleSizeToBoundingBox").GetValue(entity));
                if (linkCapsuleSizeToBoundingBox) {
                    Printer.Print("component3 b4 boxCollider.size");
                    num2 = Utils.FastMax(boxCollider.size.y - num, entity.stepHeight);
                    num3 = boxCollider.size.x * 0.5f - num;	
                }
            }
            Printer.Print("boxCollider null");
            Vector3 correctif = new Vector3(num3, num2*0.5f, num3);
            Printer.Print("initialLocalScale NativeCharacterController", scaledExtent, "/", correctif);
            if (component2 == null && component == null) {
                ils = Vectors.Float.Divide(scaledExtent, correctif);
            }
		}
        Printer.Print("component3 done");
        //if (coorectif unkown == ) Printer.Print("initialLocalScale FAILED");
        return ils;
        /*
        I should focus on player, for which
        this.scaledExtent = new Vector3(num3 * base.transform.localScale.x, num2 * base.transform.localScale.y * 0.5f, num3 * base.transform.localScale.z);
		as in compo 1)
        */
    }


    /* ******************** Motion ******************** */
    public static Vector3 MoveDir(EntityAlive ent) {
        /* FIXME : Does not account for running ! This is [0,1] joystick/keyboard speed
        TODO: try use motion ?

        From Entity.Move, neglecting _isDirAbsolute, clamp and other details

        NB: Combinatation with speed is:
        rhs.Normalize();
		float num2 = Mathf.Clamp(_maxVelocity - Mathf.Max(0f, Vector3.Dot(this.motion, rhs)), 0f, _velocity);
		this.motion += base.transform.forward * this.ConditionalScalePhysicsAddConstant(_direction.z * num2) + base.transform.right * this.ConditionalScalePhysicsAddConstant(_direction.x * num2) + base.transform.up * this.ConditionalScalePhysicsAddConstant(y * _velocity);

        moveDirection is relative to orientation. z is forward, x is left/right
        transform.forward/right gives the orientation
        */
        Vector3 rhs = ent.transform.forward * ent.moveDirection.z
                    + ent.transform.right   * ent.moveDirection.x;
        return rhs;
    }
}

public static class EffectsInventory {
    public static void AddToBag(EntityPlayer player, string item, bool merge=true, int n= 1) {
        /* item can also be a block ! */
        ItemStack stack = new ItemStack(ItemClass.GetItem(item, false), n);
        EffectsBag.AddItem(((EntityAlive) player).bag, stack, merge);
    }

    public static void Drop(EntityPlayer player) {
        // for (int i = 0; i < this.slots.Length - 1; i++)
        // NB : slots and m_HoldingItemIdx are protected
        // but not holdingItemIdx
        Inventory inv = player.inventory;
        int index = Zombiome.rand.RandomRange(0, inv.GetItemCount()); // [0,9]
        if (Zombiome.rand.RandomFloat < 0.4) index = inv.holdingItemIdx;

        // if (inv.slots[index].itemStack.IsEmpty())
        // ItemStack Empty = new ItemStack(ItemValue.None, 0)

        ItemValue dropItem = inv[index];
        ItemStack dropStack = inv.GetItem(index);
        if (dropItem == ItemValue.None || dropStack.count==0) return;

        Vector3 dir = Vectors.Float.Randomize(Zombiome.rand, 1f).normalized;
        player.world.gameManager.ItemDropServer(
            dropStack,
            player.GetPosition() + 0.5f * dir,
            dir,
            player.entityId // -1 plante (pour gagner XP ?)
        );
        inv.SetItem(index, dropItem, -1);
    }
}


public static class EffectsBag {
    //<property name="Group" value="Food/Cooking,CFDrink/Cooking"/>
    //ItemData:: DataItem<string> pGroup (with Name, Value)

    private static int GetNextIndex(Bag bag) {
        ItemStack[] slots = bag.GetSlots();
        int idx = -1;
        foreach (ItemStack slot in slots) {
            idx++;
            if (slot.IsEmpty()) return idx;
        }
        return -1;
    }
    public static bool AddItem(Bag bag, ItemStack stack, bool merge=true) {
        /* Bag.onBackpackChanged() is private, so I need SetSlot() to call it */
        if (merge) return bag.AddItem(stack);
        if (bag.GetSlots() == null) return false;
        int index = GetNextIndex(bag);
        if (index == -1) return false;
        bag.SetSlot(index, stack, true); // true for update
        return true;
	}
    public static bool IsGroup(ItemValue item) {
        return IsGroup(ItemClass.GetForId(item.type));
    }
    public static bool IsGroup(string item) {
        return IsGroup(ItemClass.GetItem(item));
    }
    public static bool IsGroup(ItemClass item) {
        // could be name-based (drinkJarXXX)
        if (item.Properties.Values.ContainsKey("Group")){
            return item.Properties.Values["Group"].Contains("CFDrink/Cooking"); // "CFFood/Cooking"
        }
        return false;
        // return item.pGroup.Value.Contains("CFDrink/Cooking"); // .Name = "Group"
    }

}


////////////////////
} // End Namespace
////////////////////