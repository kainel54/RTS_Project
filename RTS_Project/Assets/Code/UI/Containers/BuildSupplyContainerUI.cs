using System.Collections;
using Code.Environments;
using Code.UI.Components;
using Code.Units;
using TMPro;
using UnityEngine;

namespace Code.UI.Containers
{
    public class BuildSupplyContainerUI : MonoBehaviour, IUIElement<BaseBuilding>
    {
        [SerializeField] private ProgressBarUI progressBar;
        [SerializeField] private BuildingQueueButtonUI[] queueButtons;
        [SerializeField] private TextMeshProUGUI firstSupplyNameText;

        private BaseBuilding _currentBuilding;
        private Coroutine _generateCoroutine;

        public void EnableFor(BaseBuilding building)
        {
            if (_currentBuilding != null)
                _currentBuilding.OnSupplyQueueUpdated -= HandleQueueUpdate;

            gameObject.SetActive(true);
            _currentBuilding = building;
            _currentBuilding.OnSupplyQueueUpdated += HandleQueueUpdate;

            SetUpQueueButtons();
            if (_currentBuilding.SupplyQueueSize > 0)
                _generateCoroutine = StartCoroutine(UpdateSupplyProgress());
        }

        public void Disable()
        {
            if (_currentBuilding != null)
                _currentBuilding.OnSupplyQueueUpdated -= HandleQueueUpdate;

            if (_generateCoroutine != null)
                StopCoroutine(_generateCoroutine);

            _currentBuilding = null;
            _generateCoroutine = null;
            gameObject.SetActive(false);
        }

        private void SetUpQueueButtons()
        {
            SupplySO[] supplies = _currentBuilding.SupplyQueue;
            int i = 0;
            for (; i < supplies.Length && i < queueButtons.Length; i++)
            {
                int index = i;
                queueButtons[i].EnableFor(supplies[i], () => _currentBuilding.CancelSupply(index));
            }

            for (; i < queueButtons.Length; i++)
                queueButtons[i].Disable();

            firstSupplyNameText.SetText(supplies.Length > 0 ? supplies[0].name : string.Empty);
        }

        private void HandleQueueUpdate(SupplySO[] suppliesInQueue)
        {
            if (_currentBuilding == null) return;

            SetUpQueueButtons();
            if (_generateCoroutine != null)
                StopCoroutine(_generateCoroutine);

            _generateCoroutine = null;
            if (suppliesInQueue.Length > 0 && _currentBuilding.SupplySOBeingBuilt != null)
                _generateCoroutine = StartCoroutine(UpdateSupplyProgress());
        }

        private IEnumerator UpdateSupplyProgress()
        {
            while (_currentBuilding != null
                   && _currentBuilding.SupplyQueueSize > 0
                   && _currentBuilding.SupplySOBeingBuilt != null)
            {
                float buildTime = _currentBuilding.SupplySOBeingBuilt.BuildTime;
                float progress = buildTime <= 0f
                    ? 1f
                    : (Time.time - _currentBuilding.CurrentQueueStartTime) / buildTime;

                progressBar.SetProgress(progress);
                yield return null;
            }

            _generateCoroutine = null;
        }
    }
}
