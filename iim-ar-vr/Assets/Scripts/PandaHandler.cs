using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class PandaHandler : MonoBehaviour
{
    [SerializeField] private InputAction action;
    [SerializeField] private Transform player;
    [SerializeField] private GameObject interactionPrompt;
    [SerializeField] private float interactionDistance = 2f;

    [SerializeField] private float jumpHeight = 2f;
    [SerializeField] private float saltoDuration = 1f;
    [SerializeField] private bool backflip = true;

    private bool isFlipping = false;

    void Start()
    {
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

        if (interactionPrompt != null) interactionPrompt.SetActive(inRange && !isFlipping);

        if (action.triggered)
        {
            Debug.Log("Action triggered");
            if (inRange && !isFlipping)
            {
                Debug.Log("Panda fait un salto !");
                StartCoroutine(DoSalto());
            }
            else if (!inRange)
            {
                Debug.Log("Le panda est trop loin");
            }
        }
    }

    private IEnumerator DoSalto()
    {
        isFlipping = true;

        Vector3 startPos = transform.position;
        Quaternion startRot = transform.rotation;
        float flipAngle = backflip ? -360f : 360f;

        float elapsed = 0f;
        while (elapsed < saltoDuration)
        {
            float t = elapsed / saltoDuration;

            float height = 4f * jumpHeight * t * (1f - t);
            transform.position = startPos + Vector3.up * height;

            float angle = Mathf.Lerp(0f, flipAngle, t);
            transform.rotation = startRot * Quaternion.Euler(angle, 0f, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = startPos;
        transform.rotation = startRot;
        isFlipping = false;
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
