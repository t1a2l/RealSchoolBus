using ColossalFramework;
using ColossalFramework.UI;
using HarmonyLib;
using RealSchoolBus.AI;
using RealSchoolBus.Utils.UIUtils;
using UnityEngine;

namespace RealSchoolBus.HarmonyPatches
{
    [HarmonyPatch]
    public static class CityServiceWorldInfoPanelPatch
    {
        public static UIButton m_drawSchoolBusRouteBtn;

        public static UIButton m_deleteSchoolBusRouteBtn;

        [HarmonyPatch(typeof(CityServiceWorldInfoPanel), "OnSetTarget")]
        [HarmonyPrefix]
        public static void OnSetTarget()
        {
            if (m_drawSchoolBusRouteBtn == null || m_deleteSchoolBusRouteBtn == null)
            {
                CreateUI();
            }

            // Currently selected building.
            ushort building = WorldInfoPanel.GetCurrentInstanceID().Building;
            Building data = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building];

            if(data.Info.GetAI() is NewSchoolAI)
            {
                m_drawSchoolBusRouteBtn.Show();
                m_deleteSchoolBusRouteBtn.Show();
            }
            else
            {
                m_drawSchoolBusRouteBtn.Hide();
                m_deleteSchoolBusRouteBtn.Hide();
            }
        }

        private static void CreateUI()
        {
            var m_cityServiceWorldInfoPanel = GameObject.Find("(Library) CityServiceWorldInfoPanel").GetComponent<CityServiceWorldInfoPanel>();
            var wrapper = m_cityServiceWorldInfoPanel?.Find("Wrapper");
            var mainSectionPanel = wrapper?.Find("MainSectionPanel");
            var mainBottom = mainSectionPanel?.Find("MainBottom");
            var buttonPanels = mainBottom?.Find("ButtonPanels").GetComponent<UIPanel>();

            if (buttonPanels == null)
            {
                return;
            }

            if (m_drawSchoolBusRouteBtn == null)
            {
                m_drawSchoolBusRouteBtn = UIButtons.CreateButton(buttonPanels, 10f, 0f, "DrawSchoolBusRouteBtn", "Draw Line", "", 120f);
                m_drawSchoolBusRouteBtn.eventClicked += OnDrawRoute;
                m_drawSchoolBusRouteBtn.Hide();
            }

            if (m_deleteSchoolBusRouteBtn == null)
            {
                m_deleteSchoolBusRouteBtn = UIButtons.CreateButton(buttonPanels, 10f, 0f, "DeleteSchoolBusRouteBtn", "Delete Line", "", 120f);
                m_deleteSchoolBusRouteBtn.eventClicked += OnDeleteRoute;
                m_deleteSchoolBusRouteBtn.Hide();
            }

        }

        private static void OnDrawRoute(UIComponent c, UIMouseEventParameter eventParameter)
        {
            TransportInfo info = Singleton<TransportManager>.instance.GetTransportInfo((TransportInfo.TransportType)18);
            ushort building = WorldInfoPanel.GetCurrentInstanceID().Building;
            if (!(info != null) || building == 0)
            {
                return;
            }
            TransportTool transportTool = ToolsModifierControl.SetTool<TransportTool>();
            if (transportTool != null)
            {
                transportTool.m_prefab = info;
                transportTool.m_building = building;
                Singleton<SimulationManager>.instance.AddAction(delegate
                {
                    transportTool.StartEditingBuildingLine(info, building);
                    Singleton<BuildingManager>.instance.m_escapeRoutes.Deactivate();
                });
            }
        }

        private static void OnDeleteRoute(UIComponent c, UIMouseEventParameter eventParameter)
        {
            ushort building = WorldInfoPanel.GetCurrentInstanceID().Building;
            if (building == 0)
            {
                return;
            }
            Singleton<SimulationManager>.instance.AddAction(delegate
            {
                ushort num = TransportTool.FindBuildingLine(building);
                if (num != 0)
                {
                    Singleton<TransportManager>.instance.ReleaseLine(num);
                }
            });
        }
    }
}
