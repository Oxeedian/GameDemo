using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoundStart : MonoBehaviour
{
    void StartGame(List<Unit> playerUnits, List<Unit> zombieUnits)
    {
        MapController mapController = MapController.onRequestMapController.Invoke();
        mapController.RandomGenerateMap();


        foreach(PlayerUnit player in playerUnits)
        {

        }
        //Spawnea zombies

        // spawnea spelaren ihop
    }
}
