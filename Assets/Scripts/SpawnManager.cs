using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;

public class SpawnManager : MonoBehaviour
{
    public ObjectMaterial material = ObjectMaterial.METAL;
    public TMP_Text materialText;
    public TouchDragScaleManager dragManager;
    public GameObject prefabParent;
    public Transform spawnPoint;

    private Dictionary<ObjectMaterial, Tuple<Material, PhysicsMaterial2D>> materialDictionary;
    private Dictionary<ObjectMaterial, Tuple<float, float, float, float, float, float>> behavioursDictionary;
    private Dictionary<ObjectMaterial, AudioClip> audioDictionary;
    private Dictionary<ObjectMaterial, AudioMixerGroup> mixerGroupDictionary;

    [Header("Foam")]
    [SerializeField] private Material foamTexture;
    [SerializeField] private PhysicsMaterial2D foamMaterial;
    [SerializeField] private AudioClip foamImpact;
    [SerializeField] private AudioMixerGroup foamGroup;

    [Header("Ice")]
    [SerializeField] private Material iceTexture;
    [SerializeField] private PhysicsMaterial2D iceMaterial;
    [SerializeField] private AudioClip iceImpact;
    [SerializeField] private AudioMixerGroup iceGroup;

    [Header("Rubber")]
    [SerializeField] private Material rubberTexture;
    [SerializeField] private PhysicsMaterial2D rubberMaterial;
    [SerializeField] private AudioClip rubberImpact;
    [SerializeField] private AudioMixerGroup rubberGroup;

    [Header("Metal")]
    [SerializeField] private Material metalTexture;
    [SerializeField] private PhysicsMaterial2D metalMaterial;
    [SerializeField] private AudioClip metalImpact;
    [SerializeField] private AudioMixerGroup metalGroup;

    [Header("Wood")]
    [SerializeField] private Material woodTexture;
    [SerializeField] private PhysicsMaterial2D woodMaterial;
    [SerializeField] private AudioClip woodImpact;
    [SerializeField] private AudioMixerGroup woodGroup;


    [Header("Object Pool")]
    public Collider2D[] cubes, rectangles, triangles, spheres, wheels_left, wheels_right, conveyors_left, conveyors_right, rockets, springs, pins, bombs;

