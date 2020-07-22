using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Linq;
using System.Reflection.Emit;

using CSutils;
using SdtdUtils;
using UnityEngine;

public class ConsoleCmdExplShowExplo : ConsoleCmdAbstract {
   // Token: 0x060012FC RID: 4860 RVA: 0x00077EB4 File Offset: 0x000760B4
   public override string GetDescription() {return "ConsoleCmdExplShowExplo";}
   public override string[] GetCommands() {
       return new string[] {"she"};
   }

    /* A19
        1: +etincelle et debris rose
        2:  a18
        3: petite gerbe + debris rose
        4: explo de voiture avec carcasse !
        5: yellow/orange expl, black smoke
        6 :idem, plus grosse
        7: vomit,
        8: gore block explo
        9: like 6 : sans les flammes

        10: molotov (fire ball when in the air)
        11-12: yellow etincelles only, no smoke
        13: orange boom no smoke, like 1
        14-19: crée un bout de bidons détruit
            16: 3 bidons
        14: le bout part en l'air avec une trainée de flamme ("napalm")
        15/ 16: additionnal "napalm"
    */


    /* A18
        1: petite explo (mine qui pete, small smoke trail only, no fire)
        2: med/Large explo orange / rouge + etincelles (pas vraiment explo)
        3: redish fire, flamme rouge montante + bcp black smoke
        4: redish idem, larger
        5: yellow/orange expl, +bcp black smoke
        6 :idem
        7: vomit, 8: gore block explo
        9: like 6

        10: molotov (fire ball when in the air)
        11-12: yellow etincelles only, no smoke
        13: orange boom no smoke, like 1
        14-19: crée un bout de bidons détruit
        14: le bout part en l'air avec une trainée de flamme ("napalm")
        15/ 16: additionnal "napalm"
    */

    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
        Printer.Print("ConsoleCmdExplShowExplo", _params);
        EntityPlayerLocal player = GameManager.Instance.World.GetLocalPlayers()[0]; 

        if (_params[0] == "p") {
            Color color = Color.white;
            if (_params.Count >=3 ) {
                string[] split = _params[2].Split(',');
                float[] def = new float[]{0,0,0,1};
                for( int k=0; k<split.Length; k++ ) def[k] = float.Parse(split[k]);
                color = new Color(def[0], def[1], def[2], def[3]);
            }
            Printer.Print(color);
            ExecuteParticle(_params[1], player.GetPosition() + new Vector3(0,0,6), color);
            return;
        } else if (_params[0] == "pc") {
            Color color;
            color = new Color(1f, 0f, 0f);
            ExecuteParticle(_params[1], player.GetPosition() + new Vector3(-3,0,6), color);
            color = new Color(0f, 1f, 0f);
            ExecuteParticle(_params[1], player.GetPosition() + new Vector3(0,0,6), color);
            color = new Color(0f, 0f, 1f);
            ExecuteParticle(_params[1], player.GetPosition() + new Vector3(3,0,6), color);
            return;        
        } else if (_params[0] == "pa") {
            ExecuteParticleAttach(_params[1], player.GetPosition());
            return;
        } else if (_params[0] == "pg") {
            ExecuteParticleGhost(_params[1], player.GetPosition());
            return;
        }
        else if (_params[0] == "s") {
            float intens = 1f;
            if (_params.Count > 0) intens = float.Parse(_params[1]);
            ExecuteScreenEffect(_params[1]);
            return;
        }
        int ei = int.Parse(_params[0]);

        Emplacement place = Emplacement.At(player.GetPosition() + new Vector3(0,1,10), Vectors.Float.Zero); // N

        ItemClass itemClass = ItemClass.GetItemClass("thrownAmmoMolotovCocktail", false);
        DynamicProperties baseProps = itemClass.Properties;
        ExplosionData ed = new ExplosionData(baseProps); 

