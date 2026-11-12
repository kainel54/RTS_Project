using Code.Units.Animations;
using Code.Units.BT;
using UnityEngine;
using UnityEngine.AI;

namespace Code.Units
{
    public sealed class MeleeCombat : MonoBehaviour
    {
        public enum Team
        {
            Player,
            Enemy
        }

        [SerializeField] private Team team = Team.Player;
        [SerializeField, Min(1f)] private float maxHealth = 100f;
        [SerializeField, Min(0f)] private float damage = 10f;
        [SerializeField, Min(0.1f)] private float attackRange = 1.5f;
        [SerializeField, Min(0.1f)] private float attackInterval = 1f;
        [SerializeField] private ParamSO attackAnimation;
        [SerializeField] private ParamSO speedAnimationParameter;

        private AbstractUnit _unit;
        private MeleeCombat _target;
        private float _nextAttackTime;
        private Vector3 _lastDestination;
        private bool _isAttacking;

        public Team Faction => team;
        public float CurrentHealth { get; private set; }
        public float MaxHealth => maxHealth;
        public bool IsDead { get; private set; }

        private void Awake()
        {
            _unit = GetComponent<AbstractUnit>();
            CurrentHealth = maxHealth;
        }

        private void Update()
        {
            if (_target == null || _target.IsDead || _target.Faction == team)
            {
                ClearTarget();
                return;
            }

            NavMeshAgent agent = _unit != null ? _unit.Agent : null;
            if (agent == null || !agent.isOnNavMesh)
                return;

            Vector3 direction = _target.transform.position - transform.position;
            direction.y = 0f;
            float targetRadius = _target.GetComponent<Collider>() is Collider collider
                ? Mathf.Max(collider.bounds.extents.x, collider.bounds.extents.z)
                : 0f;
            float distanceToTarget = Mathf.Max(0f, direction.magnitude - targetRadius);

            if (distanceToTarget > attackRange)
            {
                agent.isStopped = false;
                Vector3 destination = _target.transform.position - direction.normalized * (attackRange * 0.7f);
                destination.y = _target.transform.position.y;
                if (Vector3.Distance(destination, _lastDestination) > 0.3f)
                {
                    agent.SetDestination(destination);
                    _lastDestination = destination;
                }

                if (_unit.UnitAnimator != null && speedAnimationParameter != null)
                    _unit.UnitAnimator.SetParam(speedAnimationParameter, agent.velocity.magnitude);
                return;
            }

            agent.isStopped = true;
            agent.ResetPath();
            FaceTarget(direction);

            if (Time.time < _nextAttackTime)
                return;

            _nextAttackTime = Time.time + attackInterval;
            if (attackAnimation != null)
                _unit.UnitAnimator?.SetParam(attackAnimation);
            _target.TakeDamage(damage);
        }

        public bool CanAttack(MeleeCombat target)
        {
            return target != null && target != this && !IsDead && !target.IsDead && target.Faction != team;
        }

        public bool Attack(MeleeCombat target)
        {
            if (!CanAttack(target))
                return false;

            _target = target;
            _isAttacking = true;
            _nextAttackTime = Time.time;
            _lastDestination = new Vector3(float.PositiveInfinity, 0f, 0f);

            if (_unit != null && _unit.GraphAgent != null)
                _unit.SetBTVariable(BTVariables.Command, UnitCommands.Attack);

            return true;
        }

        public void CancelAttack()
        {
            ClearTarget();
        }

        public void TakeDamage(float amount)
        {
            if (IsDead || amount <= 0f)
                return;

            CurrentHealth = Mathf.Max(0f, CurrentHealth - amount);
            if (CurrentHealth > 0f)
                return;

            IsDead = true;
            if (_unit != null && _unit.Agent != null)
                _unit.Agent.enabled = false;

            foreach (Collider ownCollider in GetComponentsInChildren<Collider>())
                ownCollider.enabled = false;

            Destroy(gameObject);
        }

        private void FaceTarget(Vector3 direction)
        {
            if (direction.sqrMagnitude > 0.001f)
                transform.rotation = Quaternion.LookRotation(direction);
        }

        private void ClearTarget()
        {
            if (!_isAttacking)
                return;

            _target = null;
            _isAttacking = false;
            if (_unit == null || _unit.Agent == null || !_unit.Agent.enabled || !_unit.Agent.isOnNavMesh)
            {
                if (_unit?.GraphAgent != null) _unit.SetBTVariable(BTVariables.Command, UnitCommands.Stop);
                return;
            }

            _unit.Agent.isStopped = false;
            _unit.Agent.ResetPath();
            if (_unit.GraphAgent != null) _unit.SetBTVariable(BTVariables.Command, UnitCommands.Stop);
        }
    }
}
