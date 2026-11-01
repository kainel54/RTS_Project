using Code.Commands;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Code.UI.Components
{
    public class CommandButtonUI : MonoBehaviour, IUIElement<BaseCommandSO, UnityAction>
    {
        [SerializeField] private Image icon;
        [SerializeField] private Button button;
        
        private void Awake()
        {
            Debug.Assert(icon != null && button != null, $"icon 또는 button이 할당되어 있지 않습니다. : {gameObject.name}");
        }
        
        public void EnableFor(BaseCommandSO command, UnityAction onClickAction)
        {
            SetIcon(command.CommandIcon);
            button.interactable = true;
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(onClickAction);
        }

        public void Disable()
        {
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
    }
}