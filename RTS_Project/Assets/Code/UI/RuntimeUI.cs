using System;
using System.Collections.Generic;
using System.Linq;
using Code.Commands;
using Code.CoreSystem;
using Code.GameEvents;
using Code.UI.Containers;
using Code.Units;
using TMPro;
using UnityEngine;

namespace Code.UI
{
    public class RuntimeUI : MonoBehaviour
    {
        [SerializeField] private CommandContainerUI commandContainerUI;
        [SerializeField] private BuildUnitContainerUI buildUnitContainerUI;
        [SerializeField] private BuildSupplyContainerUI buildSupplyContainerUI;
        [SerializeField] private ResourceContainerUI resourceContainerUI;
        [SerializeField] private TextMeshProUGUI populationText;
        [SerializeField] private TextMeshProUGUI workerCountText;

        private HashSet<AbstractCommandable> _selectedUnits = new HashSet<AbstractCommandable>(24);
        private PopulationSystem _populationSystem;
        private TextMeshProUGUI _populationText;
        private BaseBuilding _displayedBuilding;

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

            _populationSystem = PopulationSystem.Instance;
            workerCountText?.transform.parent.gameObject.SetActive(false);
            if (_populationSystem != null)
            {
                _populationText = populationText;
                _populationSystem.PopulationChanged += UpdatePopulationUI;
                UpdatePopulationUI(_populationSystem.CurrentPopulation, _populationSystem.MaximumPopulation);
            }
        }

        private void OnDestroy()
        {
            Bus<UnitSelectEvent>.OnEvent -= HandleUnitSelect;
            Bus<UnitDeSelectEvent>.OnEvent -= HandleUnitDeSelect;
            Bus<UserSupplyReadyEvent>.OnEvent -= HandleUserSupplyReady;
            Bus<UnitDeSpawnEvent>.OnEvent -= HandleUnitDeSpawn;
            if (_populationSystem != null)
                _populationSystem.PopulationChanged -= UpdatePopulationUI;
            if (_displayedBuilding != null)
                _displayedBuilding.AssignedWorkersUpdated -= UpdateWorkerCountUI;
        }

        private void UpdatePopulationUI(int currentPopulation, int maximumPopulation)
        {
            if (_populationText != null)
                _populationText.SetText($"{currentPopulation}/{maximumPopulation}");
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
            UpdateDisplayedBuilding();
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

        private void UpdateDisplayedBuilding()
        {
            if (_displayedBuilding != null)
                _displayedBuilding.AssignedWorkersUpdated -= UpdateWorkerCountUI;

            _displayedBuilding = _selectedUnits.Count == 1 ? _selectedUnits.First() as BaseBuilding : null;
            if (workerCountText == null) return;

            if (_displayedBuilding == null)
            {
                workerCountText.transform.parent.gameObject.SetActive(false);
                return;
            }

            _displayedBuilding.AssignedWorkersUpdated += UpdateWorkerCountUI;
            UpdateWorkerCountUI(_displayedBuilding.AssignedWorkers);
        }

        private void UpdateWorkerCountUI(Worker[] workers)
        {
            if (_displayedBuilding == null || workerCountText == null) return;

            workerCountText.transform.parent.gameObject.SetActive(true);
            workerCountText.SetText($"{workers.Length}/{_displayedBuilding.WorkerCapacity}");
        }

        private void DisableAllUI()
        {
            commandContainerUI.Disable();
            buildUnitContainerUI.Disable();
            buildSupplyContainerUI.Disable();
        }
    }
}
