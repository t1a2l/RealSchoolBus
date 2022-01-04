using HarmonyLib;
using RealSchoolBus.Panels;

namespace RealSchoolBus.HarmonyPatches
{
    [HarmonyPatch(typeof(CityServiceWorldInfoPanel))]
    public static class CityServiceWorldInfoPanelPatch
    {
		[HarmonyPatch(typeof(CityServiceWorldInfoPanel), "OnSetTarget")]
        [HarmonyPostfix]
        public static void OnSetTarget()
		{
			BuildingPanelManager.TargetChanged();
		}

    }
}
