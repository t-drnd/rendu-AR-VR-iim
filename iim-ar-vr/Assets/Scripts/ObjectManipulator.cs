using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Vise un objet spawné et maintiens E pour :
/// - déplacer (mouvement souris, drag & drop sur le plan de la caméra)
/// - tourner (molette de la souris ou touches R/F)
/// - supprimer (touche Suppr ou X)
/// </summary>
public class ObjectManipulator : MonoBehaviour
{
    [Header("Références")]
    [SerializeField] private Camera targetCamera;

    [Header("Paramètres")]
    [SerializeField] private float maxRayDistance = 15f;
    [SerializeField] private Key holdKey = Key.E;
    [SerializeField] private Key deleteKey = Key.X;
    [SerializeField] private Key rotateLeftKey = Key.R;
    [SerializeField] private Key rotateRightKey = Key.F;
    [SerializeField] private Key rotateUpKey = Key.T;
    [SerializeField] private Key rotateDownKey = Key.G;
    [SerializeField] private float rotateSpeedKeys = 90f;   // deg/sec
    [SerializeField] private float rotateSpeedWheel = 15f;  // deg par cran de molette
    [SerializeField] private float dragSmoothing = 20f;

    private Transform grabbed;
    private float grabDistance;
    private Rigidbody grabbedRb;
    private bool grabbedRbWasKinematic;

    private void Start()
    {
        if (targetCamera == null) targetCamera = Camera.main;
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        var mouse = Mouse.current;
        if (keyboard == null || mouse == null || targetCamera == null) return;

        bool holding = keyboard[holdKey].isPressed;

        // Relâchement de la touche : on lâche l'objet
        if (!holding)
        {
            ReleaseGrabbed();
            return;
        }

        // Si on ne tient pas encore d'objet, on fait un raycast pour en "saisir" un
        if (grabbed == null)
        {
            Ray ray = targetCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));
            if (Physics.Raycast(ray, out RaycastHit hit, maxRayDistance))
            {
                // On remonte au root qui porte le SpawnedObjectTag
                var tag = hit.collider.GetComponentInParent<SpawnedObjectTag>();
                if (tag != null)
                {
                    grabbed = tag.transform;
                    grabDistance = Vector3.Distance(targetCamera.transform.position, grabbed.position);

                    // Désactive la gravité le temps du grab
                    grabbedRb = grabbed.GetComponent<Rigidbody>();
                    if (grabbedRb != null)
                    {
                        grabbedRbWasKinematic = grabbedRb.isKinematic;
                        grabbedRb.linearVelocity = Vector3.zero;
                        grabbedRb.angularVelocity = Vector3.zero;
                        grabbedRb.isKinematic = true;
                    }
                    Debug.Log($"[ObjectManipulator] Objet saisi : {grabbed.name}");
                }
            }
            return;
        }

        // --- Supprimer ---
        if (keyboard[deleteKey].wasPressedThisFrame)
        {
            Debug.Log($"[ObjectManipulator] Suppression : {grabbed.name}");
            Destroy(grabbed.gameObject);
            grabbed = null;
            grabbedRb = null;
            return;
        }

        // --- Drag & drop : l'objet suit le centre de l'écran à distance constante ---
        Ray centerRay = targetCamera.ScreenPointToRay(new Vector3(Screen.width * 0.5f, Screen.height * 0.5f, 0));
        Vector3 targetPos = centerRay.origin + centerRay.direction * grabDistance;
        grabbed.position = Vector3.Lerp(grabbed.position, targetPos, Time.deltaTime * dragSmoothing);

        // --- Rotation : molette + touches R/F (axe Y monde) ---
        float wheel = mouse.scroll.ReadValue().y;
        if (Mathf.Abs(wheel) > 0.01f)
        {
            grabbed.Rotate(Vector3.up, Mathf.Sign(wheel) * rotateSpeedWheel, Space.World);
        }

        if (keyboard[rotateLeftKey].isPressed)
            grabbed.Rotate(Vector3.up, -rotateSpeedKeys * Time.deltaTime, Space.World);
        if (keyboard[rotateRightKey].isPressed)
            grabbed.Rotate(Vector3.up, rotateSpeedKeys * Time.deltaTime, Space.World);

        // Rotation axe horizontal (pitch) — aligné sur la droite de la caméra pour rester intuitif
        Vector3 pitchAxis = targetCamera.transform.right;
        if (keyboard[rotateUpKey].isPressed)
            grabbed.Rotate(pitchAxis, -rotateSpeedKeys * Time.deltaTime, Space.World);
        if (keyboard[rotateDownKey].isPressed)
            grabbed.Rotate(pitchAxis, rotateSpeedKeys * Time.deltaTime, Space.World);
    }

    private void ReleaseGrabbed()
    {
        if (grabbed != null && grabbedRb != null)
        {
            grabbedRb.isKinematic = grabbedRbWasKinematic;
            grabbedRb.linearVelocity = Vector3.zero;
            grabbedRb.angularVelocity = Vector3.zero;
        }
        grabbed = null;
        grabbedRb = null;
    }
}
