using ColossalFramework;
using HarmonyLib;
using RealSchoolBus.AI;
using RealSchoolBus.Panels;
using UnityEngine;

namespace RealSchoolBus.HarmonyPatches
{
    [HarmonyPatch]
    public static class DefaultToolPatch
    {
        [HarmonyPatch(typeof(DefaultTool), "OpenWorldInfoPanel")]
        [HarmonyPrefix]
        public static bool OpenWorldInfoPanel(DefaultTool __instance, InstanceID id, Vector3 position)
        {
            if (id.Building != 0)
            {
                BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[id.Building].Info;
                NewSchoolAI newSchoolAI = info.m_buildingAI as NewSchoolAI;
                if (Singleton<InstanceManager>.instance.SelectInstance(id))
                {
                    if (newSchoolAI != null)
                    {
                        WorldInfoPanel.Show<NewSchoolWorldInfoPanel>(position, id);
                        return false;
                    }
                }
                else
                {
                    WorldInfoPanel.Hide<NewSchoolWorldInfoPanel>();
                }
            }
            return true;
        }
    }
}
