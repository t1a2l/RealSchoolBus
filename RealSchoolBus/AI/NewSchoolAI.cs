using ColossalFramework;
using ColossalFramework.DataBinding;
using ColossalFramework.Math;
using System;
using UnityEngine;

namespace RealSchoolBus.AI
{
    public class NewSchoolAI : PlayerBuildingAI
	{
		[CustomizableProperty("Uneducated Workers", "Workers", 0)]
		public int m_workPlaceCount0 = 10;

		[CustomizableProperty("Educated Workers", "Workers", 1)]
		public int m_workPlaceCount1 = 9;

		[CustomizableProperty("Well Educated Workers", "Workers", 2)]
		public int m_workPlaceCount2 = 5;

		[CustomizableProperty("Highly Educated Workers", "Workers", 3)]
		public int m_workPlaceCount3 = 1;

		[CustomizableProperty("Student Count")]
		public int m_studentCount = 100;

		[CustomizableProperty("Education Accumulation")]
		public int m_educationAccumulation = 100;

		[CustomizableProperty("Education Radius")]
		public float m_educationRadius = 500f;

		[CustomizableProperty("School Bus Count")]
		public int m_schoolBusCount = 10;

		public TransportInfo m_transportInfo;

		public Vector3 m_spawnPosition;

		public Vector3 m_spawnTarget;

		public bool m_canInvertTarget;

		public int StudentCount => UniqueFacultyAI.IncreaseByBonus(UniqueFacultyAI.FacultyBonus.Education, m_studentCount);

		public int EducationAccumulation => UniqueFacultyAI.IncreaseByBonus(UniqueFacultyAI.FacultyBonus.Education, m_educationAccumulation);

		public override Color GetColor(ushort buildingID, ref Building data, InfoManager.InfoMode infoMode)
		{
			if (infoMode == InfoManager.InfoMode.Education)
			{
				InfoManager.SubInfoMode currentSubMode = Singleton<InfoManager>.instance.CurrentSubMode;
				ItemClass.Level level = ItemClass.Level.None;
				switch (currentSubMode)
				{
				case InfoManager.SubInfoMode.Default:
					level = ItemClass.Level.Level1;
					break;
				case InfoManager.SubInfoMode.WaterPower:
					level = ItemClass.Level.Level2;
					break;
				case InfoManager.SubInfoMode.WindPower:
					level = ItemClass.Level.Level3;
					break;
				}
				if (level == m_info.m_class.m_level && m_info.m_class.m_service == ItemClass.Service.Education)
				{
					if ((data.m_flags & Building.Flags.Active) != 0)
					{
						return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_activeColor;
					}
					return Singleton<InfoManager>.instance.m_properties.m_modeProperties[(int)infoMode].m_inactiveColor;
				}
				return base.GetColor(buildingID, ref data, infoMode);
			}
			return base.GetColor(buildingID, ref data, infoMode);
		}

		public virtual void GetStudentCount(ushort buildingID, ref Building data, out int count, out int capacity, out int global)
		{
			int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
			int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
			count = data.m_customBuffer1;
			capacity = Mathf.Min((productionRate * StudentCount + 99) / 100, StudentCount * 5 / 4);
			global = 0;
			BuildingManager instance = Singleton<BuildingManager>.instance;
			FastList<ushort> serviceBuildings = instance.GetServiceBuildings(ItemClass.Service.Education);
			int size = serviceBuildings.m_size;
			ushort[] buffer = serviceBuildings.m_buffer;
			if (buffer != null && size <= buffer.Length)
			{
				for (int i = 0; i < size; i++)
				{
					ushort num = buffer[i];
					if (num != 0)
					{
						BuildingInfo info = instance.m_buildings.m_buffer[num].Info;
						if (info.m_class.m_service == ItemClass.Service.Education && info.m_class.m_level == m_info.m_class.m_level)
						{
							global += instance.m_buildings.m_buffer[num].m_customBuffer1;
						}
					}
				}
			}
			if (m_info.m_class.m_level != ItemClass.Level.Level3)
			{
				return;
			}
			FastList<ushort> serviceBuildings2 = instance.GetServiceBuildings(ItemClass.Service.PlayerEducation);
			size = serviceBuildings2.m_size;
			buffer = serviceBuildings2.m_buffer;
			if (buffer == null || size > buffer.Length)
			{
				return;
			}
			for (int j = 0; j < size; j++)
			{
				ushort num2 = buffer[j];
				if (num2 != 0)
				{
					BuildingInfo info2 = instance.m_buildings.m_buffer[num2].Info;
					if (info2.m_class.m_service == ItemClass.Service.PlayerEducation && info2.m_class.m_level == m_info.m_class.m_level)
					{
						global += instance.m_buildings.m_buffer[num2].m_customBuffer1;
					}
				}
			}
		}

