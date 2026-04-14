using UnityEngine;
using UnityEngine.InputSystem;

public class DoorHandler : MonoBehaviour
{
    [SerializeField] private InputAction action;
    [SerializeField] private float slideDistance = 2f;
    [SerializeField] private float slideSpeed = 3f;

    private bool isOpen = false;
    private Vector3 closedPosition;
    private Vector3 openPosition;

    void Start()
    {
        closedPosition = transform.localPosition;
        openPosition = closedPosition + new Vector3(slideDistance, 0, 0);
    }

    void Update()
    {
        if (action.triggered)
        {
            isOpen = !isOpen;
            Debug.Log(isOpen ? "La porte s'ouvre" : "La porte se ferme");
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
