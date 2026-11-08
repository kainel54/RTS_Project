using System;
using System.Collections.Generic;
using Code.Environments;
using Code.Units;
using UnityEngine;

namespace Code.Quests
{
    public enum QuestObjectiveType
    {
        CollectSupply,
        CompleteBuilding,
        AssignWorkerToBuilding,
        CompleteResearch
    }

    [Serializable]
    public class QuestObjectiveDefinition
    {
        [field: SerializeField] public QuestObjectiveType Type { get; private set; }
        [field: SerializeField] public string Description { get; private set; }
        [field: SerializeField, Min(1)] public int TargetCount { get; private set; } = 1;
        [field: SerializeField] public SupplySO Supply { get; private set; }
        [field: SerializeField] public BuildingSO Building { get; private set; }
        [field: SerializeField] public SupplySO RewardSupply { get; private set; }
        [field: SerializeField, Min(0)] public int RewardAmount { get; private set; }
    }

    [CreateAssetMenu(fileName = "Quest Sequence", menuName = "Quests/Quest Sequence")]
    public class QuestSequenceSO : ScriptableObject
    {
        [field: SerializeField] public string QuestTitle { get; private set; } = "Main Quest";
        [field: SerializeField] public List<QuestObjectiveDefinition> Objectives { get; private set; } = new();
    }
}
