using Code.Environments;
using UnityEngine;

namespace Code.Units
{
    [CreateAssetMenu(fileName = "Supply cost", menuName = "Units/Cost", order = 5)]

    public class SupplyCostSO : ScriptableObject
    {
        [field: SerializeField] public int Wood { get; private set; } = 50;
        [field: SerializeField] public SupplySO WoodSO { get; private set; }
        [field: SerializeField] public int Stone { get; private set; } = 0;
        [field: SerializeField] public SupplySO StoneSO { get; private set; }
        [field: SerializeField] public int IronOre { get; private set; } = 0;
        [field: SerializeField] public SupplySO IronOreSO { get; private set; }
        [field: SerializeField] public int IronIngot { get; private set; } = 0;
        [field: SerializeField] public SupplySO IronIngotSO { get; private set; }
        [field: SerializeField] public int Food { get; private set; } = 0;
        [field: SerializeField] public SupplySO FoodSO { get; private set; }
    }
}
