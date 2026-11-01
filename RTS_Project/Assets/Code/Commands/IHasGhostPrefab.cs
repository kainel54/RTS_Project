using UnityEngine;

namespace Code.Commands
{
    public interface IConstructionCommand
    {
        GameObject GhostPrefab { get; }
        bool AllRestrictionPass(Vector3 point);
    }
}