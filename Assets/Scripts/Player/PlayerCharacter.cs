using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerCharacter : MonoBehaviour
{
    public BaseAlgo baseAlgo;

    [Header("Movement")]
    public float moveSpeed = 6f;
    public float gravity = -20f;
    public float jumpHeight = 1.5f;

    [Header("Camera")]
    public Transform cameraPivot;

    [Range(0.1f, 10f)]
    public float mouseSensitivity = 2f;

    [Range(-89f, 0f)]
    public float minLookAngle = -80f;

    [Range(0f, 89f)]
    public float maxLookAngle = 80f;

    [Header("Control")]
    public bool canControl = false;

    [Header("Crosshair")]
    public bool showCrosshair = true;

    [Range(1f, 6f)]
    public float crosshairThickness = 2f;

    [Range(2f, 24f)]
    public float crosshairLength = 8f;

    [Range(0f, 12f)]
    public float crosshairGap = 3f;

    public Color crosshairColor = Color.white;

    [Header("Spawn")]
    public int spawnX = 0;
    public int spawnZ = 0;
    public float spawnYOffset = 3f;

    private CharacterController controller;
    private Vector3 velocity;
    private float cameraPitch;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        if (cameraPivot == null)
        {
            Camera childCamera = GetComponentInChildren<Camera>();

            if (childCamera != null)
                cameraPivot = childCamera.transform;
        }
    }

    private void Start()
    {
        ReleaseCursor();

        if (cameraPivot != null)
            cameraPitch = GetSignedAngle(cameraPivot.localEulerAngles.x);
    }

    private void Update()
    {
        if (!canControl)
            return;

        HandleCursor();
        Look();
        Move();
    }

    public void EnableControl()
    {
        canControl = true;
        CaptureCursor();
    }

    public void DisableControl()
    {
        canControl = false;
        ReleaseCursor();
    }

    public void SpawnOnTerrain()
    {
        if (baseAlgo == null)
            return;

        int surfaceHeight = baseAlgo.GetSurfaceHeight(spawnX, spawnZ);

        transform.position = new Vector3(
            spawnX + 0.5f,
            surfaceHeight + spawnYOffset,
            spawnZ + 0.5f
        );
    }

    private void HandleCursor()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ReleaseCursor();
            return;
        }

        if (Cursor.lockState != CursorLockMode.Locked && Input.GetMouseButtonDown(0))
            CaptureCursor();
    }

    private void Look()
    {
        if (Cursor.lockState != CursorLockMode.Locked || cameraPivot == null)
            return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        transform.Rotate(Vector3.up * mouseX);

        cameraPitch -= mouseY;
        cameraPitch = Mathf.Clamp(cameraPitch, minLookAngle, maxLookAngle);

        cameraPivot.localRotation = Quaternion.Euler(cameraPitch, 0f, 0f);
    }

    private void Move()
    {
        bool isGrounded = controller.isGrounded;

        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        float inputX = Input.GetAxis("Horizontal");
        float inputZ = Input.GetAxis("Vertical");

        Vector3 move =
            transform.right * inputX +
            transform.forward * inputZ;

        controller.Move(move * moveSpeed * Time.deltaTime);

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
            velocity.y = Mathf.Sqrt(jumpHeight * -2f * gravity);

        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    private void OnGUI()
    {
        if (!showCrosshair || Cursor.lockState != CursorLockMode.Locked)
            return;

        float centerX = Screen.width * 0.5f;
        float centerY = Screen.height * 0.5f;

        Color originalColor = GUI.color;
        GUI.color = crosshairColor;

        DrawLine(new Rect(
            centerX - crosshairThickness * 0.5f,
            centerY - crosshairGap - crosshairLength,
            crosshairThickness,
            crosshairLength
        ));

        DrawLine(new Rect(
            centerX - crosshairThickness * 0.5f,
            centerY + crosshairGap,
            crosshairThickness,
            crosshairLength
        ));

        DrawLine(new Rect(
            centerX - crosshairGap - crosshairLength,
            centerY - crosshairThickness * 0.5f,
            crosshairLength,
            crosshairThickness
        ));

        DrawLine(new Rect(
            centerX + crosshairGap,
            centerY - crosshairThickness * 0.5f,
            crosshairLength,
            crosshairThickness
        ));

        GUI.color = originalColor;
    }

    private void DrawLine(Rect area)
    {
        GUI.DrawTexture(area, Texture2D.whiteTexture);
    }

    private void OnDisable()
    {
        ReleaseCursor();
    }

    private void OnApplicationFocus(bool hasFocus)
    {
        if (!hasFocus)
            ReleaseCursor();
    }

    public void CaptureCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ReleaseCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    private float GetSignedAngle(float angle)
    {
        return angle > 180f ? angle - 360f : angle;
    }
}