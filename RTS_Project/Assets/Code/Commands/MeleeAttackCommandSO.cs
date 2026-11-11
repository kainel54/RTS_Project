using Code.Units;
using UnityEngine;

namespace Code.Commands
{
    [CreateAssetMenu(fileName = "Melee Attack command", menuName = "Units/Commands/Melee Attack", order = 106)]
    public sealed class MeleeAttackCommandSO : BaseCommandSO
    {
        public override bool CanHandle(CommandContext context)
        {
            if (context.Commandable is not AbstractUnit attacker || context.Hit.collider == null)
                return false;

            MeleeCombat attackerCombat = attacker.GetComponent<MeleeCombat>();
            MeleeCombat targetCombat = context.Hit.collider.GetComponentInParent<MeleeCombat>();
            return attackerCombat != null && attackerCombat.CanAttack(targetCombat);
        }

        public override void Handle(CommandContext context)
        {
            if (context.Commandable is not AbstractUnit attacker || context.Hit.collider == null)
                return;

            MeleeCombat attackerCombat = attacker.GetComponent<MeleeCombat>();
            MeleeCombat targetCombat = context.Hit.collider.GetComponentInParent<MeleeCombat>();
            if (attackerCombat != null)
                attackerCombat.Attack(targetCombat);
        }
    }
}