    private void Awake()
    {
        if (dragManager == null)
        { 
            dragManager = FindAnyObjectByType<TouchDragScaleManager>();
        }

        if (spawnPoint == null)
        {
            spawnPoint = transform;
        }

        materialText.text = material.ToString();

        materialDictionary = new Dictionary<ObjectMaterial, Tuple<Material, PhysicsMaterial2D>>
        {
            { ObjectMaterial.ICE, new Tuple<Material, PhysicsMaterial2D>(iceTexture, iceMaterial) },
            { ObjectMaterial.FOAM, new Tuple<Material, PhysicsMaterial2D>(foamTexture, foamMaterial) },
            { ObjectMaterial.RUBBER, new Tuple<Material, PhysicsMaterial2D>(rubberTexture, rubberMaterial) },
            { ObjectMaterial.METAL, new Tuple<Material, PhysicsMaterial2D>(metalTexture, metalMaterial) },
            { ObjectMaterial.WOOD, new Tuple<Material, PhysicsMaterial2D>(woodTexture, woodMaterial) }
        };

        behavioursDictionary = new Dictionary<ObjectMaterial, Tuple<float, float, float, float, float, float>>
        {//Tuple<VelocitySmoothing = 10, MinThrowSpeed = 0.5, Mass = 1, LinearDamping = 0, AngularDamping = 0.05, GravityScale = 1>
            { ObjectMaterial.ICE, new Tuple<float, float, float, float, float, float>(10f, 0.5f, 1f, 0f, 0.05f, 1f)},
            { ObjectMaterial.FOAM, new Tuple<float, float, float, float, float, float>(10f, 0.5f, 1f, 0f, 0.05f, 0.75f)},
            { ObjectMaterial.RUBBER, new Tuple<float, float, float, float, float, float>(10f, 0.5f, 1f, 0f, 0.05f, 1f)},
            { ObjectMaterial.METAL, new Tuple<float, float, float, float, float, float>(5f, 0.5f, 2f, 0f, 0.05f, 1.5f)},
            { ObjectMaterial.WOOD, new Tuple<float, float, float, float, float, float>(10f, 0.5f, 1f, 0f, 0.05f, 1f)}
        };

        audioDictionary = new Dictionary<ObjectMaterial, AudioClip>
        {
            { ObjectMaterial.METAL, metalImpact },
            { ObjectMaterial.WOOD, woodImpact },
            { ObjectMaterial.FOAM, foamImpact },
            { ObjectMaterial.RUBBER, rubberImpact },
            { ObjectMaterial.ICE, iceImpact }
        };

        mixerGroupDictionary = new Dictionary<ObjectMaterial, AudioMixerGroup>
        {
            { ObjectMaterial.WOOD, woodGroup },
            { ObjectMaterial.METAL, metalGroup },
            { ObjectMaterial.RUBBER, rubberGroup },
            { ObjectMaterial.FOAM, foamGroup },
            { ObjectMaterial.ICE, iceGroup }
        };

        StartCoroutine(DestroyEmptyObjectsEverySecond());
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
            case ObjectType.CUBE:
                try
                {
                    SpawnNextShape(cubes);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                break;
            case ObjectType.RECTANGLE:
                try
                {
                    SpawnNextShape(rectangles);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                break;
            case ObjectType.TRIANGLE:
                try
                {
                    SpawnNextShape(triangles);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
                break;
            case ObjectType.SPHERE:
                try
                {
                    SpawnNextShape(spheres);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
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

    public void SpawnNextShape(Collider2D[] objs)
    {
        //Debug.Log($"Spawing shape from {objs}");
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

        obj.GetComponent<Renderer>().material = materialDictionary[material].Item1;
        obj.GetComponent<Collider2D>().sharedMaterial = materialDictionary[material].Item2;

        SetObjectParameters(fullObj, material);

        obj.gameObject.SetActive(true);
        fullObj.SetActive(true);
    }

    public void SetObjectParameters(GameObject obj, ObjectMaterial material)
    {//Tuple<VelocitySmoothing = 10, MinThrowSpeed = 0.5, Mass = 1, LinearDamping = 0, AngularDamping = 0.05, GravityScale = 1>
        obj.GetComponent<DragAndScale>().velocitySmoothing = behavioursDictionary[material].Item1;
        obj.GetComponent<DragAndScale>().minThrowSpeed = behavioursDictionary[material].Item2;
        obj.GetComponent<Rigidbody2D>().mass = behavioursDictionary[material].Item3;
        obj.GetComponent<Rigidbody2D>().linearDamping = behavioursDictionary[material].Item4;
        obj.GetComponent<Rigidbody2D>().angularDamping = behavioursDictionary[material].Item5;
        obj.GetComponent<Rigidbody2D>().gravityScale = behavioursDictionary[material].Item6;

        var audioOnCollision = obj.GetComponent<AudioOnCollision>();
        if (audioOnCollision == null)
        {
            audioOnCollision = obj.GetComponentInChildren<AudioOnCollision>(true);
        }

        if (audioOnCollision != null)
        {
            if (audioDictionary.TryGetValue(material, out AudioClip clip) && clip != null)
            {
                audioOnCollision.ImpactClip = clip;
            }

            if (mixerGroupDictionary.TryGetValue(material, out AudioMixerGroup group) && group != null)
            {
                if (audioOnCollision.GetComponent<AudioSource>() is AudioSource source)
                {
                    source.outputAudioMixerGroup = group;
                }
                audioOnCollision.outputGroup = group;
            }
        }
    }

    public void SetObjectParameters(GameObject obj, Material material)
    {
        foreach (var temp in materialDictionary)
        {
            if (temp.Value.Item1 == material)
            {
                SetObjectParameters(obj, temp.Key);
                return;
            }
        }
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

    private IEnumerator DestroyEmptyObjectsEverySecond()
    {
        while (true)
        {
            foreach (var obj in FindObjectsByType<DragAndScale>(FindObjectsSortMode.None))
            {
                if (obj.GetComponent<Collider2D>() == null && obj.GetComponentInChildren<Collider2D>() == null)
                {
                    Destroy(obj.gameObject);
                }
            }
            yield return new WaitForSeconds(1);
        }
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