using Code.CoreSystem;
using Code.GameEvents;
using TMPro;
using UnityEngine;

namespace Code.Quests
{
    public class QuestManager : MonoBehaviour
    {
        [SerializeField] private QuestSequenceSO quest;
        [SerializeField] private TextMeshProUGUI questTitle;
        [SerializeField] private TextMeshProUGUI questObjective;

        private int _objectiveIndex;
        private int _objectiveProgress;

        private void Awake()
        {
            Bus<SupplyEvent>.OnEvent += HandleSupply;
            Bus<BuildingCompletedEvent>.OnEvent += HandleBuildingCompleted;
            Bus<WorkerAssignedEvent>.OnEvent += HandleWorkerAssigned;
        }

        private void Start()
        {
            if (questTitle == null)
                questTitle = GameObject.Find("QuestTitle")?.GetComponent<TextMeshProUGUI>();
            if (questObjective == null)
                questObjective = GameObject.Find("QuestObjective")?.GetComponent<TextMeshProUGUI>();

            RefreshUI();
        }

        private void OnDestroy()
        {
            Bus<SupplyEvent>.OnEvent -= HandleSupply;
            Bus<BuildingCompletedEvent>.OnEvent -= HandleBuildingCompleted;
            Bus<WorkerAssignedEvent>.OnEvent -= HandleWorkerAssigned;
        }

        private void HandleSupply(SupplyEvent evt)
        {
            QuestObjectiveDefinition objective = CurrentObjective;
            if (objective == null || objective.Type != QuestObjectiveType.CollectSupply
                || objective.Supply != evt.SupplyType || evt.Amount <= 0) return;

            AddProgress(evt.Amount);
        }

        private void HandleBuildingCompleted(BuildingCompletedEvent evt)
        {
            QuestObjectiveDefinition objective = CurrentObjective;
            if (objective == null || objective.Type != QuestObjectiveType.CompleteBuilding
                || objective.Building != evt.Building) return;

            AddProgress(1);
        }

        private void HandleWorkerAssigned(WorkerAssignedEvent evt)
        {
            QuestObjectiveDefinition objective = CurrentObjective;
            if (objective == null || objective.Type != QuestObjectiveType.AssignWorkerToBuilding
                || objective.Building != evt.Building) return;

            AddProgress(1);
        }

        private QuestObjectiveDefinition CurrentObjective
        {
            get
            {
                if (quest == null || quest.Objectives == null || _objectiveIndex >= quest.Objectives.Count)
                    return null;
                return quest.Objectives[_objectiveIndex];
            }
        }

        private void AddProgress(int amount)
        {
            QuestObjectiveDefinition objective = CurrentObjective;
            if (objective == null) return;

            _objectiveProgress = Mathf.Min(objective.TargetCount, _objectiveProgress + amount);
            if (_objectiveProgress >= objective.TargetCount)
            {
                _objectiveIndex++;
                _objectiveProgress = 0;
            }

            RefreshUI();
        }

        private void RefreshUI()
        {
            if (questTitle == null || questObjective == null) return;

            if (quest == null)
            {
                questTitle.SetText("퀘스트");
                questObjective.SetText("퀘스트 데이터가 연결되지 않았습니다.");
                return;
            }

            questTitle.SetText(quest.QuestTitle);
            QuestObjectiveDefinition objective = CurrentObjective;
            if (objective == null)
            {
                questObjective.SetText("모든 목표를 완료했습니다!");
                return;
            }

            string description = string.IsNullOrWhiteSpace(objective.Description)
                ? objective.Type.ToString()
                : objective.Description;
            questObjective.SetText($"{_objectiveProgress}/{objective.TargetCount}  {description}");
        }
    }
}
