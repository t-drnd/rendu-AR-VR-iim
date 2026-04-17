using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.Interaction.Toolkit.Interactables;
using UnityEngine.XR.Interaction.Toolkit.Interactors;
using UnityEngine.XR.Interaction.Toolkit.UI;

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

    [Header("VR Menu")]
    [Tooltip("Action InputSystem qui ouvre le menu (ex: bouton Menu/Y d'une manette).")]
    [SerializeField] private InputActionProperty toggleMenuAction;
    [Tooltip("Distance devant la caméra où le menu est affiché en VR.")]
    [SerializeField] private float menuDistance = 1.5f;
    [Tooltip("Échelle du Canvas en WorldSpace (1 unité Canvas = 1 mm réel à 0.001).")]
    [SerializeField] private float menuScale = 0.002f;

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
        HookAllInteractorsForDebug();

        if (toggleMenuAction.action != null)
        {
            toggleMenuAction.action.Enable();
            toggleMenuAction.action.performed += OnToggleMenuAction;
        }
    }

    private void OnDestroy()
    {
        if (toggleMenuAction.action != null)
        {
            toggleMenuAction.action.performed -= OnToggleMenuAction;
        }
    }

    private void OnToggleMenuAction(InputAction.CallbackContext ctx)
    {
        Debug.Log("[ObjectSpawnManager] Bouton manette → menu toggle");
        SetMenuOpen(!menuOpen);
    }

    /// <summary>
    /// Debug : trouve tous les interactors XR dans la scène (y compris inactifs)
    /// et loggue leurs events. On re-scanne aussi au bout de 1s au cas où le rig
    /// ne serait pas encore prêt au Start.
    /// </summary>
    private void HookAllInteractorsForDebug()
    {
        ScanInteractors("Start");
        Invoke(nameof(ScanLater), 1f);
    }

    private void ScanLater() => ScanInteractors("T+1s");

    private readonly System.Collections.Generic.HashSet<int> hookedInteractors = new();

    private void ScanInteractors(string label)
    {
        var interactors = FindObjectsByType<XRBaseInteractor>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        Debug.Log($"[XR Debug/{label}] {interactors.Length} interactor(s) dans la scène (inactifs inclus).");
        foreach (var interactor in interactors)
        {
            int id = interactor.GetInstanceID();
            bool activeInHierarchy = interactor.isActiveAndEnabled;
            string parentName = interactor.transform.parent != null ? interactor.transform.parent.name : "(root)";
            string iname = $"[{parentName}] {interactor.gameObject.name} ({interactor.GetType().Name}) actif={activeInHierarchy}";
            Debug.Log($"[XR Debug/{label}] -> {iname}");

            if (hookedInteractors.Contains(id)) continue;
            hookedInteractors.Add(id);

            string capturedName = iname;
            interactor.selectEntered.AddListener(args =>
                Debug.Log($"[XR Debug] {capturedName} SELECT ENTERED sur {args.interactableObject}"));
            interactor.hoverEntered.AddListener(args =>
                Debug.Log($"[XR Debug] {capturedName} HOVER ENTERED sur {args.interactableObject}"));
        }
    }

    private void Update()
    {
        var keyboard = Keyboard.current;
        if (keyboard != null && keyboard[toggleKey].wasPressedThisFrame)
        {
            Debug.Log($"[ObjectSpawnManager] Touche {toggleKey} pressée → menu {(!menuOpen ? "ouvert" : "fermé")}");
            SetMenuOpen(!menuOpen);
        }

        // Si le menu est ouvert, on le maintient face à la caméra (billboard)
        if (menuOpen && canvas != null && targetCamera != null)
        {
            var camT = targetCamera.transform;
            canvas.transform.position = camT.position + camT.forward * menuDistance;
            canvas.transform.rotation = Quaternion.LookRotation(canvas.transform.position - camT.position);
        }

        // Debug : loggue si les selectInput des Near-Far Interactors reçoivent un signal
        PollNearFarSelectInputs();
    }

    private UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor[] cachedInputInteractors;
    private float nextPollTime;

    private bool dumpedConfig;

    private void PollNearFarSelectInputs()
    {
        if (Time.time < nextPollTime) return;
        nextPollTime = Time.time + 0.2f;

        if (cachedInputInteractors == null || cachedInputInteractors.Length == 0)
        {
            cachedInputInteractors = FindObjectsByType<UnityEngine.XR.Interaction.Toolkit.Interactors.XRBaseInputInteractor>(
                FindObjectsInactive.Include, FindObjectsSortMode.None);
        }

        // Dump la config des selectInput une seule fois, quand tout est actif
        if (!dumpedConfig && Time.time > 1.5f)
        {
            dumpedConfig = true;
            foreach (var i in cachedInputInteractors)
            {
                if (i == null) continue;
                var r = i.selectInput;
                if (r == null)
                {
                    Debug.LogWarning($"[XR Config] [{i.transform.parent?.name}] {i.name} selectInput = NULL");
                    continue;
                }
                var actionRef = r.inputActionReferencePerformed;
                string refName = actionRef != null && actionRef.action != null ? actionRef.action.name : "NULL";
                string enabled = actionRef != null && actionRef.action != null ? actionRef.action.enabled.ToString() : "?";
                string bindingsStr = "";
                if (actionRef != null && actionRef.action != null)
                {
                    foreach (var b in actionRef.action.bindings) bindingsStr += b.effectivePath + " | ";
                }
                Debug.Log($"[XR Config] [{i.transform.parent?.name}] {i.name} selectInput: mode={r.inputSourceMode} actionRef='{refName}' enabled={enabled} bindings=[{bindingsStr}]");
            }
        }

        foreach (var i in cachedInputInteractors)
        {
            if (i == null || !i.isActiveAndEnabled) continue;
            var reader = i.selectInput;
            if (reader == null) continue;
            bool performed = reader.ReadIsPerformed();
            float value = reader.ReadValue();
            if (performed || value > 0.01f)
            {
                Debug.Log($"[XR Input] [{i.transform.parent?.name}] {i.name} selectInput performed={performed} value={value:F2}");
            }
        }
    }

    private void SetMenuOpen(bool open)
    {
        menuOpen = open;
        if (panel != null) panel.SetActive(open);

        // Positionne tout de suite le canvas devant la caméra à l'ouverture
        if (open && canvas != null && targetCamera != null)
        {
            var camT = targetCamera.transform;
            canvas.transform.position = camT.position + camT.forward * menuDistance;
            canvas.transform.rotation = Quaternion.LookRotation(canvas.transform.position - camT.position);
        }

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
        // Canvas en WorldSpace pour la VR : placé devant la caméra à l'ouverture
        GameObject canvasGO = new GameObject("SpawnMenuCanvas");
        // Parent = null (dans la scène) pour que le canvas puisse se positionner librement
        // sans suivre ce GameObject. On le placera manuellement devant la caméra.
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvas.sortingOrder = 100;
        canvas.worldCamera = targetCamera;

        // En WorldSpace, CanvasScaler sert surtout à gérer la densité de pixels.
        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 2f;
        scaler.referencePixelsPerUnit = 100f;

        // Deux raycasters pour couvrir les deux modes :
        //  - GraphicRaycaster : clic souris en mode desktop / éditeur
        //  - TrackedDeviceGraphicRaycaster : rayons XR des manettes Quest
        canvasGO.AddComponent<GraphicRaycaster>();
        canvasGO.AddComponent<TrackedDeviceGraphicRaycaster>();

        // Taille de référence du canvas (en unités locales). On scale ensuite en mètres.
        var canvasRect = canvasGO.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(600, 400);
        canvasGO.transform.localScale = Vector3.one * menuScale;

        // EventSystem avec module compatible XR : sans XRUIInputModule, les Ray Interactors
        // ne propagent pas leurs clics vers l'UI.
        var existingES = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (existingES == null)
        {
            GameObject es = new GameObject("EventSystem");
            es.AddComponent<UnityEngine.EventSystems.EventSystem>();
            es.AddComponent<UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule>();
            Debug.Log("[ObjectSpawnManager] EventSystem créé avec XRUIInputModule.");
        }
        else if (existingES.GetComponent<UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule>() == null)
        {
            // On désactive l'ancien module (ex: InputSystemUIInputModule) pour éviter
            // le conflit, puis on ajoute le module XR.
            foreach (var m in existingES.GetComponents<UnityEngine.EventSystems.BaseInputModule>())
            {
                m.enabled = false;
                Debug.Log($"[ObjectSpawnManager] Ancien module UI désactivé : {m.GetType().Name}");
            }
            existingES.gameObject.AddComponent<UnityEngine.XR.Interaction.Toolkit.UI.XRUIInputModule>();
            Debug.Log("[ObjectSpawnManager] XRUIInputModule ajouté à l'EventSystem existant.");
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
            string capturedLabel = entry.label;
            button.onClick.AddListener(() =>
            {
                Debug.Log($"[ObjectSpawnManager] Bouton '{capturedLabel}' cliqué → SpawnInFront");
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

        // --- XR : rend l'objet attrapable par les manettes Meta Quest ---
        var grab = obj.GetComponent<XRGrabInteractable>();
        if (grab == null) grab = obj.AddComponent<XRGrabInteractable>();

        var rbForGrab = obj.GetComponent<Rigidbody>();
        if (rbForGrab == null)
        {
            rbForGrab = obj.AddComponent<Rigidbody>();
            rbForGrab.mass = defaultMass;
        }
        grab.movementType = XRBaseInteractable.MovementType.Instantaneous;
        grab.throwOnDetach = true;
        grab.useDynamicAttach = true;

        // Logs debug : on saura si la manette entre/sort du grab
        string objName = obj.name;
        grab.selectEntered.AddListener(args => Debug.Log($"[XR Grab] >>> GRAB START sur '{objName}' par {args.interactorObject}"));
        grab.selectExited.AddListener(args => Debug.Log($"[XR Grab] <<< GRAB END sur '{objName}'"));
        grab.hoverEntered.AddListener(args => Debug.Log($"[XR Grab] (hover) rayon pointe sur '{objName}'"));

        // Supprimer l'objet tenu quand on presse le trigger (activate) — il faut le tenir d'abord avec le grip
        GameObject objRef = obj;
        grab.activated.AddListener(args =>
        {
            Debug.Log($"[XR Grab] Suppression de '{objName}' (trigger pressé en grab)");
            Destroy(objRef);
        });

        Debug.Log($"[ObjectSpawnManager] Spawn '{obj.name}' avec XRGrabInteractable — attrapable par les manettes.");
    }
}
