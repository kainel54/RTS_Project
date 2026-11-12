using UnityEngine;
using Code.Environments;

namespace Code.Units
{
    [CreateAssetMenu(fileName = "building data", menuName = "Buildings/Building data", order = 11)]
    public class BuildingSO : AbstractUnitSO
    {
        [field: SerializeField] public GameObject ConstructionDummy { get; private set; }
        [field: SerializeField, Min(0)] public int PopulationBonus { get; private set; }
        [field: SerializeField, Min(0)] public int WorkerCapacity { get; private set; }
        [field: SerializeField, Min(0)] public int StorageCapacity { get; private set; }
        [field: SerializeField] public SupplySO WorkerProductionSupply { get; private set; }
        [field: SerializeField, Min(0.1f)] public float WorkerProductionInterval { get; private set; } = 10f;
        [field: SerializeField, Min(1)] public int WorkerProductionAmount { get; private set; } = 8;
        [field: SerializeField] public bool ConsumeAssignedWorkerPerUnit { get; private set; }
    }
}
