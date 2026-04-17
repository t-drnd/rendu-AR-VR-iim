using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Appuyez sur 1, 2 ou 3 pour faire apparaître un objet au point de spawn.
/// Un seul GameObject dans la scène doit porter ce script (pas sur les prefabs à cloner).
/// </summary>
[DisallowMultipleComponent]
public class ObjectSpawner : MonoBehaviour
{
    /// <summary>Seul le premier spawner de la scène réagit aux touches (évite les doublons si plusieurs scripts).</summary>
    private static ObjectSpawner instanceActive;

    [Header("Prefabs à placer dans l'inspecteur")]
    [SerializeField] private GameObject prefabTable;
    [SerializeField] private GameObject prefabChaise;
    [SerializeField] private GameObject prefabDecoration;

    [Header("Où les objets apparaissent")]
    [SerializeField] private Transform pointDeSpawn;

    private readonly List<GameObject> spawnedObjects = new List<GameObject>();

    private void Awake()
    {
        if (instanceActive != null && instanceActive != this)
        {
            Debug.LogWarning(
                "Plusieurs 'ObjectSpawner' dans la scène : seul le premier reste actif. " +
                "Supprime les doublons ou garde un seul GameObject avec ce script.",
                this);
            enabled = false;
            return;
        }

        instanceActive = this;
    }

    private void OnDestroy()
    {
        if (instanceActive == this)
            instanceActive = null;
    }

    private void Start()
    {
        // Si aucun point n'est défini, on utilise la position de cet objet
        if (pointDeSpawn == null)
        {
            pointDeSpawn = transform;
        }

    }

    private void Update()
    {
        // Nouveau Input System (compatible avec Player Settings > Active Input Handling = Input System Package)
        Keyboard clavier = Keyboard.current;
        if (clavier == null)
            return;

        if (clavier.digit1Key.wasPressedThisFrame)
            Spawn(prefabTable);
        else if (clavier.digit2Key.wasPressedThisFrame)
            Spawn(prefabChaise);
        else if (clavier.digit3Key.wasPressedThisFrame)
            Spawn(prefabDecoration);
        else if (clavier.cKey.wasPressedThisFrame)
            ResetScene();
    }

    private void Spawn(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Prefab manquant : assignez-le dans l'inspecteur.");
            return;
        }

        GameObject obj = Instantiate(prefab, pointDeSpawn.position, pointDeSpawn.rotation);
        // Même tag que l'autre spawner : le DestroyHandler nettoie automatiquement
        // ces objets quand ils tombent sous killY.
        if (obj.GetComponent<SpawnedObjectTag>() == null)
            obj.AddComponent<SpawnedObjectTag>();
        spawnedObjects.Add(obj);
    }

    /// <summary>
    /// Détruit tous les objets taggés SpawnedObjectTag (touches 1/2/3 + menu Tab),
    /// SAUF le player et ses enfants (caméra, rig XR) — pour éviter de perdre
    /// le rendu si un tag traîne par erreur sur le rig.
    /// </summary>
    public void ResetScene()
    {
        var players = Object.FindObjectsByType<CharacterController>(FindObjectsSortMode.None);
        var tagged = Object.FindObjectsByType<SpawnedObjectTag>(FindObjectsSortMode.None);
        int count = 0;
        for (int i = 0; i < tagged.Length; i++)
        {
            if (tagged[i] == null) continue;
            if (IsUnderAnyPlayer(tagged[i].transform, players)) continue;
            Destroy(tagged[i].gameObject);
            count++;
        }
        spawnedObjects.Clear();
        Debug.Log($"Scène reset : {count} objet(s) spawné(s) supprimé(s).");
    }

    private static bool IsUnderAnyPlayer(Transform t, CharacterController[] players)
    {
        for (int i = 0; i < players.Length; i++)
        {
            var root = players[i].transform;
            if (t == root || t.IsChildOf(root)) return true;
        }
        return false;
    }
}
