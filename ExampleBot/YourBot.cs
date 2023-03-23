using System.Collections.Generic;
using System.Linq;
using Google.Protobuf.Collections;
using SC2APIProtocol;

namespace SC2Sharp
{
    class Bot : SC2API_CSharp.Bot
    {
        private uint playerId;
        private ulong? firstTargetTag;
        private RepeatedField<ulong> mainMineralWorkerTags;
        private RepeatedField<ulong> mainArmyTags;
        private RepeatedField<ulong> usedUnitTags;
        private MapData mapData;
        private List<ulong> nexusIds;
        private List<ulong> gatewayIds;
        private ulong? builderTag;
        private uint pylonsBuilt;
        private uint gatewaysBuilt;
        private UNIT_TYPEID nextBuildingType;
        private Point2D nextBuildingPos;
        private bool doneBuilding;
        private BuildOrderStatus buildOrderStatus;

        public Bot()
        {
            mainMineralWorkerTags = new RepeatedField<ulong>();
            mainArmyTags = new RepeatedField<ulong>();
            usedUnitTags = new RepeatedField<ulong>();
        }

        public void OnStart(ResponseGameInfo gameInfo, ResponseData data, ResponsePing pingResponse, ResponseObservation observation, uint playerId, string opponentID)
        {
            this.playerId = playerId;
            mapData = new MapData(gameInfo.StartRaw, observation.Observation.RawData.Units);
            doneBuilding = false;
            buildOrderStatus = new BuildOrderStatus();
        }

        public IEnumerable<Action> OnFrame(ResponseObservation observation)
        {
            ActionList actions = new ActionList();
            RepeatedField<Unit> unitList = observation.Observation.RawData.Units;
            List<Unit> myUnits = unitList.Where(unit => unit.Owner == playerId).ToList();
            List<Unit> enemyUnits = unitList.Where(unit => unit.Alliance == Alliance.Enemy).ToList();
            List<Unit> myWorkers = myUnits.Where(unit => unit.UnitType == (int)UNIT_TYPEID.PROTOSS_PROBE).ToList();
            foreach (Unit worker in myWorkers)
            {
                ulong tag = worker.Tag;
                if (builderTag == null)
                {
                    builderTag = worker.Tag;
                    actions.DoUntargetedAction(ABILITY_ID.STOP, worker.Tag);
                }
                if (tag == builderTag && !doneBuilding)
                {
                    if (worker.Orders.Count == 0 && (worker.Pos.X != nextBuildingPos.X || worker.Pos.Y != nextBuildingPos.Y))
                    {
                        actions.DoSpaceAction(ABILITY_ID.MOVE, worker.Tag, nextBuildingPos);
                    }
                    else if (observation.Observation.PlayerCommon.Minerals >= 100 && nextBuildingType == UNIT_TYPEID.PROTOSS_PYLON)
                    {
                        actions.DoSpaceAction(ABILITY_ID.BUILD_PYLON, worker.Tag, nextBuildingPos);
                        pylonsBuilt += 1;
                    }
                    else if (observation.Observation.PlayerCommon.Minerals >= 150 && nextBuildingType == UNIT_TYPEID.PROTOSS_GATEWAY)
                    {
                        actions.DoSpaceAction(ABILITY_ID.BUILD_GATEWAY, worker.Tag, nextBuildingPos);
                        gatewaysBuilt += 1;
                    }
                }
            }
            return actions.actionList;
        }

        //private void computeNextBuilding()
        //{
        //    nextBuildingPos = new Point2D();
        //    if (pylonsBuilt < mapData.pylonArray.Length || (pylonsBuilt < mapData.pylonArray.Length + mapData.secondPylonArray.Length && gatewaysBuilt >= mapData.buildingArray.Length ))
        //    {
        //        nextBuildingType = UNIT_TYPEID.PROTOSS_PYLON;
        //        nextBuildingPos.X = (pylonsBuilt < mapData.pylonArray.Length ? mapData.pylonArray[pylonsBuilt] : mapData.secondPylonArray[pylonsBuilt - mapData.pylonArray.Length]).X + 1;
        //        nextBuildingPos.Y = (pylonsBuilt < mapData.pylonArray.Length ? mapData.pylonArray[pylonsBuilt] : mapData.secondPylonArray[pylonsBuilt - mapData.pylonArray.Length]).Y + 1;
        //    }
        //    else if (gatewaysBuilt < mapData.buildingArray.Length)
        //    {
        //        nextBuildingType = UNIT_TYPEID.PROTOSS_GATEWAY;
        //        nextBuildingPos.X = mapData.buildingArray[gatewaysBuilt].X + (float)1.5;
        //        nextBuildingPos.Y = mapData.buildingArray[gatewaysBuilt].Y + (float)1.5;
        //    }
        //    else
        //    {
        //        doneBuilding = true;
        //    }
        //}

        public void OnEnd(ResponseObservation observation, Result result)
        { }
    }
}
