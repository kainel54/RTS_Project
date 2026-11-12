using UnityEngine;

namespace Code.Units
{
    /// <summary>Homes an arrow toward a combat target and applies its damage on impact.</summary>
    public sealed class ArrowProjectile : MonoBehaviour
    {
        [SerializeField] private Vector3 modelForwardEulerOffset = new Vector3(90f, 0f, 0f);
        private MeleeCombat _target;
        private float _damage;
        private float _speed;
        private float _lifeRemaining = 8f;

        public void Launch(MeleeCombat target, float damage, float speed)
        {
            _target = target;
            _damage = damage;
            _speed = speed;

            if (_target != null)
                FaceTarget(GetTargetPosition() - transform.position);
        }

        private void Update()
        {
            _lifeRemaining -= Time.deltaTime;
            if (_lifeRemaining <= 0f || _target == null || _target.IsDead)
            {
                Destroy(gameObject);
                return;
            }

            Vector3 toTarget = GetTargetPosition() - transform.position;
            float step = _speed * Time.deltaTime;

            if (toTarget.sqrMagnitude <= step * step)
            {
                _target.TakeDamage(_damage);
                Destroy(gameObject);
                return;
            }

            Vector3 direction = toTarget.normalized;
            FaceTarget(direction);
            transform.position += direction * step;
        }

        private Vector3 GetTargetPosition()
        {
            Collider targetCollider = _target.GetComponent<Collider>();
            return targetCollider != null
                ? targetCollider.bounds.center
                : _target.transform.position + Vector3.up;
        }

        private void FaceTarget(Vector3 direction)
        {
            if (direction.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(direction.normalized, Vector3.up)
                    * Quaternion.Euler(modelForwardEulerOffset);
        }
    }
}
