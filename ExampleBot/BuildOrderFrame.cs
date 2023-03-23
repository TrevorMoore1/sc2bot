using Google.Protobuf.Collections;
using SC2APIProtocol;
using System.Collections.Generic;
using System.Linq;

namespace SC2Sharp
{
    internal class BuildOrderFrame
    {
        private Observation observation;
        private MapData mapData;
        private ulong builderTag;
        private uint pylonsBuilt;
        private uint gatewaysBuilt;
        private BuildOrderStatus buildOrderStatus;

        public ActionList actions;

        public BuildOrderFrame(Observation observation, MapData mapData, BuildOrderStatus buildOrderStatus)
        {
            this.observation = observation;
            this.mapData = mapData;
            this.buildOrderStatus = buildOrderStatus;
            actions = new ActionList();
            AttemptBuildOrderAction();
        }

        private void AttemptBuildOrderAction()
        {
            ulong? highestEnergyNexusId = null;
            RepeatedField<Unit> unitList = observation.RawData.Units;
            List<Unit> myUnits = unitList.Where(unit => unit.Alliance == Alliance.Self).ToList();
            List<Unit> enemyUnits = unitList.Where(unit => unit.Alliance == Alliance.Enemy).ToList();
            List<Unit> myWorkers = myUnits.Where(unit => unit.UnitType == (int)UNIT_TYPEID.PROTOSS_PROBE).ToList();
            BuildOrderEvents nextBuildOrderAction = BuildOrder.buildOrder[buildOrderStatus.BuildOrderProgress];
            if (buildOrderStatus.chronoNextAction)
            {
                List<Unit> nexusList = myUnits.Where(unit => unit.UnitType == (int)UNIT_TYPEID.PROTOSS_NEXUS).ToList();
                float highestEnergy = 50;
                foreach (Unit nexus in nexusList)
                {
                    if (nexus.Energy >= highestEnergy)
                    {
                        highestEnergy = nexus.Energy;
                        highestEnergyNexusId = nexus.Tag;
                    }
                }
            }
            switch (nextBuildOrderAction)
            {
                case BuildOrderEvents.BuildOrderBegin:
                    buildOrderStatus.AdvanceBuildOrder(false);
                    MoveBuilder();
                    break;
                case BuildOrderEvents.Probe:
                    if (observation.PlayerCommon.Minerals >= 50)
                    {
                        ulong? freeNexusId = myUnits.Where(unit => unit.UnitType == (int)UNIT_TYPEID.PROTOSS_NEXUS && !unit.IsActive).Select(u => (ulong?)u.Tag).ToList().FirstOrDefault();
                        if (freeNexusId != null)
                        {
                            actions.DoUntargetedAction(ABILITY_ID.TRAIN_PROBE, (ulong)freeNexusId);
                            buildOrderStatus.AdvanceBuildOrder(false);
                            if (buildOrderStatus.chronoNextAction && highestEnergyNexusId != null)
                            {
                                actions.DoTargetedAction(ABILITY_ID.EFFECT_CHRONOBOOST, (ulong)highestEnergyNexusId, (ulong)freeNexusId);
                                buildOrderStatus.chronoNextAction = false;
                            }
                        }
                    }
                    break;
                case BuildOrderEvents.Pylon:
                    if (observation.PlayerCommon.Minerals >= 100) //Need to check if space is empty
                    {
                        actions.DoSpaceAction(ABILITY_ID.BUILD_PYLON, builderTag, buildOrderStatus.NextBuildingPos.location);
                        buildOrderStatus.NextBuildingPos.status = StructureStatus.Warping;
                        buildOrderStatus.AdvanceBuildOrder(true);
                        MoveBuilder();
                    }
                    break;
                case BuildOrderEvents.Gateway:
                    if (observation.PlayerCommon.Minerals >= 150) //Need to check if space is empty, powered
                    {
                        buildOrderStatus.NextBuildingPos.status = StructureStatus.Warping;
                        actions.DoSpaceAction(ABILITY_ID.BUILD_GATEWAY, builderTag, buildOrderStatus.NextBuildingPos.location);
                        buildOrderStatus.AdvanceBuildOrder(true);
                        MoveBuilder();
                    }
                    break;
                case BuildOrderEvents.ChronoNext:
                    buildOrderStatus.chronoNextAction = true;
                    break;
                default:
                    buildOrderStatus.AdvanceBuildOrder(false);
                    break;

            }
        }
        
