using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Base : MonoBehaviour
{
    [SerializeField] GameObject characterPrefab;
    List<PlayerData> units = new List<PlayerData>();

    void Start()
    {
        List<string> presetNames = new List<string> { "Oscar", "Bajs", "PeePee", "Mordor" };
        List<Weapons.WeaponType> weapons = new List<Weapons.WeaponType> { Weapons.WeaponType.Pistol, Weapons.WeaponType.Pistol, Weapons.WeaponType.Sniper, Weapons.WeaponType.Pistol };

        for (int i = 0; i < 4; i++)
        {
            PlayerData playerData = new PlayerData();
            playerData.charName = presetNames[i];
            playerData.weapon = weapons[i];
            units.Add(playerData);
        }

        PlayerTransfer.SaveUnits(units);
    }

}
