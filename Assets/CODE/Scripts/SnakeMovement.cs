using System.Collections;
using System.Collections.Generic;
using System.Linq;
using MoreMountains.Feedbacks;
using Sirenix.OdinInspector;
using UnityEngine;
using Utilities.Extensions;

public enum FaceDirection { Right, Left, Up, Down }

/// Place this script on the HEAD of the snake.
/// Assign all body parts (including head) in the bodyParts array.
public class SnakeMovement : MonoBehaviour
{
    [Title("Body Parts")]
    [Tooltip("All snake parts in order: [Head (this), Body1, Body2, Tail]")]
    [SerializeField] private Transform[] bodyParts;
    [SerializeField] private SnakeSkin[] snakeSkins;

    [Title("Movement")]
    [SerializeField] private float fallSpeed = 2f;
    [SerializeField] private MMFeedbacks stepFeedbacks;

    [Title("Collision")]
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private LayerMask obstacleMask;

    [Title("Goal (Portal)")]
    [SerializeField] private float spiralStepTime = 0.15f;
    [SerializeField] private float shrinkTime = 0.10f;
    [SerializeField] private float stepDistance = 1f;

    [Title("Undo")]
    [SerializeField] private int maxUndoSteps = 20;

    public FaceDirection Direction { get; private set; } = FaceDirection.Right;
    public bool IsInSpiral { get; private set; }
    public bool CanUndo => _undoStack.Count > 0 && !IsInSpiral;

    private Vector3[] _currentPositions;
    private Quaternion[] _currentRotations;
    private bool _isGrounded = true;
    private int _activeShrinks;
    private bool _enteredSpiral;
    private readonly Stack<Snapshot> _undoStack = new(32);

    private void Awake()
    {
        ValidateSetup();
        SaveBodyState();

        LoadSkin();
    }

    private void Update()
    {
        if (!IsInSpiral) CheckGroundAndFall();
    }

    private void ValidateSetup()
    {
        if (bodyParts == null || bodyParts.Length == 0)
        {
            Debug.LogError("[SnakeMovement] No body parts assigned!", this);
            enabled = false;
            return;
        }

        if (bodyParts[0] != transform)
        {
            Debug.LogWarning("[SnakeMovement] First body part should be the head (this transform)!", this);
        }
    }

    #region Movement

    public void MoveLeft()
    {
        if (!CanMove(FaceDirection.Left)) return;
        if (IsBlocked(Vector3.left)) return;

        Direction = FaceDirection.Left;
        TryStep(Vector3.left);
    }

    public void MoveRight()
    {
        if (!CanMove(FaceDirection.Right)) return;
        if (IsBlocked(Vector3.right)) return;

        Direction = FaceDirection.Right;
        TryStep(Vector3.right);
    }

    public void MoveUp()
    {
        if (!CanMove(FaceDirection.Up)) return;
        if (IsBlocked(Vector3.up)) return;

        Direction = FaceDirection.Up;
        TryStep(Vector3.up);
    }

    public void MoveDown()
    {
        if (!CanMove(FaceDirection.Down)) return;
        if (IsBlocked(Vector3.down)) return;

        Direction = FaceDirection.Down;
        TryStep(Vector3.down);
    }

    private bool CanMove(FaceDirection newDirection)
    {
        if (IsInSpiral || !_isGrounded) return false;

        // Can't reverse direction (180° turn prevention)
        return Direction switch
        {
            FaceDirection.Left when newDirection == FaceDirection.Right => false,
            FaceDirection.Right when newDirection == FaceDirection.Left => false,
            FaceDirection.Up when newDirection == FaceDirection.Down => false,
            FaceDirection.Down when newDirection == FaceDirection.Up => false,
            _ => true
        };
    }

    private void TryStep(Vector3 direction)
    {
        transform.forward = direction.normalized;

        if (direction == Vector3.up || direction == Vector3.down) transform.localEulerAngles = transform.localEulerAngles.WithY(0f);

        PushSnapshot();
        transform.position += direction;
        MoveBodyParts();

        FindFirstObjectByType<ObjectiveTracker>()?.OnMoveUsed();
        stepFeedbacks?.PlayFeedbacks();
    }

    private bool IsBlocked(Vector3 direction)
    {
        int mask = obstacleMask.value != 0 ? obstacleMask.value : Physics.DefaultRaycastLayers;
        return Physics.Raycast(transform.position, direction, 1f, mask, QueryTriggerInteraction.Ignore);
    }