        private bool SetNextBuildingPos( )
        {
            bool positionFound = false;
            if (buildOrderStatus.NextBuildingType == BuildOrderEvents.Pylon)
            {
                for (int i = 0; i < mapData.basePlan.Count && (!positionFound); i++)
                {
                    if (mapData.basePlan[i].primaryPylon.status == StructureStatus.EmptyUnpowered)
                    {
                        positionFound = true;
                        buildOrderStatus.NextBuildingPos = mapData.basePlan[i].primaryPylon;
                    }
                }
                for (int i = 0; i < mapData.basePlan.Count && (!positionFound); i++)
                {
                    for (int j = 0; j < mapData.basePlan[i].secondaryPylonLocations.Count && (!positionFound); j++)
                    {
                        if (mapData.basePlan[i].secondaryPylonLocations[j].status == StructureStatus.EmptyUnpowered)
                        {
                            positionFound = true;
                            buildOrderStatus.NextBuildingPos = mapData.basePlan[i].secondaryPylonLocations[j];
                        }
                    }
                }
            }
            else
            {
                for (int i = 0; i < mapData.basePlan.Count && (!positionFound); i++)
                {
                    for (int j = 0; j < mapData.basePlan[i].buildingLocations.Count && (!positionFound); j++)
                    {
                        if (mapData.basePlan[i].buildingLocations[j].status == StructureStatus.EmptyPowered)
                        {
                            positionFound = true;
                            buildOrderStatus.NextBuildingPos = mapData.basePlan[i].buildingLocations[j];
                        }
                    }
                }
            }
            return positionFound;
        }

        private void MoveBuilder()
        {
            SetNextBuildingPos();
            actions.QueueSpaceAction(ABILITY_ID.MOVE, builderTag, buildOrderStatus.NextBuildingPos.location);
        }

        private void UpdatePylonData()
        {
            foreach (BuildingCluster cluster in mapData.basePlan)
            {
                cluster.primaryPylon.status = StructureStatus.EmptyUnpowered;
                foreach (BuildingLocation secondaryPylon in cluster.secondaryPylonLocations)
                {
                    secondaryPylon.status = StructureStatus.EmptyUnpowered;
                }

            }
            List<Unit> myPylons = observation.RawData.Units.Where(unit => unit.Alliance == Alliance.Self && unit.UnitType == (uint)UNIT_TYPEID.PROTOSS_PYLON).ToList();
            foreach (Unit pylon in myPylons)
            {
                bool foundCluster = false;
                for (int i = 0; i < mapData.basePlan.Count && (!foundCluster); i++)
                {
                    if (mapData.basePlan[i].primaryPylon.location.X == pylon.Pos.X && mapData.basePlan[i].primaryPylon.location.Y == pylon.Pos.Y)
                    {
                        if (pylon.BuildProgress == 1) { mapData.basePlan[i].primaryPylon.status = StructureStatus.CompletedPowered; }
                        else { mapData.basePlan[i].primaryPylon.status = StructureStatus.Warping; }
                    }
                    else
                    {
                        for (int j = 0; j < mapData.basePlan[i].secondaryPylonLocations.Count && (!foundCluster); j++)
                        {
                            if (mapData.basePlan[i].secondaryPylonLocations[j].location.X == pylon.Pos.X && mapData.basePlan[i].secondaryPylonLocations[j].location.Y == pylon.Pos.Y)
                            {
                                if (pylon.BuildProgress == 1) { mapData.basePlan[i].secondaryPylonLocations[j].status = StructureStatus.CompletedPowered; }
                                else { mapData.basePlan[i].secondaryPylonLocations[j].status = StructureStatus.Warping; }
                            }
                        }
                    }
                }
            }
            foreach (BuildingCluster cluster in mapData.basePlan)
            {
                cluster.updateBuildingPowerStatus();
            }
        }

    }
}