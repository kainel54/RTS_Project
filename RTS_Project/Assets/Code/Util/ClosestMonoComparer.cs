using System.Collections.Generic;
using UnityEngine;

namespace Code.Util
{
    public struct ClosestMonoComparer<T> : IComparer<T> where T : MonoBehaviour
    {
        private Vector3 _targetPosition;

        public ClosestMonoComparer(Vector3 targetPosition)
        {
            _targetPosition = targetPosition;
        }
        
        public int Compare(T x, T y)
        {
            return (x.transform.position - _targetPosition).sqrMagnitude
                .CompareTo((y.transform.position - _targetPosition).sqrMagnitude);
        }
    }
}