    private void MoveBodyParts()
    {
        if (bodyParts == null || bodyParts.Length < 2) return;

        // Each part takes the position and rotation the part in front just vacated
        for (int i = 1; i < bodyParts.Length; i++)
        {
            if (bodyParts[i] == null) continue;

            bodyParts[i].position = _currentPositions[i - 1];
            bodyParts[i].rotation = _currentRotations[i - 1];
        }

        SaveBodyState();
    }

    // Cache these to avoid GC allocations every move
    private void SaveBodyState()
    {
        if (bodyParts == null) return;

        // Only initialize if the size changed or it's the first time
        if (_currentPositions == null || _currentPositions.Length != bodyParts.Length)
        {
            _currentPositions = new Vector3[bodyParts.Length];
            _currentRotations = new Quaternion[bodyParts.Length];
        }

        for (int i = 0; i < bodyParts.Length; i++)
        {
            _currentPositions[i] = bodyParts[i] ? bodyParts[i].position : Vector3.zero;
            _currentRotations[i] = bodyParts[i] ? bodyParts[i].rotation : Quaternion.identity;
        }
    }

    #endregion

    #region Ground Detection & Falling

    private void CheckGroundAndFall()
    {
        if (bodyParts == null || bodyParts.Length == 0) return;

        int mask = groundLayer.value != 0 ? groundLayer.value : Physics.DefaultRaycastLayers;
        bool anyGrounded = bodyParts.Any(part => part && Physics.Raycast(part.position, Vector3.down, 0.6f, mask, QueryTriggerInteraction.Ignore));

        if (!anyGrounded)
        {
            _isGrounded = false;

            // Move all parts down together
            foreach (Transform part in bodyParts)
            {
                if (part)
                {
                    part.position += Vector3.down * fallSpeed * Time.deltaTime;
                }
            }
        }
        else
        {
            _isGrounded = true;
            SaveBodyState();
        }
    }

    #endregion

    #region Collision Detection

    private void OnTriggerEnter(Collider other)
    {
        if (!other) return;

        if (other.CompareTag("LoseZone"))
        {
            GameManager.Instance.OnGameLose?.Invoke("Hit obstacle");
        }
        else if (other.CompareTag("Fruit"))
        {
            FindFirstObjectByType<ObjectiveTracker>()?.OnFruitCollected();
            var feedbacks = other.GetComponentInChildren<MMFeedbacks>();
            if (feedbacks)
            {
                feedbacks.transform.SetParent(other.transform.parent);
                feedbacks.PlayFeedbacks();
            }

            Destroy(other.gameObject);
        }
        else if (other.CompareTag("SpiralGate") && !_enteredSpiral)
        {
            other.GetComponentInChildren<MMFeedbacks>()?.PlayFeedbacks();

            var tracker = FindFirstObjectByType<ObjectiveTracker>();
            if (!tracker)
            {
                _enteredSpiral = true;
                StartSpiral(transform);
                return;
            }

            if (tracker.CanEnterSpiral(out _))
            {
                _enteredSpiral = true;
                tracker.StopTimer();
                StartSpiral(transform);
            }
            else
            {
                if (tracker.ApplesRemaining > 0)
                {
                    _enteredSpiral = true;
                    tracker.OnReachedSpiral();
                }
            }
        }
    }

    #endregion

    #region Spiral Portal Animation

    public void StartSpiral(Transform spiralCenter)
    {
        if (IsInSpiral || !spiralCenter) return;
        StartCoroutine(SpiralSequence(spiralCenter));
    }

