using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

/// <summary>
/// Ouvre un menu runtime (touche Tab) pour choisir un prefab à faire apparaître devant la caméra.
/// La liste des prefabs est renseignée dans l'inspecteur.
/// </summary>
public class ObjectSpawnManager : MonoBehaviour
{
    [System.Serializable]
    public class SpawnEntry
    {
        public string label = "Objet";
        public GameObject prefab;
    }

    [Header("Prefabs sélectionnables dans le menu")]
    [SerializeField] private List<SpawnEntry> prefabs = new List<SpawnEntry>();

    [Header("Spawn")]
    [SerializeField] private Camera targetCamera;
    [SerializeField] private float spawnDistance = 2.5f;
    [SerializeField] private Key toggleKey = Key.Tab;

    [Header("Physique")]
    [SerializeField] private bool enableGravity = true;
    [SerializeField] private float defaultMass = 1f;

    private Canvas canvas;
    private GameObject panel;
    private bool menuOpen;

    // On mémorise l'état précédent du curseur pour le restaurer à la fermeture.
    private CursorLockMode previousLockState;
    private bool previousCursorVisible;

    private void Start()
    {
        if (targetCamera == null) targetCamera = Camera.main;
        BuildUI();
        SetMenuOpen(false);
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard == null) return;

        if (keyboard[toggleKey].wasPressedThisFrame)
            SetMenuOpen(!menuOpen);
    }

    private void SetMenuOpen(bool open)
    {
        menuOpen = open;
        if (panel != null) panel.SetActive(open);

        if (open)
        {
            previousLockState = Cursor.lockState;
            previousCursorVisible = Cursor.visible;
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            CameraMovement.LookLocked = true;
        }
        else
        {
            Cursor.lockState = previousLockState == CursorLockMode.None ? CursorLockMode.Locked : previousLockState;
            Cursor.visible = previousCursorVisible && previousLockState != CursorLockMode.Locked ? previousCursorVisible : false;
            CameraMovement.LookLocked = false;
        }
    }

    private void BuildUI()
    {
        // Canvas
        GameObject canvasGO = new GameObject("SpawnMenuCanvas");
        canvasGO.transform.SetParent(transform, false);
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;
        canvasGO.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasGO.AddComponent<GraphicRaycaster>();

        // S'assure qu'un EventSystem existe
        if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.InputSystem.UI.InputSystemUIInputModule>();
        }

        // Panel background
        panel = new GameObject("Panel");
        panel.transform.SetParent(canvasGO.transform, false);
        var panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.5f, 0.5f);
        panelRect.anchorMax = new Vector2(0.5f, 0.5f);
        panelRect.pivot = new Vector2(0.5f, 0.5f);
        panelRect.sizeDelta = new Vector2(360, 60 + prefabs.Count * 60);
        var bg = panel.AddComponent<Image>();
        bg.color = new Color(0f, 0f, 0f, 0.75f);

        // Titre
        GameObject title = new GameObject("Title");
        title.transform.SetParent(panel.transform, false);
        var titleRect = title.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0, 1);
        titleRect.anchorMax = new Vector2(1, 1);
        titleRect.pivot = new Vector2(0.5f, 1f);
        titleRect.anchoredPosition = new Vector2(0, -10);
        titleRect.sizeDelta = new Vector2(0, 40);
        var titleText = title.AddComponent<Text>();
        titleText.text = "Choisir un objet à faire apparaître";
        titleText.alignment = TextAnchor.MiddleCenter;
        titleText.color = Color.white;
        titleText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        titleText.fontSize = 18;

        // Boutons
        for (int i = 0; i < prefabs.Count; i++)
        {
            var entry = prefabs[i];
            GameObject btn = new GameObject("Btn_" + entry.label);
            btn.transform.SetParent(panel.transform, false);
            var rect = btn.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 1f);
            rect.anchorMax = new Vector2(0.5f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(300, 48);
            rect.anchoredPosition = new Vector2(0, -60 - i * 56);

            var img = btn.AddComponent<Image>();
            img.color = new Color(0.2f, 0.5f, 0.9f, 1f);

            var button = btn.AddComponent<Button>();
            GameObject labelGO = new GameObject("Label");
            labelGO.transform.SetParent(btn.transform, false);
            var labelRect = labelGO.AddComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;
            var label = labelGO.AddComponent<Text>();
            label.text = entry.label;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = Color.white;
            label.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            label.fontSize = 18;

            GameObject captured = entry.prefab;
            button.onClick.AddListener(() =>
            {
                SpawnInFront(captured);
                SetMenuOpen(false);
            });
        }
    }

    private void SpawnInFront(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("[ObjectSpawnManager] Prefab manquant.");
            return;
        }
        if (targetCamera == null)
        {
            Debug.LogWarning("[ObjectSpawnManager] Caméra introuvable.");
            return;
        }

        Vector3 pos = targetCamera.transform.position + targetCamera.transform.forward * spawnDistance;
        // Rotation : l'objet regarde la caméra (axe Y seulement pour rester droit)
        Vector3 lookDir = targetCamera.transform.position - pos;
        lookDir.y = 0f;
        Quaternion rot = lookDir.sqrMagnitude > 0.001f
            ? Quaternion.LookRotation(lookDir)
            : Quaternion.identity;

        GameObject obj = Instantiate(prefab, pos, rot);

        // S'assure que l'objet est manipulable : ajoute un collider s'il n'y en a pas
        if (obj.GetComponentInChildren<Collider>() == null)
        {
            var bc = obj.AddComponent<BoxCollider>();
            Debug.Log("[ObjectSpawnManager] Aucun collider trouvé, BoxCollider ajouté par défaut.");
        }

        // Gravité : ajoute un Rigidbody si absent
        if (enableGravity)
        {
            var rb = obj.GetComponent<Rigidbody>();
            if (rb == null)
            {
                rb = obj.AddComponent<Rigidbody>();
                rb.mass = defaultMass;
                Debug.Log("[ObjectSpawnManager] Rigidbody ajouté (gravité activée).");
            }
            rb.useGravity = true;
            rb.isKinematic = false;
        }

        // Tag pour que le manipulateur reconnaisse l'objet
        obj.tag = "Untagged";
        // On ajoute un marqueur component pour identifier les objets spawnés
        obj.AddComponent<SpawnedObjectTag>();
    }
}
