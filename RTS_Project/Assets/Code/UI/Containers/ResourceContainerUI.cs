using Code.Environments;
using Code.Players;
using System;
using TMPro;
using UnityEngine;

namespace Code.UI.Containers
{
    public class ResourceContainerUI : MonoBehaviour, IUIElement<UserSupplies>
    {
        [SerializeField] private SupplySO mineralSO;
        [SerializeField] private TextMeshProUGUI mineralText;
        [SerializeField] private SupplySO gasSO;
        [SerializeField] private TextMeshProUGUI gasText;

        private UserSupplies _userSupplies;

        public void EnableFor(UserSupplies item)
        {
            _userSupplies = item;
            _userSupplies.OnSupplyChanged -= UpdateSupplyUI;
            _userSupplies.OnSupplyChanged += UpdateSupplyUI;
            UpdateSupplyUI(_userSupplies.WoodAmount, mineralSO);
            UpdateSupplyUI(_userSupplies.StoneAmount, gasSO);

            gameObject.SetActive(true);
        }
        public void Disable()
        {
            gameObject.SetActive(false);
            if(_userSupplies!=null)
                _userSupplies.OnSupplyChanged -= UpdateSupplyUI;
            _userSupplies = null;
        }

        private void UpdateSupplyUI(int amount, SupplySO supplyType)
        {
            if (supplyType.Equals(mineralSO))
            {
                mineralText.SetText(amount.ToString());
            }

            if (supplyType.Equals(gasSO))
            {
                gasText.SetText(amount.ToString());
            }
        }

    }
}