using Code.Units.Animations;
using UnityEngine;

namespace Code.Units
{
    /// <summary>A building that uses its archer visual to attack nearby enemy units.</summary>
    public sealed class DefenseTower : BaseBuilding
    {
        [SerializeField, Min(0.1f)] private float attackRange = 16f;
        [SerializeField, Min(0f)] private float damage = 15f;
        [SerializeField, Min(0.1f)] private float attackInterval = 1.4f;
        [SerializeField, Min(0.1f)] private float projectileSpeed = 16f;
        [SerializeField, Min(1f)] private float turnSpeed = 540f;
        [SerializeField] private Transform archer;
        [SerializeField] private Animator archerAnimator;
        [SerializeField] private ParamSO attackAnimation;
        [SerializeField] private GameObject projectilePrefab;

        private Quaternion _initialArcherRotation;
        private float _nextAttackTime;

        protected override void Awake()
        {
            base.Awake();

            if (archer == null)
            {
                Transform archerTransform = transform.Find("Visual/Archer");
                archer = archerTransform != null ? archerTransform : transform.Find("Archer");
            }

            if (archerAnimator == null && archer != null)
                archerAnimator = archer.GetComponent<Animator>();

            if (archer != null)
                _initialArcherRotation = archer.rotation;
        }

        private void Update()
        {
            MeleeCombat target = FindClosestEnemy();
            if (target == null || archer == null)
                return;

            Vector3 targetPosition = GetTargetPosition(target);
            Vector3 direction = targetPosition - archer.position;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.001f)
                return;

            direction.Normalize();
            Vector3 initialForward = Vector3.ProjectOnPlane(_initialArcherRotation * Vector3.forward, Vector3.up);
            if (initialForward.sqrMagnitude < 0.001f)
                initialForward = transform.forward;
            initialForward.Normalize();

            Quaternion desiredRotation = Quaternion.FromToRotation(initialForward, direction) * _initialArcherRotation;
            archer.rotation = Quaternion.RotateTowards(archer.rotation, desiredRotation, turnSpeed * Time.deltaTime);

            if (Time.time < _nextAttackTime || Quaternion.Angle(archer.rotation, desiredRotation) > 3f)
                return;

            _nextAttackTime = Time.time + attackInterval;
            if (archerAnimator != null && attackAnimation != null)
                archerAnimator.SetTrigger(attackAnimation.HashValue);

            FireArrow(target, targetPosition);
        }

        private MeleeCombat FindClosestEnemy()
        {
            MeleeCombat closestEnemy = null;
            float closestDistance = attackRange * attackRange;

            foreach (MeleeCombat candidate in FindObjectsOfType<MeleeCombat>())
            {
                if (candidate == null || candidate.IsDead || candidate.Faction != MeleeCombat.Team.Enemy)
                    continue;

                // Team values alone can be misconfigured on a placed unit; only the explicit enemy AI is hostile.
                if (!candidate.TryGetComponent<EnemyUnitAI>(out _))
                    continue;

                float distance = (candidate.transform.position - transform.position).sqrMagnitude;
                if (distance > closestDistance) continue;

                closestDistance = distance;
                closestEnemy = candidate;
            }

            return closestEnemy;
        }

        private static Vector3 GetTargetPosition(MeleeCombat target)
        {
            Collider targetCollider = target.GetComponent<Collider>();
            return targetCollider != null ? targetCollider.bounds.center : target.transform.position + Vector3.up;
        }

        private void FireArrow(MeleeCombat target, Vector3 targetPosition)
        {
            if (projectilePrefab == null)
            {
                target.TakeDamage(damage);
                return;
            }

            Vector3 spawnPosition = archer.position + Vector3.up * 1.1f;
            Vector3 direction = targetPosition - spawnPosition;
            Quaternion rotation = direction.sqrMagnitude > 0.001f
                ? Quaternion.LookRotation(direction.normalized)
                : archer.rotation;
            GameObject arrowObject = Instantiate(projectilePrefab, spawnPosition, rotation);

            if (arrowObject.TryGetComponent(out ArrowProjectile arrow))
                arrow.Launch(target, damage, projectileSpeed);
            else
            {
                Destroy(arrowObject);
                target.TakeDamage(damage);
            }
        }
    }
}
