using System.Collections.Generic;
using System.Linq;
using System;
using Google.Protobuf.Collections;
using SC2APIProtocol;

namespace SC2Sharp
{
    internal class MapData
    {
        public SC2APIProtocol.StartRaw rawMap;
        public int mapSizeX;
        public int mapSizeY;
        public int minMainX;
        public int minMainY;
        public int maxMainX;
        public int maxMainY;
        public int mainSizeX;
        public int mainSizeY;
        public List<IntPoint> myMainBoundary;
        public List<IntPoint> myMainRampBoundary;
        public List<IntPoint> myNatBoundary;
        public List<IntPoint> oppMainBoundary;
        public List<IntPoint> oppMainRampBoundary;
        public List<IntPoint> oppNatBoundary;
        public List<BuildingCluster> basePlan;
        public MainData[] myMainData;
        public MapRegion[] mapRegions;
        public MainData[] bestMainPlan;

        public MapData(SC2APIProtocol.StartRaw rawMap, RepeatedField<SC2APIProtocol.Unit> unitList)
        {
            IntPoint myStartLoc = new IntPoint(0, 0);
            for (int i = 0; i < unitList.Count; i++)
            {
                if (unitList[i].UnitType == (int) UNIT_TYPEID.PROTOSS_NEXUS)
                {
                    myStartLoc = new IntPoint((int)unitList[i].Pos.X, (int)unitList[i].Pos.Y);
                }
            }
            Random random = new Random();
            this.rawMap = rawMap;
            mapSizeX = rawMap.MapSize.X;
            mapSizeY = rawMap.MapSize.Y;
            (oppMainBoundary, oppMainRampBoundary) = DetermineMain(new IntPoint(rawMap.StartLocations[0]));
            (myMainBoundary, myMainRampBoundary) = DetermineMain(myStartLoc);
            mapRegions = constructMapRegions(myMainBoundary, oppMainBoundary);
            minMainX = myMainBoundary.Select(b => b.X).Min();
            maxMainX = myMainBoundary.Select(b => b.X).Max();
            minMainY = myMainBoundary.Select(b => b.Y).Min();
            maxMainY = myMainBoundary.Select(b => b.Y).Max();
            mainSizeX = maxMainX - minMainX + 1;
            mainSizeY = maxMainY - minMainY + 1;
            myMainData = new MainData[mainSizeX * mainSizeY];
            bestMainPlan = new MainData[mainSizeX * mainSizeY];
            int bestMainBuildings = 0;
            MainData[] currentMainPlan = new MainData[mainSizeX * mainSizeY];
            MainData[] tempMainPlan = new MainData[mainSizeX * mainSizeY];
            IntPoint[] BuildingOffsets = new IntPoint[] {
                new IntPoint(-6, -2), new IntPoint(-3, -2), new IntPoint(2, -2), new IntPoint(5, -2),
                new IntPoint(-6, 1), new IntPoint(-3, 1), new IntPoint(2, 1), new IntPoint(5, 1)};
            for (int x = minMainX; x <= maxMainX; x++)
            {
                for (int y = minMainY; y < maxMainY; y++)
                {
                    myMainData[(x - minMainX) + mainSizeX * (y - minMainY)] =
                        mapRegions[x + y * mapSizeX] != MapRegion.myMain ? MainData.notMain : 
                        (IsBuildable(new IntPoint(x, y)) ? MainData.buildable :
                        (IsPathable(new IntPoint(x, y)) ? MainData.pathable : MainData.unpathable));  
                }
            }
            AssignBuilding(myStartLoc.X - 2 - minMainX, myStartLoc.Y - 2 - minMainY, 5, 5, myMainData, mainSizeX, MainData.nexus);
            foreach (SC2APIProtocol.Unit unit in unitList)
            {
                if (
                    unit.UnitType == (int) UNIT_TYPEID.NEUTRAL_MINERALFIELD || unit.UnitType == (int) UNIT_TYPEID.NEUTRAL_MINERALFIELD750 ||
                    unit.UnitType == (int) UNIT_TYPEID.NEUTRAL_PURIFIERMINERALFIELD || unit.UnitType == (int) UNIT_TYPEID.NEUTRAL_PURIFIERMINERALFIELD750 ||
                    unit.UnitType == (int) UNIT_TYPEID.NEUTRAL_LABMINERALFIELD || unit.UnitType == (int) UNIT_TYPEID.NEUTRAL_LABMINERALFIELD750 ||
                    unit.UnitType == (int) UNIT_TYPEID.NEUTRAL_BATTLESTATIONMINERALFIELD || unit.UnitType == (int) UNIT_TYPEID.NEUTRAL_BATTLESTATIONMINERALFIELD750)
                {
                    int BLX = (int)unit.Pos.X - 1;
                    int BLY = (int)unit.Pos.Y;
                    if (mapRegions[BLY * mapSizeX + BLX] == MapRegion.myMain)
                    {
                        AssignBuilding(BLX - minMainX, BLY - minMainY, 2, 1, myMainData, mainSizeX, MainData.mineralField);
                    }
                }
                if (unit.UnitType == (int) UNIT_TYPEID.NEUTRAL_VESPENEGEYSER || unit.UnitType == (int) UNIT_TYPEID.NEUTRAL_PROTOSSVESPENEGEYSER ||
                    unit.UnitType == (int) UNIT_TYPEID.NEUTRAL_PURIFIERVESPENEGEYSER || unit.UnitType == (int) UNIT_TYPEID.NEUTRAL_SPACEPLATFORMGEYSER ||
                    unit.UnitType == (int) UNIT_TYPEID.NEUTRAL_SHAKURASVESPENEGEYSER)
                {
                    int BLX = (int)unit.Pos.X - 1;
                    int BLY = (int)unit.Pos.Y - 1;
                    if (mapRegions[BLY * mapSizeX + BLX] == MapRegion.myMain)
                    {
                        AssignBuilding(BLX - minMainX, BLY - minMainY, 3, 3, myMainData, mainSizeX, MainData.gasGeyser);
                    }
                }
            }
            for (int i = 0; i < 5000; i++)
            {
                List<BuildingCluster> currentClusteredPlan = new List<BuildingCluster>();
                int totalBuildingsPossible = 0;
                int totalPylonsPossible = 0;
                myMainData.CopyTo(currentMainPlan, 0);
                myMainData.CopyTo(tempMainPlan, 0);
                for (int j = 0; j < 500; j++)
                {
                    bool horizontal = random.Next(0, 2) == 1;
                    int centerPylonBLX = random.Next(2, mainSizeX - 1);
                    int centerPylonBLY = random.Next(2, mainSizeY - 1);
                    if (SpaceClear(centerPylonBLX, centerPylonBLY, 2, 2, currentMainPlan, mainSizeX))
                    {
                        int buildingsPossible = 0;
                        Point2D centerPylonCoords = new Point2D();
                        centerPylonCoords.X = centerPylonBLX + 1;
                        centerPylonCoords.Y = centerPylonBLY + 1;
                        BuildingCluster possibleCluster = new BuildingCluster(centerPylonCoords);
                        foreach (IntPoint offset in BuildingOffsets)
                        {
                            int buildingBLX = centerPylonBLX + (horizontal ? offset.X : offset.Y);
                            int buildingBLY = centerPylonBLY + (horizontal ? offset.Y : offset.X);
                            if (SpaceClear(buildingBLX, buildingBLY, 3, 3, currentMainPlan, mainSizeX))
                            {
                                buildingsPossible += 1;
                                Point2D buildingCoords = new Point2D();
                                buildingCoords.X = buildingBLX + (float)1.5;
                                buildingCoords.Y = buildingBLY + (float)1.5;
                                possibleCluster.addBuilding(buildingCoords);
                                AssignBuilding(buildingBLX, buildingBLY, 3, 3, tempMainPlan, mainSizeX, MainData.buildingPlanned);
                            }
                        }
                        if (buildingsPossible > 0)
                        {
                            totalBuildingsPossible += buildingsPossible;
                            totalPylonsPossible += 1;
                            AssignBuilding(centerPylonBLX, centerPylonBLY, 2, 2, tempMainPlan, mainSizeX, MainData.pylonPlanned);
                            if (SpaceClear(
                                horizontal ? centerPylonBLX : centerPylonBLX + 2,
                                horizontal ? centerPylonBLY + 2 : centerPylonBLY,
                                2, 2, currentMainPlan, mainSizeX))
                            {
                                AssignBuilding(
                                    horizontal ? centerPylonBLX : centerPylonBLX + 2,
                                    horizontal ? centerPylonBLY + 2 : centerPylonBLY,
                                    2, 2, tempMainPlan, mainSizeX, MainData.pylonPlanned);
                                totalPylonsPossible += 1;
                                Point2D pylonCoords = new Point2D();
                                pylonCoords.X = horizontal ? centerPylonBLX + 1 : centerPylonBLX + 3;
                                pylonCoords.Y = horizontal ? centerPylonBLY + 3 : centerPylonBLY + 1;
                                possibleCluster.addBuilding(pylonCoords);
                            }
                            if (SpaceClear(
                                horizontal ? centerPylonBLX : centerPylonBLX - 2,
                                horizontal ? centerPylonBLY - 2 : centerPylonBLY,
                                2, 2, currentMainPlan, mainSizeX))
                            {
                                AssignBuilding(horizontal ? centerPylonBLX : centerPylonBLX - 2,
                                horizontal ? centerPylonBLY - 2 : centerPylonBLY,
                                2, 2, tempMainPlan, mainSizeX, MainData.pylonPlanned);
                                totalPylonsPossible += 1;
                                Point2D pylonCoords = new Point2D();
                                pylonCoords.X = horizontal ? centerPylonBLX + 1 : centerPylonBLX - 1;
                                pylonCoords.Y = horizontal ? centerPylonBLY - 1 : centerPylonBLY + 1;
                                possibleCluster.addBuilding(pylonCoords);                                
                            }
                            tempMainPlan.CopyTo(currentMainPlan, 0);
                            currentClusteredPlan.Add(possibleCluster);
                        }
                    }
                }
                if (totalBuildingsPossible > bestMainBuildings)
                {
                    currentMainPlan.CopyTo(bestMainPlan, 0);
                    bestMainBuildings = totalBuildingsPossible;
                    basePlan = currentClusteredPlan;
                }
            }
        }

