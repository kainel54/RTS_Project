using UnityEngine;

namespace Code.Units
{
    /// <summary>Homes an arrow toward a combat target and applies its damage on impact.</summary>
    public sealed class ArrowProjectile : MonoBehaviour
    {
        private MeleeCombat _target;
        private float _damage;
        private float _speed;
        private float _lifeRemaining = 8f;

        public void Launch(MeleeCombat target, float damage, float speed)
        {
            _target = target;
            _damage = damage;
            _speed = speed;
        }

        private void Update()
        {
            _lifeRemaining -= Time.deltaTime;
            if (_lifeRemaining <= 0f || _target == null || _target.IsDead)
            {
                Destroy(gameObject);
                return;
            }

            Collider targetCollider = _target.GetComponent<Collider>();
            Vector3 targetPosition = targetCollider != null
                ? targetCollider.bounds.center
                : _target.transform.position + Vector3.up;
            Vector3 toTarget = targetPosition - transform.position;
            float step = _speed * Time.deltaTime;

            if (toTarget.sqrMagnitude <= step * step)
            {
                _target.TakeDamage(_damage);
                Destroy(gameObject);
                return;
            }

            Vector3 direction = toTarget.normalized;
            transform.position += direction * step;
            transform.rotation = Quaternion.LookRotation(direction);
        }
    }
}
