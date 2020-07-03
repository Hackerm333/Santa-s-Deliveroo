using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(BoxCollider))]
[RequireComponent(typeof(Rigidbody))]
public class UnitRTS : MonoBehaviour, IOutlineable
{
    private const byte CollectionCapacity = 5;
    private List<Item> collectedItems = new List<Item>(CollectionCapacity);
    [SerializeField] private Transform collectedGiftParent;
    private bool _moving;

    [SerializeField] private float rotSpeed = 5f;
    private Vector3 _targetPosition = Vector3.zero;
    [SerializeField] private float speed = 5f;
    [SerializeField] private float decreaseSpeed = 0.25f;
    private readonly float _accuracyWp = 0.2f;

    public UnityEvent onItemDelivered;
    public Events.EventUnit onBeingCaptured;

    [SerializeField] private List<Vector3> destinations;
    [SerializeField] private GameObject selection;

    private Vector3 _direction;
    private Vector3 _lastCollectedItemPos;

    [SerializeField] private LineRenderer pathLineRenderer;

    private Quaternion _defaultRingRotation;
    private int _lastPathLineRendererIndex;
    private GameObject _currentRingTargetSpawned;

    private Vector3 _lastDestination;

    private static readonly Vector3 RingRotation = new Vector3(90, 0, 0);
    private List<GameObject> _ringSpawned = new List<GameObject>();
    private List<GameObject> _lineRendSpawned = new List<GameObject>();

    private void Awake()
    {
        if (onItemDelivered == null)
            onItemDelivered = new UnityEvent();

        if (onBeingCaptured == null)
            onBeingCaptured = new Events.EventUnit();

        _defaultRingRotation = selection.transform.rotation;
    }

    private void OnTriggerEnter(Collider other)
    {
        var item = other.gameObject.GetComponent<Item>();
        if (other.gameObject.CompareTag("Item"))
        {
            if (collectedItems.Count >= collectedItems.Capacity)
            {
                AudioManager.Instance.PlayAudio(AudioManager.Instance.cantPickItem);
                return;
            }

            if (!item) return;
            collectedItems.Add(item);
            other.gameObject.SetActive(false);
            AddItem(item);
            speed -= decreaseSpeed;
            AudioManager.Instance.PlayAudio(AudioManager.Instance.itemCollectedClip);
        }

        else if (other.gameObject.CompareTag("House"))
        {
            var triggeredHouse = other.gameObject.GetComponent<House>();
            if (!triggeredHouse) return;
            for (var i = collectedItems.Count - 1; i >= 0; i--)
            {
                var gift = collectedItems[i];
                if (gift.AssignedHouse == triggeredHouse)
                {
                    onItemDelivered.Invoke();
                    collectedItems.Remove(gift);
                    triggeredHouse.RemoveItem(gift);
                    Destroy(gift.gameObject);
                    speed += decreaseSpeed;
                    GameManager.Instance.UpdateItems();
                    AudioManager.Instance.PlayAudio(AudioManager.Instance.itemDeliveredClip);
                    GameStats.Instance.UpdateGameStatsUi();
                }
            }
        }

        else if (other.gameObject.CompareTag("Enemy"))
        {
            onBeingCaptured.Invoke(this);
            Destroy(other.gameObject);
            AudioManager.Instance.PlayAudio(AudioManager.Instance.unitCapturedClip);
            GameStats.Instance.UpdateGameStatsUi();
            foreach (var line in _lineRendSpawned)
            {
                line.SetActive(false);
            }

            foreach (var rig in _ringSpawned)
            {
                rig.SetActive(false);
            }

            this.enabled = false;
            Destroy(gameObject);
        }
    }

    private void AddItem(Item item)
    {
        item.gameObject.transform.parent = collectedGiftParent;
        item.transform.localRotation = Quaternion.identity;
        item.transform.localPosition = _lastCollectedItemPos + Vector3.up * 0.01f;
        _lastCollectedItemPos = item.transform.localPosition;
    }

    private void Update()
    {
        if (_moving)
            Move();
    }

    private void LateUpdate()
    {
        selection.transform.rotation = _defaultRingRotation;
    }

    public void SetTargetPosition(Vector3 pos)
    {
        destinations.Clear();
        foreach (var line in _lineRendSpawned)
        {
            line.SetActive(false);
        }

        foreach (var ring in _ringSpawned)
        {
            ring.SetActive(false);
        }

        _targetPosition = pos;
        SetPathLineRenderer(transform.position, _targetPosition);
        _moving = true;
    }

