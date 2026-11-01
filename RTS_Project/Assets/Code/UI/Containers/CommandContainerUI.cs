using System;
using System.Collections.Generic;
using System.Linq;
using Code.Commands;
using Code.CoreSystem;
using Code.GameEvents;
using Code.UI.Components;
using Code.Units;
using UnityEngine;
using UnityEngine.Events;

namespace Code.UI.Containers
{
    public class CommandContainerUI : MonoBehaviour, IUIElement<HashSet<AbstractCommandable>>
    {
        [SerializeField] private CommandButtonUI[] commandButtons;
        
        public void EnableFor(HashSet<AbstractCommandable> selectedUnits)
        {
            RefreshButtons(selectedUnits);
        }

        public void Disable()
        {
            foreach (CommandButtonUI commandButton in commandButtons)
            {
                commandButton.Disable();
            }
        }

        private void RefreshButtons(HashSet<AbstractCommandable> selectedUnits)
        {
            HashSet<BaseCommandSO> availableCommands = new HashSet<BaseCommandSO>(9);

            foreach (AbstractCommandable commandable in selectedUnits)
            {
                if(commandable.AvailableCommands != null)
                    availableCommands.UnionWith(commandable.AvailableCommands);
                //가능한 명령을 합집합으로 합해준다.(나중엔 교집합으로 변경할거다. 일단은 합집합으로)
            }

            for (int i = 0; i < commandButtons.Length; i++)
            {
                BaseCommandSO commandForSlot = availableCommands.FirstOrDefault(command => command.SlotIndex == i);

                if (commandForSlot != null)
                {
                    commandButtons[i].EnableFor(commandForSlot, HandleClickGenerator(commandForSlot));
                }
                else
                {
                    commandButtons[i].Disable();
                }
            }
        }

        private UnityAction HandleClickGenerator(BaseCommandSO commandSO)
            => () => Bus<CommandSelectEvent>.Raise(new CommandSelectEvent(commandSO));

        
    }
}