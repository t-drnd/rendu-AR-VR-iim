using UnityEngine;
using System.Collections.Generic;

public class DestroyHandler : MonoBehaviour
{
    [SerializeField] private List<GameObject> objectsToDestroy;
    [SerializeField] private Transform player;
    [SerializeField] private Vector3 playerSpawnPosition = new Vector3(-4.12f, 8.04f, -7.9f);
    [SerializeField] private float killY = -10f;

    private Quaternion playerSpawnRotation;
    private CharacterController playerController;

    void Start()
    {
        if (player != null)
        {
            playerSpawnRotation = player.rotation;
            playerController = player.GetComponent<CharacterController>();
        }
    }

    void Update()
    {
        // Nettoyage liste inspecteur — on refuse explicitement de détruire le player
        // si quelqu'un l'a ajouté par erreur.
        for (int i = objectsToDestroy.Count - 1; i >= 0; i--)
        {
            var go = objectsToDestroy[i];
            if (go == null)
            {
                objectsToDestroy.RemoveAt(i);
                continue;
            }
            if (IsPartOfPlayer(go.transform))
            {
                objectsToDestroy.RemoveAt(i);
                continue;
            }
            if (go.transform.position.y < killY)
            {
                Debug.Log("Objet detruit : " + go.name);
                Destroy(go);
                objectsToDestroy.RemoveAt(i);
            }
        }

        // Objets instanciés par ObjectSpawnManager : nettoyés automatiquement.
        // Le player est explicitement exclu — si un SpawnedObjectTag traîne dessus
        // par erreur, on ne le détruit jamais (sinon la caméra disparaît).
        var spawned = Object.FindObjectsByType<SpawnedObjectTag>(FindObjectsSortMode.None);
        for (int i = 0; i < spawned.Length; i++)
        {
            if (spawned[i] == null) continue;
            if (IsPartOfPlayer(spawned[i].transform)) continue;
            if (spawned[i].transform.position.y < killY)
            {
                Debug.Log("Objet spawn detruit : " + spawned[i].name);
                Destroy(spawned[i].gameObject);
            }
        }

        // Player : on téléporte, jamais on ne détruit.
        if (player != null && player.position.y < killY)
        {
            Debug.Log("Le joueur est tombe, retour au spawn");
            RespawnPlayer();
        }
    }

    // Vrai si `t` est le player lui-même ou un de ses enfants (caméra, rig XR, etc.)
    private bool IsPartOfPlayer(Transform t)
    {
        if (player == null || t == null) return false;
        return t == player || t.IsChildOf(player);
    }

    private void RespawnPlayer()
    {
        // Avec un CharacterController actif, écrire transform.position est ignoré :
        // on le désactive le temps du teleport.
        if (playerController != null)
        {
            playerController.enabled = false;
            player.SetPositionAndRotation(playerSpawnPosition, playerSpawnRotation);
            Physics.SyncTransforms();
            playerController.enabled = true;
        }
        else
        {
            player.SetPositionAndRotation(playerSpawnPosition, playerSpawnRotation);
        }

        var rb = player.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