		public override void GetPlacementInfoMode(out InfoManager.InfoMode mode, out InfoManager.SubInfoMode subMode, float elevation)
		{
			mode = InfoManager.InfoMode.Education;
			if (m_info.m_class.m_level == ItemClass.Level.Level1)
			{
				subMode = InfoManager.SubInfoMode.Default;
			}
			else if (m_info.m_class.m_level == ItemClass.Level.Level2)
			{
				subMode = InfoManager.SubInfoMode.WaterPower;
			}
			else
			{
				subMode = InfoManager.SubInfoMode.WindPower;
			}
		}

		public override void CreateBuilding(ushort buildingID, ref Building data)
		{
			base.CreateBuilding(buildingID, ref data);
			int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
			Singleton<CitizenManager>.instance.CreateUnits(out data.m_citizenUnits, ref Singleton<SimulationManager>.instance.m_randomizer, buildingID, 0, 0, workCount, 0, 0, StudentCount * 5 / 4);
		}

		public override void BuildingLoaded(ushort buildingID, ref Building data, uint version)
		{
			base.BuildingLoaded(buildingID, ref data, version);
			int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
			EnsureCitizenUnits(buildingID, ref data, 0, workCount, 0, StudentCount * 5 / 4);
		}

		public override void ReleaseBuilding(ushort buildingID, ref Building data)
		{
			ushort num = TransportTool.FindBuildingLine(buildingID);
			if (num != 0)
			{
				Singleton<TransportManager>.instance.ReleaseLine(num);
			}
			base.ReleaseBuilding(buildingID, ref data);
		}

		public override void EndRelocating(ushort buildingID, ref Building data)
		{
			base.EndRelocating(buildingID, ref data);
			int workCount = m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
			EnsureCitizenUnits(buildingID, ref data, 0, workCount, 0, StudentCount * 5 / 4);
			ushort num = TransportTool.FindBuildingLine(buildingID);
			if (num == 0)
			{
				return;
			}
			TransportManager instance = Singleton<TransportManager>.instance;
			SimulationManager instance2 = Singleton<SimulationManager>.instance;
			VehicleManager instance3 = Singleton<VehicleManager>.instance;
			if (instance.m_lines.m_buffer[num].m_stops != 0)
			{
				VehicleInfo randomVehicleInfo = instance3.GetRandomVehicleInfo(ref instance2.m_randomizer, m_info.m_class.m_service, m_info.m_class.m_subService, m_info.m_class.m_level);
				if ((object)randomVehicleInfo != null)
				{
					CalculateSpawnPosition(buildingID, ref data, ref instance2.m_randomizer, randomVehicleInfo, out var position, out var _);
					instance.m_lines.m_buffer[num].MoveStop(num, 0, position, fixedPlatform: false);
				}
			}
		}

