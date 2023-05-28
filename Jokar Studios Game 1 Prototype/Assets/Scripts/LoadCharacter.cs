using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.IO;

public class LoadCharacter : MonoBehaviour
{
    public GameObject[] characterPrefabs;
    public Transform spawnPoint;
    // Start is called before the first frame update
    void Start()
    {
        if (PlayerPrefs.HasKey("selectedCharacter"))
            Debug.Log("key is present");
        int selectedCharacter = 0   ;
        Debug.Log($"checking for player prefs {selectedCharacter}");
        GameObject prefab = characterPrefabs[selectedCharacter];
        GameObject clone = Instantiate(prefab, spawnPoint.position, Quaternion.identity);


    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            ClearPlayerprefs();
            Debug.Log("player prefs cleared");
        }
    }

    public void ClearPlayerprefs()
    {
        PlayerPrefs.DeleteAll();
    }
}
