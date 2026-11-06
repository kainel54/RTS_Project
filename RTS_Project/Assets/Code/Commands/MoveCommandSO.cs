using Code.Units;
using UnityEngine;

namespace Code.Commands
{
    [CreateAssetMenu(fileName = "Move command", menuName = "Units/Commands/Move", order = 100)]
    public class MoveCommandSO : BaseCommandSO
    {
        [SerializeField] private float radiusMultiplier = 2.5f;
        private int _unitsOnOrbit = 0; //궤도에 있는 유닛 수
        private int _maxUnitsOnOrbit = 1; //링에 있을 유닛 수
        private float _circleRadius = 0; //궤도의 반지름
        private float _radialOffset = 0; //링 외곽에서 떨어진 정도
        
        public override bool CanHandle(CommandContext context)
        {
            return context.Commandable is AbstractUnit;
        }

        public override void Handle(CommandContext context)
        {
            AbstractUnit unit = context.Commandable as AbstractUnit;
            if (unit is Worker worker)
                worker.UnassignFromBuilding();

            if (context.UnitIndex == 0)
            {
                _unitsOnOrbit = 0;
                _maxUnitsOnOrbit = 1;
                _circleRadius = 0;
                _radialOffset = 0;
            }
            
            Vector3 targetPosition = new Vector3(
                context.Hit.point.x + _circleRadius * Mathf.Cos(_radialOffset * _unitsOnOrbit),
                context.Hit.point.y,
                context.Hit.point.z + _circleRadius * Mathf.Sin(_radialOffset * _unitsOnOrbit)
            );
            
            unit.MoveTo(targetPosition);
            _unitsOnOrbit++;
            
            if (_unitsOnOrbit >= _maxUnitsOnOrbit)
            {
                _unitsOnOrbit = 0; //다음 궤도로 넘어가기 위한 초기화
                _circleRadius += unit.AgentRadius * radiusMultiplier;
                _maxUnitsOnOrbit = Mathf.FloorToInt(2 * Mathf.PI * _circleRadius / (unit.AgentRadius * 2));
                _radialOffset = 2 * Mathf.PI / _maxUnitsOnOrbit; //360을 갯수로 나눠서 각을 구한다.
            }
        }
    }
}
