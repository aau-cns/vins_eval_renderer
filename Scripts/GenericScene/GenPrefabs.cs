/* Copyright (C) 2021 Martin Scheiber, Alexander Hardt-Stremayr,
 * Control of Networked Systems, University of Klagenfurt, Austria.
 *
 * This software is licensed under the terms of the BSD-2-Clause-License
 * with no commercial use allowed, the full terms of which are made available
 * in the LICENSE file. No license in patents is granted.
 *
 * You can contact the authors at <martin.scheiber@ieee.org>
 * and <alexander.hardt-stremayr@aau.at>.
 */

﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessageSpec;

public class GenPrefabs : MonoBehaviour
{
    // strucutres
    public struct ObjectInfo {
        public ObjectInfo(Vector3 meshSize) {
            this.meshSize = meshSize;

            // precalculate these values, for faster access (memory is not an issue)
            this.area = this.meshSize.x * this.meshSize.z;
            this.volume = this.area * this.meshSize.y;
            this.diagonal = Mathf.Sqrt(this.meshSize.x* this.meshSize.x + this.meshSize.z* this.meshSize.z);
        }

        public Vector3 meshSize { get; }
        public float volume { get; }
        public float area { get; }
        public float diagonal { get; }
    }

    // public struct PrefabObject {
    //     public PrefabObject(GameObject gameobject) {
    //         this.gameobject = gameobject;

    //         this.info = new ObjectInfo(GetChildRendererBounds(this.gameobject).size);
    //     }

    //     public GameObject gameobject;
    //     public ObjectInfo info;
    // }

    private GameObject[] objects;

    public float density = 0.5f; // prob that something is placed, and if candidates are placed
    // objects are placed
    private float placement_propability = 0.2f; // prob if something will be placed

    [SerializeField]
    private float min_size = .5f;

    private Vector3[] volumes;
    private float[] sizes;
    private float[] areas;
    private ObjectInfo[] objectInfos;
    private int nrObjects;

    private GameObject genParent;
    private GameObject genParentClone;

    // Noise Objects
    // public GameObject distNoiseObject;
    private GenPerlinNoise featuresNoise;
    // public GameObject heightNoiseObject;
    private GenPerlinNoise heightNoise;


    private SceneState_t sceneState;
    private SceneParams sceneParameters;

    private bool isStarted = false;
    private bool isInitialized = false;