        private MapRegion[] constructMapRegions(List<IntPoint> myMainBoundary, List<IntPoint> oppMainBoundary)
        {
            MapRegion[] regions = new MapRegion[mapSizeX * mapSizeY];
            for (int x = 0; x < mapSizeX; x++)
            {
                for (int y = 0; y < mapSizeY; y++)
                {
                    bool boundedAbove = false;
                    bool boundedBelow = false;
                    bool boundedLeft = false;
                    bool boundedRight = false;
                    foreach (IntPoint boundarySquare in myMainBoundary)
                    {
                        if (boundarySquare.X == x && boundarySquare.Y >= y) { boundedAbove = true; }
                        if (boundarySquare.X == x && boundarySquare.Y <= y) { boundedBelow = true; }
                        if (boundarySquare.X <= x && boundarySquare.Y == y) { boundedLeft = true; }
                        if (boundarySquare.X >= x && boundarySquare.Y == y) { boundedRight = true; }
                    }
                    if (boundedAbove && boundedBelow && boundedLeft && boundedRight) { regions[x + y * mapSizeX] = MapRegion.myMain; }
                    boundedAbove = false;
                    boundedBelow = false;
                    boundedLeft = false;
                    boundedRight = false;
                    foreach (IntPoint boundarySquare in oppMainBoundary)
                    {
                        if (boundarySquare.X == x && boundarySquare.Y >= y) { boundedAbove = true; }
                        if (boundarySquare.X == x && boundarySquare.Y <= y) { boundedBelow = true; }
                        if (boundarySquare.X <= x && boundarySquare.Y == y) { boundedLeft = true; }
                        if (boundarySquare.X >= x && boundarySquare.Y == y) { boundedRight = true; }
                    }
                    if (boundedAbove && boundedBelow && boundedLeft && boundedRight) { regions[x + y * mapSizeX] = MapRegion.oppMain; }
                }
            }
            return regions;
        }

