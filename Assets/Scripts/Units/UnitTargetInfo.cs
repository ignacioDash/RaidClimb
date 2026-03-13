using System.Collections.Generic;
using Units.UnitTypes;

namespace Units
{
    public class UnitTargetInfo
    {
        public HashSet<BaseUnit> TargetedBy { get; private set; } = new();

        public void AddTargeter(BaseUnit unit)
        {
            TargetedBy.Add(unit);
        }

        public void RemoveTargeter(BaseUnit unit)
        {
            if (TargetedBy.Contains(unit))
                TargetedBy.Remove(unit);
        }
    }
}