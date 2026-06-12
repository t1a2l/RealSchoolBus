using RealSchoolBus.Utils;
using CitiesHarmony.API;
using ICities;

namespace RealSchoolBus {

    public class RealSchoolBusMod : LoadingExtensionBase, IUserMod {

        string IUserMod.Name => "Real School Bus Mod";
        string IUserMod.Description => "Allow elementry and high schools to set up a line and send buses to pickup and dropoff students";

        public void OnEnabled()
        {
            HarmonyHelper.DoOnHarmonyReady(() => PatchUtil.PatchAll());
        }

        public void OnDisabled()
        {
            if (HarmonyHelper.IsHarmonyInstalled) PatchUtil.UnpatchAll();
        }

    }

}