        ed.ParticleIndex = ei;
        GameManager.Instance.ExplosionServer(0, place.position, place.ipos, Quaternion.identity, ed, player.entityId, 0.1f, false, null); // try -1
   }

    public void ExecuteScreenEffect(string name, float intens=1f) {
        EntityPlayerLocal player = GameManager.Instance.World.GetLocalPlayers()[0]; 
        player.ScreenEffectManager.SetScreenEffect(name);
        GameManager.Instance.StartCoroutine(Routines.Call(
            () => player.ScreenEffectManager.SetScreenEffect(name, intens),
            4f
        ));
        /*
        Blur : flou
		Bright : brillant lumineux
		Cold : contour ecran
		Dark : contour ecran
		Dead (no intensity) : contour ecran
		Distortion : taré !
		Drunk
		Dying (no intensity)
		Hot : contour ecran
		Posterize :  effets de gamma bizarre ?
		Underwater
		Vibrant : lumineux 
		VibrantDeSat
        */
    }

   public void ExecuteParticle(string name, Vector3 pos, Color color) {
        if (name.StartsWith("p_")) name = name.Substring(2);
        Printer.Print("ConsoleCmdExplShowExplo ParticleEffect", name);
        float lightValue = GameManager.Instance.World.GetLightBrightness(Vectors.ToInt(pos)) / 2f;
        ParticleEffect pe = new ParticleEffect(
            name, pos, lightValue,
            color, "electric_fence_impact", null, false
        ); //2 e string is sound
        GameManager.Instance.SpawnParticleEffectServer(pe, -1);

   }

    public void ExecuteParticleAttach(string name, Vector3 pos) {
        if (name.StartsWith("p_")) name = name.Substring(2);
        EntityPlayerLocal player = GameManager.Instance.World.GetLocalPlayers()[0]; 
        Printer.Print("ConsoleCmdExplShowExplo ParticleEffect", name);
        float lightValue = GameManager.Instance.World.GetLightBrightness(Vectors.ToInt(pos)) / 2f;

        ParticleEffect pe = new ParticleEffect(
            name, player.getHeadPosition(),
            lightValue, Color.white,
            null, player.transform, false
        );
        GameManager.Instance.SpawnParticleEffectServer(pe, player.entityId);
   }

/*

p_smoke : warning, toujours ...

p_candleWall_flame : aura

p_treeGib_winter : fall from above, cool freeze effect ?

-- Ground candidates

p_treeGib_dead_01.prefab
		p_treeGib_dead_02.prefab: ok

        p_treeGib_birch_small: feuilles 

        smoke & feuille p_treeGib_birch_6m


        p_paint_block: bouts de bois blanc/metal

        p_paint_splash2: petite fumée blanche


    p_impact_metal_on_snow, p_impact_bullet_on_snow (+gros)


    p_impact_wood_on_metal: petite fumée noire

    p_impact_bullet_on_earth: petite fumée noire
    p_impact_metal_on_earth+
    p_impact_stone_on_earth++
    p_impact_wood_on_earth plus étendue moins dense

    p_impact_wood_on_plant: pareil
    p_impact_stone_on_plant+++

    p_impact_wood_on_water largest

    p_impact_stone_on_xt_tallgrass quelques débris gris

    p_blockdestroy_boulder. grosse fumee et pierres
    p_blockdestroy_earth: unbloc tourne un moment

    p_blockdestroy_metal: petite etincelle
    p_blockdestroy_snow: boules de neige
    p_blockdestroy_stone: fumée et petite pierre

    p_blockdestroy_xt_leaves1: quelques feuilles
    p_blockdestroy_xt_tallgrass: quelques brindilles

*/

/* Particles, see XML.txt "p_"
        ---------
        *** no warning no entity:

        p_fire_small : flamme jaune, no smoke
        p_forge: base brulante
        burning_barrel: flamette et fumée



        peu visibles
        p_hotembers
        p_hotembersZombie

        p_electric_fence_sparks
        p_electric_shock

        signal_flarePrefab : pas mal

        candleWall_flame lueur infinie non retirée on death ?
        critical

        sandstorm : ca fait un brouillard !
        smokestorm
        snowstorm1:b brouillard blanc

        wire_tool_sparks (la boule)

        SharkNato : nuée de poissons dans les airs ...

        p_torch_wall : aura lumineuse jaune visible

        blockdestroy_hearth, blockdestroy_snow : petits mvt ausol

        *** avec W sur entity:

        p_big_smoke
        p_smoke
        burntZombieSmoke



        -------

        big_smoke: fini, court, warning meme sur G

        p_smoke.prefab : feu jaune, Inf, W
		p_smokestorm.prefab
		p_snowstorm1.prefab

        p_electric_fence_sparks.prefab : boule en l'air, sparks au sol ,finie, NW
		p_electric_shock.prefab : petite boule, infinie

		p_fat_cop_explosion.prefab : il reste des bouts de corps
		pFire_small.prefab
		p_forge : boule+fumée, infinie, NW
		p_generator.prefab : minuscule fumée noire
		p_hotembers : petite particules rouges
		p_hotembersZombie.prefab : infini

        fire_small : petit feu jaune, inf, NW
        burning_barrel : flamme jaune au sol, inf, NW

        candleWall_flame: small white light aura, inf, NW
        critical : blood explo, fin

        impact_ : tout petit
        impact_metal_on_metal : tout petit feu d'artifice, spot

        paint_splash : petite fumée, spot - A19: tres blanche/etincelle

        rocketLauncherFire : spot, petite explo blanche, NW a19: jaune

        sandstorm: degradé aérien leger, inf

        signal_flarePrefab: black smoke, inf, (NW - pas sur ??)

        smoke: boule de feu jaune. gives yellow warning, inf
        smokestorm: W, brouillard
        snowstorm1: W
        supply_crate_impact: black smoke, small, spot
        treefall: grosse fumée noir étalée, spot - A19: bcp moins grande

        treeGib_birch_15m: fumée grise, spot a19: rougeatre + feuille

        wire_tool_sparks: boule electrique, spot - a19: etincelle petit cyan

        treeGib_winter01: chute de feuille et neige, fin

        p_treeGib_maple_17m : feuille tombante presque jaunes

        treeGib_sapling : juste fumée, petite
        treeGib_small_dust : juste fumée, plus grande, jolie

        p_treeGib_winter_XXX : feuilles qui tombe puis neige

        treeGib_dead_01: fuméee noir et qqs feuilles vertes

        treeGib_burnt_small: avec des débris et feuilles en plus
        p_treeGib_burnt_XXX: avec des débris en plus

        treeGib_birch_small: + petites feuilles vertes (bcp)
        treeGib_birch_XXX
        treeGib_birch : + jaunes


        supply_crate_impact : fumée noir, pas haute
        supply_crate_gib_Prefab: juste du bois

        signal_flarePrefab : grande fumée, inf


        -------
        No warnings:

        blockdestroy_ : block size, petit

        blockdestroy_boulder : pas mal
        blockdestroy_plant: toute petite fumée
        hearth, snow (boules de neige): pas mal

        * Coloriable
        p_treeGib_small_dust : size ok !
        electric_fence_sparks (not in the p_ list of xml !)
        treeGib_sapling : small
        p_tiresmoke : too small

*/

