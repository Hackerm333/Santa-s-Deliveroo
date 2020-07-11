using UnityEngine;

/*
 This class is used to move a camera freely in a 3D space.
 
    CONTROLS:
             WASD: movement
             Q/E: up and down movement
             SHIFT/CTRL: move faster or slower
             END: toggle cursor locking
     
*/
public class FlyCam : MonoBehaviour
{
    [SerializeField] private float cameraSensitivity = 90;
    [SerializeField] private float climbSpeed = 4;
    [SerializeField] private float normalMoveSpeed = 10;
    [SerializeField] private float slowMoveFactor = 0.25f;
    [SerializeField] private float fastMoveFactor = 3;

    private float _rotationX;
    private float _rotationY;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void Update()
    {
        if (GameManager.Instance.currentGameState == GameManager.GameState.EndGame)
            return;
        
        _rotationX += Input.GetAxis("Mouse X") * cameraSensitivity * Time.deltaTime;
        _rotationY += Input.GetAxis("Mouse Y") * cameraSensitivity * Time.deltaTime;
        _rotationY = Mathf.Clamp(_rotationY, -90, 90);

        var localRotation = Quaternion.AngleAxis(_rotationX, Vector3.up);
        localRotation *= Quaternion.AngleAxis(_rotationY, Vector3.left);
        transform.localRotation = localRotation;

        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
        {
            var position = transform.position;
            position += transform.forward * (normalMoveSpeed * fastMoveFactor * Input.GetAxis("Vertical") * Time.deltaTime);
            position += transform.right * (normalMoveSpeed * fastMoveFactor * Input.GetAxis("Horizontal") * Time.deltaTime);
            transform.position = position;
        }
        else if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            var position = transform.position;
            position += transform.forward * (normalMoveSpeed * slowMoveFactor * Input.GetAxis("Vertical") * Time.deltaTime);
            position += transform.right * (normalMoveSpeed * slowMoveFactor * Input.GetAxis("Horizontal") * Time.deltaTime);
            transform.position = position;
        }
        else
        {
            var position = transform.position;
            position += transform.forward * (normalMoveSpeed * Input.GetAxis("Vertical") * Time.deltaTime);
            position += transform.right * (normalMoveSpeed * Input.GetAxis("Horizontal") * Time.deltaTime);
            transform.position = position;
        }


        if (Input.GetKey(KeyCode.E))
        {
            var transform1 = transform;
            transform1.position += transform1.up * (climbSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            var transform1 = transform;
            transform1.position -= transform1.up * (climbSpeed * Time.deltaTime);
        }

        #if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.End))
        {
            Cursor.lockState = (Cursor.lockState == CursorLockMode.None) ? CursorLockMode.Locked : CursorLockMode.None;
        }
        #endif
    }
}