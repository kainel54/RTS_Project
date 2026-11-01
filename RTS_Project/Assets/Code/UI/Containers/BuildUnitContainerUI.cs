using System.Collections;
using Code.UI.Components;
using Code.Units;
using TMPro;
using UnityEngine;

namespace Code.UI.Containers
{
    public class BuildUnitContainerUI : MonoBehaviour, IUIElement<BaseBuilding>
    {
        [SerializeField] private ProgressBarUI progressBar;
        [SerializeField] private BuildingQueueButtonUI[] queueButtons;
        [SerializeField] private TextMeshProUGUI firstUnitNameText;
        
        private BaseBuilding _currentBaseBuilding;
        private Coroutine _generateCoroutine;

        private void SetUpQueueButtons()
        {
            int i = 0;
            for (; i < _currentBaseBuilding.QueueSize; i++)
            {
                int idx = i;
                queueButtons[i].EnableFor(_currentBaseBuilding.Queue[i], () => _currentBaseBuilding.CancelGenerate(idx));
                if (i == 0)
                {
                    firstUnitNameText.SetText(_currentBaseBuilding.Queue[i].name);
                }
            }

            for (; i < queueButtons.Length; i++)
            {
                queueButtons[i].Disable();
            }

            if (_currentBaseBuilding.QueueSize == 0)
            {
                firstUnitNameText.SetText(string.Empty);
            }
        }
        
        public void EnableFor(BaseBuilding item)
        {
            if (_currentBaseBuilding != null)
            {
                _currentBaseBuilding.OnQueueUpdated -= HandleQueueUpdate;
            }
            
            gameObject.SetActive(true);
            _currentBaseBuilding = item;
            _currentBaseBuilding.OnQueueUpdated += HandleQueueUpdate;

            SetUpQueueButtons();
                
            _generateCoroutine = StartCoroutine(UpdateUnitProgress());
        }

        public void Disable()
        {
            if(_currentBaseBuilding != null)
                _currentBaseBuilding.OnQueueUpdated -= HandleQueueUpdate;
            gameObject.SetActive(false);
            _currentBaseBuilding = null;
            _generateCoroutine = null;
        }
        
        private void HandleQueueUpdate(UnitSO[] unitsInQueue)
        {
            if (unitsInQueue.Length == 1 && _generateCoroutine == null)
            {
                _generateCoroutine = StartCoroutine(UpdateUnitProgress());
            }

            if (_currentBaseBuilding != null)
            {
                SetUpQueueButtons();
            }
        }

        private IEnumerator UpdateUnitProgress()
        {
            while (_currentBaseBuilding != null && _currentBaseBuilding.QueueSize > 0)
            {
                float startTime = _currentBaseBuilding.CurrentQueueStartTime;
                float progress = Mathf.Clamp01(
                    (Time.time - startTime) / _currentBaseBuilding.SOBeingBuilt.BuildTime);

                progressBar.SetProgress(progress);
                yield return null;
            }

            _generateCoroutine = null;
        }
    }
}