/*
finite: retrigger
infinite: call and stop when intensity = 0 ?
*/

    static class GhostParticle {
        // static string nameForGhosts = "";
        static Coroutine runningGhost = null;
        private static bool _StopGhost = false;
        private static string currentName = "";
        private static long lastExec = -1;
        public static IEnumerator _Ghosts(Vector3 ppos) {
            /* Using bear ghost creates Warning, when ZMoe ghost does not ...
            but moe stops moving ?
            */
            //EntityPool EntityPool = new EntityPool("zombieMoeGhost", 20);
            // EntityPool EntityPool = new EntityPool("animalBearGhost", 20);
            // EntityPool EntityPool = new EntityPool("animalChickenGhost", 20);
            EntityPool EntityPool = new EntityPool("animalChickenGhostV2", 20);

            HashSet<Entity> done = new HashSet<Entity>();
            lastExec = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            while (true) {
                lastExec = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                Bounds area = new Bounds(ppos, new Vector3(40, 40, 40));
                EntityPool.Update(area);
                foreach(Entity entity in EntityPool.Entities) { 
                    lastExec = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    if (entity == null) continue;
                    if (entity as EntityAlive == null) continue;
                    if (_StopGhost) {
                        Attaching.Remove(entity as EntityAlive, currentName);
                        // if (done.Contains(entity)) done.Remove(entity);
                        done.Clear();
                    } else {
                        if (done.Contains(entity)) continue;
                        Attaching.Apply(entity as EntityAlive, currentName);
                        done.Add(entity);
                    }
                    lastExec = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                    yield return new WaitForEndOfFrame();
                }
                lastExec = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                yield return new WaitForEndOfFrame();
            }

        }

        public static IEnumerator ExecuteParticleGhost(string name, Vector3 pos) {
            if (runningGhost == null) {
                _StopGhost = false;
                currentName = name;
                runningGhost = GameManager.Instance.StartCoroutine(_Ghosts(pos));
            } else if(DateTimeOffset.UtcNow.ToUnixTimeSeconds() - lastExec > 6) {
               Printer.Print("GhostParticle restart routine"); // en cas d'erreur
                _StopGhost = false;
                currentName = name;
               runningGhost = GameManager.Instance.StartCoroutine(_Ghosts(pos));
            } else {
                _StopGhost = true;
                Printer.Print("GhostParticle clearing") ;
                yield return new WaitForSeconds(2.5f);
                currentName = name;
                _StopGhost = false;
                Printer.Print("GhostParticle re apply") ;
            }
        }
    }

    public void ExecuteParticleGhost(string name, Vector3 pos) {
        EntityPlayerLocal player = GameManager.Instance.World.GetLocalPlayers()[0]; 
        GameManager.Instance.StartCoroutine(GhostParticle.ExecuteParticleGhost(name, player.GetPosition()));
    }








}