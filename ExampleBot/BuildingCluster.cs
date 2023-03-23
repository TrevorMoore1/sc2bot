using SC2APIProtocol;
using System.Collections.Generic;

namespace SC2Sharp
{
    public class BuildingCluster
    {
        public BuildingLocation primaryPylon;
        public List<BuildingLocation> buildingLocations;
        public List<BuildingLocation> secondaryPylonLocations;

        public BuildingCluster (Point2D primaryPylon)
        {
            this.primaryPylon = new BuildingLocation(primaryPylon);
            secondaryPylonLocations = new List<BuildingLocation>();
            buildingLocations = new List<BuildingLocation>();
        }

        public void addPylon (Point2D pylonCoords)
        {
            BuildingLocation pylonLoc = new BuildingLocation(pylonCoords);
            secondaryPylonLocations.Add(pylonLoc);
        }

        public void addBuilding(Point2D buildingCoords)
        {
            BuildingLocation buildingLoc = new BuildingLocation(buildingCoords);
            secondaryPylonLocations.Add(buildingLoc);
        }

        public void updateBuildingPowerStatus() //Need to account for only secondary pylons
        {
            PowerStatus powerStatus = PowerStatus.noPower;
            if (primaryPylon.status == StructureStatus.CompletedPowered) { powerStatus = PowerStatus.fullPower; }
            foreach (BuildingLocation building in buildingLocations)
            {
                if (powerStatus == PowerStatus.fullPower && building.status == StructureStatus.EmptyUnpowered) { building.status = StructureStatus.EmptyPowered; }
                if (powerStatus == PowerStatus.noPower && building.status == StructureStatus.EmptyPowered) { building.status = StructureStatus.EmptyUnpowered; }
            }
        }

        private enum PowerStatus
        {
            fullPower,
            noPower,
            topPower,
            bottomPower,
            leftPower,
            rightPower
        }
    }
}