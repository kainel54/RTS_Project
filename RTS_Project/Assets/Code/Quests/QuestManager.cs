using Code.CoreSystem;
using Code.GameEvents;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Code.Quests
{
    public class QuestManager : MonoBehaviour
    {
        [SerializeField] private QuestSequenceSO quest;

        private int _objectiveIndex;
        private int _objectiveProgress;
        private TextMeshProUGUI _titleText;
        private TextMeshProUGUI _objectiveText;

        private void Awake()
        {
            Bus<SupplyEvent>.OnEvent += HandleSupply;
            Bus<BuildingCompletedEvent>.OnEvent += HandleBuildingCompleted;
            Bus<WorkerAssignedEvent>.OnEvent += HandleWorkerAssigned;
        }

        private void Start()
        {
            CreateQuestPanel();
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

        private void CreateQuestPanel()
        {
            GameObject canvasObject = new GameObject("QuestCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 20;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            GameObject panelObject = new GameObject("QuestPanel", typeof(RectTransform), typeof(Image));
            panelObject.transform.SetParent(canvasObject.transform, false);
            RectTransform panel = panelObject.GetComponent<RectTransform>();
            panel.anchorMin = new Vector2(0, 1);
            panel.anchorMax = new Vector2(0, 1);
            panel.pivot = new Vector2(0, 1);
            panel.anchoredPosition = new Vector2(24, -100);
            panel.sizeDelta = new Vector2(360, 112);
            panelObject.GetComponent<Image>().color = new Color(0.07f, 0.09f, 0.08f, 0.88f);

            _titleText = CreateText(panel, "QuestTitle", new Vector2(16, -12), 22, FontStyles.Bold);
            _objectiveText = CreateText(panel, "QuestObjective", new Vector2(16, -48), 18, FontStyles.Normal);
            _objectiveText.rectTransform.sizeDelta = new Vector2(328, 54);
        }

        private TextMeshProUGUI CreateText(Transform parent, string objectName, Vector2 position,
            float fontSize, FontStyles style)
        {
            GameObject textObject = new GameObject(objectName, typeof(RectTransform), typeof(TextMeshProUGUI));
            textObject.transform.SetParent(parent, false);
            RectTransform rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0, 1);
            rect.anchorMax = new Vector2(0, 1);
            rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(328, 30);

            TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
            text.font = TMP_Settings.defaultFontAsset;
            text.fontSize = fontSize;
            text.fontStyle = style;
            text.color = Color.white;
            text.raycastTarget = false;
            return text;
        }

        private void RefreshUI()
        {
            if (_titleText == null || _objectiveText == null) return;

            if (quest == null)
            {
                _titleText.SetText("퀘스트");
                _objectiveText.SetText("퀘스트 데이터가 연결되지 않았습니다.");
                return;
            }

            _titleText.SetText(quest.QuestTitle);
            QuestObjectiveDefinition objective = CurrentObjective;
            if (objective == null)
            {
                _objectiveText.SetText("모든 목표를 완료했습니다!");
                return;
            }

            string description = string.IsNullOrWhiteSpace(objective.Description)
                ? objective.Type.ToString()
                : objective.Description;
            _objectiveText.SetText($"{_objectiveProgress}/{objective.TargetCount}  {description}");
        }
    }
}
