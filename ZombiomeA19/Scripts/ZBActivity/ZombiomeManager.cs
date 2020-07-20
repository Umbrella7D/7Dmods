using System.Xml;
using System;
using System.Collections;
using UnityEngine;
using System.Collections.Generic;

using System.Linq;
using System.Reflection.Emit;




public class ZombiomeManager {
    /*
    FIXED:
    - Started twice (in what seems 2 different memory spaces !!!) -> use a buff to unicize
    - Does not terminate after game exit -> (almost) ok with PlayerDisconnect Harmony

    FIXME: clean buff refresh:
    - Limit to local player
    - buff left persistent => trigger on respawn => use it smartly

    */

    private static bool started = false;


    // Static deleguation to Instance //
    private static ZombiomeManager Instance = null;

    public static void Reset(int pid, string sel="") {
        GameManager.Instance.StartCoroutine(_Reset(pid, sel)); 
    }
    private static IEnumerator _Reset(int pid, string sel) {
        ZombiomeManager.Stop(pid, false);
        yield return new WaitForSeconds(1f);
        ZBActivity.ZombiomeActivitySelector.SelectDebug(sel);
        Zone.ClearCache();
        yield return new WaitForSeconds(0.2f);
        ZombiomeManager.Start(pid);
    }

    public static void Start(int entityId) {
        /// Not sufficient, 2 instances are running, let's try a buff
        Debug.Log("ZombiomeManager started: " + started.ToString());
        if (started) return;
        Instance = new ZombiomeManager();
        Instance.Run();    // TODO ref bool only once ?
    }

    public static void Stop(int entityId, bool forever) {
        Printer.Print("ZombiomeManager Stopping", entityId, Instance);
        if (Instance == null) Printer.Print("ZombiomeManager Stop, but Instance is null");
        else {
            ZombiomeManager prev = Instance;
            prev.Stop(forever);
        }
        Instance = null;
    }

    // Non-static part //
    private Coroutine MainCoroutine = null;

    public void Stop(bool forever) {
        if(MainCoroutine == null) {
            Printer.Print("ZBM: MainCoroutine not found, could not stop");
            return;
        }
        Zombiome.Routines.Stop(forever);
        GameManager.Instance.StopCoroutine(MainCoroutine);
    }
    public void Run() {
        // MainCoroutine = GameManager.Instance.StartCoroutine(Running());
        MainCoroutine = Zombiome.Routines.Start(Running(), "ZombiomeManager");
        Printer.Print("ZombiomeManager Run started", MainCoroutine);
    }
    public IEnumerator Running() {
        // I need yield the update (eg block process during ZB generation)
        while(true) {
            yield return Update();
            yield return new WaitForSeconds(Zombiome.FrequencyManager);
            // This 10 sec ! compare to neverending effects (ghost)
            // are ghost never eneding orperiodic slow ?
            // can I give lifetime to ghost, and regen at same speed ?
            // not directly with the lifetime of EntityItem, but can use damage over time !
        }
    }

    private List<EntityPlayerLocal> GetPlayers() {
        // TODO: not at every iteration ?!
        List<EntityPlayerLocal> players = GameManager.Instance.World.GetLocalPlayers();
        // there may be multiple clusters ... let's assume 2 (local) players maximum ...
        if (players.Count <= 1) return players;
        float dist = (players[1].GetPosition() - players[0].GetPosition()).magnitude;
        if (dist <= 3 * ZChunk.size) {
            List<EntityPlayerLocal> rdm = new List<EntityPlayerLocal>();
            if (Zombiome.rand.RandomFloat < 0.5) rdm.Append(players[0]);
            else rdm.Append(players[1]);
            return rdm;
        }
        else return players;
    }

    public IEnumerator Update() {
        /** Retest game ended whenever we iterate */
        /* This routine should not bother on the initial playerId */

        // Printer.Print("ZombiomeManager update");
        // EntityPlayerLocal player = GameManager.Instance.World.GetLocalPlayers()[0];
        if (GameManager.Instance == null || GameManager.Instance.World == null) {
            Zombiome.ExitGame(); yield break;
        }
        List<EntityPlayerLocal> callAtplayers = GetPlayers();
        string infos = "";
        foreach(EntityPlayerLocal player in callAtplayers) {
            if (UpdateCluster(player)) continue;
            infos = String.Format("{0} +p{1}{2}", infos, player.EntityName, player.GetPosition());
            Zone[] currentZones = Zone.Get(player.GetPosition());
            Printer.Print("ZombiomeManager update", player, currentZones);
            foreach (Zone current in currentZones) {
                // Printer.Print("ZombiomeManager update", player, current);
                string inf = current.Next(player, player.GetPosition()); 
                infos = String.Format("{0} * {1}@{2}({3},{4})", infos, inf, current.biome.name[0], current.x, current.z);
                yield return null;
                if (GameManager.Instance == null || GameManager.Instance.World == null) {
                    Zombiome.ExitGame(); yield break;
                }
                yield return null;
            }
            if (GameManager.Instance == null || GameManager.Instance.World == null) {
                Zombiome.ExitGame(); yield break;
            }
        }
        Printer.Print("ZombiomeManager update", callAtplayers.Count, infos);
    }

    public IEnumerator _TestInstanceWait() {
        // test concluant, pas besoin de re instancier
        WaitForSeconds dt = new WaitForSeconds(3);
        for (int k=0; k<10; k++) {
            Printer.Print("TestInstanceWait (3 sec?)", k);
            yield return dt;
        }
    }

    private Dictionary<int,PlayerClusterState> PlayerClusters = new Dictionary<int,PlayerClusterState>();
    private bool UpdateCluster(EntityPlayerLocal player) {
        if (! PlayerClusters.ContainsKey(player.entityId)) {
            PlayerClusterState Insert = new PlayerClusterState(player);
            PlayerClusters[player.entityId] = Insert;
        }
        PlayerClusterState State = PlayerClusters[player.entityId];
        State.inner_state.MoveNext();
        return State.stopped;
    }
    /* Each local player has a different state (stopped, stop, index) */
    public class PlayerClusterState {
        public int id;
        public EntityPlayerLocal player;
        public bool stopped = false;
        public IEnumerator inner_state;
        public PlayerClusterState(EntityPlayerLocal player) {
            this.player = player;
            this.id = player.entityId;
            inner_state = StepPlayer();
        }
        public IEnumerator StepPlayer() {
            while (true) {
                bool ok = true;
                foreach (EntityPlayer other in GameManager.Instance.World.Players.list) {
                    // let's base order on id instead of name ?
                    // if (other.entityId == player.entityId) continue;
                    if (other.entityId >= this.id) continue; // "smallest" player in cluster
                    Vector3 pother = other.GetPosition();
                    Vector3 ppos = this.player.GetPosition();
                    if (Math.Abs(pother.x - ppos.x) + Math.Abs(pother.z - ppos.z) < 50) {
                        ok = false;
                        this.stopped = true;
                    }
                    yield return null;
                }
                if (ok) this.stopped = false; // re run when all players have been checked !
                yield return null;
            } 
        }

    }

}