using System;
using UnityEngine;

namespace Code.Environments
{
    public class GatherableSupply : MonoBehaviour, IGatherable
    {
        [field: SerializeField] public SupplySO SupplyData { get; private set;}
        [field: SerializeField] public int Amount { get; private set;}
        [field: SerializeField] public bool IsBusy { get; private set;}

        private void Start()
        {
            Amount = SupplyData.MaxAmount;
        }

        public bool BeginGather()
        {
            if(IsBusy) return false;

            IsBusy = true;
            return true;
        }

        public int EndGather()
        {
            IsBusy = false;
            int amountGather = Mathf.Min(SupplyData.AmountPerGather, Amount);
            Amount -= amountGather;

            if (Amount <= 0)
            {
                Destroy(gameObject);
            }

            return amountGather;
        }

        public void AbortGather()
        {
            IsBusy = false;
        }
    }
}