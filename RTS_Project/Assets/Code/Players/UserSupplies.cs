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

        [SerializeField] private SupplySO mineralSO;
        [SerializeField] private SupplySO gasSO;

        [field: SerializeField] public int MineralAmount { get; private set; }
        [field: SerializeField] public int GasAmount { get; private set; }

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

            MineralAmount = GasAmount = 0;

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
            if (evt.SupplyType.Equals(mineralSO))
            {
                MineralAmount += evt.Amount;
                OnSupplyChanged?.Invoke(MineralAmount,mineralSO);
            }

            if (evt.SupplyType.Equals(gasSO))
            {
                GasAmount += evt.Amount;
                OnSupplyChanged?.Invoke(GasAmount, gasSO);
            }
        }

        public bool HasEnoughSupplies(SupplyCostSO cost)
        {
            return MineralAmount >= cost.Minerals && GasAmount >= cost.Gas;
        }

    }
}