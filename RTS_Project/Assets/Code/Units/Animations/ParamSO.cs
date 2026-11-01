using UnityEngine;

namespace Code.Units.Animations
{
    [CreateAssetMenu(fileName = "Animator paramiter", menuName = "Units/Parameter", order = 20)]
    public class ParamSO : ScriptableObject
    {
        [field: SerializeField] public string ParameterName { get; private set; }
        [field: SerializeField] public int HashValue { get; private set; }

        private void OnValidate()
        {
            HashValue = Animator.StringToHash(ParameterName);
        }
    }

}

