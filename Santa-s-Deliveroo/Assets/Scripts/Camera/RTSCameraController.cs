using UnityEngine;

[RequireComponent(typeof(Camera))]
public class RTSCameraController : MonoBehaviour
{
    // [SerializeField] private float movementSpeed = 20f;
    // [SerializeField] private float screenEdgeBorderSize = 10f;
    [SerializeField] private float scrollWheelSensitivity = 20f;
    //[SerializeField] private bool edgeScrolling = true;

    // [Header("Limit movement")] [SerializeField]
    // private Vector2 panLimit;

    [Header("ScrollWheel Zooming")] [SerializeField]
    private float minY = 20f;

    [SerializeField] private float maxY = 120f;
    [SerializeField] private Camera cam;

    private void Update()
    {
        if (GameManager.Instance.currentGameState == GameManager.GameState.EndGame)
            return;

        //var cameraFollowPosition = transform.position;
        
        // if (Input.GetKey(KeyCode.W) || Input.mousePosition.y > Screen.height - screenEdgeBorderSize && edgeScrolling)
        //     cameraFollowPosition.z += movementSpeed * Time.deltaTime;
        //
        // if (Input.GetKey(KeyCode.S) || Input.mousePosition.y < screenEdgeBorderSize && edgeScrolling)
        //     cameraFollowPosition.z -= movementSpeed * Time.deltaTime;
        //
        // if (Input.GetKey(KeyCode.D) || Input.mousePosition.x > Screen.width - screenEdgeBorderSize && edgeScrolling)
        //     cameraFollowPosition.x += movementSpeed * Time.deltaTime;
        //
        // if (Input.GetKey(KeyCode.A) || Input.mousePosition.x < screenEdgeBorderSize && edgeScrolling)
        //     cameraFollowPosition.x -= movementSpeed * Time.deltaTime;

        var scroll = Input.GetAxis("Mouse ScrollWheel");
        var orthographicSize = cam.orthographicSize;
        orthographicSize -= scroll * scrollWheelSensitivity * 100f * Time.deltaTime;
        cam.orthographicSize = orthographicSize;
        cam.orthographicSize = Mathf.Clamp(orthographicSize, minY, maxY);

        //cameraFollowPosition.x = Mathf.Clamp(cameraFollowPosition.x, -panLimit.x, panLimit.x);
        //cameraFollowPosition.z = Mathf.Clamp(cameraFollowPosition.z, -panLimit.y, panLimit.y);
        //transform.position = cameraFollowPosition;
    }
}