using UnityEngine;

namespace Code.Units
{
    /// <summary>Automatically attacks player units first, then player buildings.</summary>
    [RequireComponent(typeof(CombatUnit), typeof(MeleeCombat))]
    public sealed class EnemyUnitAI : MonoBehaviour
    {
        [SerializeField, Min(0.1f)] private float targetRefreshInterval = 0.5f;
        [SerializeField, Min(0.1f)] private float targetDetectionRange = 12f;

        private CombatUnit _unit;
        private MeleeCombat _combat;
        private Transform _keepTarget;
        private BaseBuilding _targetBuilding;
        private float _nextTargetRefresh;
        private float _nextBuildingAttack;
        private float _nextBuildingMoveAttempt;

        private void Awake()
        {
            _unit = GetComponent<CombatUnit>();
            _combat = GetComponent<MeleeCombat>();
            GameObject keep = GameObject.Find("Keep");
            _keepTarget = keep != null ? keep.transform : null;
        }

        public void SetKeepTarget(Transform keepTarget)
        {
            _keepTarget = keepTarget;
        }

        private void Update()
        {
            if (Time.time >= _nextTargetRefresh)
            {
                _nextTargetRefresh = Time.time + targetRefreshInterval;
                RefreshTarget();
            }

            if (_targetBuilding != null)
                UpdateBuildingAttack();
        }

        private void RefreshTarget()
        {
            MeleeCombat closestUnit = null;
            float closestUnitDistance = float.PositiveInfinity;
            foreach (MeleeCombat candidate in FindObjectsOfType<MeleeCombat>())
            {
                if (candidate == null || candidate == _combat || candidate.Faction != MeleeCombat.Team.Player
                    || candidate.IsDead)
                    continue;

                float distance = (candidate.transform.position - transform.position).sqrMagnitude;
                if (distance > targetDetectionRange * targetDetectionRange) continue;
                if (distance >= closestUnitDistance) continue;

                closestUnitDistance = distance;
                closestUnit = candidate;
            }

            if (closestUnit != null)
            {
                _targetBuilding = null;
                if (_combat.CurrentTarget != closestUnit)
                    _combat.Attack(closestUnit);
                return;
            }

            _combat.CancelAttack();

            BaseBuilding closestBuilding = null;
            float closestBuildingDistance = float.PositiveInfinity;
            foreach (BaseBuilding candidate in FindObjectsOfType<BaseBuilding>())
            {
                if (candidate == null) continue;

                float distance = (candidate.transform.position - transform.position).sqrMagnitude;
                if (distance > targetDetectionRange * targetDetectionRange) continue;
                if (distance >= closestBuildingDistance) continue;

                closestBuildingDistance = distance;
                closestBuilding = candidate;
            }

            if (_targetBuilding == closestBuilding)
            {
                if (_targetBuilding == null)
                    MoveTowardKeep();
                return;
            }

            _targetBuilding = closestBuilding;
            _nextBuildingAttack = Time.time;
            if (_targetBuilding != null)
                MoveTowardBuilding();
            else
                MoveTowardKeep();
        }

        private void MoveTowardKeep()
        {
            if (_keepTarget == null || _unit.Agent == null || !_unit.Agent.isOnNavMesh)
            {
                _unit.Stop();
                return;
            }

            Vector3 outward = transform.position - _keepTarget.position;
            outward.y = 0f;
            if (outward.sqrMagnitude < 0.001f)
                outward = Vector3.forward;
            outward.Normalize();

            Collider keepCollider = _keepTarget.GetComponent<Collider>();
            float keepRadius = keepCollider != null
                ? Mathf.Max(keepCollider.bounds.extents.x, keepCollider.bounds.extents.z)
                : 3f;
            Vector3 destination = _keepTarget.position + outward * (keepRadius + 2.5f);
            float distanceToDestination = Vector3.Distance(transform.position, destination);
            if (distanceToDestination <= 1.5f) return;

            if (!_unit.Agent.pathPending
                && (!_unit.Agent.hasPath || Vector3.Distance(_unit.Agent.destination, destination) > 1f))
                _unit.MoveTo(destination);
        }

        private void UpdateBuildingAttack()
        {
            if (_targetBuilding == null) return;

            Vector3 offset = _targetBuilding.transform.position - transform.position;
            offset.y = 0f;
            float buildingRadius = 1f;
            Collider buildingCollider = _targetBuilding.GetComponent<Collider>();
            if (buildingCollider != null)
                buildingRadius = Mathf.Max(buildingCollider.bounds.extents.x, buildingCollider.bounds.extents.z);

            float distanceToBuilding = Mathf.Max(0f, offset.magnitude - buildingRadius);
            if (distanceToBuilding > _combat.AttackRange)
            {
                if (_unit.Agent != null && _unit.Agent.isOnNavMesh
                    && !_unit.Agent.pathPending
                    && (!_unit.Agent.hasPath || _unit.Agent.remainingDistance <= _unit.Agent.stoppingDistance)
                    && Time.time >= _nextBuildingMoveAttempt)
                    MoveTowardBuilding();
                return;
            }

            if (_unit.Agent != null && _unit.Agent.isOnNavMesh)
            {
                _unit.Agent.isStopped = true;
                _unit.Agent.ResetPath();
            }

            if (Time.time < _nextBuildingAttack) return;

            _targetBuilding.TakeDamage(_combat.Damage);
            _nextBuildingAttack = Time.time + _combat.AttackInterval;
        }

        private void MoveTowardBuilding()
        {
            Vector3 offset = _targetBuilding.transform.position - transform.position;
            offset.y = 0f;
            Vector3 destination = _targetBuilding.transform.position;
            if (offset.sqrMagnitude > 0.001f)
                destination -= offset.normalized * (GetBuildingRadius() + Mathf.Max(0.5f, _combat.AttackRange * 0.8f));

            _nextBuildingMoveAttempt = Time.time + targetRefreshInterval;
            _unit.MoveTo(destination);
        }

        private float GetBuildingRadius()
        {
            Collider buildingCollider = _targetBuilding.GetComponent<Collider>();
            return buildingCollider != null
                ? Mathf.Max(buildingCollider.bounds.extents.x, buildingCollider.bounds.extents.z)
                : 1f;
        }
    }
}
