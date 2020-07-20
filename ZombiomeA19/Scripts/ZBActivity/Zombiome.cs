using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Linq;
using System.Reflection.Emit;



public class Zombiome {
    /** Start and manage Zombiome coroutines

    Zombiome heavily relies on coroutines (instead of buff.onSelfUpdate callback).
    It is orchestrated by ZombiomeManager.Instance.update, but someone need to start it, and once !

    StartOnPlayerSpawn and OnDisconnect are managed by Harmony patches.
    When harmony is available (I cant make it trigger on non-host player !!), the buff allows for start,
    but stop is not applied and all exceptions of handling ZB routines are swallowed.

    MP:
    - Currently, each client should start its own routine (so twice more activity when players are close)
    - Some weird MP effect with entities (giant, ghost)
        -> we should protect (islocal or different ghost classes)
        -> make sure we recheck null when given runtime.
    - AtPlayer effects should use World.Players and check distance, instead of local players


    */

    /* Options parsed by MinEventActionZBManagerOption. Some are modifiable by commands */
    public static int Log = -1;
    public static bool AutoStart = false;
    public static bool nz4 = false;
    public static bool buffUnicization = false;
    public static bool IgnoreScale = false;
    public static float FrequencyManager = 2;
    public static bool SwallowError = false;


    public static GameRandom rand;
    public static string worldSeed;
    public static int worldSize;
    public static CSutils.Routines Routines = new CSutils.Routines();

    private static bool IsInit = false; // prevent on 2nd game ?
    public static void Init(int playerId) {
        /*
        Should be safe to do multiple calls ...
        - Depends on Map so need re apply on game change
        - Should only be called once per game (mostly because of thread start)

        - Check: multiplayer local
            - Called twice with 2 local players -> if below
            - Each player triggers on connect ?
         */

        /* I should put this all in the buff ?? Anythin here may be duplicate */
        if (IsInit) {
            Printer.Write("Zombiome is already init !", playerId);
            // if (AutoStart) {
            //     ZombiomeManager.Reset(playerId, "");
            // }
            return;
        }

        Printer.Write("Zombiome.Init():", playerId);

        CSutils.Catcher.SwallowErrors = SwallowError;

        rand = GameRandomManager.Instance.CreateGameRandom();
        worldSeed = GamePrefs.GetString(EnumGamePrefs.WorldGenSeed);
        worldSize = GamePrefs.GetInt(EnumGamePrefs.WorldGenSize);

        ZBiomeInfo.Define();

        if (nz4) Zone.Get = Zone.GetFour;
        else Zone.Get = position => new Zone[]{Zone.GetSingle(position)};
        ZBActivity.ZombiomeActivitySelector.Initialize();

        if (AutoStart) {
            ZombiomeManager.Reset(playerId, "");
        }
        IsInit = true; // bugs on restart new game coz no one set to none - use World hash ?

        Printer.Write("Zombiome..Init: Done");
        // ZombiomeManager.Start(playerId); // manually until release 
    }

    public static void StartOnPlayerSpawn(ClientInfo _cInfo, RespawnType _respawnReason, Vector3i _pos, int playerId) { 
        /* Careful, may be called multiple times
        - If some DMT patch use ApplyAll()
        - On respawn and on teleport (=> check RespawnType)
        - On multiple local players
        */
        Printer.Write("Zombiome..StartOnPlayerSpawn:", _cInfo, _respawnReason, _pos, playerId);
        Routines.Start(); // in case we left a previous game
        if (buffUnicization) GameManager.Instance.StartCoroutine(setBuff(playerId));
        else Init(playerId);
        Printer.Write("Zombiome.StartOnPlayerSpawn Done. Unicize=", buffUnicization);
    }

    public static void OnDisconnect() {
        /* Done: quit game and restart another
        - Don't stop "forever"
        - Allow bools to restart

        */
        EntityPlayerLocal player = GameManager.Instance.World.GetLocalPlayers()[0];
        ZombiomeManager.Stop(player.entityId, true);
        IsInit = false;
    }

    public static void ExitGame() {
        /* Done: quit game and restart another
        - Don't stop "forever"
        - Allow bools to restart

        */
        ZombiomeManager.Stop(-1, false);
        IsInit = false;
    }

    private static IEnumerator<object> setBuff(int entityId) {
        yield return SdtdUtils.EntityCreation.WaitEntity(entityId);
        EntityAlive player = GameManager.Instance.World.GetEntity(entityId) as EntityAlive;
        string buffname="ZombiomeManager";
        Printer.Print("Applying buff ", buffname, player);
        if(! player.Buffs.HasBuff(buffname)) player.Buffs.AddBuff(buffname);
   }

}