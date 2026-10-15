using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndlessLevelHandler : MonoBehaviour
{
    [SerializeField]
    GameObject[] sectionPrefabs;
    GameObject[] sectionsPool = new GameObject[20];
    GameObject[] section = new GameObject[10];
    Transform playerCarTransform;
    WaitForSeconds waitFor100ms = new WaitForSeconds(0.1f);

    const float sectionLength = 26;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerCarTransform = GameObject.FindGameObjectWithTag("Player").transform;

        int prefabIndex = 0;

        for (int i = 0; i < sectionsPool.Length; i++)
        {
            sectionsPool[i] = Instantiate(sectionPrefabs[prefabIndex]);
            sectionsPool[i].SetActive(false);

            prefabIndex++;

            if (prefabIndex > sectionPrefabs.Length - 1)
                prefabIndex = 0;
        }

        for (int i = 0; i < section.Length; i++)
        {
            GameObject randomsection = GetRandomSectionFromPool(); 

            randomsection.transform.position = new Vector3(sectionsPool[i].transform.position.x, -10 , i * sectionLength);
            randomsection.SetActive(true);

            section[i] = randomsection;
        }

        StartCoroutine(UpdateLessOften());
    }

    IEnumerator UpdateLessOften()
    {
        while (true)
        {
            UpdateSectionPositions();
            yield return waitFor100ms;

        }
    }

    void UpdateSectionPositions()
    {
        for (int i = 0; i < section.Length; i++)
        {
            if (section[i].transform.position.z - playerCarTransform.position.z < -sectionLength)
            {
                Vector3 lastSectionPosition = section[i].transform.position;
                section[i].SetActive(false);

                section[i] = GetRandomSectionFromPool();

                section[i].transform.position = new Vector3(lastSectionPosition.x, -10, lastSectionPosition.z + sectionLength * section.Length);
                section[i].SetActive(true);
            }
        }
    }

    GameObject GetRandomSectionFromPool()
    {
        int randomIndex = Random.Range(0, sectionsPool.Length);

        bool isNewSectionFound = false;

        while (!isNewSectionFound)
        {
            if (!sectionsPool[randomIndex].activeInHierarchy)
                isNewSectionFound = true;
            else
            {
                randomIndex++;

                if (randomIndex > sectionsPool.Length - 1)
                    randomIndex = 0;
            } 
        }

        return sectionsPool[randomIndex];
    }
}