    private void SetPathLineRenderer(Vector3 startPos, Vector3 endPos)
    {
        pathLineRenderer.SetPosition(0, startPos);
        pathLineRenderer.SetPosition(1, endPos);
        if (_currentRingTargetSpawned)
            _currentRingTargetSpawned.SetActive(false);

        var ringSpawnPos = _targetPosition;
        ringSpawnPos.y = 0.5f;
        _currentRingTargetSpawned =
            ObjectPooler.Instance.SpawnFromPool("RingPath", ringSpawnPos, Quaternion.Euler(RingRotation));
        _ringSpawned.Add(_currentRingTargetSpawned);
        var axisScale = selection.transform.localScale.x * selection.transform.parent.localScale.x;
        _currentRingTargetSpawned.transform.localScale = new Vector3(axisScale, axisScale, axisScale);
    }

    private void Move()
    {
        RotateTowardsTarget();

        transform.position = Vector3.MoveTowards(transform.position,
            _targetPosition,
            speed * Time.deltaTime);

        if (Vector3.Distance(transform.position, _targetPosition) < _accuracyWp)
        {
            destinations.Remove(_targetPosition);

            if (destinations.Count > 0)
            {
                _targetPosition = destinations[0];
            }

            else
            {
                _moving = false;
            }

            transform.eulerAngles = new Vector3(0, transform.rotation.eulerAngles.y, 0);

            if (pathLineRenderer.GetPosition(1) != Vector3.zero)
            {
                pathLineRenderer.SetPosition(0, Vector3.zero);
                pathLineRenderer.SetPosition(1, Vector3.zero);
                if (_lineRendSpawned.Count != 0)
                    _lineRendSpawned[0].GetComponent<LineRenderer>().material = pathLineRenderer.material;
            }

            else
            {
                _lineRendSpawned[0].SetActive(false);
                _lineRendSpawned.Remove(_lineRendSpawned[0]);
                if (_lineRendSpawned.Count != 0)
                    _lineRendSpawned[0].GetComponent<LineRenderer>().material = pathLineRenderer.material;
            }

            _currentRingTargetSpawned.SetActive(false);
            _ringSpawned[0].SetActive(false);
            _ringSpawned.Remove(_ringSpawned[0]);
        }
    }

    public void OnMouseEnter()
    {
        GameManager.Instance.UpdateCursor(false);
    }

    public void OnMouseExit()
    {
        GameManager.Instance.UpdateCursor(true);
    }

    public GameObject Selection
    {
        get { return selection; }
    }

    public void AddWaypoint(Vector3 destination)
    {
        var pathLine = ObjectPooler.Instance.SpawnFromPool("PathLine", _targetPosition, Quaternion.identity);
        pathLine.GetComponent<LineRenderer>().material = RTSController.Instance.WaypointPathMaterial;
        _lineRendSpawned.Add(pathLine);
        var lineRenderer = pathLine.GetComponent<LineRenderer>();
        Vector3 startPosition;

        if (destinations.Count == 0)
            startPosition = _targetPosition;
        else
            startPosition = destinations[destinations.Count - 1];

        lineRenderer.SetPosition(0, startPosition);
        lineRenderer.SetPosition(1, destination);
        var newDestination = destination;
        newDestination.y = 0.5f;
        var ring = ObjectPooler.Instance.SpawnFromPool("RingPath", newDestination, Quaternion.Euler(RingRotation));
        var axisScale = selection.transform.localScale.x * selection.transform.parent.localScale.x;
        ring.transform.localScale = new Vector3(axisScale, axisScale, axisScale);
        destinations.Add(destination);
        _ringSpawned.Add(ring);
    }

    public void ManageSelection()
    {
        selection.SetActive(!selection.activeInHierarchy);

        if (collectedItems.Count > 0)
        {
            foreach (var item in collectedItems)
                item.gameObject.SetActive(selection.activeInHierarchy);
        }
    }

    private void RotateTowardsTarget()
    {
        _direction = _targetPosition - transform.position;
        this.transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_direction),
            rotSpeed * Time.deltaTime);
        this.transform.Translate(0, 0, Time.deltaTime);
    }
}