		protected override void ManualActivation(ushort buildingID, ref Building buildingData)
		{
			if (EducationAccumulation != 0)
			{
				Vector3 position = buildingData.m_position;
				position.y += m_info.m_size.y;
				Singleton<NotificationManager>.instance.AddEvent(NotificationEvent.Type.GainHappiness, position, 1.5f);
				if (m_info.m_class.m_level == ItemClass.Level.Level3)
				{
					Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Happy, ImmaterialResourceManager.Resource.EducationUniversity, EducationAccumulation, m_educationRadius);
				}
				else if (m_info.m_class.m_level == ItemClass.Level.Level2)
				{
					Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Happy, ImmaterialResourceManager.Resource.EducationHighSchool, EducationAccumulation, m_educationRadius);
				}
				else
				{
					Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Happy, ImmaterialResourceManager.Resource.EducationElementary, EducationAccumulation, m_educationRadius);
				}
			}
		}

		protected override void ManualDeactivation(ushort buildingID, ref Building buildingData)
		{
			if ((buildingData.m_flags & Building.Flags.Collapsed) != 0)
			{
				Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Happy, ImmaterialResourceManager.Resource.Abandonment, -buildingData.Width * buildingData.Length, 64f);
			}
			else if (EducationAccumulation != 0)
			{
				Vector3 position = buildingData.m_position;
				position.y += m_info.m_size.y;
				Singleton<NotificationManager>.instance.AddEvent(NotificationEvent.Type.LoseHappiness, position, 1.5f);
				if (m_info.m_class.m_level == ItemClass.Level.Level3)
				{
					Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Sad, ImmaterialResourceManager.Resource.EducationUniversity, -EducationAccumulation, m_educationRadius);
				}
				else if (m_info.m_class.m_level == ItemClass.Level.Level2)
				{
					Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Sad, ImmaterialResourceManager.Resource.EducationHighSchool, -EducationAccumulation, m_educationRadius);
				}
				else
				{
					Singleton<NotificationManager>.instance.AddWaveEvent(buildingData.m_position, NotificationEvent.Type.Sad, ImmaterialResourceManager.Resource.EducationElementary, -EducationAccumulation, m_educationRadius);
				}
			}
		}

		public override void StartTransfer(ushort buildingID, ref Building data, TransferManager.TransferReason material, TransferManager.TransferOffer offer)
		{
			base.StartTransfer(buildingID, ref data, material, offer);
		}

		public override void BuildingDeactivated(ushort buildingID, ref Building data)
		{
			TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
			offer.Building = buildingID;
			if (m_info.m_class.m_level == ItemClass.Level.Level3)
			{
				Singleton<TransferManager>.instance.RemoveIncomingOffer(TransferManager.TransferReason.Student3, offer);
			}
			else if (m_info.m_class.m_level == ItemClass.Level.Level2)
			{
				Singleton<TransferManager>.instance.RemoveIncomingOffer(TransferManager.TransferReason.Student2, offer);
			}
			else
			{
				Singleton<TransferManager>.instance.RemoveIncomingOffer(TransferManager.TransferReason.Student1, offer);
			}
			base.BuildingDeactivated(buildingID, ref data);
		}

		public override float GetCurrentRange(ushort buildingID, ref Building data)
		{
			int num = data.m_productionRate;
			if ((data.m_flags & (Building.Flags.Evacuating | Building.Flags.Active)) != Building.Flags.Active)
			{
				num = 0;
			}
			else if ((data.m_flags & Building.Flags.RateReduced) != 0)
			{
				num = Mathf.Min(num, 50);
			}
			int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
			num = PlayerBuildingAI.GetProductionRate(num, budget);
			return (float)num * m_educationRadius * 0.01f;
		}

		protected override void HandleWorkAndVisitPlaces(ushort buildingID, ref Building buildingData, ref Citizen.BehaviourData behaviour, ref int aliveWorkerCount, ref int totalWorkerCount, ref int workPlaceCount, ref int aliveVisitorCount, ref int totalVisitorCount, ref int visitPlaceCount)
		{
			workPlaceCount += m_workPlaceCount0 + m_workPlaceCount1 + m_workPlaceCount2 + m_workPlaceCount3;
			GetWorkBehaviour(buildingID, ref buildingData, ref behaviour, ref aliveWorkerCount, ref totalWorkerCount);
			HandleWorkPlaces(buildingID, ref buildingData, m_workPlaceCount0, m_workPlaceCount1, m_workPlaceCount2, m_workPlaceCount3, ref behaviour, aliveWorkerCount, totalWorkerCount);
		}

		public override void SimulationStep(ushort buildingID, ref Building buildingData, ref Building.Frame frameData)
		{
			base.SimulationStep(buildingID, ref buildingData, ref frameData);
			if ((buildingData.m_flags & Building.Flags.Active) == 0)
			{
				Citizen.BehaviourData behaviour = default(Citizen.BehaviourData);
				int aliveCount = 0;
				int totalCount = 0;
				GetStudentBehaviour(buildingID, ref buildingData, ref behaviour, ref aliveCount, ref totalCount);
				buildingData.m_customBuffer1 = (ushort)aliveCount;
			}
		}

		protected override int AdjustMaintenanceCost(ushort buildingID, ref Building data, int maintenanceCost)
		{
			int num = base.AdjustMaintenanceCost(buildingID, ref data, maintenanceCost);
			DistrictManager instance = Singleton<DistrictManager>.instance;
			byte district = instance.GetDistrict(data.m_position);
			DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[district].m_servicePolicies;
			if ((servicePolicies & DistrictPolicies.Services.ForProfitEducation) != 0)
			{
				instance.m_districts.m_buffer[district].m_servicePoliciesEffect |= DistrictPolicies.Services.ForProfitEducation;
				num -= (maintenanceCost * 50 + 99) / 100;
			}
			if ((servicePolicies & DistrictPolicies.Services.EducationBoost) != 0)
			{
				instance.m_districts.m_buffer[district].m_servicePoliciesEffect |= DistrictPolicies.Services.EducationBoost;
				num += (maintenanceCost * 25 + 99) / 100;
			}
			return num;
		}

		protected override void ProduceGoods(ushort buildingID, ref Building buildingData, ref Building.Frame frameData, int productionRate, int finalProductionRate, ref Citizen.BehaviourData behaviour, int aliveWorkerCount, int totalWorkerCount, int workPlaceCount, int aliveVisitorCount, int totalVisitorCount, int visitPlaceCount)
		{
			base.ProduceGoods(buildingID, ref buildingData, ref frameData, productionRate, finalProductionRate, ref behaviour, aliveWorkerCount, totalWorkerCount, workPlaceCount, aliveVisitorCount, totalVisitorCount, visitPlaceCount);
			int aliveCount = 0;
			int totalCount = 0;
			GetStudentBehaviour(buildingID, ref buildingData, ref behaviour, ref aliveCount, ref totalCount);
			if (aliveCount != 0)
			{
				behaviour.m_crimeAccumulation = behaviour.m_crimeAccumulation * aliveWorkerCount / (aliveWorkerCount + aliveCount);
			}
			DistrictManager instance = Singleton<DistrictManager>.instance;
			byte district = instance.GetDistrict(buildingData.m_position);
			DistrictPolicies.Services servicePolicies = instance.m_districts.m_buffer[district].m_servicePolicies;
			int num = productionRate * EducationAccumulation / 100;
			if ((servicePolicies & DistrictPolicies.Services.EducationalBlimps) != 0)
			{
				num = (num * 21 + 10) / 20;
				instance.m_districts.m_buffer[district].m_servicePoliciesEffect |= DistrictPolicies.Services.EducationalBlimps;
			}
			if (num != 0)
			{
				if (m_info.m_class.m_level == ItemClass.Level.Level3)
				{
					Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.EducationUniversity, num, buildingData.m_position, m_educationRadius);
				}
				else if (m_info.m_class.m_level == ItemClass.Level.Level2)
				{
					Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.EducationHighSchool, num, buildingData.m_position, m_educationRadius);
				}
				else
				{
					Singleton<ImmaterialResourceManager>.instance.AddResource(ImmaterialResourceManager.Resource.EducationElementary, num, buildingData.m_position, m_educationRadius);
				}
			}
			if (finalProductionRate == 0)
			{
				return;
			}
			buildingData.m_customBuffer1 = (ushort)aliveCount;
			if (m_info.m_class.m_level == ItemClass.Level.Level3 && (servicePolicies & DistrictPolicies.Services.SchoolsOut) != 0)
			{
				instance.m_districts.m_buffer[district].m_servicePoliciesEffect |= DistrictPolicies.Services.SchoolsOut;
			}
			int num2 = Mathf.Min((finalProductionRate * StudentCount + 99) / 100, StudentCount * 5 / 4);
			int num3 = num2 - totalCount;
			if (m_info.m_class.m_level == ItemClass.Level.Level1)
			{
				instance.m_districts.m_buffer[district].m_productionData.m_tempEducation1Capacity += (uint)num2;
				instance.m_districts.m_buffer[district].m_student1Data.m_tempCount += (uint)aliveCount;
			}
			else if (m_info.m_class.m_level == ItemClass.Level.Level2)
			{
				instance.m_districts.m_buffer[district].m_productionData.m_tempEducation2Capacity += (uint)num2;
				instance.m_districts.m_buffer[district].m_student2Data.m_tempCount += (uint)aliveCount;
			}
			else
			{
				instance.m_districts.m_buffer[district].m_productionData.m_tempEducation3Capacity += (uint)num2;
				instance.m_districts.m_buffer[district].m_student3Data.m_tempCount += (uint)aliveCount;
			}
			CampusBuildingAI campusBuildingAI = buildingData.Info.m_buildingAI as CampusBuildingAI;
			if (campusBuildingAI != null)
			{
				campusBuildingAI.HandleDead2(buildingID, ref buildingData, ref behaviour, totalWorkerCount + totalCount);
			}
			else
			{
				HandleDead(buildingID, ref buildingData, ref behaviour, totalWorkerCount + totalCount);
			}
			if (num3 >= 1)
			{
				TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
				offer.Priority = Mathf.Max(1, num3 * 8 / num2);
				offer.Building = buildingID;
				offer.Position = buildingData.m_position;
				offer.Amount = num3;
				if (m_info.m_class.m_level == ItemClass.Level.Level3)
				{
					Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.Student3, offer);
				}
				else if (m_info.m_class.m_level == ItemClass.Level.Level2)
				{
					Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.Student2, offer);
				}
				else
				{
					Singleton<TransferManager>.instance.AddIncomingOffer(TransferManager.TransferReason.Student1, offer);
				}
			}
			HandleSchoolVehicles(buildingID, ref buildingData, finalProductionRate);
		}

		protected override bool CanEvacuate()
		{
			return m_workPlaceCount0 != 0 || m_workPlaceCount1 != 0 || m_workPlaceCount2 != 0 || m_workPlaceCount3 != 0 || StudentCount != 0;
		}

		public override bool EnableNotUsedGuide()
		{
			return true;
		}

		public override bool GetEducationLevel1()
		{
			return m_info.m_class.m_level == ItemClass.Level.Level1;
		}

		public override bool GetEducationLevel2()
		{
			return m_info.m_class.m_level == ItemClass.Level.Level2;
		}

		public override bool GetEducationLevel3()
		{
			return m_info.m_class.m_level == ItemClass.Level.Level3;
		}

		public override string GetLocalizedTooltip()
		{
			string text = LocaleFormatter.FormatGeneric("AIINFO_WATER_CONSUMPTION", GetWaterConsumption() * 16) + Environment.NewLine + LocaleFormatter.FormatGeneric("AIINFO_ELECTRICITY_CONSUMPTION", GetElectricityConsumption() * 16);
			return TooltipHelper.Append(base.GetLocalizedTooltip(), TooltipHelper.Format(LocaleFormatter.Info1, text, LocaleFormatter.Info2, LocaleFormatter.FormatGeneric("AIINFO_STUDENT_COUNT", m_studentCount)));
		}

		public override string GetLocalizedStats(ushort buildingID, ref Building data)
		{
			GetStudentCount(buildingID, ref data, out var count, out var capacity, out var global);
			string text = LocaleFormatter.FormatGeneric("AIINFO_STUDENTS", count, capacity) + Environment.NewLine;
			string localeID = string.Empty;
			if (m_info.m_class.m_level == ItemClass.Level.Level1)
			{
				localeID = "AIINFO_ELEMENTARY_STUDENTCOUNT";
			}
			else if (m_info.m_class.m_level == ItemClass.Level.Level2)
			{
				localeID = "AIINFO_HIGHSCHOOL_STUDENTCOUNT";
			}
			else if (m_info.m_class.m_level == ItemClass.Level.Level3)
			{
				localeID = "AIINFO_UNIVERSITY_STUDENTCOUNT";
			}
			int budget = Singleton<EconomyManager>.instance.GetBudget(m_info.m_class);
			int productionRate = PlayerBuildingAI.GetProductionRate(100, budget);
			int num5 = (productionRate * m_schoolBusCount + 99) / 100;
			text += LocaleFormatter.FormatGeneric(localeID, global);
			if(m_info.m_class.m_level < ItemClass.Level.Level3)
            {
				return text + Environment.NewLine + LocaleFormatter.FormatGeneric("School Bus Count", num5);
            } 
			else
            {
				return text;
            }
			
		}

		public override bool RequireRoadAccess()
		{
			return true;
		}

		public override TransportInfo GetTransportLineInfo()
		{
			return m_transportInfo;
		}

		public override void CalculateSpawnPosition(ushort buildingID, ref Building data, ref Randomizer randomizer, VehicleInfo info, out Vector3 position, out Vector3 target)
		{
			if (info.m_vehicleType == m_transportInfo.m_vehicleType)
			{
				Vector3 spawnPosition = m_spawnPosition;
				Vector3 spawnTarget = m_spawnTarget;
				position = data.CalculatePosition(spawnPosition);
				if (m_canInvertTarget && Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic == SimulationMetaData.MetaBool.True)
				{
					target = data.CalculatePosition(spawnPosition * 2f - spawnTarget);
				}
				else
				{
					target = data.CalculatePosition(spawnTarget);
				}
			}
			else
			{
				base.CalculateSpawnPosition(buildingID, ref data, ref randomizer, info, out position, out target);
			}
		}

		public override void CalculateUnspawnPosition(ushort buildingID, ref Building data, ref Randomizer randomizer, VehicleInfo info, out Vector3 position, out Vector3 target)
		{
			if (info.m_vehicleType == m_transportInfo.m_vehicleType)
			{
				Vector3 spawnPosition = m_spawnPosition;
				Vector3 spawnTarget = m_spawnTarget;
				position = data.CalculatePosition(spawnPosition);
				if (m_canInvertTarget && Singleton<SimulationManager>.instance.m_metaData.m_invertTraffic != SimulationMetaData.MetaBool.True)
				{
					target = data.CalculatePosition(spawnPosition * 2f - spawnTarget);
				}
				else
				{
					target = data.CalculatePosition(spawnTarget);
				}
			}
			else
			{
				base.CalculateUnspawnPosition(buildingID, ref data, ref randomizer, info, out position, out target);
			}
		}

		private void HandleSchoolVehicles(ushort buildingID, ref Building data, int productionRate)
		{
			VehicleManager instance = Singleton<VehicleManager>.instance;
			TransferManager.TransferReason transferReason = TransferManager.TransferReason.Student1;
			TransferManager.TransferReason transferReason2 = TransferManager.TransferReason.Student2;
			int num = (productionRate * m_schoolBusCount + 99) / 100;
			int num2 = num;
			ushort num3 = TransportTool.FindBuildingLine(buildingID);
			ushort num4 = 0;
			if (num3 == 0)
			{
				num2 = 0;
			}
			else
			{
				TransportManager instance2 = Singleton<TransportManager>.instance;
				num4 = instance2.m_lines.m_buffer[num3].m_stops;
				if (!instance2.m_lines.m_buffer[num3].Complete)
				{
					num4 = 0;
				}
				if (num4 == 0)
				{
					num2 = 0;
				}
			}
			if ((data.m_flags & Building.Flags.Downgrading) != 0)
			{
				num2 = 0;
			}
			if (num3 != 0)
			{
				TransportManager instance3 = Singleton<TransportManager>.instance;
				bool flag = num4 != 0 && num2 != 0;
				instance3.m_lines.m_buffer[num3].SetActive(flag, flag);
			}
			ushort num5 = data.m_ownVehicles;
			int num6 = 0;
			int num7 = 0;
			int num8 = 0;
			while (num5 != 0)
			{
				if ((TransferManager.TransferReason)instance.m_vehicles.m_buffer[num5].m_transferType == transferReason || (TransferManager.TransferReason)instance.m_vehicles.m_buffer[num5].m_transferType == transferReason2)
				{
					if ((instance.m_vehicles.m_buffer[num5].m_flags & Vehicle.Flags.GoingBack) == 0)
					{
						if (num6 >= num2)
						{
							VehicleInfo info = instance.m_vehicles.m_buffer[num5].Info;
							info.m_vehicleAI.SetTransportLine(num5, ref instance.m_vehicles.m_buffer[num5], 0);
						}
						else
						{
							num6++;
						}
					}
					num7++;
				}
				num5 = instance.m_vehicles.m_buffer[num5].m_nextOwnVehicle;
				if (++num8 > 16384)
				{
					CODebugBase<LogChannel>.Error(LogChannel.Core, "Invalid list detected!\n" + Environment.StackTrace);
					break;
				}
			}
			if (num2 <= num6 || num <= num7)
			{
				return;
			}
			VehicleInfo randomVehicleInfo = Singleton<VehicleManager>.instance.GetRandomVehicleInfo(ref Singleton<SimulationManager>.instance.m_randomizer, m_info.m_class.m_service, m_info.m_class.m_subService, m_info.m_class.m_level);
			if ((object)randomVehicleInfo != null)
			{
				Array16<Vehicle> vehicles = Singleton<VehicleManager>.instance.m_vehicles;
				CalculateSpawnPosition(buildingID, ref data, ref Singleton<SimulationManager>.instance.m_randomizer, randomVehicleInfo, out var position, out var _);
				if (Singleton<VehicleManager>.instance.CreateVehicle(out var vehicle, ref Singleton<SimulationManager>.instance.m_randomizer, randomVehicleInfo, position, transferReason, transferToSource: false, transferToTarget: true))
				{
					TransferManager.TransferOffer offer = default(TransferManager.TransferOffer);
					offer.TransportLine = num3;
					offer.Position = Singleton<NetManager>.instance.m_nodes.m_buffer[num4].m_position;
					offer.Amount = 1;
					offer.Active = false;
					randomVehicleInfo.m_vehicleAI.SetSource(vehicle, ref vehicles.m_buffer[vehicle], buildingID);
					randomVehicleInfo.m_vehicleAI.StartTransfer(vehicle, ref vehicles.m_buffer[vehicle], transferReason, offer);
				}
			}
		}
	}
}
