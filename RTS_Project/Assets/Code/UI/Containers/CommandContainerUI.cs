using System;
using System.Collections.Generic;
using System.Linq;
using Code.Commands;
using Code.CoreSystem;
using Code.GameEvents;
using Code.Players;
using Code.Research;
using Code.UI.Components;
using Code.Units;
using UnityEngine;
using UnityEngine.Events;

namespace Code.UI.Containers
{
    public class CommandContainerUI : MonoBehaviour, IUIElement<HashSet<AbstractCommandable>>
    {
        [SerializeField] private CommandButtonUI[] commandButtons;
        private HashSet<AbstractCommandable> _lastSelectedUnits;
        private BaseBuilding _researchBuilding;
        private UserSupplies _userSupplies;

        private void Awake()
        {
            Bus<BuildingUnlockedEvent>.OnEvent += HandleBuildingUnlocked;
            Bus<ResearchStateChangedEvent>.OnEvent += HandleResearchStateChanged;
            Bus<UserSupplyReadyEvent>.OnEvent += HandleUserSupplyReady;
        }

        private void OnDestroy()
        {
            Bus<BuildingUnlockedEvent>.OnEvent -= HandleBuildingUnlocked;
            Bus<ResearchStateChangedEvent>.OnEvent -= HandleResearchStateChanged;
            Bus<UserSupplyReadyEvent>.OnEvent -= HandleUserSupplyReady;
            if (_userSupplies != null)
                _userSupplies.OnSupplyChanged -= HandleSupplyChanged;
            if (_researchBuilding != null)
                _researchBuilding.AssignedWorkersUpdated -= HandleAssignedWorkersUpdated;
        }

        private void HandleBuildingUnlocked(BuildingUnlockedEvent evt)
        {
            if (_lastSelectedUnits != null)
                RefreshButtons(_lastSelectedUnits);
        }

        private void HandleResearchStateChanged(ResearchStateChangedEvent evt)
        {
            if (_lastSelectedUnits != null)
                RefreshButtons(_lastSelectedUnits);
        }

        private void HandleUserSupplyReady(UserSupplyReadyEvent evt)
        {
            if (_userSupplies != null)
                _userSupplies.OnSupplyChanged -= HandleSupplyChanged;

            _userSupplies = evt.UserSupplies;
            _userSupplies.OnSupplyChanged += HandleSupplyChanged;
        }

        private void HandleSupplyChanged(int amount, Code.Environments.SupplySO supply)
        {
            if (_lastSelectedUnits != null)
                RefreshButtons(_lastSelectedUnits);
        }

        private void HandleAssignedWorkersUpdated(Worker[] workers)
        {
            if (_lastSelectedUnits != null)
                RefreshButtons(_lastSelectedUnits);
        }
        
        public void EnableFor(HashSet<AbstractCommandable> selectedUnits)
        {
            _lastSelectedUnits = selectedUnits;
            if (_researchBuilding != null)
                _researchBuilding.AssignedWorkersUpdated -= HandleAssignedWorkersUpdated;
            _researchBuilding = selectedUnits.Count == 1
                ? selectedUnits.FirstOrDefault() as BaseBuilding
                : null;
            if (_researchBuilding != null)
                _researchBuilding.AssignedWorkersUpdated += HandleAssignedWorkersUpdated;
            RefreshButtons(selectedUnits);
        }

        public void Disable()
        {
            _lastSelectedUnits = null;
            if (_researchBuilding != null)
                _researchBuilding.AssignedWorkersUpdated -= HandleAssignedWorkersUpdated;
            _researchBuilding = null;
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

            ResearchManager researchManager = ResearchManager.Instance;
            if (researchManager != null)
            {
                availableCommands.RemoveWhere(command =>
                {
                    if (command is ConstructBuildingCommandSO constructionCommand)
                        return !researchManager.IsBuildingUnlocked(constructionCommand.BuildingData);

                    if (command is ResearchCommandSO researchCommand)
                    {
                        return selectedUnits.Count != 1
                               || selectedUnits.FirstOrDefault() is not BaseBuilding library
                               || !researchCommand.CanHandle(new CommandContext(library, new RaycastHit()));
                    }

                    return false;
                });
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
