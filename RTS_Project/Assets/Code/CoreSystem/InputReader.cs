using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Code.CoreSystem
{
    [CreateAssetMenu(fileName = "Input reader", menuName = "SO/Input reader", order = 5)]
    public class InputReader : ScriptableObject, Controls.IPlayerActions
    {
        private Controls _controls;

        //[SerializeField] private LayerMask selectableUnitLayer;
        
        public Vector2 MousePosition { get; private set; }
        public Vector2 KeyboardMovement { get; private set; }

        public Action<bool> OnMouseLeftButton;
        public Action<bool> OnMouseRightButton;

        private void OnEnable()
        {
            if (_controls == null)
            {
                _controls = new Controls();
                _controls.Player.SetCallbacks(this);
            }
            _controls.Player.Enable();
        }

        private void OnDisable()
        {
            _controls.Player.Disable();
        }

        public void OnMove(InputAction.CallbackContext context)
        {
            KeyboardMovement = context.ReadValue<Vector2>();
        }

        public void OnAim(InputAction.CallbackContext context)
        {
            MousePosition = context.ReadValue<Vector2>();
        }

        public void OnLeftButton(InputAction.CallbackContext context)
        {
            if(context.performed)
                OnMouseLeftButton?.Invoke(true);
            else if(context.canceled)
                OnMouseLeftButton?.Invoke(false);
        }

        public void OnRightButton(InputAction.CallbackContext context)
        {
            if(context.performed)
                OnMouseRightButton?.Invoke(true);
            else if(context.canceled)
                OnMouseRightButton?.Invoke(false);
        }

        public bool GetMousePosition(out RaycastHit hit, LayerMask targetMask)
        {
            Camera mainCam = Camera.main;
            hit = default;
            if (mainCam == null) return false;

            Ray ray = mainCam.ScreenPointToRay(MousePosition);
            if (Physics.Raycast(ray, out hit, mainCam.farClipPlane, targetMask))
            {
                return true;
            }

            return false;
        }
    }
}