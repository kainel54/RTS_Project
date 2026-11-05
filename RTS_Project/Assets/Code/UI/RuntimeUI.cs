using System;
using System.Collections.Generic;
using System.Linq;
using Code.Commands;
using Code.CoreSystem;
using Code.GameEvents;
using Code.UI.Containers;
using Code.Units;
using UnityEngine;

namespace Code.UI
{
    public class RuntimeUI : MonoBehaviour
    {
        [SerializeField] private CommandContainerUI commandContainerUI;
        [SerializeField] private BuildUnitContainerUI buildUnitContainerUI;
        [SerializeField] private BuildSupplyContainerUI buildSupplyContainerUI;
        [SerializeField] private ResourceContainerUI resourceContainerUI;

        private HashSet<AbstractCommandable> _selectedUnits = new HashSet<AbstractCommandable>(24);

        private void Awake()
        {
            Bus<UnitSelectEvent>.OnEvent += HandleUnitSelect;
            Bus<UnitDeSelectEvent>.OnEvent += HandleUnitDeSelect;
            Bus<UserSupplyReadyEvent>.OnEvent += HandleUserSupplyReady;
            Bus<UnitDeSpawnEvent>.OnEvent += HandleUnitDeSpawn;
        }

       
        private void Start()
        {
            DisableAllUI();
        }

        private void OnDestroy()
        {
            Bus<UnitSelectEvent>.OnEvent -= HandleUnitSelect;
            Bus<UnitDeSelectEvent>.OnEvent -= HandleUnitDeSelect;
            Bus<UserSupplyReadyEvent>.OnEvent -= HandleUserSupplyReady;
            Bus<UnitDeSpawnEvent>.OnEvent -= HandleUnitDeSpawn;
        }
        private void HandleUnitDeSpawn(UnitDeSpawnEvent evt)
        {
            _selectedUnits.Remove(evt.Unit);
            RefreshUI();
        }

        private void HandleUserSupplyReady(UserSupplyReadyEvent evt)
        {
            resourceContainerUI.EnableFor(evt.UserSupplies);
        }

        private void HandleUnitSelect(UnitSelectEvent evt)
        {
            if (evt.Unit is AbstractCommandable commandable)
                _selectedUnits.Add(commandable);
            RefreshUI();
        }

        private void HandleUnitDeSelect(UnitDeSelectEvent evt)
        {
            if (evt.Unit is AbstractCommandable commandable)
                _selectedUnits.Remove(commandable);
            RefreshUI();
        }

        private void RefreshUI()
        {
            DisableAllUI();
            if (_selectedUnits.Count > 0)
            {
                commandContainerUI.EnableFor(_selectedUnits);

                if (_selectedUnits.Count == 1)
                {
                    AbstractCommandable firstUnit = _selectedUnits.First();
                    if (firstUnit is BaseBuilding baseBuilding)
                    {
                        bool hasSupplyProduction = baseBuilding.AvailableCommands != null
                                                   && baseBuilding.AvailableCommands.Any(command => command is BuildSupplyCommandSO);

                        if (hasSupplyProduction)
                            buildSupplyContainerUI.EnableFor(baseBuilding);
                        else
                            buildUnitContainerUI.EnableFor(baseBuilding);
                    }
                }

            }
        }

        private void DisableAllUI()
        {
            commandContainerUI.Disable();
            buildUnitContainerUI.Disable();
            buildSupplyContainerUI.Disable();
        }
    }
}
