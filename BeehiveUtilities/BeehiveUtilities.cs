using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace BeehiveUtilities
{
    [BepInPlugin("smallo.mods.beehiveutilities", "Beehive Utilities", Version)]
    [HarmonyPatch]
    class BeehiveUtilitiesPlugin : BaseUnityPlugin
    {
        public const string Version = "1.1.0.1";
        private static ConfigEntry<bool> enableMod;
        private static ConfigEntry<bool> proxCheck;
        private static ConfigEntry<bool> showAlternateText;
        private static ConfigEntry<bool> biomeCheck;
        private static ConfigEntry<bool> nightCheck;
        private static ConfigEntry<bool> beeStatus;
        private static ConfigEntry<bool> honeySpawn;
        private static ConfigEntry<int> maxHoney;
        private static ConfigEntry<double> minsCreation;

        private static int creationTime = 0;

        void Awake()
        {
            enableMod = Config.Bind("1 - Global", "Enable Mod", true, "Enable or disable this mod");
            if (!enableMod.Value) return;

            proxCheck = Config.Bind("2 - General", "Disable Proximity Check", false, "Disables the \"Bees need more space\" check");
            showAlternateText = Config.Bind("2 - General", "Show Alternate Text", true, "Show the amount of honey generated next to the Beehive name \"Honey ( 0/4 )\" instead of \"( EMPTY )\" or \"( Honey x honeyAmount )\"");
            maxHoney = Config.Bind("2 - General", "Max Honey", 4, "The maximum amount of honey a beehive can generate (default is 4)");
            minsCreation = Config.Bind("2 - General", "Minutes Per Creation", 20.0, "The amount of minutes it takes to generate 1 piece of honey (default is 20)");
            biomeCheck = Config.Bind("2 - General", "Remove Biome Check", false, "Allows beehives to work in any biome");
            nightCheck = Config.Bind("2 - General", "Remove Night/Rain Check", false, "Allows beehives to work at night and during rain");
            beeStatus = Config.Bind("2 - General", "Display Bee Status On Hover", true, "Shows the bee status on hover instead of having to press E, this also shows when there is honey in the hive");
            honeySpawn = Config.Bind("2 - General", "Spawn Honey In Front", false, "Spawns the honey in front of the hive instead of on top of it. Here is a picture to show which way is the front of the hive, it's the side where the thatch overlaps from the sides on top. https://i.imgur.com/zK5FT47.png");

            creationTime = (int)(minsCreation.Value * 60);

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), null);
        }

        private static string LocaliseString(string text) { return Localization.instance.Localize(text); }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Beehive), "UpdateBees")]
        public static void BeehiveUpdateBees_Patch(Beehive __instance)
        {
            if (!nightCheck.Value) return;

            bool flag = __instance.CheckBiome() && __instance.HaveFreeSpace();
            __instance.m_beeEffect.SetActive(flag);
            if (!__instance.m_nview.IsOwner() || !flag)
                return;
            float timeSinceLastUpdate = __instance.GetTimeSinceLastUpdate();
            float num = __instance.m_nview.GetZDO().GetFloat("product") + timeSinceLastUpdate;
            if (num > __instance.m_secPerUnit)
            {
                __instance.IncreseLevel((int)(num / __instance.m_secPerUnit));
                num = 0.0f;
            }
            __instance.m_nview.GetZDO().Set("product", num);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Beehive), "Interact")]
        public static void BeehiveInteract_Patch(Humanoid character, bool repeat, Beehive __instance)
        {
            if (beeStatus.Value)
            {
                character.Message(MessageHud.MessageType.Center, "");
                return;
            }

            if (!nightCheck.Value) return;

            if (!EnvMan.IsDaylight())
            {
                character.Message(MessageHud.MessageType.Center, "");
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Beehive), "Awake")]
        public static void BeehiveAwake_Patch(Beehive __instance)
        {
            if (honeySpawn.Value) __instance.m_spawnPoint.localPosition = new Vector3(0.8f, 0f, 0f);
            if (proxCheck.Value) __instance.m_maxCover = 1000f;
            if (maxHoney.Value != __instance.m_maxHoney) __instance.m_maxHoney = maxHoney.Value;
            if (creationTime != __instance.m_secPerUnit) __instance.m_secPerUnit = creationTime;
            if (biomeCheck.Value) __instance.m_biome = Heightmap.Biome.All;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(Beehive), "GetHoverText")]
        public static string BeehiveGetHoverText_Patch(string __result, Beehive __instance)
        {
            string result = __result;
            if (!showAlternateText.Value) return result;

            string statusToReplace = $"\n[<color=yellow><b>$KEY_Use</b></color>] $piece_beehive_check";
            string honeyString = LocaliseString(__instance.m_honeyItem.m_itemData.m_shared.m_name);
            string EMPTY = LocaliseString($"( $piece_container_empty )");
            string honeyCount = LocaliseString($"\n{honeyString} ( {__instance.GetHoneyLevel()} / {__instance.m_maxHoney} )");
            string hasHoney = LocaliseString($"( {honeyString} x {__instance.GetHoneyLevel()} )");

            if (beeStatus.Value)
            {
                string status;
                if (!__instance.CheckBiome()) status = "<color=red>$piece_beehive_area</color>";
                else if (!__instance.HaveFreeSpace()) status = "<color=red>$piece_beehive_freespace</color>";
                else if (!EnvMan.IsDaylight() && !nightCheck.Value) status = "<color=yellow>$piece_beehive_sleep</color>";
                else status = "<color=lime>$piece_beehive_happy</color>";

                result = result.Replace(LocaliseString(statusToReplace), "");
                result = result.Replace(LocaliseString($"{__instance.m_name}"), LocaliseString($"{__instance.m_name}\n{LocaliseString(status)}"));
            }

            if (result.Contains(EMPTY))
            {
                if (beeStatus.Value) return result.Replace(EMPTY, honeyCount);
                return result.Replace(EMPTY, honeyCount);
            }

            if (result.Contains(hasHoney))
            {
                if (beeStatus.Value) return result.Replace(hasHoney, honeyCount);
                return result.Replace(hasHoney, honeyCount);
            }

            return result;
        }
    }
}