using System.Collections.Generic;
using System.Linq;
using Code.Commands;
using Code.CoreSystem;
using Code.GameEvents;
using Code.Units;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace Code.Players
{
    public enum ButtonState
    {
        None,
        Pressed,
        Released,
        Hold
    }

    public class PlayerController : MonoBehaviour
    {
        [Header("Selection settings")] [SerializeField]
        private InputReader inputReader;

        [SerializeField] private LayerMask selectableUnitLayer;
        [SerializeField] private LayerMask floorLayer;
        [SerializeField] private LayerMask interactableLayer;
        [SerializeField] private RectTransform selectionBox;

        [Space] [Header("Camera settings")] [SerializeField]
        private CameraConfig cameraConfig;

        [SerializeField] private Rigidbody cameraTarget;

        // private ISelectable _selectedUnit;
        private HashSet<ISelectable> _selectedUnits = new HashSet<ISelectable>(24);
        private HashSet<AbstractUnit> _aliveUnits = new HashSet<AbstractUnit>(200);
        private HashSet<AbstractUnit> _addedUnits = new HashSet<AbstractUnit>(24);

        private ButtonState _leftButtonState = ButtonState.None;
        private Vector2 _startMousePosition;
        private BaseCommandSO _activeCommand; //현재 클릭된 커맨드를 저장하는 변수.
        private bool _wasMouseOverOnUI = false; //UI위에 현재 마우스가 있는가?

        private GhostPlacement _ghostInstance; //마우스를 따라다닐 고스트 인스턴스

        private void Awake()
        {
            // inputReader.OnMouseLeftButton += HandleMouseLeftButton;
            inputReader.OnMouseRightButton += HandleMouseRightButton;

            Bus<UnitSelectEvent>.OnEvent += HandleUnitSelected;
            Bus<UnitDeSelectEvent>.OnEvent += HandleUnitDeSelected;
            Bus<UnitSpawnEvent>.OnEvent += HandleUnitSpawn;
            Bus<UnitDeSpawnEvent>.OnEvent += HandleUnitDeSpawn;
            Bus<CommandSelectEvent>.OnEvent += HandleCommandSelect;
        }

        private void OnDestroy()
        {
            // inputReader.OnMouseLeftButton -= HandleMouseLeftButton;
            inputReader.OnMouseRightButton -= HandleMouseRightButton;

            Bus<UnitSelectEvent>.OnEvent -= HandleUnitSelected;
            Bus<UnitDeSelectEvent>.OnEvent -= HandleUnitDeSelected;
            Bus<UnitSpawnEvent>.OnEvent -= HandleUnitSpawn;
            Bus<UnitDeSpawnEvent>.OnEvent -= HandleUnitDeSpawn;
            Bus<CommandSelectEvent>.OnEvent -= HandleCommandSelect;
        }

        #region Event Handler logic

        private void HandleUnitSpawn(UnitSpawnEvent evt) => _aliveUnits.Add(evt.Unit);
        private void HandleUnitDeSpawn(UnitDeSpawnEvent evt)
        {
            _aliveUnits.Remove(evt.Unit);
            _selectedUnits.Remove(evt.Unit);
        }
        private void HandleUnitSelected(UnitSelectEvent evt) => _selectedUnits.Add(evt.Unit);
        private void HandleUnitDeSelected(UnitDeSelectEvent evt) => _selectedUnits.Remove(evt.Unit);

        private void HandleCommandSelect(CommandSelectEvent evt)
        {
            _activeCommand = evt.Command;

            if (_activeCommand.RequireClickToActivate == false)
            {
                ActivateCommand(new RaycastHit());
            }
            else if (_activeCommand is IConstructionCommand buildCommand)
            {
                GameObject instance = Instantiate(buildCommand.GhostPrefab);
                _ghostInstance = instance.GetComponent<GhostPlacement>();
                Debug.Assert(_ghostInstance != null, "GhostPlacement component is missing on the ghost prefab.");
            }
        }

        private void ActivateCommand(RaycastHit hit)
        {
            if(_ghostInstance != null)
            {
                Destroy(_ghostInstance.gameObject);
                _ghostInstance = null;
                
            }

            List<AbstractCommandable> abstractUnits =
                _selectedUnits.OfType<AbstractCommandable>().ToList();

            for (int i = 0; i < abstractUnits.Count; i++)
            {
                CommandContext context = new CommandContext(abstractUnits[i], hit, i);
                _activeCommand.Handle(context);
            }

            _activeCommand = null;
        }

        private void HandleGhostInstance()
        {
            if (_ghostInstance == null)
                return;


            if (Keyboard.current.escapeKey.wasReleasedThisFrame)
            {
                Destroy(_ghostInstance.gameObject);
                _ghostInstance = null;
                _activeCommand = null;
                return;
            }
            if (inputReader.GetMousePosition(out RaycastHit hit, floorLayer))
            {
                _ghostInstance.transform.position = hit.point;
                if (_activeCommand is IConstructionCommand constructionCommand)
                {
                    bool isPassRestriction = constructionCommand.AllRestrictionPass(hit.point);
                    _ghostInstance.SetPassRestriction(isPassRestriction);
                }
            }

        }
        

        #endregion

        private void Update()
        {
            UpdateLeftButtonState();
            HandlePanning();
            HandleDragSelect();
            HandleGhostInstance();
        }

        private void UpdateLeftButtonState()
        {
            if (Mouse.current.leftButton.wasPressedThisFrame)
                _leftButtonState = ButtonState.Pressed;
            else if (Mouse.current.leftButton.isPressed && !Mouse.current.leftButton.wasPressedThisFrame)
                _leftButtonState = ButtonState.Hold;
            else if (Mouse.current.leftButton.wasReleasedThisFrame)
                _leftButtonState = ButtonState.Released;
            else
                _leftButtonState = ButtonState.None;
        }

        #region Drag Logic

        private void HandleDragSelect()
        {
            if (selectionBox == null) return; //안전코드
            switch (_leftButtonState)
            {
                case ButtonState.Pressed:
                    HandleDragStart();
                    break;
                case ButtonState.Hold:
                    HandleMouseDrag();
                    break;
                case ButtonState.Released:
                    HandleDragReleased();
                    break;
            }
        }

        private void HandleDragReleased()
        {
            if(!_wasMouseOverOnUI && _activeCommand == null && !Keyboard.current.shiftKey.isPressed)
                DeSelectAllUnits();
            
            HandleMouseLeftButtonReleased();

            foreach (AbstractUnit unit in _addedUnits)
            {
                unit.Select(); //이렇게 하면 자동으로 메시징에 의해서 _selectedUnit에 들어가게 된다.
            }

            selectionBox.gameObject.SetActive(false);
        }

        private void DeSelectAllUnits()
        {
            //현재 선택된 녀석들을 전부 디셀렉트 해준다.
            foreach (ISelectable selectable in _selectedUnits.ToArray())
                selectable.DeSelect();
        }

        private void HandleMouseDrag()
        {
            if (_activeCommand != null || _wasMouseOverOnUI) return;
            
            Vector2 mousePosition = inputReader.MousePosition;
            Bounds bound = ResizeSelectionBox(mousePosition); //나중에 씁니다.

            foreach (AbstractUnit unit in _aliveUnits)
            {
                if (unit.gameObject.activeInHierarchy == false) continue;

                Vector2 unitScreenPosition = Camera.main.WorldToScreenPoint(unit.transform.position);
                if (bound.Contains(unitScreenPosition))
                {
                    _addedUnits.Add(unit);
                }
                else
                {
                    _addedUnits.Remove(unit);
                }
            }
        }

        private void HandleDragStart()
        {
            selectionBox.sizeDelta = Vector2.zero;
            selectionBox.gameObject.SetActive(true);
            _startMousePosition = inputReader.MousePosition;
            _addedUnits.Clear(); //드래그 박스안에 있던 유닛을 전부 클리어해주고 시작.
            _wasMouseOverOnUI = EventSystem.current.IsPointerOverGameObject();
        }

        #endregion
        
        private void HandleMouseLeftButtonReleased()
        {
            if(_wasMouseOverOnUI) return;

            RaycastHit hit;
            if (_activeCommand == null
                && inputReader.GetMousePosition(out hit, selectableUnitLayer)
                && hit.collider.TryGetComponent(out ISelectable selectable)
                && _addedUnits.Count == 0)
            {
                selectable.Select();
            }else if (_activeCommand != null
                      && inputReader.GetMousePosition(out hit, floorLayer | interactableLayer))
            {
                ActivateCommand(hit);
            }
        }

        private void HandleMouseRightButton(bool isPressed)
        {
            if (_selectedUnits.Count == 0) return;

            if (!isPressed
                && inputReader.GetMousePosition(out RaycastHit hit, floorLayer | interactableLayer))
            {
                //이동이 가능한 유닛들을 전부 가져온다.
                List<AbstractUnit> selectedUnits = _selectedUnits.OfType<AbstractUnit>().ToList();


                for (int i = 0; i < selectedUnits.Count; i++)
                {
                    CommandContext context = new CommandContext(selectedUnits[i], hit, i, MouseButton.Right);
                    foreach (BaseCommandSO command in GetAvailableCommands(selectedUnits[i]))
                    {
                        if (command.CanHandle(context))
                        {
                            command.Handle(context);
                            break; //첫번째로 맞는 커맨드를 찾았다면 수행한다.
                        }
                    }
                }
            }
        }
        private List<BaseCommandSO> GetAvailableCommands(AbstractUnit targetUnit)
        {
            //오버라이드 커맨드 메뉴들을 먼저 가져와서
            OverrideCommandsCommandSO[] overrideCommands
                = targetUnit.AvailableCommands.OfType<OverrideCommandsCommandSO>().ToArray();

            //오버라이드 커맨드 안에 있는 것중 또다른 오버라이드 메뉴가 아닌것만 골라서 넣어준다.(안그러면 무한 재귀에 빠짐)
            // 메뉴는 2뎁스로만 설계함. 
            List<BaseCommandSO> allAvailableCommands = new List<BaseCommandSO>();
            foreach (var overrideCommand in overrideCommands)
            {
                allAvailableCommands.AddRange(
                    overrideCommand.Commands.Where(command => command is not OverrideCommandsCommandSO));
            }

            //그외 명령어들을 추가해준다.
            allAvailableCommands.AddRange(
                targetUnit.AvailableCommands.Where(command => command is not OverrideCommandsCommandSO));

            //추가되는 순서에 따라 건설메뉴가 우선권을 가지게 된다.
            return allAvailableCommands;
        }



        #region Utility logic

        private Bounds ResizeSelectionBox(Vector2 mousePosition)
        {
            Vector2 delta = mousePosition - _startMousePosition;
            selectionBox.anchoredPosition = _startMousePosition + delta * 0.5f;
            selectionBox.sizeDelta = new Vector2(Mathf.Abs(delta.x), Mathf.Abs(delta.y));

            return new Bounds(selectionBox.anchoredPosition, selectionBox.sizeDelta);
        }

        private void HandlePanning()
        {
            Vector2 movement = inputReader.KeyboardMovement * cameraConfig.KeyboardPanSpeed;
            movement += GetMouseMoveAmount();
            cameraTarget.linearVelocity = new Vector3(movement.x, 0, movement.y);
        }
        
        private Vector2 GetMouseMoveAmount()
        {
            Vector2 moveAmount = Vector2.zero;

            if (cameraConfig.EnableEdgePan == false) return moveAmount;

            Vector2 mousePosition = inputReader.MousePosition;
            Vector2 screenSize = new Vector2(Screen.width, Screen.height);

            if (mousePosition.x < cameraConfig.EdgePanSize)
            {
                moveAmount.x -= cameraConfig.MousePanSpeed;
            }
            else if (mousePosition.x > screenSize.x - cameraConfig.EdgePanSize)
            {
                moveAmount.x += cameraConfig.MousePanSpeed;
            }

            if (mousePosition.y < cameraConfig.EdgePanSize)
            {
                moveAmount.y -= cameraConfig.MousePanSpeed;
            }
            else if (mousePosition.y > screenSize.y - cameraConfig.EdgePanSize)
            {
                moveAmount.y += cameraConfig.MousePanSpeed;
            }

            return moveAmount;
        }

        #endregion
        
    }
}