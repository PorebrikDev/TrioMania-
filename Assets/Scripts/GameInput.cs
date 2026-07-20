using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameInput : MonoBehaviour
{
    private PlayerAction _input;
    private Vector2 _startPosition;

    public event Action<Vector2> OnTouchStart;
    public event Action<Vector2> OnToutchEnd;
    public event Action OnKey;

    private void Awake()
    {
        _input = new PlayerAction();
        _input.Interact.Touch.started += TouchStart;
        _input.Interact.Touch.canceled += TouchEnd;
        _input.Interact.Menu.started += Key;

        _input.Enable();
    }

    private void OnDestroy()
    {
        _input.Interact.Touch.started -= TouchStart;
        _input.Interact.Touch.canceled -= TouchEnd;

        _input.Disable();
    }

    private void TouchStart(InputAction.CallbackContext context)
    {
        _startPosition = _input.Interact.Value.ReadValue<Vector2>();
        OnTouchStart?.Invoke(_startPosition);
    }
    private void Key(InputAction.CallbackContext context)
    {
        _startPosition = _input.Interact.Value.ReadValue<Vector2>();
        OnKey?.Invoke();
    }

    private void TouchEnd(InputAction.CallbackContext context)
    {

        Vector2 endPosition = _input.Interact.Value. ReadValue<Vector2>();
        Vector2 delta = endPosition - _startPosition;

        if (delta.magnitude < 25)
            return;

        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
        {
            OnToutchEnd?.Invoke(delta.x > 0 ? Vector2.right : Vector2.left);
        }

        else
        {
            OnToutchEnd?.Invoke(delta.y > 0 ? Vector2.up : Vector2.down);
        }
    }
}