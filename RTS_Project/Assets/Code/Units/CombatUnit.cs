using Code.CoreSystem;
using Code.GameEvents;
using Code.Units.Animations;
using UnityEngine;

namespace Code.Units
{
    /// <summary>A directly controlled unit used for soldier prefabs without a Behavior Graph.</summary>
    public sealed class CombatUnit : AbstractUnit
    {
        [SerializeField] private ParamSO speedAnimationParameter;

        protected override void Start()
        {
            Bus<UnitSpawnEvent>.Raise(new UnitSpawnEvent(this));
        }

        private void Update()
        {
            if (Agent != null && UnitAnimator != null && speedAnimationParameter != null)
                UnitAnimator.SetParam(speedAnimationParameter, Agent.velocity.magnitude);
        }

        public override void MoveTo(Vector3 position)
        {
            GetComponent<MeleeCombat>()?.CancelAttack();
            if (Agent == null || !Agent.enabled) return;
            Agent.isStopped = false;
            Agent.SetDestination(position);
        }

        public override void Stop()
        {
            GetComponent<MeleeCombat>()?.CancelAttack();
            if (Agent == null || !Agent.enabled || !Agent.isOnNavMesh) return;
            Agent.isStopped = true;
            Agent.ResetPath();
        }
    }
}
