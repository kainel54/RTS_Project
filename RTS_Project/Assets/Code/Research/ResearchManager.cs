using System.Collections;
using System.Collections.Generic;
using Code.CoreSystem;
using Code.GameEvents;
using Code.Players;
using Code.Units;
using TMPro;
using UnityEngine;

namespace Code.Research
{
    public class ResearchManager : MonoBehaviour
    {
        public static ResearchManager Instance { get; private set; }

        [SerializeField] private ResearchSO[] availableResearches;
        [SerializeField] private TextMeshProUGUI researchTitleText;
        [SerializeField] private TextMeshProUGUI researchProgressText;

        private readonly HashSet<BuildingSO> _unlockedBuildings = new();
        private Coroutine _researchCoroutine;
        private float _researchWorkDone;

        public ResearchSO ActiveResearch { get; private set; }
        public BaseBuilding ActiveResearchBuilding { get; private set; }
        public float ResearchStartedAt { get; private set; }
        public float ResearchProgress => ActiveResearch == null
            ? 0f
            : Mathf.Clamp01(_researchWorkDone / ActiveResearch.ResearchTime);

        private void Update()
        {
            if (researchTitleText == null || researchProgressText == null) return;

            if (ActiveResearch == null)
            {
                researchTitleText.SetText("연구 대기");
                researchProgressText.SetText(string.Empty);
                return;
            }

            researchTitleText.SetText(ActiveResearch.ResearchName);
            researchProgressText.SetText($"연구 진행도: {Mathf.FloorToInt(ResearchProgress * 100f)}%");
        }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;
        }

        private void OnDestroy()
        {
            if (Instance == this)
                Instance = null;
        }

        public bool IsBuildingUnlocked(BuildingSO building)
        {
            if (building == null) return true;
            if (availableResearches == null || availableResearches.Length == 0) return true;

            bool requiresResearch = false;
            foreach (ResearchSO research in availableResearches)
            {
                if (research == null || research.BuildingToUnlock != building) continue;
                requiresResearch = true;
                break;
            }

            return !requiresResearch || _unlockedBuildings.Contains(building);
        }

        public bool CanStartResearch(ResearchSO research, BaseBuilding library)
        {
            return research != null && library != null && ActiveResearch == null
                   && library.AssignedWorkerCount > 0
                   && research.BuildingToUnlock != null
                   && !IsBuildingUnlocked(research.BuildingToUnlock)
                   && UserSupplies.Instance != null
                   && UserSupplies.Instance.HasEnoughSupplies(research.Cost);
        }

        public bool TryStartResearch(ResearchSO research, BaseBuilding library)
        {
            if (!CanStartResearch(research, library)
                || UserSupplies.Instance == null
                || !UserSupplies.Instance.TrySpendSupplies(research.Cost)) return false;

            ActiveResearch = research;
            ActiveResearchBuilding = library;
            ResearchStartedAt = Time.time;
            _researchWorkDone = 0f;
            _researchCoroutine = StartCoroutine(Research(research));
            Bus<ResearchStateChangedEvent>.Raise(new ResearchStateChangedEvent());
            return true;
        }

        private IEnumerator Research(ResearchSO research)
        {
            while (_researchWorkDone < research.ResearchTime)
            {
                if (ActiveResearchBuilding != null && ActiveResearchBuilding.AssignedWorkerCount > 0)
                    _researchWorkDone += Time.deltaTime * ActiveResearchBuilding.AssignedWorkerCount;

                yield return null;
            }

            _unlockedBuildings.Add(research.BuildingToUnlock);
            ActiveResearch = null;
            ActiveResearchBuilding = null;
            _researchCoroutine = null;

            Bus<BuildingUnlockedEvent>.Raise(new BuildingUnlockedEvent(research.BuildingToUnlock));
            Bus<ResearchStateChangedEvent>.Raise(new ResearchStateChangedEvent());
            Debug.Log($"Research completed: {research.ResearchName}");
        }
    }
}
