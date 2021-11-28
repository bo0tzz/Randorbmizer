using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using UnityEngine;

namespace PeglinRandorbmizer
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    [BepInProcess("Peglin.exe")]
    public class Plugin : BaseUnityPlugin
    {
        private readonly Harmony harmony = new(PluginInfo.PLUGIN_GUID);
        
        private static System.Random Rand = new();
        
        public static GameObject[] OrbPool;

        private static readonly string[] OrbBlacklist = {
            "Orb",
            "Orb Variant",
            "Oreb-Lvl1",
            "NavigationOrb",
            "DisplayPrefab"
        };

        private void Awake()
        {
            GameObject[] orbs = Resources.LoadAll<GameObject>("Prefabs/Orbs/");
            GameObject[] availableOrbs = orbs.Where(o => !OrbBlacklist.Contains(o.name)).ToArray();

            OrbPool = availableOrbs;
            
            harmony.PatchAll();
            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_NAME} is loaded!");
        }
        
        public static void RandomizeDeck(DeckManager deckManager)
        {
            List<GameObject> orbs = OrbPool.OrderBy(_ => Rand.Next()).Take(Rand.Next(2, 5)).ToList();
            string pickedOrbNames = orbs.Select(GetName).Join();
            Debug.Log("Randomized deck to orbs: " + pickedOrbNames);
            deckManager.InstantiateDeck(orbs);
        }
    }

    [HarmonyPatch(typeof(DeckManager), "ShuffleBattleDeck")]
    public class DeckManagerPatch
    {
        public static bool Prefix(DeckManager __instance)
        {
            Plugin.RandomizeDeck(__instance);
            __instance.ShuffleCompleteDeck();
            return false;
        }
    }

    [HarmonyPatch(typeof(BattleController), "Start")]
    public class BattleControllerPatch
    {
        public static void Prefix(DeckManager ____deckManager)
        {
            Plugin.RandomizeDeck(____deckManager);
        }
    }
}