    // Start is called before the first frame update
    void Start()
    {
        // get noise objects
        featuresNoise = GameObject.Find("FeaturesNoise").GetComponent<GenPerlinNoise>();
        heightNoise = GameObject.Find("HeightNoise").GetComponent<GenPerlinNoise>();

        // get generic parent object
        genParent = GameObject.Find("GenericParent");
        if (!genParent)
            Debug.LogError("[PREFABS] Have not found generic parent.");
        else
            Debug.Log("[PREFABS] Have Parent " + genParent.name);

        string[] paths = {
            // "Prefabs/Rocks and Boulders 2/Rocks/Prefabs/",
            // "Prefabs/RPG_FPS_game_assets_industrial/Barrels/Barrel_v1",
            // "Prefabs/RPG_FPS_game_assets_industrial/Barrels/Barrel_v2",
            // "Prefabs/RPG_FPS_game_assets_industrial/Barrels/Barrel_v3",
            // "Prefabs/RPG_FPS_game_assets_industrial/Boxes/Wooden_box_v1",
            // "Prefabs/RPG_FPS_game_assets_industrial/Dumpsters/Dumpsters_v1",
            // "Prefabs/RPG_FPS_game_assets_industrial/Fences/Road_blocks/Road_block_v1",
            // "Prefabs/RPG_FPS_game_assets_industrial/Oil_tanks/Oil_tank_v1",
            // "Prefabs/RPG_FPS_game_assets_industrial/Oil_tanks/Oil_tank_v2",
            // "Prefabs/RPG_FPS_game_assets_industrial/Other_props/Conditioners/Conditioner_v1",
            // "Prefabs/RPG_FPS_game_assets_industrial/Other_props/Electric_box/Electric_box_v1",
            // "Prefabs/RPG_FPS_game_assets_industrial/Other_props/Electric_box/Electric_box_v2",
            // "Prefabs/RPG_FPS_game_assets_industrial/Other_props/Electric_box/Electric_box_v3",
            // "Prefabs/RPG_FPS_game_assets_industrial/Other_props/Palets/Palet_v1",
            // "Prefabs/NatureStarterKit2/Nature",
            "Prefabs/Stones/Mesh",
            // "Prefabs/Crates And Barrels Pack Volume 1 - Free Version/Prefabs"
            "Prefabs/StonesFree/StoneStandard_1/Prefabs/",
            "Prefabs/StonesFree/StoneStandard_2/Prefabs/",
            "Prefabs/StonesFree/StoneStandard_3/Prefabs/",
            "Prefabs/StonesFree/StoneStandard_4/Prefabs/",
            "Prefabs/StonesFree/StoneStandard_5/Prefabs/",
            "Prefabs/Rocks/Prefabs/",
            "Prefabs/RockFREE/mesh/",
            "Prefabs/Rock Package/Mesh/",
            "Prefabs/HQ Rocks/Prefabs/",
            // "Prefabs/Free_Rocks/_prefabs/"
        };

        // add all prefabs to list
        List<GameObject> prefabList = new List<GameObject>();
        foreach (string path in paths)
        {
            GameObject[] loadedObjects = Resources.LoadAll<GameObject>(path);
            Debug.Log("[PREFABS] Loaded " + loadedObjects.Length + " prefabs from " + path);
            prefabList.AddRange(loadedObjects);
        }
        objects = prefabList.ToArray();

        // create object mesh and bounding boxes
        nrObjects = objects.GetLength(0);
        objectInfos = new ObjectInfo[nrObjects];
        volumes = new Vector3[nrObjects];
        sizes = new float[nrObjects];
        areas = new float[nrObjects];
        for (int i = 0; i < nrObjects; i++)
        {
            Vector3 size = GetChildRendererBounds(objects[i]).size;
            objectInfos[i] = new ObjectInfo(size);
            // volumes[i] = size;
            // sizes[i] = Mathf.Sqrt(size.x * size.x + size.z * size.z);
            // areas[i] = size.x * size.z;
            // Debug.Log("object " + i + ": " + objectInfos[i].meshSize + " area " + objectInfos[i].area);
        }

        isStarted = true;
    }

    // Update is called once per frame
    void Update()
    {

    }

    /* =====================
     * Initialization Functions
     * =====================
     */

    public bool Generate()
    {
        if (!isInitialized && isStarted)
        {
            Debug.Log("[PREFABS] Generating Prefabs.");

            // get parent scene state
            sceneState = gameObject.GetComponentInParent<GenScene>().sceneState;
            sceneParameters = gameObject.GetComponentInParent<GenScene>().sceneParameters;

            // update noise in objects
            density = sceneParameters.objectsDensityMultiplier;
            featuresNoise.scale = sceneParameters.objectsDistributionScale;
            featuresNoise.uniform = sceneParameters.objectsDistUniformity;
            featuresNoise.multiplier = sceneParameters.objectsDistMultiplier;
            featuresNoise.offset = sceneParameters.objectsDistOffset;

            //clear
            Destroy(genParentClone);

            // create prefabs clone
            genParentClone = new GameObject("GenericParent(Clone)");
            genParentClone.transform.parent = gameObject.transform;

            // generate scene with given widths
            float left = -sceneState.size.x/2.0f;
            float top = -sceneState.size.y/2.0f;
            Generate(left, top, sceneState.size.x, sceneState.size.y);

            isInitialized = true;
        }

        return isInitialized;
    }

    public void Regenerate() {
        isInitialized = false;
    }

