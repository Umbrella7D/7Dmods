using System;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;

using CSutils;
using SdtdUtils;

// Token: 0x0200038B RID: 907
public class EntityGhost : EntityAlive {
	/** Manage an invisible entity
	- Some effects require an arbitrary controler to work (eg drop ItemStack and projectiles)
	- Allow to use "wild" buff
	*/
	public string OnContact = "FallGhostOnContact";
	public override void VisiblityCheck(float _distanceSqr, bool _masterIsZooming) {
		this.emodel.SetVisible(false, false); // public EModelBase emodel
	}

    /* Protection */
    public override int DamageEntity(DamageSource _damageSource, int _strength, bool _criticalHit, float impulseScale = 1f) {
		return base.DamageEntity(_damageSource, _strength, _criticalHit, impulseScale); // <=> this.setBeenAttacked(); return 0;
	}
    public override void OnDamagedByExplosion() {}
	
	protected override void fallHitGround(float _v, Vector3 _fallMotion) {
		base.fallHitGround(_v, _fallMotion);
		if (this.Buffs != null && this.Buffs.HasBuff("buffZBExploseOnFall"))
				MinEventActionExplodeEntity.Trigger(this);
	}
	
	public static Type[] Concretes = new Type[]{
		typeof(EntityGhost0),typeof(EntityGhost1),typeof(EntityGhost2),typeof(EntityGhost3),
		typeof(EntityGhost4),typeof(EntityGhost5),typeof(EntityGhost6),typeof(EntityGhost7),
		typeof(EntityGhost8),typeof(EntityGhost9)
	};
	public static int ConcreteIdx(int x, int y) {
		// Expects zone center
		int i = (x / Zone.ZoneSize) % 3;
		int j = (y / Zone.ZoneSize) % 3;
		return Math.Abs(i+3*j);
	}

	public static IEnumerator Create(GhostData data, Emplacement place, string entityCls) {
		/** Create a Ghost with buffs, death-fragment (string cvar) and slowfall */
		// all ghost are lost at +100 and 3s
		if (data.drop > 0) place.position.y = place.position.y + 40;
		Entity[] Tracker = new Entity[]{null};
		Entity Requested = EntityCreation.Spawn(place.position, entityCls);
        yield return EntityCreation.WaitEntity(Requested, 1.5f, Tracker);
		EntityAlive created = Tracker[0] as EntityAlive;
		if (created == null) {
			// Printer.Print("EntityGhost. Null:", Tracker[0]);
			yield break;
		}
		// Printer.Print("EntityGhost.Create:", created);
		if (data.deadFragment != "") {
			// created.SetCVar("ZBvalue_buffZBexplosif", StringMap.Encode(data.deadFragment));
			created.SetCVar(MinEventActionExplodeEntity.cvarKey, StringMap.Encode(data.deadFragment));
			//Printer.Print("EntityGhost.Create:", "ZBvalue_buffZBexplosif", StringMap.Encode(data.deadFragment));
		}
		foreach (string buff in data.buffs) {
			created.Buffs.AddBuff(buff);
			//Printer.Print("EntityGhost.Create:", buff, created);
		}
		if (data.drop > 0) {
			//Printer.Print("EntityGhost.SlowFall:", data.drop, created);
			EntityMover.SlowFall.Start(Tracker[0], data.drop, 0);
		}
	}
}

/* Different concrete class used as flags:
- Currently, each zone identifies its own ghost (not to mix up buffs)
- Might be necessary to have each player identify their own ghost ?
*/
public class EntityGhost0 : EntityGhost {}
public class EntityGhost1 : EntityGhost {}
public class EntityGhost2 : EntityGhost {}
public class EntityGhost3 : EntityGhost {}
public class EntityGhost4 : EntityGhost {}
public class EntityGhost5 : EntityGhost {}
public class EntityGhost6 : EntityGhost {}
public class EntityGhost7 : EntityGhost {}
public class EntityGhost8 : EntityGhost {}
public class EntityGhost9 : EntityGhost {}


public class GhostData {
    /* 3 main type, is its structuring ?

    - ghost: wander around, aura, jump

    - bomb: run around and explode on contact (TODO:via aura) or dt

    - bolt: fall fast from sky and explode

    - ? slow fall + explode or persist / bomb ?
    */
    public string entity;
    public float intensity; // manual motion/jump strength
    public float drop = -1f;
    public string name;
    public string[] buffs;
	public string deadFragment="";
    /* FromAir : -1: ground, 0: air and do nothing, >0: from air and OnImpact */
    // public int FromAir;

	public static Dictionary<string,GhostData> Ghosts = new Dictionary<string,GhostData>();

