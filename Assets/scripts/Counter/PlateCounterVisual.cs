using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
public class PlateCounterVisual : MonoBehaviour
{
    [SerializeField] PlatesCounter platesCounter;
    [SerializeField] Transform  HoldPoint;
    [SerializeField] Transform plateVisualPrefab;

    private List<GameObject> platePrefabList=new List<GameObject>();

    private void Awake()
    {
        platesCounter.OnPlateSpawned += Visual_OnPlatesSpawned;
        platesCounter.OnPlateCounterInteract+=Visual_OnPlateCounterInteract;
    }
    public void Visual_OnPlatesSpawned(object sender, EventArgs e)
    {
       Transform plateVisual =Instantiate(plateVisualPrefab,HoldPoint);
       plateVisual.localPosition=new Vector3 (0,0.1f*platePrefabList.Count,0);
       platePrefabList.Add(plateVisual.gameObject);
    }
    public void Visual_OnPlateCounterInteract(object sender, EventArgs e)
    {
        GameObject newPlate=platePrefabList[platePrefabList.Count-1];
        platePrefabList.Remove(newPlate);
        Destroy(newPlate);

    }
    void OnDestroy()
    {
        platesCounter.OnPlateSpawned-=Visual_OnPlatesSpawned;
        platesCounter.OnPlateCounterInteract-=Visual_OnPlateCounterInteract;
    }
}
