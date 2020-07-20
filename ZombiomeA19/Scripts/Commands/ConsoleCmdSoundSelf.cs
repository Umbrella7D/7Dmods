using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Linq;
using System.Reflection.Emit;



public class ConsoleCmdSoundSelf : ConsoleCmdAbstract {
    /** Does not emit noise, contrary to
        public void PlaySoundAtPositionServer(Vector3 _pos, string _audioClipName, AudioRolloffMode _mode, int _distance, int _entityId)
	
    */
    public override string GetDescription() {return "ConsoleCmdSoundSelf";}
    public override string[] GetCommands() {
        return new string[] {"ssound"};
    }
    public override void Execute(List<string> _params, CommandSenderInfo _senderInfo) {
        EntityPlayerLocal player = GameManager.Instance.World.GetLocalPlayers()[0]; 
        string sound = _params[0];
        Printer.Print("ConsoleCmdBuffSelf", _params, sound);
        // Audio.Manager.PlayInsidePlayerHead(sound, player.entityId); // 0f, false, false // -1 isentityID, could be player's
        // local player only (Vs. Manager.BroadcastPlay)
        Audio.Manager.Play(player, sound, 1f, false);
        // NB: sound uses # and $ for 1/2 and male/female
   }


}


/*
        for molotov
        <SoundDataNode name="FireMediumLoop"> <AudioSource name="Sounds/AudioSource_Amb_Small"/>
            <AudioClip ClipName="Sounds/Ambient_Loops/aFire_med_lp" Loop="true"/>
            <LocalCrouchVolumeScale value="1.0"/> <CrouchNoiseScale value="0.5"/> <NoiseScale value="1"/> <MaxVoices value="18"/> <MaxRepeatRate value="0.001"/> </SoundDataNode>

        sounds:
        stepglasssmallcreak
        stepmetalbigcreak (poubelle deplacée)
        stepwoodsmallcreak, stepwoodbigcreak
*       loops: fires,
            NeonSignLP, FridgeLP
            bruit de drapeau/vent: FlagLP
            flies_lp ; pourait etre vent

            tous les club_swinglight / heavy
            stunbaton_hit1: elec
            stunbaton_hit5: sparkle
            FlagLP
            stunbaton_charged_lp: loop

            player_swim, water_emerge, waterfallinginto

            avalanche !
            keystone_destroyed

            petit bruit: pistol_remove_clip,blunderbuss_reload_part_04,blunderbuss_reload_part_03 (+sourd)
                        + click:blunderbuss_reload_part_02
                        ak47_reload_part_01
                        m60_reload_part_07
                        weapon_jam
                        weapon_unholster
                        boarattack

                        rabbitdeath: grincant, rabbitpain
                        snakealert

                        playerlandlight


                        clothdestroy
                        grazearmormetal
                        entityhitsground

                        close_apache_artifact_chest
                        player_bandage
                        paint
                        trunkbreak

                             weirdo:
                        keystone_build_warning
                        password_set

                        read_mod

                        metalhitmetal etc

        icons: fireman almanac (joli feu)
        attr  perception / general percep /infiltrator : eye (lighened)
        */