using UnityEngine;
using UnityEngine.InputSystem;

public class FireHandler : MonoBehaviour
{
    [SerializeField] private InputAction action;
    [SerializeField] private Transform player;
    [SerializeField] private GameObject fire;
    [SerializeField] private float interactionDistance = 2f;

    private bool isActive = false;

    
    void Start()
    {
        fire.SetActive(isActive);
    }


    void Update()
    {
        if (action.triggered)
        {
            Debug.Log("Action triggered");
            if (Vector3.Distance(player.position, transform.position) <= interactionDistance)
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
