using System;
using UnityEngine;
using ColossalFramework.UI;
using RealSchoolBus.Utils;
using ColossalFramework;

namespace RealSchoolBus.AI
{
    public class SchoolBusPanel : UIPanel
	{
		protected const float Margin = 5f;
        protected const float MenuWidth = 60f;
        protected virtual float PanelHeight => 350f;

		public ushort targetID;

		private UIButton m_DrawRouteButton;

		private UIButton m_DeleteRouteButton;

		internal virtual void Setup()
        {
            try
            {
                // Basic setup.
                autoLayout = false;
                backgroundSprite = "MenuPanel2";
                opacity = 0.95f;
                isVisible = true;
                canFocus = true;
                isInteractive = true;
                size = new Vector2(220f, PanelHeight);

                // Draw Route button.
                m_DrawRouteButton = UiUtil.AddButton(this, Margin, PanelHeight - 80f, "Draw Route", this.width - (Margin * 2), tooltip: "Draw a school bus route");

                // Delete Route button.
                m_DeleteRouteButton = UiUtil.AddButton(this, Margin, PanelHeight - 40f, "Delete Route", this.width - (Margin * 2), tooltip: "Delete a school bus route");
            
				m_DrawRouteButton.eventClicked += OnDrawRouteButtonClicked;
				m_DeleteRouteButton.eventClicked += OnDeleteRouteButtonClicked;
				
			}
            catch (Exception e)
            {
				LogHelper.Error("exception setting up panel base", e);
            }
        }

		/// <summary>
        /// Called when the selected building has changed.
        /// </summary>
        internal void BuildingChanged()
        {
            // Update selected building ID.
            targetID = WorldInfoPanel.GetCurrentInstanceID().Building;

            BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetID].Info;

            if (info.GetAI() is SchoolAI schoolAI && schoolAI.m_info.GetClassLevel() < ItemClass.Level.Level3)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

		private void OnDrawRouteButtonClicked(UIComponent component, UIMouseEventParameter eventParam)
		{
			OnDrawRoute();
		}

		private void OnDeleteRouteButtonClicked(UIComponent component, UIMouseEventParameter eventParam)
		{
			OnDeleteRoute();
		}

		public void OnDrawRoute()
		{
			TransportInfo info = Singleton<TransportManager>.instance.GetTransportInfo((TransportInfo.TransportType)30);	
			ushort building = targetID;
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
				});
			}
		}

		public void OnDeleteRoute()
		{
			ushort building = targetID;
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


