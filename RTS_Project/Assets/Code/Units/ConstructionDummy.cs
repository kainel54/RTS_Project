using Code.CoreSystem;
using Code.GameEvents;
using System;
using UnityEngine;

namespace Code.Units
{
    public class ConstructionDummy : AbstractCommandable
    {
        [SerializeField] private GameObject[] constructionStages;
        [SerializeField] private GameObject ghostVisual;

        private int _currentStageIndex = 0;

        [field: SerializeField] public ConstructionProgress ProgressData { get; private set; } = new ConstructionProgress(BuildingState.Ghost, 0, 0);   

        private IBuildingConstructor _unitBuildingThis;

        [field:SerializeField] public Renderer MainRenderer {  get; private set; }
        public BuildingSO BuildingSo { get; private set; }


        public IBuildingConstructor UnitBuildingThis
        {
            get => _unitBuildingThis;
            set
            {
                if (_unitBuildingThis != value)
                {
                    Bus<UnitDeSpawnEvent>.OnEvent -= HandleUnitDeSpawn;
                    if (value != null)
                        Bus<UnitDeSpawnEvent>.OnEvent += HandleUnitDeSpawn;
                }
                _unitBuildingThis = value;
            }
        }

        private void HandleUnitDeSpawn(UnitDeSpawnEvent evt)
        {
            if (evt.Unit is IBuildingConstructor constructor && constructor == _unitBuildingThis)
            {
                if (ProgressData.State == BuildingState.Ghost)
                {
                    Destroy(gameObject);
                    return;
                }

                ProgressData = new ConstructionProgress(
                    BuildingState.Paused,
                    ProgressData.StartTime,
                    (Time.time - ProgressData.StartTime) / AbstractUnitData.BuildTime
                );

                Bus<UnitDeSpawnEvent>.OnEvent -= HandleUnitDeSpawn;
            }

        }

        protected override void Start()
        {
            base.Start();
            BuildingSo = AbstractUnitData as BuildingSO;
            Debug.Assert(BuildingSo != null, $"BuildingSO is not assigned in {gameObject.name}");
        }


        public void StartPlacementGhost(IBuildingConstructor constructor)
        {
            UnitBuildingThis = constructor;

            SetGhostVisual(true);
            ProgressData = new ConstructionProgress(BuildingState.Ghost, Time.time, 0);
        }

        public void StartConstruction(IBuildingConstructor constructor)
        {
            UnitBuildingThis = constructor;
            if (ProgressData.State == BuildingState.Ghost)
            {
                SetGhostVisual(false);
                ChangeConstructionStage(0);
            }

            //건설 재개를 고려하여 현재까지 빌드된 데이터만큼 시작시간을 당겨준다.
            ProgressData = new ConstructionProgress(BuildingState.Constructing,
                Time.time - AbstractUnitData.BuildTime * ProgressData.Progress,
                ProgressData.Progress);

        }


        private void ChangeConstructionStage(int nextStageIndex)
        {
            if (nextStageIndex < 0 || nextStageIndex >= constructionStages.Length)
                return;

            constructionStages[_currentStageIndex].SetActive(false);
            _currentStageIndex = nextStageIndex;
            constructionStages[_currentStageIndex].SetActive(true);
        }
        private void SetGhostVisual(bool isVisible)
        {
            constructionStages[_currentStageIndex].SetActive(!isVisible);
            ghostVisual.SetActive(isVisible);
        }

        public void UpdateConstructionProgress(float progress)
        {
            if (progress < 0 || progress > 1 || _currentStageIndex >= constructionStages.Length)
                return;

            if (_currentStageIndex == 0 && progress > 0.45f)
                ChangeConstructionStage(1);
            if (_currentStageIndex == 1 && progress > 0.9f)
                ChangeConstructionStage(2);
        }
        protected override void OnDestroy()
        {
            base.OnDestroy();
            Bus<UnitDeSpawnEvent>.OnEvent -= HandleUnitDeSpawn; //파괴시에도 구독해제는 반드시.
        }
        public void ConstructionComplete()
        {
            if (IsSelected)
            {
                DeSelect();
            }
        }


    }
}