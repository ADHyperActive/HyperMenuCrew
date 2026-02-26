using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AmongUs.Data;
using HarmonyLib;
using UnityEngine;
using DateTime = Il2CppSystem.DateTime;

namespace ModMenuCrew.Patches
{
    /// <summary>
    /// Harmony patch that unlocks ALL cosmetics (skins, hats, pets, visors, nameplates, bundles)
    /// when HatManager initializes. Uses reflection to set items as free and mark them as purchased.
    /// 
    /// <b>HOW IT WORKS:</b>
    /// 1. Patches HatManager.Initialize (runs once when the game loads cosmetics).
    /// 2. Iterates through every cosmetic category and sets "Free" = true via reflection.
    /// 3. Registers each item as "purchased" in the player's save data.
    /// 4. Validates bundles to skip any with invalid/missing pets.
    /// 5. Resets store view dates to suppress "new item" notifications.
    /// </summary>
    [HarmonyPatch(typeof(HatManager), nameof(HatManager.Initialize))]
    public static class CosmeticsUnlockPatch
    {
        // --- Reflection Cache ---
        private static readonly Dictionary<(Type, string), MemberInfo> memberCache = new Dictionary<(Type, string), MemberInfo>();
        private static readonly BindingFlags CaseInsensitiveInstanceFlags =
            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.IgnoreCase;

        private static object ConvertIfNeeded(object value, Type targetType)
        {
            if (value == null) return null;
            var valueType = value.GetType();
            if (targetType.IsAssignableFrom(valueType)) return value;
            try
            {
                return Convert.ChangeType(value, targetType);
            }
            catch
            {
                return value;
            }
        }

        private static MemberInfo GetCachedMember(Type type, string name)
        {
            var key = (type, name.ToLowerInvariant());
            if (memberCache.TryGetValue(key, out var mi)) return mi;

            var prop = type.GetProperty(name, CaseInsensitiveInstanceFlags);
            if (prop != null)
            {
                memberCache[key] = prop;
                return prop;
            }
            var field = type.GetField(name, CaseInsensitiveInstanceFlags);
            if (field != null)
            {
                memberCache[key] = field;
                return field;
            }
            memberCache[key] = null;
            return null;
        }

        private static bool TrySetPropertyOrField(object target, object value, params string[] candidateNames)
        {
            if (target == null || candidateNames == null || candidateNames.Length == 0) return false;
            var type = target.GetType();
            foreach (var name in candidateNames)
            {
                var mi = GetCachedMember(type, name);
                if (mi is PropertyInfo prop && prop.CanWrite)
                {
                    try
                    {
                        var converted = ConvertIfNeeded(value, prop.PropertyType);
                        prop.SetValue(target, converted, null);
                        return true;
                    }
                    catch { }
                }

                if (mi is FieldInfo field && !field.IsInitOnly)
                {
                    try
                    {
                        var converted = ConvertIfNeeded(value, field.FieldType);
                        field.SetValue(target, converted);
                        return true;
                    }
                    catch { }
                }
            }
            return false;
        }

        private static string GetStringPropertyOrFieldOrDefault(object target, string defaultValue, params string[] candidateNames)
        {
            if (target == null || candidateNames == null || candidateNames.Length == 0) return defaultValue;
            var type = target.GetType();
            foreach (var name in candidateNames)
            {
                var mi = GetCachedMember(type, name);
                if (mi is PropertyInfo prop && prop.CanRead)
                {
                    try
                    {
                        var objVal = prop.GetValue(target, null);
                        if (objVal is string s && !string.IsNullOrEmpty(s)) return s;
                    }
                    catch { }
                }

                if (mi is FieldInfo field)
                {
                    try
                    {
                        var objVal = field.GetValue(target);
                        if (objVal is string s && !string.IsNullOrEmpty(s)) return s;
                    }
                    catch { }
                }
            }
            return defaultValue;
        }

        public static void Postfix(HatManager __instance)
        {
            UnlockAllItems(__instance);
        }

        private static void UnlockAndPurchaseAll<T>(IEnumerable<T> items, PlayerPurchasesData purchasesData) where T : class
        {
            if (items == null) return;
            foreach (var item in items)
            {
                if (item == null) continue;

                // Mark as free
                TrySetPropertyOrField(item, true, "Free");

                // Register as purchased
                var productId = GetStringPropertyOrFieldOrDefault(item, null, "ProductId", "productId");
                if (!string.IsNullOrEmpty(productId))
                {
                    try { purchasesData?.SetPurchased(productId); }
                    catch { /* Ignore duplicate purchase errors */ }
                }
            }
        }

