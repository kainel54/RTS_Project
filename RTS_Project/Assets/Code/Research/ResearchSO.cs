using Code.Units;
using UnityEngine;

namespace Code.Research
{
    [CreateAssetMenu(fileName = "Research", menuName = "Research/Research")]
    public class ResearchSO : ScriptableObject
    {
        [field: SerializeField] public string ResearchName { get; private set; }
        [field: SerializeField, Min(1)] public float ResearchTime { get; private set; } = 15f;
        [field: SerializeField] public SupplyCostSO Cost { get; private set; }
        [field: SerializeField] public BuildingSO BuildingToUnlock { get; private set; }
    }
}
