using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCityToken : MonoBehaviour
{
    private static readonly int PlayerPosShaderRef = Shader.PropertyToID("_PlayerPos");

    [SerializeField] private CameraController _cameraController;
    
    [SerializeField] 
    private InputActionAsset actions;
    private InputAction _moveAction;

    [SerializeField] private float moveSpeed = 10;
    [SerializeField] private float rotateSpeed = 10;

    private void Awake()
    {
        _moveAction = actions.FindActionMap("Player_Map").FindAction("Movement");
        _moveAction.Enable();
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // movement 
        var inputVector = _moveAction.ReadValue<Vector2>();
        if (inputVector.magnitude > 1) 
            inputVector = inputVector.normalized;
        var worldDir = new Vector3(inputVector.x, 0, inputVector.y);
        var cameraMoveDir = Quaternion.AngleAxis(_cameraController.CameraFacingAngle, Vector3.up) * worldDir;
        var movementThisFrame = cameraMoveDir * (Time.deltaTime * moveSpeed);
        transform.position += movementThisFrame;

        // token rotation
        if (movementThisFrame.magnitude > 0)
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(movementThisFrame),
                Time.deltaTime * rotateSpeed);
    }
    
    void LateUpdate()
    {
        SetGlobalShaderPosition();
    }

    public void SetGlobalShaderPosition()
    {
        Shader.SetGlobalVector(PlayerPosShaderRef, transform.position);
    }
}
