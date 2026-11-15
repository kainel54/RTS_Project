using System.Text;
using Code.Commands;
using Code.Environments;
using Code.Units;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Code.UI.Components
{
    public class CommandButtonUI : MonoBehaviour, IUIElement<BaseCommandSO, UnityAction>,
        IPointerEnterHandler, IPointerExitHandler
    {
        [SerializeField] private Image icon;
        [SerializeField] private Button button;
        [SerializeField] private GameObject tooltipRoot;
        [SerializeField] private TMP_Text tooltipText;

        private string _costText;
        
        private void Awake()
        {
            Debug.Assert(icon != null && button != null, $"icon 또는 button이 할당되어 있지 않습니다. : {gameObject.name}");
            HideTooltip();
        }
        
        public void EnableFor(BaseCommandSO command, UnityAction onClickAction)
        {
            SetIcon(command.CommandIcon);
            _costText = BuildCostText(command.GetSupplyCost());
            HideTooltip();
            button.interactable = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(onClickAction);
        }

        public void Disable()
        {
            _costText = null;
            HideTooltip();
            SetIcon(null);
            button.interactable = false;
            button.onClick.RemoveAllListeners();
        }

        private void SetIcon(Sprite sprite)
        {
            if (sprite == null)
                icon.enabled = false;
            else
            {
                icon.sprite = sprite;
                icon.enabled = true;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (button != null && button.interactable && !string.IsNullOrEmpty(_costText)
                && tooltipRoot != null && tooltipText != null)
            {
                tooltipText.text = _costText;
                tooltipRoot.SetActive(true);
            }
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HideTooltip();
        }

        private void HideTooltip()
        {
            if (tooltipRoot != null)
                tooltipRoot.SetActive(false);
        }

        private static string BuildCostText(SupplyCostSO cost)
        {
            if (cost == null) return null;

            var text = new StringBuilder();
            AppendCostLine(text, cost.WoodSO, "Wood", cost.Wood);
            AppendCostLine(text, cost.StoneSO, "Stone", cost.Stone);
            AppendCostLine(text, cost.FoodSO, "Food", cost.Food);
            AppendCostLine(text, cost.IronOreSO, "Iron Ore", cost.IronOre);
            AppendCostLine(text, cost.IronIngotSO, "Iron Ingot", cost.IronIngot);
            return text.Length > 0 ? text.ToString() : null;
        }

        private static void AppendCostLine(StringBuilder text, SupplySO supply, string fallbackName, int amount)
        {
            if (amount <= 0) return;

            if (text.Length > 0)
                text.AppendLine();

            string supplyName = supply != null ? supply.name.Replace(" Data", string.Empty) : fallbackName;
            text.Append(supplyName).Append(": ").Append(amount);
        }
    }
}
