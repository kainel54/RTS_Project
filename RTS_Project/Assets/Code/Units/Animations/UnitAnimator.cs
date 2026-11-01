using UnityEngine;

namespace Code.Units.Animations
{
    [RequireComponent(typeof(Animator))]
    public class UnitAnimator : MonoBehaviour
    {
        private Animator _animator;

        private void Awake()
        {
            _animator = GetComponent<Animator>();
        }

        public void SetParam(ParamSO param,bool value) => _animator.SetBool(param.HashValue, value);
        public void SetParam(ParamSO param,float value) => _animator.SetFloat(param.HashValue, value);
        public void SetParam(ParamSO param,int value) => _animator.SetInteger(param.HashValue, value);
        public void SetParam(ParamSO param) => _animator.SetTrigger(param.HashValue);
    }
}

