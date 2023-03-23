using SC2APIProtocol;

namespace SC2Sharp
{
    public class BuildingLocation
    {
        public readonly Point2D location;
        public StructureStatus status;

        public BuildingLocation(Point2D location)
        {
            this.location = location;
            status = StructureStatus.EmptyUnpowered;
        }

    }
}