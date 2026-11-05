using UnityEngine;

namespace Code.Units
{
    [CreateAssetMenu(fileName = "building data", menuName = "Buildings/Building data", order = 11)]
    public class BuildingSO : AbstractUnitSO
    {
        [field: SerializeField] public GameObject ConstructionDummy { get; private set; }
        [field: SerializeField, Min(0)] public int PopulationBonus { get; private set; }
    }
}