    void Generate(float currleft, float currtop, float currwidth, float currheight)
    {
        float size = currwidth * currheight;
        // Debug.Log("Generating Object:\n" +
        //     "Size: " + currwidth + "x" + currheight +
        //     "\nTop: (" + currleft  + "," + currtop + ")" +
        //     "\nSize:" + size);

        if (size <= min_size)
            return;

        // calculate object orientation
        float ry = Random.Range(-180, 180) * 1;
        Quaternion orientation = Quaternion.AngleAxis(ry, new Vector3(0, 1, 0));

        float distDensity = featuresNoise.CalcNoise(currleft, currtop);
        float currDensity = density * distDensity * distDensity; // quadratic for more impact

        //deciding if larger or smaller objects are placed
        float randplace = Random.Range(0.0f, 1.0f);
        // bool place = randplace <= placement_propability * currDensity;
        bool place = randplace <= sceneParameters.objectsPlacementProbability * currDensity;

        // float factor_max = 4.0f / (currDensity + 0.00001f);
        float factor_max = sceneParameters.objectsPlacementMaxFactor / (currDensity + 0.00001f);

        // check all possible candidates for available size
        List<int> candidates = new List<int>();
        for (int i = 0; i < nrObjects; i++)
        {
            float objarea = objectInfos[i].area;
            float objdiag = objectInfos[i].diagonal;
            if (objarea * sceneParameters.objectsPlacementMinFactor < size
                && objarea * factor_max > size
                && objdiag < currwidth
                && objdiag < currheight) {
                candidates.Add(i);
            }
        }


        if (place && candidates.Count > 0)
        {
            // select random object
            int objnr = candidates[Random.Range(0, candidates.Count)];

            // get object size information
            Vector3 sizeMesh = objectInfos[objnr].meshSize;
            float sizeDiagonal = objectInfos[objnr].diagonal;
            float sizeArea = objectInfos[objnr].area;

            // calculate offset in x (width) and z (height)
            float xoffset = (currwidth - sizeDiagonal) / 2;
            float zoffset = (currheight - sizeDiagonal) / 2;

            // set object position (objects have centered origin)
            float x = currleft + xoffset + sizeDiagonal/2;
            float z = currtop + zoffset + sizeDiagonal/2;

            // determine if object is in origin region
            float radialMin = Mathf.Sqrt(x*x+z*z) - sizeDiagonal/2;
            if (radialMin > sceneParameters.objectOriginRadius) {
                // place object

                // calc height
                // float zcoord = groundNoise.CalcNoise((xcoord + halfwidth) / sceneState.size.x,
                //                             (ycoord + halflength) / sceneState.size.y);
                // float y = sizeMesh.y/2 + heightNoise.CalcNoise(x / currwidth + 0.5f, z / currheight + 0.5f);
                // float y = sizeMesh.y/2 + heightNoise.CalcNoise(x / sceneState.size.x + 0.5f, z / sceneState.size.y + 0.5f);
                float y = heightNoise.CalcNoise(x / sceneState.size.x + 0.5f, z / sceneState.size.y + 0.5f);

                // set object position and create object
                Vector3 vec = new Vector3(x, y, z);
                Instantiate(objects[objnr], vec, orientation, genParentClone.transform);

                // Debug.Log("Placed object at: " + vec.ToString());

                // for remaining unused filds, call generate again
                Generate(currleft, currtop, currwidth, zoffset); // upper
                Generate(currleft, currtop + zoffset, xoffset, sizeDiagonal); // left
                Generate(currleft + xoffset + sizeDiagonal, currtop + zoffset, xoffset, sizeDiagonal); // right
                Generate(currleft, currtop  + zoffset + sizeDiagonal, currwidth, zoffset); // lower
            } else {
                // do not place object but perform non-place subdivision
                place = false;
            }
        }
        // else

        if (!place)
        {
            // precalculate half dimensions
            float halfheight = currheight / 2;
            float halfwidth = currwidth / 2;

            // check for rect size
            if (currwidth * 2 < currheight) {
                //split only vertical
                Generate(currleft, currtop, currwidth, halfheight);
                Generate(currleft, currtop + halfheight, currwidth, halfheight);
            } else if (currheight * 2 < currwidth) {
                // split only horizontal
                Generate(currleft, currtop, halfwidth, currheight);
                Generate(currleft + halfwidth, currtop, halfwidth, currheight);
            } else {
                // split both
                Generate(currleft, currtop, halfwidth, halfheight);
                Generate(currleft + halfwidth, currtop, currwidth - halfwidth, halfheight);
                Generate(currleft, currtop + halfheight, halfwidth, currheight - halfheight);
                Generate(currleft + halfwidth, currtop + halfheight, currwidth - halfwidth, currheight - halfheight);
            }
        }
    }

    Bounds GetChildRendererBounds(GameObject go)
    {
        Renderer[] renderers = go.GetComponentsInChildren<Renderer>();
        int length = renderers.GetLength(0);

        if (length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            for (int i = 1; i < length; i++)
            {
                bounds.Encapsulate(renderers[i].bounds);
            }
            return bounds;
        }
        else
        {
            return new Bounds();
        }
    }

    /* ==================================
     * Helper Functions
     * ==================================
     */
}
