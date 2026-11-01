using Code.Commands;
using Code.CoreSystem;
using Code.GameEvents;
using UnityEngine;

namespace Code.Units
{
    public abstract class AbstractCommandable : MonoBehaviour, ISelectable
    {
        [SerializeField] protected GameObject decalProjector;
        
        public bool IsSelected { get; protected set; }

        [field: SerializeField] public AbstractUnitSO AbstractUnitData { get; private set; }
        [field: SerializeField] public int CurrentHealth { get; protected set; }
        [field: SerializeField] public int MaxHealth { get; protected set; }
        [field: SerializeField] public BaseCommandSO[] AvailableCommands { get; private set; }

        private BaseCommandSO[] _initialCommands;
        
        protected virtual void Awake()
        {
            Debug.Assert(decalProjector != null && AbstractUnitData != null, $"필수 오브젝트가 할당되지 않았습니다 {gameObject}");
            decalProjector.SetActive(false);
            _initialCommands = AvailableCommands; //초기명령셋 저장
        }

        protected virtual void Start()
        {
            CurrentHealth = MaxHealth = AbstractUnitData.Health;
        }
        
        protected virtual void OnDestroy() { }
        
        public void Select()
        {
            decalProjector.SetActive(true);
            IsSelected = true;
            Bus<UnitSelectEvent>.Raise(new UnitSelectEvent(this));
        }

        public virtual void DeSelect()
        {
            decalProjector.SetActive(false);
            IsSelected = false;
            SetCommandOverrides(null);
            Bus<UnitDeSelectEvent>.Raise(new UnitDeSelectEvent(this));
        }

        public void SetCommandOverrides(BaseCommandSO[] commands)
        {
            if(commands == null || commands.Length == 0)
            {
                AvailableCommands = _initialCommands;
            }
            else
            {
                AvailableCommands = commands;
            }

            if (IsSelected)
            {
                Bus<UnitSelectEvent>.Raise(new UnitSelectEvent(this));
            }
        }
    }
}