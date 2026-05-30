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
    private Vector2Int _bufferedDirection;
    private bool       _hasBufferedInput;
    private float _holdDelay = 0.2f;     // seconds before repeat starts
    private float _holdInterval => _config.playerMoveSpeed;
    private Vector2Int _heldDirection = Vector2Int.zero;
    private float _nextMoveTime = 0f;

    // input is always enabled before the game starts (player must walk to the mask)
    // and disabled only after the game ends
    private bool _isInputEnabled = true;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        EnhancedTouchSupport.Enable();
    }

    private void OnEnable()
    {
        GameEvents.OnGameWon     += DisableInput;
        GameEvents.OnGameLost    += DisableInput;
        GameEvents.OnGameStarted   += EnableInput;
        GameEvents.OnGameRestarted += EnableInput;
        GameEvents.OnPlayerRespawned    += ClearBuffer;
    }

    private void OnDisable()
    {
        GameEvents.OnGameWon     -= DisableInput;
        GameEvents.OnGameLost    -= DisableInput;
        GameEvents.OnGameStarted   -= EnableInput;
        GameEvents.OnGameRestarted -= EnableInput;
        GameEvents.OnPlayerRespawned    -= ClearBuffer;
    }

    private void EnableInput()  => _isInputEnabled = true;
    private void DisableInput() => _isInputEnabled = false;

    private void Update()
    {
        if (!_isInputEnabled) return;
        HandleKeyboard();
        HandleTouch();
        FlushBuffer();
    }

    private void ClearBuffer()
    {
        _bufferedDirection = Vector2Int.zero;
        _hasBufferedInput  = false;
        _heldDirection     = Vector2Int.zero;
        _nextMoveTime      = 0f;              
    }

    private void HandleKeyboard()
    {
        var kb = Keyboard.current;
        if (kb == null) return;

        // Check if E is held for super jump
        bool superJumpHeld = kb.eKey.isPressed;

        Vector2Int dir = Vector2Int.zero;

        if (kb.wKey.isPressed || kb.upArrowKey.isPressed)         dir = GridDirection.Up;
        else if (kb.sKey.isPressed || kb.downArrowKey.isPressed)  dir = GridDirection.Down;
        else if (kb.aKey.isPressed || kb.leftArrowKey.isPressed)  dir = GridDirection.Left;
        else if (kb.dKey.isPressed || kb.rightArrowKey.isPressed) dir = GridDirection.Right;

        if (dir == Vector2Int.zero)
        {
            _heldDirection = Vector2Int.zero;
            _nextMoveTime = 0f;
            return;
        }

        // E is held — super jump instead of normal move, no repeat on hold
        if (superJumpHeld)
        {
            // Only trigger once per fresh keypress, not on hold repeat
            // We detect "fresh press" by checking if direction just changed
            if (dir != _heldDirection)
            {
                _heldDirection = dir;
                _nextMoveTime = Time.time + _holdDelay;
                _playerController.TrySuperJump(dir); // direct call, no buffer needed
            }
            return; // skip normal move logic entirely
        }

        // Normal movement below — unchanged from original
        if (dir != _heldDirection)
        {
            _heldDirection = dir;
            _nextMoveTime = Time.time + _holdDelay;
            BufferOrMove(dir);
            return;
        }

        if (Time.time >= _nextMoveTime)
        {
            _nextMoveTime = Time.time + _holdInterval;
            BufferOrMove(dir);
        }
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
                if (delta.magnitude >= _config.swipeMinDistance)
                    BufferOrMove(GetSwipeDirection(delta));
                _isSwiping = false;
            }
        }
    }

    private Vector2Int GetSwipeDirection(Vector2 delta)
    {
        if (Mathf.Abs(delta.x) > Mathf.Abs(delta.y))
            return delta.x > 0 ? GridDirection.Right : GridDirection.Left;
        else
            return delta.y > 0 ? GridDirection.Up : GridDirection.Down;
    }

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

    private void OnDestroy() => EnhancedTouchSupport.Disable();
}
