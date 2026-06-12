using System.Collections;
using ColossalFramework;
using ColossalFramework.UI;
using RealSchoolBus.AI;
using UnityEngine;

namespace RealSchoolBus.Panels
{
    public class NewSchoolWorldInfoPanel : BuildingWorldInfoPanel
    {
        private UIButton m_MoveButton;

        private UIComponent m_MovingPanel;

        private UILabel m_Type;

        private UILabel m_Status;

        private UILabel m_Upkeep;

        private UISprite m_Thumbnail;

        private UILabel m_BuildingInfo;

        private UILabel m_BuildingDesc;

        private UIPanel m_VehicleSelectorContainer;

        private ServiceBuildingVehicleSelector m_VehicleSelector;

        private UICheckBox m_OnOff;

        private bool m_IsRelocating;

        public UIComponent movingPanel
        {
            get
            {
                if (m_MovingPanel == null)
                {
                    m_MovingPanel = UIView.Find("MovingPanel");
                    m_MovingPanel.Find<UIButton>("Close").eventClick += OnMovingPanelCloseClicked;
                    m_MovingPanel.Hide();
                }
                return m_MovingPanel;
            }
        }

        public bool isCityServiceEnabled
        {
            get
            {
                if (Singleton<BuildingManager>.exists && m_InstanceID.Building != 0)
                {
                    return Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].m_productionRate != 0;
                }
                return false;
            }
            set
            {
                if (Singleton<SimulationManager>.exists && m_InstanceID.Building != 0)
                {
                    Singleton<SimulationManager>.instance.AddAction(ToggleBuilding(m_InstanceID.Building, value));
                }
            }
        }

        private IEnumerator ToggleBuilding(ushort id, bool value)
        {
            if (Singleton<BuildingManager>.exists)
            {
                BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[id].Info;
                info.m_buildingAI.SetProductionRate(id, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[id], (byte)(value ? 100 : 0));
            }
            yield return 0;
        }

        protected override void Start()
        {
            base.Start();
            m_Type = Find<UILabel>("Type");
            m_Status = Find<UILabel>("Status");
            m_Upkeep = Find<UILabel>("Upkeep");
            m_Thumbnail = Find<UISprite>("Thumbnail");
            m_BuildingInfo = Find<UILabel>("Info");
            m_BuildingDesc = Find<UILabel>("Desc");
            m_MoveButton = Find<UIButton>("RelocateAction");
            m_OnOff = Find<UICheckBox>("On/Off");
            m_OnOff.eventCheckChanged += OnOnOffCheck;
            m_VehicleSelectorContainer = Find<UIPanel>("VehicleSelectorContainer");
            m_VehicleSelector = m_VehicleSelectorContainer.GetComponent<ServiceBuildingVehicleSelector>();
        }

        private void OnOnOffCheck(UIComponent comp, bool value)
        {
            isCityServiceEnabled = value;
        }

        private IEnumerator ToggleEmptying(ushort id, bool value)
        {
            if (Singleton<BuildingManager>.exists)
            {
                BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[id].Info;
                info.m_buildingAI.SetEmptying(id, ref Singleton<BuildingManager>.instance.m_buildings.m_buffer[id], value);
            }
            yield return 0;
        }

        private void OnMovingPanelCloseClicked(UIComponent comp, UIMouseEventParameter p)
        {
            m_IsRelocating = false;
            ToolsModifierControl.GetTool<BuildingTool>().CancelRelocate();
        }

        private void TempHide()
        {
            ToolsModifierControl.cameraController.ClearTarget();
            ValueAnimator.Animate("Relocating", delegate (float val)
            {
                base.component.opacity = val;
            }, new AnimatedFloat(1f, 0f, 0.33f), delegate
            {
                UIView.library.Hide(GetType().Name);
            });
            movingPanel.Find<UILabel>("MovingLabel").text = LocaleFormatter.FormatGeneric("BUILDING_MOVING", base.buildingName);
            movingPanel.Show();
        }

        public void TempShow(Vector3 worldPosition, InstanceID instanceID)
        {
            movingPanel.Hide();
            WorldInfoPanel.Show<ShelterWorldInfoPanel>(worldPosition, instanceID);
            ValueAnimator.Animate("Relocating", delegate (float val)
            {
                base.component.opacity = val;
            }, new AnimatedFloat(0f, 1f, 0.33f));
        }

        private void Update()
        {
            if (m_IsRelocating)
            {
                BuildingTool currentTool = ToolsModifierControl.GetCurrentTool<BuildingTool>();
                if (currentTool != null && IsValidTarget() && currentTool.m_relocate != 0 && !movingPanel.isVisible)
                {
                    movingPanel.Show();
                }
                else if (!IsValidTarget() || (currentTool != null && currentTool.m_relocate == 0))
                {
                    ToolsModifierControl.mainToolbar.ResetLastTool();
                    movingPanel.Hide();
                    m_IsRelocating = false;
                }
            }
        }

