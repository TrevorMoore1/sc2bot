using SC2APIProtocol;
using System.Collections.Generic;

namespace SC2Sharp
{
    internal class ActionList
    {
        public List<Action> actionList;

        public ActionList()
        {
            actionList = new List<Action>();
        }

        public void DoSpaceAction(ABILITY_ID actionType, ulong unitTag, Point2D targetSpace)
        {
            InternalDoSpaceAction(actionType, unitTag, targetSpace, false);
        }
        public void QueueSpaceAction(ABILITY_ID actionType, ulong unitTag, Point2D targetSpace)
        {
            InternalDoSpaceAction(actionType, unitTag, targetSpace, true);
        }

        public void DoTargetedAction(ABILITY_ID actionType, ulong unitTag, ulong targetTag)
        {
            InternalDoTargetedAction(actionType, unitTag, targetTag, false);
        }
        public void QueueTargetedAction(ABILITY_ID actionType, ulong unitTag, ulong targetTag)
        {
            InternalDoTargetedAction(actionType, unitTag, targetTag, true);
        }

        public void DoUntargetedAction(ABILITY_ID actionType, ulong unitTag)
        {
            InternalDoUntargetedAction(actionType, unitTag, false);
        }
        public void QueueUntargetedAction(ABILITY_ID actionType, ulong unitTag)
        {
            InternalDoUntargetedAction(actionType, unitTag, true);
        }

        private void InternalDoSpaceAction(ABILITY_ID actionType, ulong unitTag, Point2D targetSpace, bool queued)
        {
            Action action = new Action();
            action.ActionRaw = new ActionRaw();
            action.ActionRaw.UnitCommand = new ActionRawUnitCommand();
            action.ActionRaw.UnitCommand.AbilityId = (int)actionType;
            action.ActionRaw.UnitCommand.TargetWorldSpacePos = targetSpace;
            action.ActionRaw.UnitCommand.UnitTags.Add(unitTag);
            if (queued) { action.ActionRaw.UnitCommand.QueueCommand = true; }
            actionList.Add(action);
        }

        private void InternalDoTargetedAction(ABILITY_ID actionType, ulong unitTag, ulong targetTag, bool queued)
        {
            Action action = new Action();
            action.ActionRaw = new ActionRaw();
            action.ActionRaw.UnitCommand = new ActionRawUnitCommand();
            action.ActionRaw.UnitCommand.AbilityId = (int)actionType;
            action.ActionRaw.UnitCommand.TargetUnitTag = targetTag;
            action.ActionRaw.UnitCommand.UnitTags.Add(unitTag);
            if (queued) { action.ActionRaw.UnitCommand.QueueCommand = true; }
            actionList.Add(action);
        }

        private void InternalDoUntargetedAction(ABILITY_ID actionType, ulong unitTag, bool queued)
        {
            Action action = new Action();
            action.ActionRaw = new ActionRaw();
            action.ActionRaw.UnitCommand = new ActionRawUnitCommand();
            action.ActionRaw.UnitCommand.AbilityId = (int)actionType;
            action.ActionRaw.UnitCommand.UnitTags.Add(unitTag);
            if (queued) { action.ActionRaw.UnitCommand.QueueCommand = true; }
            actionList.Add(action);
        }
    }
}