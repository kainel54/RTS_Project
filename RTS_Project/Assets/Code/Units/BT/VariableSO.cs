using UnityEngine;

namespace Code.Units.BT
{
    [CreateAssetMenu(fileName = "Variable data", menuName = "BT/Variable", order = 10)]
    public class VariableSO : ScriptableObject
    {
        [field: SerializeField] public BTVariables VariableName { get; private set; }
    }
}