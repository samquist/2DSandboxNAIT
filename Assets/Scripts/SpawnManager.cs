
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    public ObjectMaterial material = ObjectMaterial.METAL;
    public TMP_Text materialText;
    public TouchDragScaleManager dragManager;
    public GameObject prefabParent;

    [Header("Object Pools")]

    public Collider2D[] cubes, rectangles, triangles, spheres, wheels, conveyors, rockets, springs, pins, bombs;

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
        if (!Enum.TryParse(objName.ToUpper(), out type))
        {
            return;
        }

        switch (type)
        {
            case ObjectType.WHEEL:
                try
                {
                    dragManager.LoadObjectOnPointer(GetNextObject(wheels));
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                break;
            case ObjectType.CONVEYOR:
                try
                {
                    dragManager.LoadObjectOnPointer(GetNextObject(conveyors));
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                break;
            case ObjectType.ROCKET:
                try
                {
                    dragManager.LoadObjectOnPointer(GetNextObject(rockets));
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                break;
            case ObjectType.SPRING:
                try
                {
                    dragManager.LoadObjectOnPointer(GetNextObject(springs));
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                break;
            case ObjectType.PIN:
                try
                {
                    dragManager.LoadObjectOnPointer(GetNextObject(pins));
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                break;
            case ObjectType.BOMB:
                try
                {
                    dragManager.LoadObjectOnPointer(GetNextObject(bombs));
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                break;
            default:

                break;
        }
    }

    public GameObject GetNextObject(Collider2D[] objs)
    {
        GameObject obj = null;

        bool flag = false;
        for (int i = 0; i < objs.Length && !flag; i++)
        {
            if (!objs[i].gameObject.activeSelf)
            {
                obj = objs[i].gameObject;
                flag = true;
            }
        }
        if (!flag)
            return null;

        GameObject fullObj = Instantiate(prefabParent);
        obj.transform.parent = fullObj.transform;
        obj.transform.localPosition = Vector3.zero;
        obj.gameObject.SetActive(true);
        return fullObj;
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
