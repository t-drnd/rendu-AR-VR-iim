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
    }

    private void Spawn(GameObject prefab)
    {
        if (prefab == null)
        {
            Debug.LogWarning("Prefab manquant : assignez-le dans l'inspecteur.");
            return;
        }

        Instantiate(prefab, pointDeSpawn.position, pointDeSpawn.rotation);
    }
}
