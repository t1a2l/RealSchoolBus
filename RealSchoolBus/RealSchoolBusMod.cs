using RealSchoolBus.Utils;
using CitiesHarmony.API;
using ICities;
using System;
using RealSchoolBus.Panels;

namespace RealSchoolBus {

    public class RealSchoolBusMod : LoadingExtensionBase, IUserMod {

        string IUserMod.Name => "Real School Bus Mod";
        string IUserMod.Description => "Allow elementry and high schools to set up a line and send buses to pcikup and dropoff students";

        public void OnEnabled()
        {
            HarmonyHelper.DoOnHarmonyReady(() => PatchUtil.PatchAll());
        }

        public void OnDisabled()
        {
            if (HarmonyHelper.IsHarmonyInstalled) PatchUtil.UnpatchAll();
        }

        public override void OnLevelLoaded(LoadMode mode)
        {
            try
            {              
                var loadedBuildingInfoCount = PrefabCollection<BuildingInfo>.LoadedCount();
                for (uint i = 0; i < loadedBuildingInfoCount; i++) {
                    var bi = PrefabCollection<BuildingInfo>.GetLoaded(i);
                    if (bi is null) continue;
                    if (bi.GetAI() is SchoolAI schoolAI && schoolAI.m_info.GetClassLevel() < ItemClass.Level.Level3) {
                        AiReplacementHelper.ApplyNewAIToBuilding(bi);
                    }
                }
                BuildingPanelManager.Hook();
                LogHelper.Information("Reloaded Mod");
            }
            catch (Exception e) {
                LogHelper.Information(e.ToString());
            }
        }
    }

}
