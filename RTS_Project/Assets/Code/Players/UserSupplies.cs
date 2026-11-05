using Code.CoreSystem;
using Code.Environments;
using Code.GameEvents;
using Code.Units;
using NUnit.Framework;
using System;
using UnityEngine;

namespace Code.Players
{
    public class UserSupplies : MonoBehaviour
    {
        public static UserSupplies Instance { get; private set; }

        [SerializeField] private SupplySO woodSO;
        [SerializeField] private SupplySO stoneSO;
        [SerializeField] private SupplySO ironOreSO;
        [SerializeField] private SupplySO ironIngotSO;

        [field: SerializeField] public int WoodAmount { get; private set; }
        [field: SerializeField] public int StoneAmount { get; private set; }
        [field: SerializeField] public int IronOreAmount { get; private set; }
        [field: SerializeField] public int IronIngotAmount { get; private set; }

        public delegate void SupplyChanged(int amount, SupplySO supplyType);
        public event SupplyChanged OnSupplyChanged; 

        private void Awake()
        {
            if(Instance != null&& Instance != this)
            {
                Destroy(this.gameObject);
                return;
            }
            Instance = this;

            WoodAmount = StoneAmount = IronOreAmount = IronIngotAmount = 0;

            Bus<SupplyEvent>.OnEvent += HandleSupplyEvent;
        }

        private void Start()
        {
            Bus<UserSupplyReadyEvent>.Raise(new UserSupplyReadyEvent(this));
        }

        private void OnDestroy()
        {
            Bus<SupplyEvent>.OnEvent -= HandleSupplyEvent;
        }

        private void HandleSupplyEvent(SupplyEvent evt)
        {
            if (evt.SupplyType.Equals(woodSO))
            {
                WoodAmount += evt.Amount;
                OnSupplyChanged?.Invoke(WoodAmount, woodSO);
            }

            if (evt.SupplyType.Equals(stoneSO))
            {
                StoneAmount += evt.Amount;
                OnSupplyChanged?.Invoke(StoneAmount, stoneSO);
            }

            if (evt.SupplyType.Equals(ironOreSO))
            {
                IronOreAmount += evt.Amount;
                OnSupplyChanged?.Invoke(IronOreAmount, ironOreSO);
            }

            if (evt.SupplyType.Equals(ironIngotSO))
            {
                IronIngotAmount += evt.Amount;
                OnSupplyChanged?.Invoke(IronIngotAmount, ironIngotSO);
            }
        }

        public bool HasEnoughSupplies(SupplyCostSO cost)
        {
            return cost != null
                   && WoodAmount >= cost.Wood
                   && StoneAmount >= cost.Stone
                   && IronOreAmount >= cost.IronOre
                   && IronIngotAmount >= cost.IronIngot;
        }

        public bool TrySpendSupplies(SupplyCostSO cost)
        {
            if (!HasEnoughSupplies(cost)) return false;

            WoodAmount -= cost.Wood;
            StoneAmount -= cost.Stone;
            IronOreAmount -= cost.IronOre;
            IronIngotAmount -= cost.IronIngot;

            if (cost.Wood > 0) OnSupplyChanged?.Invoke(WoodAmount, woodSO);
            if (cost.Stone > 0) OnSupplyChanged?.Invoke(StoneAmount, stoneSO);
            if (cost.IronOre > 0) OnSupplyChanged?.Invoke(IronOreAmount, ironOreSO);
            if (cost.IronIngot > 0) OnSupplyChanged?.Invoke(IronIngotAmount, ironIngotSO);

            return true;
        }

    }
}
