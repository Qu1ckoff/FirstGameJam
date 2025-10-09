using UnityEngine;

public class Eatable : MonoBehaviour
{
    [Header("Eatable Settings")]
    public string trashName = "Fish Bones";
    public GameObject resultFoodPrefab;  // во что превратится
    public float eatTime = 2f;            // время, сколько нужно зажать E
    public float digestionTime = 5f;      // сколько переваривается
    public float stomachLoad = 10f;       // сколько места занимает в желудке

    public bool canBeEaten = true;
}
