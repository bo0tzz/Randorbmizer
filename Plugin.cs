using System.Collections.Generic;
using System.Linq;
using BepInEx;
using HarmonyLib;
using Relics;
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
        public static List<Relic> RelicPool;

        private static readonly string[] OrbBlacklist =
        {
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

        public static List<Relic> GetRelicPool(RelicManager relicManager)
        {
            if (RelicPool != null)
            {
                return RelicPool;
            }

            RelicPool = relicManager._commonRelicPool.relics.Union(relicManager._rareRelicPool.relics)
                .Union(relicManager._bossRelicPool.relics).ToList();
            return RelicPool;
        }

        public static void RandomizeRelics(RelicManager relicManager)
        {
            foreach (RelicEffect relic in relicManager._ownedRelics.Keys.ToList())
            {
                try
                {
                    relicManager.RemoveRelic(relic);
                }
                catch (KeyNotFoundException e)
                {
                    //This always happens, just swallow it.
                }
            }

            List<Relic> relics = GetRelicPool(relicManager)
                .OrderBy(_ => Rand.Next())
                .Take(Rand.Next(2, 5))
                .ToList();

            Debug.Log("Randomized relics to: " + relics.Select(relic => relic.effect.ToString()).Join());
            foreach (var relic in relics)
            {
                relicManager.AddRelic(relic);
            }
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
        public static void Prefix(DeckManager ____deckManager, RelicManager ____relicManager)
        {
            Plugin.RandomizeDeck(____deckManager);
            Plugin.RandomizeRelics(____relicManager);
        }
    }
}