using UnityEngine;

namespace Code.Units
{
    [CreateAssetMenu(fileName = "Unit", menuName = "Units/Unit", order = 10)]
    public class UnitSO : AbstractUnitSO
    {
        [field: SerializeField, Min(1)] public int PopulationCost { get; private set; } = 1;
    }
}
