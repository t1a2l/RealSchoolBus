using System;
using HarmonyLib;
using RealSchoolBus.AI;
using RealSchoolBus.Utils;
using Object = UnityEngine.Object;

namespace RealSchoolBus.HarmonyPatches
{
    [HarmonyPatch]
    public static class InitializePrefabBuildingPatch
    {
        [HarmonyPatch(typeof(BuildingInfo), "InitializePrefab")]
        [HarmonyPrefix]
        public static void InitializePrefabPrefix(BuildingInfo __instance)
        {
            try
            {
                if (__instance.m_class.m_service == ItemClass.Service.Education && (__instance.m_class.m_level == ItemClass.Level.Level1 || __instance.m_class.m_level == ItemClass.Level.Level2))
                {
                    var oldAI = __instance.GetComponent<PrefabAI>();
                    Object.DestroyImmediate(oldAI);
                    var newAI = (PrefabAI)__instance.gameObject.AddComponent<NewSchoolAI>();
                    PrefabUtil.TryCopyAttributes(oldAI, newAI, false);
                }
            }
            catch (Exception e)
            {
                LogHelper.Error("Error initializing prefab building AI.", e);
            }
        }
    }
}