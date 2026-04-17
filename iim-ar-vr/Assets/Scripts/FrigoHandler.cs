using UnityEngine;
using UnityEngine.InputSystem;

public class FrigoHandler : MonoBehaviour
{
    [SerializeField] private InputAction action;
    [SerializeField] private Transform player;
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private float rotateSpeed = 5f;

    private bool isOpen = false;
    private Quaternion closedRotation;
    private Quaternion openRotation;

    void Start()
    {
        closedRotation = transform.localRotation;
        openRotation = closedRotation * Quaternion.Euler(0f, 0f, -160f);
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
    }

    void Update()
    {
        bool inRange = Vector3.Distance(player.position, transform.position) <= interactionDistance;

        if (interactionPrompt != null) interactionPrompt.SetActive(inRange);

        if (action.triggered)
        {
            Debug.Log("Action triggered");
            if (inRange)
            {
                isOpen = !isOpen;
                Debug.Log(isOpen ? "Le frigo s'ouvre" : "Le frigo se ferme");
            }
            else
            {
                Debug.Log("Le frigo est trop loin");
            }
        }

        Quaternion targetRotation = isOpen ? openRotation : closedRotation;
        transform.localRotation = Quaternion.Lerp(transform.localRotation, targetRotation, Time.deltaTime * rotateSpeed);
    }

    public void OnEnable()
    {
        action.Enable();
    }

    public void OnDisable()
    {
        action.Disable();
    }
}
