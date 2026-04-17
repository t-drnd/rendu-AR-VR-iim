using UnityEngine;
using UnityEngine.InputSystem;

public class FireHandler : MonoBehaviour
{
    [SerializeField] private InputAction action;
    [SerializeField] private Transform player;
    [SerializeField] private GameObject fire;
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private float interactionDistance = 2f;

    private bool isActive = false;


    void Start()
    {
        fire.SetActive(isActive);
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
                isActive = !isActive;
                fire.SetActive(isActive);
                Debug.Log(isActive ? "Fire activé" : "Fire désactivé");
            }
            else
            {
                Debug.Log("Le fire est trop loin");
            }
        }
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
