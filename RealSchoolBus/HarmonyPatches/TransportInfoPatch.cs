using HarmonyLib;
using System;

namespace RealSchoolBus.HarmonyPatches {

    [HarmonyPatch(typeof(TransportInfo))]
    public static class TransportInfoPatch {

        [HarmonyPatch(typeof(TransportInfo), "InitializePrefab")]
        [HarmonyPostfix]
        public static void InitializePrefab()
	    {
            bool isValueDefined1 = Enum.IsDefined(typeof(TransportInfo.TransportType), 30);
            if(!isValueDefined1)
            {
                TransportInfo.TransportType SchoolBus = (TransportInfo.TransportType)30;
                LogHelper.Information(SchoolBus.ToString());
            }
	    }
    }
}
