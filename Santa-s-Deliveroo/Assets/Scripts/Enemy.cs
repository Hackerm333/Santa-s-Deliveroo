using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] private float movementSpeed;
    [SerializeField] private float rotSpeed = 5f;
    [SerializeField] private float accuracyWp = 0.2f;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private float angleDetection = 10f;
    [SerializeField] private Transform[] moveSpots;

    private int _randomSpots;
    private Vector3 _direction;
    private Vector3 _destination;
    private UnitRTS _chasedTarget;

    private float sqrDetectionRange;

    public enum EnemyState
    {
        Patrol,
        Chase
    }

    private EnemyState _currentEnemyState = EnemyState.Patrol;

    private void Start()
    {
        _randomSpots = Random.Range(0, moveSpots.Length);
        _destination = moveSpots[_randomSpots].position;
        sqrDetectionRange = detectionRange * detectionRange;
    }

    private void Update()
    {
        if (GameManager.Instance.currentGameState == GameManager.GameState.EndGame)
            return;

        if (_currentEnemyState == EnemyState.Patrol)
        {
            foreach (var unitRts in RTSController.Instance.AvailableUnits)
            {
                if(Vector3.SqrMagnitude(transform.position - unitRts.transform.position) <= sqrDetectionRange)
                {
                    var targetDir = unitRts.transform.position - transform.position;
                    var angle = Vector3.Angle(targetDir, transform.forward);

                    if (angle < angleDetection)
                    {
                        _chasedTarget = unitRts;
                        _currentEnemyState = EnemyState.Chase;
                        transform.LookAt(_chasedTarget.transform);
                        _destination = _chasedTarget.transform.position;
                        break;
                    }
                }
            }

            if (Vector3.Distance(transform.position, moveSpots[_randomSpots].position) < accuracyWp)
            {
                var currentRandomSpot = _randomSpots;
                while (currentRandomSpot == _randomSpots)
                    _randomSpots = Random.Range(0, moveSpots.Length);
            }

            else
                RotateTowardsTarget();

            _destination = moveSpots[_randomSpots].position;
        }

        else if (_currentEnemyState == EnemyState.Chase && _chasedTarget)
        {
            if (Vector3.Distance(transform.position, _chasedTarget.transform.position) > detectionRange)
            {
                _currentEnemyState = EnemyState.Patrol;
                _chasedTarget = null;
                return;
            }

            _destination = _chasedTarget.transform.position;
        }

        Move(_destination);
    }

    private void Move(Vector3 destination)
    {
        transform.position = Vector3.MoveTowards(transform.position, destination, movementSpeed * Time.deltaTime);
    }

    private void RotateTowardsTarget()
    {
        _direction = moveSpots[_randomSpots].position - transform.position;
        this.transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(_direction),
            rotSpeed * Time.deltaTime);
        this.transform.Translate(0, 0, Time.deltaTime);
    }
}