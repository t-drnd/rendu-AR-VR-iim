using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class LightHandler : MonoBehaviour
{
    [SerializeField] private InputAction action;
    [SerializeField] private List<GameObject> ObjectsLights;

    private bool isLightOn = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (action.triggered)
        {
            isLightOn = !isLightOn;
            Debug.Log(isLightOn ? "La lumière est allumée donc il ne fais pas tout sombre" : "La lumière est éteinte donc il fait tout sombre (bah non parce que la lumière est dans le mur)");
            foreach (var light in ObjectsLights)
            {
                light.SetActive(isLightOn);
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
