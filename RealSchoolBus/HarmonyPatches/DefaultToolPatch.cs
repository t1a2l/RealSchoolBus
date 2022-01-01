using ColossalFramework;
using HarmonyLib;
using UnityEngine;
using RealSchoolBus.AI;

namespace RealSchoolBus.HarmonyPatches
{
    [HarmonyPatch(typeof(DefaultTool))]
    public static class DefaultToolPatch
    {
		[HarmonyPatch(typeof(DefaultTool), "OpenWorldInfoPanel")]
        [HarmonyPrefix]
        public static bool OpenWorldInfoPanel(InstanceID id, Vector3 position)
		{
			if (id.Building != 0)
			{
				BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[id.Building].Info;
				NewSchoolAI newSchoolAI = info.m_buildingAI as NewSchoolAI;
				if (!Singleton<InstanceManager>.instance.SelectInstance(id))
				{
					return false;
				}
				if (newSchoolAI != null)
				{
					WorldInfoPanel.Show<SchoolWorldInfoPanel>(position, id);
					return false;
				}
				return true;
			}
			else 
			{
				return true;
			}
		}

    }
}
