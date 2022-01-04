using HarmonyLib;
using System.Reflection;

namespace RealSchoolBus.HarmonyPatches {

    [HarmonyPatch(typeof(TransportManager))]
    public static class TransportManagerPatch {

        [HarmonyPatch(typeof(TransportManager), "Awake")]
        [HarmonyPostfix]
        public static void Awake(TransportManager __instance)
		{
            TransportInfo[] transportTypeLoaded = new TransportInfo[18];
            typeof(TransportManager).GetField("m_transportTypeLoaded", BindingFlags.NonPublic | BindingFlags.Instance).SetValue(__instance, transportTypeLoaded);
		}
    }
}
