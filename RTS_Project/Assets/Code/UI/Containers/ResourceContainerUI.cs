using Code.Environments;
using Code.Players;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.UI.Containers
{
    public class ResourceContainerUI : MonoBehaviour, IUIElement<UserSupplies>
    {
        [SerializeField] private SupplySO mineralSO;
        [SerializeField] private TextMeshProUGUI mineralText;
        [SerializeField] private SupplySO gasSO;
        [SerializeField] private TextMeshProUGUI gasText;
        [SerializeField] private SupplySO foodSO;
        [SerializeField] private TextMeshProUGUI foodText;

        private UserSupplies _userSupplies;

        private void Awake()
        {
            if (foodText == null && foodSO != null)
                foodText = CreateCounter("FoodBar", foodSO.Icon);
        }

        public void EnableFor(UserSupplies item)
        {
            _userSupplies = item;
            _userSupplies.OnSupplyChanged -= UpdateSupplyUI;
            _userSupplies.OnSupplyChanged += UpdateSupplyUI;
            UpdateSupplyUI(_userSupplies.WoodAmount, mineralSO);
            UpdateSupplyUI(_userSupplies.StoneAmount, gasSO);
            UpdateSupplyUI(_userSupplies.FoodAmount, foodSO);

            gameObject.SetActive(true);
        }

        public TextMeshProUGUI CreateCounter(string counterName, Sprite icon)
        {
            GameObject template = mineralText.transform.parent.gameObject;
            GameObject counter = Instantiate(template, transform, false);
            counter.name = counterName;

            Image iconImage = counter.GetComponentInChildren<Image>();
            if (iconImage != null)
            {
                iconImage.sprite = icon;
                iconImage.enabled = icon != null;
                iconImage.raycastTarget = false;
            }

            TextMeshProUGUI counterText = counter.GetComponentInChildren<TextMeshProUGUI>();
            if (counterText != null)
                counterText.SetText("0");

            return counterText;
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
            if (supplyType == null) return;

            if (supplyType == mineralSO)
            {
                mineralText.SetText(amount.ToString());
            }

            if (supplyType == gasSO)
            {
                gasText.SetText(amount.ToString());
            }

            if (foodSO != null && supplyType == foodSO && foodText != null)
                foodText.SetText(amount.ToString());
        }

    }
}