        private bool SpaceClear(int cornerBLX, int cornerBLY, int sizeX, int sizeY, MainData[] currentPlan, int MainSizeX)
        {
            for (int x = cornerBLX - 1; x <= cornerBLX + sizeX; x++)
            {
                for (int y = cornerBLY - 1; y <= cornerBLY + sizeY; y++)
                {
                    if (x > mainSizeX || x < 0 || y > mainSizeY || y < 0)
                    {
                        return false;
                    }
                    if (currentPlan[x+y*MainSizeX] != MainData.buildable && 
                        !(currentPlan[x+y*MainSizeX] == MainData.pathable && (x == cornerBLX - 1 || x == cornerBLX + sizeX || y == cornerBLY - 1 || y == cornerBLY + sizeY)))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        private void AssignBuilding(int cornerBLX, int cornerBLY, int sizeX, int sizeY, MainData[] plan, int MainSizeX, MainData buildingType)
        {
            for (int x = cornerBLX; x <= cornerBLX + sizeX - 1; x++)
            {
                for (int y = cornerBLY; y <= cornerBLY + sizeY - 1; y++)
                {
                    plan[x + y * MainSizeX] = buildingType;
                }
            }
        }

        private bool AttemptAssignBuilding(int cornerBLX, int cornerBLY, int sizeX, int sizeY, MainData[] plan, int mainSizeX, MainData buildingType)
        {
            if (SpaceClear(cornerBLX, cornerBLY, sizeX, sizeY, plan, mainSizeX))
            {
                AssignBuilding(cornerBLX, cornerBLY, sizeX, sizeY, plan, mainSizeX, buildingType);
                return true;
            }
            return false;
        }

        private (List<IntPoint>, List<IntPoint>) DetermineMain(IntPoint startLoc)
        {
            List<IntPoint> mainBoundary = new List<IntPoint>();
            List<IntPoint> rampBoundary = new List<IntPoint>();
            int y;
            int x = startLoc.X;
            int mainHeight = GetHeight(startLoc);
            bool mainNotComplete = true;
            for (y = startLoc.Y; GetHeight(new IntPoint(x, y)) == mainHeight; y++) { }
            Corner firstCorner = new Corner(x,y);
            Corner currentCorner = new Corner(x,y);
            Directions cameFrom = Directions.Right;
            IntPoint newBoundarySquare = firstCorner.SquareRightDown();
            IntPoint newOutsideSquare = firstCorner.SquareRightUp();
            mainBoundary.Add(newBoundarySquare);
            while (mainNotComplete)
            {
                IntPoint rightDown = currentCorner.SquareRightDown(); //Maybe don't instatiate new objects here
                int rightDownHeightDiff = mainHeight - GetHeight(rightDown);
                IntPoint leftDown = currentCorner.SquareLeftDown();
                int leftDownHeightDiff = mainHeight - GetHeight(leftDown);
                IntPoint rightUp = currentCorner.SquareRightUp();
                int rightUpHeightDiff = mainHeight - GetHeight(rightUp);
                IntPoint leftUp = currentCorner.SquareLeftUp();
                int leftUpHeightDiff = mainHeight - GetHeight(leftUp);
                IntPoint lastBoundarySquare = mainBoundary.Last();
                
                if (cameFrom != Directions.Down && (rightDownHeightDiff == 0 ^ leftDownHeightDiff == 0))
                {
                    cameFrom = Directions.Up;
                    currentCorner.Y -= 1;
                    newBoundarySquare = rightDownHeightDiff == 0 ? rightDown : leftDown;
                    newOutsideSquare = rightDownHeightDiff == 0 ? leftDown : rightDown;
                }
                else if (cameFrom != Directions.Left && (leftDownHeightDiff == 0 ^ leftUpHeightDiff == 0))
                {
                    cameFrom = Directions.Right;
                    currentCorner.X -= 1;
                    newBoundarySquare = leftDownHeightDiff == 0 ? leftDown : leftUp;
                    newOutsideSquare = leftDownHeightDiff == 0 ? leftUp : leftDown;
                }
                else if (cameFrom != Directions.Up && (leftUpHeightDiff == 0 ^ rightUpHeightDiff == 0))
                {
                    cameFrom = Directions.Down;
                    currentCorner.Y += 1;
                    newBoundarySquare = leftUpHeightDiff == 0 ? leftUp : rightUp;
                    newOutsideSquare = leftUpHeightDiff == 0 ? rightUp : leftUp;
                }
                else if (cameFrom != Directions.Right && (rightUpHeightDiff == 0 ^ rightDownHeightDiff == 0))
                {
                    cameFrom = Directions.Left;
                    currentCorner.X += 1;
                    newBoundarySquare = rightUpHeightDiff == 0 ? rightUp : rightDown;
                    newOutsideSquare = rightUpHeightDiff == 0 ? rightDown : rightUp;
                }
                if (currentCorner.X == firstCorner.X && currentCorner.Y == firstCorner.Y)
                {
                    mainNotComplete = false;
                }
                else if (lastBoundarySquare.X != newBoundarySquare.X || lastBoundarySquare.Y != newBoundarySquare.Y)
                {
                    mainBoundary.Add(newBoundarySquare);
                    int heightDiff = mainHeight - GetHeight(newOutsideSquare);
                    if (heightDiff > 0 && heightDiff <= 8)
                    {
                        rampBoundary.Add(newBoundarySquare);
                    }    
                }
            }
            return (mainBoundary, rampBoundary);
        }

        private int GetHeight(IntPoint point)
        {
            return rawMap.TerrainHeight.Data[mapSizeX * point.Y + point.X];
        }


        private bool IsBuildable(IntPoint point)
        {
            int pointNum = mapSizeX * point.Y + point.X;
            int byteNum = pointNum >> 3;
            int bytePos = pointNum & 7;
            return ((rawMap.PlacementGrid.Data[byteNum] >> (7 - bytePos)) & 1) > 0;
        }

        private bool IsPathable(IntPoint point)
        {
            int pointNum = mapSizeX * point.Y + point.X;
            int byteNum = pointNum >> 3;
            int bytePos = pointNum & 7;
            return ((rawMap.PathingGrid.Data[byteNum] >> (7 - bytePos)) & 1) > 0;
        }

        enum Directions
        {
            Left,
            Right,
            Up,
            Down
        }

        enum Heights
        {
            None,
            RampDown,
            RampUp,
            Cliff
        }

        public enum MainData
        {
            notMain,
            unpathable,
            pathable,
            buildable,
            pylonPlanned,
            buildingPlanned,
            nexus,
            mineralField,
            gasGeyser
        }

        public enum MapRegion
        {
            neutral,
            myMain,
            myNat,
            oppMain,
            oppNat,
        }

    }
}