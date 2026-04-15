using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class CameraMovement : MonoBehaviour
{
    public float moveSpeed = 5f;
    public float mouseSensitivity = 150f;
    public float jumpForce = 0.3f;

    private float rotationX = 0f;
    private float fixedY;
    private float verticalVelocity = 0f;
    private bool isJumping = false;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        fixedY = transform.position.y; // Mémorise la hauteur de départ
    }

    void Update()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;

        if (keyboard == null || mouse == null) return;

        // --- Déplacement ZQSD (layout physique AZERTY) ---
        float moveX = 0f;
        float moveZ = 0f;

        if (keyboard[Key.Z].isPressed) moveZ = 1f;   // Position physique du Z azerty
        if (keyboard[Key.S].isPressed) moveZ = -1f;
        if (keyboard[Key.Q].isPressed) moveX = -1f;  // Position physique du Q azerty
        if (keyboard[Key.D].isPressed) moveX = 1f;

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // --- Saut ---
        if (keyboard[Key.Space].wasPressedThisFrame && !isJumping)
        {
            verticalVelocity = jumpForce;
            isJumping = true;
        }

        if (isJumping)
        {
            verticalVelocity -= 0.01f; // Gravité simulée
            fixedY += verticalVelocity;

            if (fixedY <= transform.position.y - 0.01f || verticalVelocity < -jumpForce)
            {
                fixedY = transform.position.y; // Retour à la hauteur d'origine
                isJumping = false;
                verticalVelocity = 0f;
            }
        }

        // Applique le mouvement en verrouillant Y sauf pendant le saut
        transform.position = new Vector3(
            transform.position.x + move.x * moveSpeed * Time.deltaTime,
            isJumping ? fixedY : transform.position.y,
            transform.position.z + move.z * moveSpeed * Time.deltaTime
        );

        // --- Rotation souris ---
        Vector2 mouseDelta = mouse.delta.ReadValue();
        float mouseX = mouseDelta.x * mouseSensitivity * 0.01f;
        float mouseY = mouseDelta.y * mouseSensitivity * 0.01f;

        rotationX -= mouseY;
        rotationX = Mathf.Clamp(rotationX, -80f, 80f);

        transform.Rotate(Vector3.up * mouseX, Space.World);
        transform.localRotation = Quaternion.Euler(rotationX, transform.localEulerAngles.y, 0f);
    }
}