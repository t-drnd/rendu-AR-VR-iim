using UnityEngine;
using System.Collections.Generic;

public class DestroyHandler : MonoBehaviour
{
    [SerializeField] private List<GameObject> objectsToDestroy;
    [SerializeField] private Transform player;

    private Vector3 playerSpawnPosition;

    void Start()
    {
        playerSpawnPosition = player.position;
    }

    void Update()
    {
        for (int i = objectsToDestroy.Count - 1; i >= 0; i--)
        {
            if (objectsToDestroy[i] != null && objectsToDestroy[i].transform.position.y < -10f)
            {
                Debug.Log("Objet detruit : " + objectsToDestroy[i].name);
                Destroy(objectsToDestroy[i]);
                objectsToDestroy.RemoveAt(i);
            }
        }

        if (player.position.y < -10f)
        {
            Debug.Log("Le joueur est tombe, retour au spawn");
            player.position = playerSpawnPosition;
        }
    }
}
