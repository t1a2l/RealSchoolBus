using System;
using System.Collections;
using ColossalFramework;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace RealSchoolBus.AI
{
    public class SchoolWorldInfoPanel : BuildingWorldInfoPanel
	{
		private UIPanel m_wrapper;

		private UIPanel m_right;

		private UIComponent m_MovingPanel;

		private UIPanel m_mainBottom;

		private UIButton m_MoveButton;

		private UILabel m_Type;

		private UILabel m_Status;

		private UILabel m_Upkeep;

		private UISprite m_Thumbnail;

		private UILabel m_BuildingInfo;

		private UILabel m_BuildingDesc;

		private UISprite m_BuildingService;

		private UIButton m_BudgetButton;

		private UIButton m_RebuildButton;

		private UIButton m_DrawRouteButton;

		private UIButton m_DeleteRouteButton;

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
			m_right = Find<UIPanel>("Right");
			m_mainBottom = Find<UIPanel>("MainBottom");
			base.Start();
			m_wrapper = Find<UIPanel>("Wrapper");
			m_Type = Find<UILabel>("Type");
			m_Status = Find<UILabel>("Status");
			m_Upkeep = Find<UILabel>("Upkeep");
			m_Thumbnail = Find<UISprite>("Thumbnail");
			m_BuildingInfo = Find<UILabel>("Info");
			m_BuildingDesc = Find<UILabel>("Desc");
			m_BuildingService = Find<UISprite>("Service");
			m_BudgetButton = Find<UIButton>("Budget");
			m_BudgetButton.isEnabled = ToolsModifierControl.IsUnlocked(UnlockManager.Feature.Economy);
			m_RebuildButton = Find<UIButton>("RebuildButton");
			ShelterWorldInfoPanel panel = GetComponent<ShelterWorldInfoPanel>();
			var drawbutton = panel.Find<UIButton>("DrawRoute");
			var deletebutton = panel.Find<UIButton>("DeleteRoute");
			m_DrawRouteButton = Instantiate(drawbutton);
			m_DeleteRouteButton = Instantiate(deletebutton);
			m_DrawRouteButton.eventClicked += OnDrawRouteButtonClicked;
			m_DeleteRouteButton.eventClicked += OnDeleteRouteButtonClicked;
			m_RebuildButton.isVisible = false;
			m_MoveButton = Find<UIButton>("RelocateAction");
			m_OnOff = Find<UICheckBox>("On/Off");
			m_OnOff.eventCheckChanged += OnOnOffChanged;
		}

		private void OnOnOffChanged(UIComponent comp, bool value)
		{
			isCityServiceEnabled = value;
		}

		private void OnDrawRouteButtonClicked(UIComponent component, UIMouseEventParameter eventParam)
		{
			OnDrawRoute();
		}

		private void OnDeleteRouteButtonClicked(UIComponent component, UIMouseEventParameter eventParam)
		{
			OnDeleteRoute();
		}

		private void OnMovingPanelCloseClicked(UIComponent comp, UIMouseEventParameter p)
		{
			m_IsRelocating = false;
			ToolsModifierControl.GetTool<BuildingTool>().CancelRelocate();
		}

		private void TempHide()
		{
			ToolsModifierControl.cameraController.ClearTarget();
			ValueAnimator.Animate("Relocating", delegate(float val)
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
			ValueAnimator.Animate("Relocating", delegate(float val)
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
			if (base.component.isVisible)
			{
				bool flag = isCityServiceEnabled;
				if (m_OnOff.isChecked != flag)
				{
					m_OnOff.eventCheckChanged -= OnOnOffChanged;
					m_OnOff.isChecked = flag;
					m_OnOff.eventCheckChanged += OnOnOffChanged;
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
			Building building2 = instance.m_buildings.m_buffer[building];
			BuildingInfo info = building2.Info;
			BuildingAI buildingAI = info.m_buildingAI;
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
			m_BuildingInfo.isVisible = m_BuildingInfo.text != string.Empty;
			ItemClass.Service service = info.GetService();
			if (service != 0)
			{
				string nameByValue = ColossalFramework.Utils.GetNameByValue(service, "Game");
				m_BuildingService.spriteName = "ToolbarIcon" + nameByValue;
				m_BuildingService.tooltip = ColossalFramework.Globalization.Locale.Get("MAIN_TOOL", nameByValue);
			}
			m_BuildingService.isVisible = service != ItemClass.Service.None;
			m_MoveButton.isEnabled = buildingAI != null && buildingAI.CanBeRelocated(building, ref instance.m_buildings.m_buffer[building]);
			if ((building2.m_flags & Building.Flags.Collapsed) != 0)
			{
				m_RebuildButton.tooltip = ((!IsDisasterServiceRequired()) ? LocaleFormatter.FormatCost(buildingAI.GetRelocationCost(), isDistanceBased: false) : ColossalFramework.Globalization.Locale.Get("CITYSERVICE_TOOLTIP_DISASTERSERVICEREQUIRED"));
				m_RebuildButton.isVisible = Singleton<LoadingManager>.instance.SupportsExpansion(Expansion.NaturalDisasters);
				m_RebuildButton.isEnabled = CanRebuild();
			}
			else
			{
				m_RebuildButton.isVisible = false;
			}
			base.component.size = m_wrapper.size;
			m_mainBottom.width = m_wrapper.width;
			m_BuildingInfo.width = m_right.width;
			m_BudgetButton.isEnabled = ToolsModifierControl.IsUnlocked(UnlockManager.Feature.Economy);
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

		public void OnBudgetClicked()
		{
			if (ToolsModifierControl.IsUnlocked(UnlockManager.Feature.Economy))
			{
				ToolsModifierControl.mainToolbar.ShowEconomyPanel(1);
				WorldInfoPanel.Hide<CityServiceWorldInfoPanel>();
			}
		}

		public void OnRebuildClicked()
		{
			ushort buildingID = m_InstanceID.Building;
			if (buildingID == 0)
			{
				return;
			}
			Singleton<SimulationManager>.instance.AddAction(delegate
			{
				BuildingManager instance = Singleton<BuildingManager>.instance;
				BuildingInfo info = instance.m_buildings.m_buffer[buildingID].Info;
				if ((object)info != null && (instance.m_buildings.m_buffer[buildingID].m_flags & Building.Flags.Collapsed) != 0)
				{
					int relocationCost = info.m_buildingAI.GetRelocationCost();
					Singleton<EconomyManager>.instance.FetchResource(EconomyManager.Resource.Construction, relocationCost, info.m_class);
					Vector3 position = instance.m_buildings.m_buffer[buildingID].m_position;
					float angle = instance.m_buildings.m_buffer[buildingID].m_angle;
					RebuildBuilding(info, position, angle, buildingID, info.m_fixedHeight);
					if (info.m_subBuildings != null && info.m_subBuildings.Length != 0)
					{
						Matrix4x4 matrix4x = default(Matrix4x4);
						matrix4x.SetTRS(position, Quaternion.AngleAxis(angle * 57.29578f, Vector3.down), Vector3.one);
						for (int i = 0; i < info.m_subBuildings.Length; i++)
						{
							BuildingInfo buildingInfo = info.m_subBuildings[i].m_buildingInfo;
							Vector3 position2 = matrix4x.MultiplyPoint(info.m_subBuildings[i].m_position);
							float angle2 = info.m_subBuildings[i].m_angle * ((float)Math.PI / 180f) + angle;
							bool fixedHeight = info.m_subBuildings[i].m_fixedHeight;
							ushort num = RebuildBuilding(buildingInfo, position2, angle2, 0, fixedHeight);
							if (buildingID != 0 && num != 0)
							{
								instance.m_buildings.m_buffer[buildingID].m_subBuilding = num;
								instance.m_buildings.m_buffer[num].m_parentBuilding = buildingID;
								instance.m_buildings.m_buffer[num].m_flags |= Building.Flags.Untouchable;
								buildingID = num;
							}
						}
					}
				}
			});
		}

		private ushort RebuildBuilding(BuildingInfo info, Vector3 position, float angle, ushort buildingID, bool fixedHeight)
		{
			ushort building = 0;
			bool flag = false;
			if (buildingID != 0)
			{
				Singleton<BuildingManager>.instance.RelocateBuilding(buildingID, position, angle);
				building = buildingID;
				flag = true;
			}
			else if (Singleton<BuildingManager>.instance.CreateBuilding(out building, ref Singleton<SimulationManager>.instance.m_randomizer, info, position, angle, 0, Singleton<SimulationManager>.instance.m_currentBuildIndex))
			{
				if (fixedHeight)
				{
					Singleton<BuildingManager>.instance.m_buildings.m_buffer[building].m_flags |= Building.Flags.FixedHeight;
				}
				Singleton<SimulationManager>.instance.m_currentBuildIndex++;
				flag = true;
			}
			if (flag)
			{
				int publicServiceIndex = ItemClass.GetPublicServiceIndex(info.m_class.m_service);
				if (publicServiceIndex != -1)
				{
					Singleton<BuildingManager>.instance.m_buildingDestroyed2.Disable();
					Singleton<GuideManager>.instance.m_serviceNotUsed[publicServiceIndex].Disable();
					Singleton<GuideManager>.instance.m_serviceNeeded[publicServiceIndex].Deactivate();
					Singleton<CoverageManager>.instance.CoverageUpdated(info.m_class.m_service, info.m_class.m_subService, info.m_class.m_level);
				}
				BuildingTool.DispatchPlacementEffect(info, 0, position, angle, info.m_cellWidth, info.m_cellLength, bulldozing: false, collapsed: false);
			}
			return building;
		}

		public bool CanRebuild()
		{
			ushort building = m_InstanceID.Building;
			if (building != 0 && Singleton<BuildingManager>.instance.m_buildings.m_buffer[building].m_levelUpProgress == byte.MaxValue)
			{
				BuildingInfo info = Singleton<BuildingManager>.instance.m_buildings.m_buffer[building].Info;
				if (info != null)
				{
					int relocationCost = info.m_buildingAI.GetRelocationCost();
					if (Singleton<EconomyManager>.instance.PeekResource(EconomyManager.Resource.Construction, relocationCost) == relocationCost)
					{
						return true;
					}
				}
			}
			return false;
		}

		private bool IsDisasterServiceRequired()
		{
			ushort building = m_InstanceID.Building;
			if (building != 0)
			{
				return Singleton<BuildingManager>.instance.m_buildings.m_buffer[building].m_levelUpProgress != byte.MaxValue;
			}
			return false;
		}

		public void OnDrawRoute()
		{
			TransportInfo info = Singleton<TransportManager>.instance.GetTransportInfo((TransportInfo.TransportType)30);
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


