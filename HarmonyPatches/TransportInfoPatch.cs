using HarmonyLib;

namespace RealSchoolBus.HarmonyPatches
{
    [HarmonyPatch]
    public static class TransportInfoPatch
    {
        public const TransportInfo.TransportType SchoolBusTransportType = (TransportInfo.TransportType)18;

        [HarmonyPatch(typeof(TransportInfo), nameof(TransportInfo.vehicleCategory), MethodType.Getter)]
        [HarmonyPrefix]
        public static bool Prefix(TransportInfo __instance, ref VehicleInfo.VehicleCategory __result)
        {
            if (__instance != null && __instance.m_transportType == SchoolBusTransportType)
            {
                __result = VehicleInfo.VehicleCategory.Bus;
                return false;
            }

            return true;
        }
    }
}
