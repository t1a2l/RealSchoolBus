using System;
using System.Runtime.CompilerServices;
using ColossalFramework;
using HarmonyLib;
using UnityEngine;

namespace RealSchoolBus.HarmonyPatches
{
    [HarmonyPatch]
    public static class BusAIPatch
    {
        [HarmonyPatch(typeof(BusAI), "GetColor")]
        [HarmonyPrefix]
        public static bool GetColor(BusAI __instance, ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode, InfoManager.SubInfoMode subInfoMode, ref Color __result)
        {
            if(__instance.m_transportInfo.m_transportType == TransportInfoPatch.SchoolBusTransportType)
            {
                switch (infoMode)
                {
                    case InfoManager.InfoMode.Education:
                        ushort transportLine2 = data.m_transportLine;
                        if (transportLine2 != 0)
                        {
                            __result = Singleton<TransportManager>.instance.m_lines.m_buffer[transportLine2].GetColor();
                            return false;
                        }
                        __result = Singleton<TransportManager>.instance.m_properties.m_transportColors[(int)TransportInfoPatch.SchoolBusTransportType];
                        return false;
                    case InfoManager.InfoMode.TrafficRoutes:
                        if (subInfoMode == InfoManager.SubInfoMode.Default)
                        {
                            InstanceID empty = InstanceID.Empty;
                            empty.Vehicle = vehicleID;
                            if (Singleton<NetManager>.instance.PathVisualizer.IsPathVisible(empty))
                            {
                                __result = Singleton<InfoManager>.instance.m_properties.m_routeColors[3];
                                return false;
                            }
                            __result = Singleton<InfoManager>.instance.m_properties.m_neutralColor;
                            return false;
                        }
                        __result = BaseGetColor(__instance, vehicleID, ref data, infoMode, subInfoMode);
                        return false;
                    default:
                        __result = BaseGetColor(__instance, vehicleID, ref data, infoMode, subInfoMode);
                        return false;
                }
            }
            return true;
        }

        [HarmonyReversePatch]
        [HarmonyPatch(typeof(CarAI), "GetColor")]
        [MethodImpl(MethodImplOptions.NoInlining)]
        private static Color BaseGetColor(CarAI instance, ushort vehicleID, ref Vehicle data, InfoManager.InfoMode infoMode, InfoManager.SubInfoMode subInfoMode)
        {
            string message = "GetColor reverse Harmony patch wasn't applied";
            Debug.LogError(message);
            throw new NotImplementedException(message);
        }
    }
}
