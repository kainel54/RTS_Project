using Code.CoreSystem;
using Code.Environments;
using Code.GameEvents;
using System.Collections;
using System;
using TMPro;
using UnityEngine;
using Code.Units;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Code.Players;

namespace Code.Quests
{
    public class QuestManager : MonoBehaviour
    {
        [SerializeField] private QuestSequenceSO quest;
        [SerializeField] private TextMeshProUGUI questTitle;
        [SerializeField] private TextMeshProUGUI questObjective;

        private int _objectiveIndex;
        private int _objectiveProgress;
        private bool _grantingReward;
        private bool _gameEnded;
        private BaseBuilding _keep;
        private Coroutine _completionMessageRoutine;

        public bool IsCompleted => quest != null && quest.Objectives != null
            && _objectiveIndex >= quest.Objectives.Count;
        public event Action QuestSequenceCompleted;
        public event Action DefensePhaseStarted;

        private void Awake()
        {
            Bus<SupplyAcceptedEvent>.OnEvent += HandleSupply;
            Bus<BuildingCompletedEvent>.OnEvent += HandleBuildingCompleted;
            Bus<WorkerAssignedEvent>.OnEvent += HandleWorkerAssigned;
            Bus<UnitProducedEvent>.OnEvent += HandleUnitProduced;
            Bus<BuildingDestroyedEvent>.OnEvent += HandleBuildingDestroyed;
            Bus<EnemyDefeatedEvent>.OnEvent += HandleEnemyDefeated;
        }

        private void Start()
        {
            if (questTitle == null)
                questTitle = GameObject.Find("QuestTitle")?.GetComponent<TextMeshProUGUI>();
            if (questObjective == null)
                questObjective = GameObject.Find("QuestObjective")?.GetComponent<TextMeshProUGUI>();

            GameObject keepObject = GameObject.Find("Keep");
            _keep = keepObject != null ? keepObject.GetComponent<BaseBuilding>() : null;

            RefreshUI();
        }

        private void OnDestroy()
        {
            Bus<SupplyAcceptedEvent>.OnEvent -= HandleSupply;
            Bus<BuildingCompletedEvent>.OnEvent -= HandleBuildingCompleted;
            Bus<WorkerAssignedEvent>.OnEvent -= HandleWorkerAssigned;
            Bus<UnitProducedEvent>.OnEvent -= HandleUnitProduced;
            Bus<BuildingDestroyedEvent>.OnEvent -= HandleBuildingDestroyed;
            Bus<EnemyDefeatedEvent>.OnEvent -= HandleEnemyDefeated;
        }

        private void HandleSupply(SupplyAcceptedEvent evt)
        {
            if (_grantingReward) return;

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

        private void HandleUnitProduced(UnitProducedEvent evt)
        {
            QuestObjectiveDefinition objective = CurrentObjective;
            if (objective == null || objective.Type != QuestObjectiveType.ProduceUnit
                || (objective.Unit != null && objective.Unit != evt.Unit)) return;

            AddProgress(1);
        }

        private void HandleBuildingDestroyed(BuildingDestroyedEvent evt)
        {
            if (_gameEnded || evt.Building == null) return;

            bool isKeep = evt.Building == _keep
                || evt.Building.name == "Keep"
                || (evt.Building.AbstractUnitData != null && evt.Building.AbstractUnitData.name == "KeepData");
            if (isKeep)
                ShowGameEnd(false);
        }

        private void HandleEnemyDefeated(EnemyDefeatedEvent evt)
        {
            QuestObjectiveDefinition objective = CurrentObjective;
            if (objective == null || objective.Type != QuestObjectiveType.DefeatEnemies) return;

            AddProgress(1);
        }

        private void ShowGameEnd(bool didWin)
        {
            if (_gameEnded) return;
            _gameEnded = true;
            PlayerController playerController = FindFirstObjectByType<PlayerController>();
            if (playerController != null)
                playerController.enabled = false;
            Time.timeScale = 0f;
            CreateGameEndUI(didWin);
        }

        private void CreateGameEndUI(bool didWin)
        {
            GameObject canvasObject = new GameObject("GameEndCanvas", typeof(RectTransform), typeof(Canvas),
                typeof(CanvasScaler), typeof(GraphicRaycaster));
            Canvas canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 1000;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            scaler.matchWidthOrHeight = 0.5f;

            if (EventSystem.current == null)
                new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));

            Image dimmer = CreateImage("Dimmer", canvasObject.transform, new Color(0.015f, 0.02f, 0.03f, 0.82f));
            StretchToParent(dimmer.rectTransform);

            Image panel = CreateImage("EndPanel", dimmer.transform, new Color(0.11f, 0.14f, 0.16f, 1f));
            RectTransform panelRect = panel.rectTransform;
            panelRect.anchorMin = panelRect.anchorMax = new Vector2(0.5f, 0.5f);
            panelRect.pivot = new Vector2(0.5f, 0.5f);
            panelRect.sizeDelta = new Vector2(760f, 400f);
            panelRect.anchoredPosition = Vector2.zero;

