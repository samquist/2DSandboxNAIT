
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
    public Transform spawnPoint;

    [Header("Object Pools")]

    public Collider2D[] cubes, rectangles, triangles, spheres, wheels_left, wheels_right, conveyors_left, conveyors_right, rockets, springs, pins, bombs;

    private void Awake()
    {
        if (dragManager == null)
        {
            dragManager = FindAnyObjectByType<TouchDragScaleManager>();
        }

        materialText.text = material.ToString();

        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }
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
            case ObjectType.WHEEL_LEFT:
                try
                {
                    SpawnNextObject(wheels_left);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                break;
            case ObjectType.WHEEL_RIGHT:
                try
                {
                    SpawnNextObject(wheels_right);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                break;
            case ObjectType.CONVEYOR_LEFT:
                try
                {
                    SpawnNextObject(conveyors_left);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                break;
            case ObjectType.CONVEYOR_RIGHT:
                try
                {
                    SpawnNextObject(conveyors_right);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                break;
            case ObjectType.ROCKET:
            case ObjectType.JETPACK:
                try
                {
                    SpawnNextObject(rockets);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                break;
            case ObjectType.SPRING:
                try
                {
                    SpawnNextObject(springs);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                break;
            case ObjectType.PIN:
                try
                {
                    SpawnNextObject(pins);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                break;
            case ObjectType.BOMB:
                try
                {
                    SpawnNextObject(bombs);
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

    public void SpawnNextObject(Collider2D[] objs)
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
        {
            Debug.Log("No object could be loaded");
            return;
        }

        GameObject fullObj = Instantiate(prefabParent);
        fullObj.transform.position = spawnPoint.position;
        obj.transform.parent = fullObj.transform;
        obj.gameObject.SetActive(true);
        fullObj.SetActive(true);
    }

    public void NextMaterial()
    {
        material = (ObjectMaterial)(((int)material + 1) % 5);
        materialText.text = material.ToString();
    }

    public void PreviousMaterial()
    {
        material = (ObjectMaterial)(((int)material - 1 + 5) % 5);
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
    WHEEL_LEFT,
    WHEEL_RIGHT,
    CONVEYOR_LEFT,
    CONVEYOR_RIGHT,
    ROCKET,
    JETPACK,
    SPRING,
    PIN,
    BOMB
}
