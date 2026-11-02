using System.Collections.Generic;
using UnityEngine;

namespace Code.Units
{
    [CreateAssetMenu(fileName = "BuildingSOList", menuName = "Buildings/BuildingSOList", order = 1)]
    public class BuildingSOList : ScriptableObject
    {
        public List<BuildingSO> buildingSOList;
    }
}
