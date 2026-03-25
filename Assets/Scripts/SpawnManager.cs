
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public ObjectMaterial material = ObjectMaterial.METAL;
    public TMP_Text materialText;
    public TouchDragScaleManager dragManager;

    [Header("Object Pools")]
    public GameObject[] cubes, rectangles, triangles, spheres, wheels, conveyors, rockets, springs, pins, bombs;

    private void Awake()
    {
        if (dragManager == null)
        {
            dragManager = FindAnyObjectByType<TouchDragScaleManager>();
        }

        materialText.text = material.ToString();
    }

    public void SpawnObject(string objName)
    {
        ObjectType type;
        if (!Enum.TryParse(objName, out type))
        {
            return;
        }

        switch (type)
        {
            case ObjectType.WHEEL:
                dragManager.LoadObjectOnPointer(GetNextObject(wheels));
                break;
            case ObjectType.CONVEYOR:
                dragManager.LoadObjectOnPointer(GetNextObject(conveyors));
                break;
            case ObjectType.ROCKET:
                dragManager.LoadObjectOnPointer(GetNextObject(rockets));
                break;
            case ObjectType.SPRING:
                dragManager.LoadObjectOnPointer(GetNextObject(springs));
                break;
            case ObjectType.PIN:
                dragManager.LoadObjectOnPointer(GetNextObject(pins));
                break;
            case ObjectType.BOMB:
                dragManager.LoadObjectOnPointer(GetNextObject(bombs));
                break;
            default:

                break;
        }
    }

    public GameObject GetNextObject(GameObject[] objs)
    {
        GameObject obj = null;

        bool flag = false;
        for (int i = 0; i < objs.Length && !flag; i++)
        {
            if (!objs[i].activeSelf)
            {
                obj = objs[i];
                flag = true;
            }
        }

        return obj;
    }

    public void NextMaterial()
    {
        material = (ObjectMaterial)(((int)material + 1) % 5);
        materialText.text = material.ToString();
    }

    public void PreviousMaterial()
    {
        material = (ObjectMaterial)(((int)material - 1) % 5);
        materialText.text = material.ToString();
    }
}

public enum ObjectMaterial
{
    METAL,
    WOOD,
    FOAM,
    RUBBER,
    ICE
}

public enum ObjectType
{
    CUBE,
    RECTANGLE,
    TRIANGLE,
    SPHERE,
    WHEEL,
    CONVEYOR,
    ROCKET,
    SPRING,
    PIN,
    BOMB
}