            Image accent = CreateImage("Accent", panel.transform,
                didWin ? new Color(0.88f, 0.69f, 0.28f) : new Color(0.72f, 0.24f, 0.2f));
            RectTransform accentRect = accent.rectTransform;
            accentRect.anchorMin = new Vector2(0f, 1f);
            accentRect.anchorMax = new Vector2(1f, 1f);
            accentRect.pivot = new Vector2(0.5f, 1f);
            accentRect.sizeDelta = new Vector2(0f, 8f);
            accentRect.anchoredPosition = Vector2.zero;

            TMP_FontAsset font = questTitle != null ? questTitle.font : TMP_Settings.defaultFontAsset;
            CreateLabel("ResultTitle", panel.transform, didWin ? "VICTORY" : "DEFEAT", font,
                didWin ? new Color(1f, 0.82f, 0.42f) : new Color(1f, 0.42f, 0.36f), 64f,
                new Vector2(0f, 75f), new Vector2(680f, 100f), FontStyles.Bold);
            CreateLabel("ResultMessage", panel.transform,
                didWin ? "All quests completed." : "The Keep has been destroyed.", font,
                Color.white, 30f, new Vector2(0f, -20f), new Vector2(680f, 70f), FontStyles.Normal);

            Button restartButton = CreateButton(panel.transform, "RestartButton", "Play Again", font,
                new Vector2(0f, -125f));
            restartButton.onClick.AddListener(RestartGame);
        }

        private static Image CreateImage(string objectName, Transform parent, Color color)
        {
            GameObject imageObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            imageObject.transform.SetParent(parent, false);
            Image image = imageObject.GetComponent<Image>();
            image.color = color;
            image.raycastTarget = true;
            return image;
        }

        private static void StretchToParent(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
        }

        private static void CreateLabel(string objectName, Transform parent, string text,
            TMP_FontAsset font, Color color, float fontSize, Vector2 position, Vector2 size, FontStyles style)
        {
            GameObject labelObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer),
                typeof(TextMeshProUGUI));
            labelObject.transform.SetParent(parent, false);
            RectTransform rect = labelObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = size;

            TextMeshProUGUI label = labelObject.GetComponent<TextMeshProUGUI>();
            label.font = font;
            label.text = text;
            label.fontSize = fontSize;
            label.fontStyle = style;
            label.color = color;
            label.alignment = TextAlignmentOptions.Center;
            label.raycastTarget = false;
        }

        private static Button CreateButton(Transform parent, string objectName, string text,
            TMP_FontAsset font, Vector2 position)
        {
            Image image = CreateImage(objectName, parent, new Color(0.34f, 0.29f, 0.19f, 1f));
            RectTransform rect = image.rectTransform;
            rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(260f, 64f);

            Button button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            ColorBlock colors = button.colors;
            colors.highlightedColor = new Color(0.48f, 0.4f, 0.25f, 1f);
            colors.pressedColor = new Color(0.24f, 0.21f, 0.16f, 1f);
            button.colors = colors;
            CreateLabel("Label", image.transform, text, font, Color.white, 26f,
                Vector2.zero, rect.sizeDelta, FontStyles.Bold);
            return button;
        }

        private static void RestartGame()
        {
            Time.timeScale = 1f;
            Scene activeScene = SceneManager.GetActiveScene();
            SceneManager.LoadScene(activeScene.name);
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
                SupplySO rewardSupply = objective.RewardSupply;
                int rewardAmount = objective.RewardAmount;
                _objectiveIndex++;
                _objectiveProgress = 0;
                bool sequenceCompleted = IsCompleted;

                if (rewardSupply != null && rewardAmount > 0)
                    GrantReward(rewardSupply, rewardAmount);

                ShowCompletionMessage(rewardSupply, rewardAmount);
                if (CurrentObjective?.Type == QuestObjectiveType.DefeatEnemies)
                    DefensePhaseStarted?.Invoke();

                if (sequenceCompleted)
                {
                    QuestSequenceCompleted?.Invoke();
                    ShowGameEnd(true);
                }
                return;
            }

            RefreshUI();
        }

        private void GrantReward(SupplySO supply, int amount)
        {
            _grantingReward = true;
            try
            {
                Bus<SupplyEvent>.Raise(new SupplyEvent(amount, supply));
            }
            finally
            {
                _grantingReward = false;
            }
        }

        private void ShowCompletionMessage(SupplySO rewardSupply, int rewardAmount)
        {
            if (questObjective == null) return;

            if (_completionMessageRoutine != null)
                StopCoroutine(_completionMessageRoutine);

            string rewardMessage = rewardSupply != null && rewardAmount > 0
                ? $" Reward: {rewardSupply.name.Replace(" Data", "")} +{rewardAmount}"
                : string.Empty;
            questObjective.SetText($"QuestComplete!{rewardMessage}");
            _completionMessageRoutine = StartCoroutine(RefreshAfterCompletionMessage());
        }

        private IEnumerator RefreshAfterCompletionMessage()
        {
            yield return new WaitForSeconds(2f);
            _completionMessageRoutine = null;
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
