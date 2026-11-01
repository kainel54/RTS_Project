using UnityEngine;

namespace Code.Units
{
    public interface IBuildingConstructor
    {
        bool IsBuilding { get; }
        GameObject ConstructBuilding(BuildingSO building, Vector3 targetLocation);
        void CancelConstruction();
        void ResumeConstruction(ConstructionDummy dummy);
    }
}