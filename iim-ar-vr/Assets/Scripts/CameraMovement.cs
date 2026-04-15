using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class CameraMovement : MonoBehaviour
{
    public Transform cameraTransform; // à assigner : la Camera enfant
    public float moveSpeed = 5f;
    public float mouseSensitivity = 150f;
    public float jumpForce = 5f;
    public float gravity = -9.81f;

    private CharacterController controller;
    private float rotationX = 0f;
    private float verticalVelocity = 0f;

    // Flag global : si true, la souris ne fait plus bouger la caméra (ex. menu UI ouvert)
    public static bool LookLocked = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        controller = GetComponent<CharacterController>();
        if (cameraTransform == null && Camera.main != null)
            cameraTransform = Camera.main.transform;
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
        if (keyboard == null || mouse == null) return;

        // --- ZQSD (AZERTY physique) ---
        float moveX = 0f;
        float moveZ = 0f;
        if (keyboard[Key.Z].isPressed) moveZ = 1f;
        if (keyboard[Key.S].isPressed) moveZ = -1f;
        if (keyboard[Key.Q].isPressed) moveX = -1f;
        if (keyboard[Key.D].isPressed) moveX = 1f;

        Vector3 move = (transform.right * moveX + transform.forward * moveZ) * moveSpeed;

        // --- Gravité + saut ---
        if (controller.isGrounded)
        {
            verticalVelocity = -1f;
            if (keyboard[Key.Space].wasPressedThisFrame)
                verticalVelocity = jumpForce;
        }
        else
        {
            verticalVelocity += gravity * Time.deltaTime;
        }

        move.y = verticalVelocity;
        controller.Move(move * Time.deltaTime);

        // --- Rotation souris ---
        if (LookLocked) return;

        Vector2 mouseDelta = mouse.delta.ReadValue();
        float mouseX = mouseDelta.x * mouseSensitivity * 0.01f;
        float mouseY = mouseDelta.y * mouseSensitivity * 0.01f;

        // Yaw sur le Player (capsule reste verticale)
        transform.Rotate(Vector3.up * mouseX, Space.World);

        // Pitch uniquement sur la Camera enfant
        if (cameraTransform != null)
        {
            rotationX -= mouseY;
            rotationX = Mathf.Clamp(rotationX, -80f, 80f);
            cameraTransform.localRotation = Quaternion.Euler(rotationX, 0f, 0f);
        }
    }
}
