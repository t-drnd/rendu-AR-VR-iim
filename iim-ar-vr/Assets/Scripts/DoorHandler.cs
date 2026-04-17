using UnityEngine;
using UnityEngine.InputSystem;

public class DoorHandler : MonoBehaviour
{
    [SerializeField] private InputAction action;
    [SerializeField] private Transform player;
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private float slideDistance = 2f;
    [SerializeField] private float slideSpeed = 3f;
    [SerializeField] private float interactionDistance = 2f;

    private bool isOpen = false;
    private Vector3 closedPosition;
    private Vector3 openPosition;

    void Start()
    {
        closedPosition = transform.localPosition;
        openPosition = closedPosition + new Vector3(slideDistance, 0, 0);
        if (interactionPrompt != null) interactionPrompt.SetActive(false);
    }

    void Update()
    {
        if (player == null)
        {
            if (interactionPrompt != null) interactionPrompt.SetActive(false);
            return;
        }

        bool inRange = Vector3.Distance(player.position, transform.position) <= interactionDistance;

        if (interactionPrompt != null) interactionPrompt.SetActive(inRange);

        if (action.triggered)
        {
            Debug.Log("Action triggered");
            if (inRange)
            {
                isOpen = !isOpen;
                Debug.Log(isOpen ? "La porte s'ouvre" : "La porte se ferme");
            }
            else
            {
                Debug.Log("La porte est trop loin");
            }
        }

        Vector3 targetPosition = isOpen ? openPosition : closedPosition;
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPosition, Time.deltaTime * slideSpeed);
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
