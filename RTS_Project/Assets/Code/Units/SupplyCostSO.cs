using Code.Environments;
using UnityEngine;

namespace Code.Units
{
    [CreateAssetMenu(fileName = "Supply cost", menuName = "Units/Cost", order = 5)]

    public class SupplyCostSO : ScriptableObject
    {
        [field: SerializeField] public int Minerals { get; private set; } = 50;
        [field: SerializeField] public SupplySO MineralSO { get; private set; }
        [field: SerializeField] public int Gas { get; private set; } = 0;
        [field: SerializeField] public SupplySO GasSO { get; private set; }
    }
}