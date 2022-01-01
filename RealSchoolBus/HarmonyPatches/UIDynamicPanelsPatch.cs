using HarmonyLib;
using ColossalFramework.UI;
using RealSchoolBus.AI;
using System.Reflection;
using System;
using System.Linq;

namespace RealSchoolBus.HarmonyPatches
{
	[HarmonyPatch]
    public static class UIDynamicPanelsPatch
    {

		static MethodBase TargetMethod()
		{
			var showMethod = typeof(UIDynamicPanels).GetMethods(BindingFlags.Public | BindingFlags.Instance)
								.Where(mi => mi.Name == "Show" && mi.IsGenericMethod == false && mi.ReturnType == typeof(UIComponent) && mi.GetParameters().Length == 3)
								.First();
			return showMethod;
		}

        [HarmonyPrefix]
		public static bool Show(string panelName, bool bringToFront, bool onlyWhenInvisible, ref UIComponent __result)
		{
			if(panelName == "SchoolWorldInfoPanel")
            {
				__result = RealSchoolBusMod.schoolWorldInfoPanel.component;
				return false;
            }
			else
            {
				return true;
            }
		}

    }
}


