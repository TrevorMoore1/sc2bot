using SC2APIProtocol;

namespace SC2Sharp
{

    internal class BuildOrderStatus
    {
        public uint BuildOrderProgress { get; private set; }
        public BuildOrderEvents NextBuildingType { get; private set; }
        public BuildingLocation NextBuildingPos;
        public bool chronoNextAction;

        public BuildOrderStatus()
        {
            BuildOrderProgress = 0;
            chronoNextAction = false;
            NextBuildingType = DetermineNextBuildingType();
        }

        public void AdvanceBuildOrder(bool resetBuildPos)
        {
            if (BuildOrderProgress < BuildOrder.buildOrder.Length)
            {
                BuildOrderProgress += 1;
                DetermineNextBuildingType();
                if (resetBuildPos)
                {
                    NextBuildingPos = null;
                }
            }
        }

        private BuildOrderEvents DetermineNextBuildingType()
        {
            for (uint i = BuildOrderProgress; i < BuildOrder.buildOrder.Length; i++)
            {
                BuildOrderEvents buildOrderEvent = BuildOrder.buildOrder[i];
                if ((int)buildOrderEvent >= 100 && (int)buildOrderEvent < 200)
                {
                    return buildOrderEvent;
                }
            }
            return BuildOrderEvents.BuildOrderEnd;
        }
    }
}