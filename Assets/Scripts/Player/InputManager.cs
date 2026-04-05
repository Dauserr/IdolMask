using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    [SerializeField] private PlayerController _playerController;
    [SerializeField] private TrapConfig       _config;

    private Vector2    _touchStartPos;
    private bool       _isSwiping;

    // input buffering — stores the last direction pressed while the player was mid-move
    private Vector2Int _bufferedDirection;
    private bool       _hasBufferedInput;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        EnhancedTouchSupport.Enable();
    }

    private void Update()
    {
        HandleKeyboard();
        HandleTouch();
        FlushBuffer();
    }

    private void HandleKeyboard()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        if      (kb.wKey.wasPressedThisFrame || kb.upArrowKey.wasPressedThisFrame)    BufferOrMove(GridDirection.Up);
        else if (kb.sKey.wasPressedThisFrame || kb.downArrowKey.wasPressedThisFrame)  BufferOrMove(GridDirection.Down);
        else if (kb.aKey.wasPressedThisFrame || kb.leftArrowKey.wasPressedThisFrame)  BufferOrMove(GridDirection.Left);
        else if (kb.dKey.wasPressedThisFrame || kb.rightArrowKey.wasPressedThisFrame) BufferOrMove(GridDirection.Right);
    }

    private void HandleTouch()
    {
        foreach (var touch in Touch.activeTouches)
        {
            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
            {
                _touchStartPos = touch.screenPosition;
                _isSwiping     = true;
            }
            else if (touch.phase == UnityEngine.InputSystem.TouchPhase.Ended && _isSwiping)
            {
                var delta = (Vector2)touch.screenPosition - _touchStartPos;

                // ignore accidental tiny taps
                if (delta.magnitude >= _config.swipeMinDistance)
                    BufferOrMove(GetSwipeDirection(delta));

                _isSwiping = false;
            }
        }
    }

    private Vector2Int GetSwipeDirection(Vector2 delta)
    {
        // whichever axis moved more is the intended direction
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            return delta.x > 0 ? GridDirection.Right : GridDirection.Left;
        else
            return delta.y > 0 ? GridDirection.Up : GridDirection.Down;
    }

    // if the player is mid-move, save the input and fire it the moment they land
    private void BufferOrMove(Vector2Int direction)
    {
        if (_playerController.IsMoving)
        {
            _bufferedDirection = direction;
            _hasBufferedInput  = true;
        }
        else
        {
            _playerController.TryMove(direction);
        }
    }

    private void FlushBuffer()
    {
        if (!_hasBufferedInput || _playerController.IsMoving) return;

        _playerController.TryMove(_bufferedDirection);
        _hasBufferedInput = false;
    }

    private void OnDestroy()
    {
        EnhancedTouchSupport.Disable();
    }
}
