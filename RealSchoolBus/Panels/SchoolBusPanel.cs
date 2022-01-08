using System;
using ColossalFramework;
using RealSchoolBus.AI;

namespace RealSchoolBus.Panels
{
    internal  class SchoolBusPanel : BasePanel
	{
		// Constants.
        protected override float PanelHeight => 220f;

		internal override void Setup()
        {
            try
            {
				base.Setup();

				// Set initial building.
                BuildingChanged();

				m_DrawRouteButton.eventClick += (control, clickEvent) =>
                {
                   Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        OnDrawRoute(targetID);
                    });

                    // Update the panel once done.
                    UpdatePanel();
                };

                m_DeleteRouteButton.eventClick += (control, clickEvent) =>
                {

                    Singleton<SimulationManager>.instance.AddAction(delegate
                    {
                        OnDeleteRoute(targetID);
                    });

                    // Update the panel once done.
                    UpdatePanel();
                };
				
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

            if (info.GetAI() is NewSchoolAI newSchoolAI && newSchoolAI.m_info.GetClassLevel() < ItemClass.Level.Level3)
            {
                Show();
            }
            else
            {
                Hide();
            }
        }

		/// <summary>
        /// Updates the panel according to building's current level settings.
        /// </summary>
        internal void UpdatePanel()
        {
            // Make sure we have a valid builidng first.
            if (targetID == 0 || (Singleton<BuildingManager>.instance.m_buildings.m_buffer[targetID].m_flags == Building.Flags.None))
            {
                // Invalid target - disable buttons.
                m_DrawRouteButton.Disable();
                m_DeleteRouteButton.Disable();
                return;
            }

        }

		public void OnDrawRoute(ushort building)
		{
			TransportInfo info = Singleton<TransportManager>.instance.GetTransportInfo(TransportInfo.TransportType.Bus);	
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

		public void OnDeleteRoute(ushort building)
		{
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


