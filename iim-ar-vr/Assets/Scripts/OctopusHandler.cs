using UnityEngine;
using UnityEngine.InputSystem;

public class OctopusHandler : MonoBehaviour
{
    [SerializeField] private InputAction action;
    [SerializeField] private Transform player;
    [SerializeField] private float interactionDistance = 2f;
    [SerializeField] private float scaleSpeed = 5f;
    [SerializeField] private float minScaleZ = 0.2f;

    private bool isSmall = false;
    private Vector3 normalScale;
    private Vector3 smallScale;

    void Start()
    {
        normalScale = transform.localScale;
        smallScale = new Vector3(normalScale.x, normalScale.y, minScaleZ);
    }

    void Update()
    {
        if (action.triggered)
        {
            Debug.Log("Action triggered");
            if (Vector3.Distance(player.position, transform.position) <= interactionDistance)
            {
                isSmall = !isSmall;
                Debug.Log(isSmall ? "Octopus shrink Z" : "Octopus back to normal");
            }
            else
            {
                Debug.Log("L'octopus est trop loin");
            }
        }

        Vector3 targetScale = isSmall ? smallScale : normalScale;
        transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scaleSpeed);
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
