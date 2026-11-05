using Code.Units;
using UnityEngine;

namespace Code.Environments
{
    [CreateAssetMenu(fileName = "Supply", menuName = "Units/Supply", order = 5)]
    public class SupplySO : ScriptableObject
    {
        [field: SerializeField] public Sprite Icon { get; private set; }
        [field: SerializeField] public int MaxAmount { get; private set; } = 1500;
        [field: SerializeField] public int AmountPerGather { get; private set; } = 8;
        [field: SerializeField] public float BaseGatherTime { get; private set; } = 1.5f;
        [field: SerializeField] public float BuildTime { get; private set; } = 10f;
        }
}