        private void RelocateCompleted(InstanceID newID)
        {
            if (ToolsModifierControl.GetTool<BuildingTool>() != null)
            {
                ToolsModifierControl.GetTool<BuildingTool>().m_relocateCompleted -= RelocateCompleted;
            }
            m_IsRelocating = false;
            if (!newID.IsEmpty)
            {
                m_InstanceID = newID;
            }
            if (IsValidTarget())
            {
                BuildingTool tool = ToolsModifierControl.GetTool<BuildingTool>();
                if (tool == ToolsModifierControl.GetCurrentTool<BuildingTool>())
                {
                    ToolsModifierControl.SetTool<DefaultTool>();
                    if (InstanceManager.GetPosition(m_InstanceID, out var position, out var _, out var size))
                    {
                        position.y += size.y * 0.8f;
                    }
                    TempShow(position, m_InstanceID);
                }
            }
            else
            {
                movingPanel.Hide();
                BuildingTool tool2 = ToolsModifierControl.GetTool<BuildingTool>();
                if (tool2 == ToolsModifierControl.GetCurrentTool<BuildingTool>())
                {
                    ToolsModifierControl.SetTool<DefaultTool>();
                }
                Hide();
            }
        }

        protected override void OnHide()
        {
            if (m_IsRelocating && ToolsModifierControl.GetTool<BuildingTool>() != null)
            {
                ToolsModifierControl.GetTool<BuildingTool>().m_relocateCompleted -= RelocateCompleted;
            }
            movingPanel.Hide();
        }

        protected override void OnSetTarget()
        {
            bool flag = isCityServiceEnabled;
            if (m_OnOff.isChecked != flag)
            {
                m_OnOff.eventCheckChanged -= OnOnOffCheck;
                m_OnOff.isChecked = flag;
                m_OnOff.eventCheckChanged += OnOnOffCheck;
            }
            bool isVisible = false;
            BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[m_InstanceID.Building].Info;
            if (info != null && info.m_buildingAI is NewSchoolAI aI)
            {
                isVisible = aI.CanChangeVehicle(m_InstanceID.Building);
            }
            m_VehicleSelectorContainer.isVisible = isVisible;
            base.OnSetTarget();
        }

        protected override void UpdateBindings()
        {
            base.UpdateBindings();
            if (!Singleton<BuildingManager>.exists || m_InstanceID.Type != InstanceType.Building || m_InstanceID.Building == 0)
            {
                return;
            }
            ushort building = m_InstanceID.Building;
            BuildingManager instance = Singleton<BuildingManager>.instance;
            BuildingInfo info = instance.m_buildings.m_buffer[building].Info;
            BuildingAI buildingAI = info.m_buildingAI;
            ShelterAI shelterAI = buildingAI as ShelterAI;
            m_Type.text = Singleton<BuildingManager>.instance.GetDefaultBuildingName(building, InstanceID.Empty);
            m_Status.text = buildingAI.GetLocalizedStatus(building, ref instance.m_buildings.m_buffer[m_InstanceID.Building]);
            m_Upkeep.text = LocaleFormatter.FormatUpkeep(buildingAI.GetResourceRate(building, ref instance.m_buildings.m_buffer[building], EconomyManager.Resource.Maintenance), isDistanceBased: false);
            m_Thumbnail.atlas = info.m_Atlas;
            m_Thumbnail.spriteName = info.m_Thumbnail;
            if (m_Thumbnail.atlas != null && !string.IsNullOrEmpty(m_Thumbnail.spriteName))
            {
                UITextureAtlas.SpriteInfo spriteInfo = m_Thumbnail.atlas[m_Thumbnail.spriteName];
                if (spriteInfo != null)
                {
                    m_Thumbnail.size = spriteInfo.pixelSize;
                }
            }
            m_BuildingDesc.text = info.GetLocalizedDescriptionShort();
            m_BuildingInfo.text = buildingAI.GetLocalizedStats(building, ref instance.m_buildings.m_buffer[building]);

            string tooltip = string.Empty;
            m_MoveButton.isEnabled = buildingAI != null && buildingAI.CanBeRelocated(building, ref instance.m_buildings.m_buffer[building], out tooltip);
            m_MoveButton.tooltipLocaleID = ((!m_MoveButton.isEnabled) ? tooltip : "CITYSERVICE_MOVE");
            m_VehicleSelector.buildingId = building;
            m_VehicleSelector.Refresh();
        }

        public void OnRelocateBuilding()
        {
            if (ToolsModifierControl.GetTool<BuildingTool>() != null)
            {
                ToolsModifierControl.GetTool<BuildingTool>().m_relocateCompleted += RelocateCompleted;
            }
            ToolsModifierControl.keepThisWorldInfoPanel = true;
            BuildingTool buildingTool = ToolsModifierControl.SetTool<BuildingTool>();
            buildingTool.m_prefab = null;
            buildingTool.m_relocate = m_InstanceID.Building;
            m_IsRelocating = true;
            TempHide();
        }

        public void OnDrawRoute()
        {
            TransportInfo info = Singleton<TransportManager>.instance.GetTransportInfo((TransportInfo.TransportType)18);
            ushort building = m_InstanceID.Building;
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

        public void OnDeleteRoute()
        {
            ushort building = m_InstanceID.Building;
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
