using System.Collections.Generic;

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
            TargetedBy.Remove(unit);
        }
    }
}