    public GhostData(string name, float intensity, float drop, string buffs, string deadFragment="") {
        this.name = name;
        this.intensity = intensity;
        this.drop = drop;

		// if (deadFragment != "") buffs = String.Join(",", buffs, "buffZBdeathFragment");
		buffs = String.Join(",", buffs, "buffZBRespiteKeep");
		this.buffs = buffs.Split(',');
		this.deadFragment = deadFragment;	

		Ghosts[name] = this;
    }
	public override string ToString() {return name == null ? "GhostData?" : name;}
	/*
	fbolt fghost fbomb
	ebolt eghost ebomb
	ybolt yghost ybomb
	lbolt lbomb
	bbol

	GhostData.fbolt,1f,   GhostData.fghost,1f,   GhostData.fbomb,1f,
	GhostData.ebolt,1f,   GhostData.eghost,1f,   GhostData.ebomb,1f,
	GhostData.ybolt,1f,   GhostData.yghost,1f,   GhostData.ybomb,1f,
	GhostData.lbolt,1f,   GhostData.lbomb,1f,
	GhostData.bbolt,1f 
	 */
	/** Bolts: fall from the sky and explode */
	public static GhostData fbolt = new GhostData("fbolt", 0f, 0.3f,
        "buffZBFiring,buffTinyGhostFix,buffZBExploseOnFall", "ZBProj_boom");
	public static GhostData ebolt = new GhostData("ebolt", 0f, 0.3f,
        "buffZBShoking,buffTinyGhostFix,buffZBExploseOnFall", "ZBProj_spark");
	public static GhostData lbolt = new GhostData("lbolt", 0f, 0.2f,
        "buffZBFlamette,buffZBExploseOnFall", "ZBProj_impact");

	public static GhostData bbolt = new GhostData("bbolt", 0f, 0.2f,
        "buffZBBurning,buffZBExploseOnFall", "ZBProj_boomr"); // buffTinyGhostFix not supported here too
	public static GhostData ybolt = new GhostData("ybolt", 0f, 0.2f,
        "buffZBYellowFire,buffTinyGhostFix,buffZBExploseOnFall", "ZBProj_boomy");


	/** Ghosts: long lifetime, moves and jump */
	public static GhostData fghost = new GhostData("fghost", 2f, -1f,
		"buffZBJumpStrong,buffZBFiring,buffTinyGhostFast");
	public static GhostData eghost = new GhostData("eghost", 2f, -1f,
		"buffZBJumpStrong,buffZBShoking,buffTinyGhostFast");
	public static GhostData yghost = new GhostData("yghost", 2f, -1f,
		"buffZBJumpStrong,buffZBYellowFire,buffTinyGhostFix,buffZBLighten");

	
	// public static GhostData yfghost2 = new GhostData("yfghost2", 2f, -1f,
	// 	"buffZBBurning,buffTinyGhostFix,buffZBLighten,buffZBLighting");

	/** Bombs: medium lifetime, explode on contact, may move/jump */
	public static GhostData fbomb = new GhostData("fbomb", 2f, -1f,
        "buffZBFiring,buffTinyGhost,buffZBexplosif,buffZBRespite30", "ZBProj_boom");
	public static GhostData ebomb = new GhostData("ebomb", 0f, -1f,
        "buffZBShoking,buffZBJumpWeak,buffZBexplosif,buffTinyGhostFast,buffZBRespite20", "ZBProj_spark");
	public static GhostData lbomb = new GhostData("lbomb", 2f, -1f,
        "buffZBFlamette,buffZBexplosif,buffZBRespite30", "ZBProj_impact");
	public static GhostData ybomb = new GhostData("ybomb", 0f, -1f,
        "buffZBYellowFire,buffZBexplosif,buffTinyGhost,buffZBRespite20", "ZBProj_boomy");
	


	public static GhostData lightEffect = new GhostData("lightEffect", 0f, -1f, "buffZBPartCandleLight,buffTinyGhostFix");
	public static GhostData csmokeEffect = new GhostData("csmokeEffect", 0f, -1f, "buffZBPartSmokeColumn,buffTinyGhostFix");
	public static GhostData ssandEffect = new GhostData("ssandEffect", 0f, -1f, "buffZBPartSandStorm,buffTinyGhostFix");
	public static GhostData ssmokeEffect = new GhostData("ssmokeEffect", 0f, -1f, "buffZBPartSmokeStorm,buffTinyGhostFix");
	public static GhostData ssnowEffect = new GhostData("ssandEffect", 0f, -1f, "buffZBPartSnowStorm,buffTinyGhostFix");

	

	// TODO: grahics (smoke, storm ...) as Data
	/**
	NB: buffZBFlamette + buffTinyGhostFix makes particles desappear (? under groud, size min treshold ?)

	*/

	/**
	Possible in snow:
	- Candle light : graphic effect for giving attractive to player
	- Flamette : explosive, eject
	- Yellow fire: attract zombies (then boost em ?)
	- lightning and yellow spark 

	- que faire de burning (petit bout de molotov)

	TODO: projectile with biome graphic particle that make slid


	*/

}