    private IEnumerator SpiralSequence(Transform spiralCenter)
    {
        IsInSpiral = true;

        // Disable all colliders during spiral
        foreach (Transform part in bodyParts)
        {
            if (part && part.TryGetComponent<Collider>(out var col))
            {
                col.enabled = false;
            }
        }

        Vector3 moveDirection = transform.forward.normalized;
        var remaining = new List<Transform>(bodyParts.Where(p => p));

        // Move parts into spiral one by one
        while (remaining.Count > 0)
        {
            remaining.RemoveAll(p => p == null);
            if (remaining.Count == 0) break;

            // Calculate movement
            Vector3[] startPos = remaining.Select(p => p.position).ToArray();
            Vector3[] endPos = startPos.Select(p => p + moveDirection * stepDistance).ToArray();

            // Animate step
            float elapsed = 0f;
            while (elapsed < spiralStepTime)
            {
                float t = elapsed / spiralStepTime;

                for (int i = 0; i < remaining.Count; i++)
                {
                    if (!remaining[i]) continue;

                    remaining[i].position = Vector3.Lerp(startPos[i], endPos[i], t);

                    // Rotate toward spiral center
                    Vector3 toCenter = (spiralCenter.position - remaining[i].position).normalized;
                    remaining[i].forward = Vector3.Slerp(remaining[i].forward, toCenter, 10f * Time.deltaTime);
                }

                elapsed += Time.deltaTime;
                yield return null;
            }

            // Finalize positions
            for (int i = 0; i < remaining.Count; i++)
            {
                if (remaining[i]) remaining[i].position = endPos[i];
            }

            // Shrink first part
            Transform toHide = remaining[0];
            if (toHide)
            {
                _activeShrinks++;
                StartCoroutine(ShrinkPart(toHide));
            }

            remaining.RemoveAt(0);
        }

        // Wait for all shrinking to finish
        while (_activeShrinks > 0) yield return null;

        IsInSpiral = false;
        GameManager.Instance.OnGameWin?.Invoke();
    }

    private IEnumerator ShrinkPart(Transform part)
    {
        if (!part)
        {
            _activeShrinks--;
            yield break;
        }

        Vector3 startScale = part.localScale;
        float elapsed = 0f;

        while (elapsed < shrinkTime)
        {
            if (!part)
            {
                _activeShrinks--;
                yield break;
            }

            float t = 1f - (elapsed / shrinkTime);
            part.localScale = startScale * t;

            elapsed += Time.deltaTime;
            yield return null;
        }

        // Hide visuals
        if (part)
        {
            foreach (Renderer r in part.GetComponentsInChildren<Renderer>())
                if (r) r.enabled = false;

            foreach (Collider c in part.GetComponentsInChildren<Collider>())
                if (c) c.enabled = false;

            part.localScale = Vector3.zero;
        }

        _activeShrinks--;
    }

    #endregion

    #region Undo System

    public bool TryUndo(bool refundMove = true)
    {
        if (!CanUndo)
        {
            UIManager.Instance?.ShowToastMessage("Nothing to undo");
            return false;
        }

        Snapshot snapshot = _undoStack.Pop();

        // Restore all state
        for (int i = 0; i < bodyParts.Length; i++)
        {
            if (bodyParts[i])
            {
                bodyParts[i].SetPositionAndRotation(snapshot.positions[i], snapshot.rotations[i]);
            }
        }

        Direction = snapshot.direction;
        SaveBodyState();

        if (refundMove) FindFirstObjectByType<ObjectiveTracker>()?.RefundMove();

        return true;
    }

    private void PushSnapshot()
    {
        if (bodyParts == null || bodyParts.Length == 0) return;

        Snapshot snapshot = new()
        {
            positions = new Vector3[bodyParts.Length],
            rotations = new Quaternion[bodyParts.Length],
            direction = Direction
        };

        for (int i = 0; i < bodyParts.Length; i++)
        {
            snapshot.positions[i] = bodyParts[i] ? bodyParts[i].position : Vector3.zero;
            snapshot.rotations[i] = bodyParts[i] ? bodyParts[i].rotation : Quaternion.identity;
        }

        _undoStack.Push(snapshot);

        // Keep only recent snapshots
        while (_undoStack.Count > maxUndoSteps)
        {
            _undoStack.Pop();
        }
    }

    private struct Snapshot
    {
        public Vector3[] positions;
        public Quaternion[] rotations;
        public FaceDirection direction;

        public override readonly string ToString()
        {
            string posStr = positions != null ? $"[{string.Join(", ", positions.Select(p => p.ToString("F2")))}]" : "null";
            string rotStr = rotations != null ? $"[{string.Join(", ", rotations.Select(r => r.eulerAngles.ToString("F2")))}]" : "null";
            return $"Snapshot(direction: {direction}, positions: {posStr}, rotations: {rotStr})";
        }
    }

    #endregion

    #region Skin

    public void LoadSkin()
    {
        // Disable all.
        foreach (Transform model in snakeSkins.SelectMany(s => s.SkinParts)) model.gameObject.SetActive(false);

        // Load new skin.
        var newSkin = snakeSkins[PlayerPrefs.GetInt(GameManager.SELECTED_CHARACTER_PREFS)];
        foreach (Transform model in newSkin.SkinParts) model.gameObject.SetActive(true);
    }

    [System.Serializable]
    public struct SnakeSkin
    {
        public Transform[] SkinParts;
    }

    #endregion
}