        private static void UnlockAllItems(HatManager manager)
        {
            if (manager == null)
            {
                Debug.LogError("[ModMenuCrew] HatManager is null.");
                return;
            }

            try
            {
                var playerData = DataManager.Player;
                if (playerData == null) return;
                
                var purchasesData = playerData.Purchases;

                // Convert to arrays once to avoid repeated LINQ allocations
                var pets = manager.allPets?.ToArray() ?? Array.Empty<PetData>();
                var hats = manager.allHats?.ToArray() ?? Array.Empty<HatData>();
                var skins = manager.allSkins?.ToArray() ?? Array.Empty<SkinData>();
                var visors = manager.allVisors?.ToArray() ?? Array.Empty<VisorData>();
                var nameplates = manager.allNamePlates?.ToArray() ?? Array.Empty<NamePlateData>();
                var bundles = manager.allBundles?.ToArray() ?? Array.Empty<BundleData>();

                // Unlock individual cosmetic categories
                UnlockAndPurchaseAll(pets, purchasesData);
                UnlockAndPurchaseAll(hats, purchasesData);
                UnlockAndPurchaseAll(skins, purchasesData);
                UnlockAndPurchaseAll(visors, purchasesData);
                UnlockAndPurchaseAll(nameplates, purchasesData);

                // Validate and unlock bundles (skip bundles with invalid pets)
                var validBundles = new List<BundleData>();
                foreach (var bundle in bundles)
                {
                    if (bundle == null) continue;
                    bool isValid = true;

                    if (bundle.cosmetics != null)
                    {
                        foreach (var cosmetic in bundle.cosmetics)
                        {
                            if (cosmetic == null)
                            {
                                isValid = false;
                                break;
                            }
                            // Check if bundle contains an invalid pet (not in the loaded list)
                            if (cosmetic is PetData petData && !pets.Any(p => p != null && p.ProductId == petData.ProductId))
                            {
                                isValid = false;
                                break;
                            }
                        }
                    }

                    if (isValid) validBundles.Add(bundle);
                }
                UnlockAndPurchaseAll(validBundles, purchasesData);

                // Unlock featured items
                try
                {
                    UnlockAndPurchaseAll(manager.allFeaturedBundles?.ToArray(), purchasesData);
                    UnlockAndPurchaseAll(manager.allFeaturedCubes?.ToArray(), purchasesData);
                    UnlockAndPurchaseAll(manager.allFeaturedItems?.ToArray(), purchasesData);
                }
                catch (Exception ex) { Debug.LogWarning($"[ModMenuCrew] Featured unlock error: {ex.Message}"); }

                // Fix StarBundle prices to 0
                var starBundles = manager.allStarBundles?.ToArray();
                if (starBundles != null)
                {
                    foreach (var sb in starBundles)
                    {
                        if (sb != null) TrySetPropertyOrField(sb, 0, "price", "Price");
                    }
                }

                // Reset store view dates to suppress "new item" badges
                if (playerData.store != null)
                {
                    var now = DateTime.Now;
                    playerData.store.LastBundlesViewDate = now;
                    playerData.store.LastHatsViewDate = now;
                    playerData.store.LastOutfitsViewDate = now;
                    playerData.store.LastVisorsViewDate = now;
                    playerData.store.LastPetsViewDate = now;
                    playerData.store.LastNameplatesViewDate = now;
                    playerData.store.LastCosmicubeViewDate = now;
                }

                playerData.Save();
                Debug.Log("[ModMenuCrew] All cosmetics unlocked and secured.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[ModMenuCrew] UnlockAllItems critical error: {ex}");
            }
        }
    }

    [HarmonyPatch(typeof(Constants), "IsVersionModded")]
    public static class Constants_IsVersionModded_Patch
    {
        [HarmonyPrefix]
        public static bool ForceReturnFalse(ref bool __result)
        {
            __result = false;
            return false;
        }
    }

    [HarmonyPatch(typeof(HatManager), nameof(HatManager.CheckValidCosmetic))]
    public static class Patch_IgnoreBlacklist
    {
        public static bool Prefix(ref bool __result)
        {
            __result = true;
            return false;
        }
    }
}