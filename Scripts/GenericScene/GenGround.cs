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

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MessageSpec;

public class GenGround : MonoBehaviour
{
    // Noise Object
    private GenPerlinNoise groundNoise;

    // Game Scene Objects
    private SceneState_t sceneState;
    private SceneParams sceneParameters;

    // Code State Flags
    private bool bIsInitialized = false;
    private bool bIsStarted = false;


    // Start is called before the first frame update
    void Start()
    {
        // get noise objects
        groundNoise = GameObject.Find("HeightNoise").GetComponent<GenPerlinNoise>();

        // set started to true
        bIsStarted = true;
    }

    public bool Generate()
    {
        if (!bIsInitialized && bIsStarted)
        {
            Debug.Log("[GROUND ] Generating ground.");

            // get parent scene state
            sceneState = gameObject.GetComponentInParent<GenScene>().sceneState;
            sceneParameters = gameObject.GetComponentInParent<GenScene>().sceneParameters;
            Debug.Log("[GROUND ] Ground size: " + sceneState.size);

            // update noise in objects
            groundNoise.scale = sceneParameters.heightDistributionScale;
            groundNoise.uniform = sceneParameters.heightDistUniformity;
            groundNoise.multiplier = sceneParameters.heightDistMultiplier;
            groundNoise.offset = sceneParameters.heightDistOffset;

            float halfwidth = sceneState.size.x / 2;
            float halflength = sceneState.size.y / 2;

            MeshRenderer meshRenderer = gameObject.GetComponent<MeshRenderer>();
            if (!meshRenderer)
            {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
            }

            // create height mesh
            int xvertices = (int)(sceneState.size.x / sceneParameters.heightMeshFactor);
            int yvertices = (int)(sceneState.size.y / sceneParameters.heightMeshFactor);

            Vector3[] vertices = new Vector3[(xvertices + 1) * (yvertices + 1)];
            Vector2[] uv = new Vector2[vertices.Length];


            for (int i = 0, y = 0; y <= yvertices; y++)
            {
                for (int x = 0; x <= xvertices; x++, i++)
                {
                    float xcoord = sceneParameters.heightMeshFactor * x - halfwidth;
                    float ycoord = sceneParameters.heightMeshFactor * y - halflength;

                    float zcoord = groundNoise.CalcNoise((xcoord + halfwidth) / sceneState.size.x,
                                        (ycoord + halflength) / sceneState.size.y);

                    vertices[i] = new Vector3(xcoord, zcoord, ycoord);
                    uv[i] = new Vector3((float)x / xvertices, (float)y / yvertices);
                }
            }

            MeshFilter meshFilter = gameObject.GetComponent<MeshFilter>();
            if (!meshFilter)
            {
                meshFilter = gameObject.AddComponent<MeshFilter>();
            }

            Mesh mesh = new Mesh();
            mesh.vertices = vertices;
            mesh.uv = uv;

            int[] triangles = new int[xvertices * yvertices * 6];
            for (int ti = 0, vi = 0, y = 0; y < yvertices; y++, vi++)
            {
                for (int x = 0; x < xvertices; x++, ti += 6, vi++)
                {
                    triangles[ti] = vi;
                    triangles[ti + 3] = triangles[ti + 2] = vi + 1;
                    triangles[ti + 4] = triangles[ti + 1] = vi + xvertices + 1;
                    triangles[ti + 5] = vi + xvertices + 2;
                }
            }
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            meshFilter.mesh = mesh;


            // Material material = Resources.Load("Grass/Grass_v1", typeof(Material)) as Material;
            // Material material = Resources.Load("Background/Grass02", typeof(Material)) as Material;
            // Material material = Resources.Load("Background/Ground01", typeof(Material)) as Material;
            // Material material = Resources.Load("ground5/ground5", typeof(Material)) as Material;
            Material material = Resources.Load("Background/HighRes/HR_Mat", typeof(Material)) as Material;
            // material.mainTextureScale = new Vector2(halfwidth, halflength);//new Vector2(100, 100);
            material.mainTextureScale = new Vector2(halfwidth/4, halflength/4);//new Vector2(100, 100); // GRASS
            // material.mainTextureScale = new Vector2(halfwidth/8, halflength/8);//new Vector2(100, 100); // SAND
            meshRenderer.material = material;

            bIsInitialized = true;
        }

        return bIsInitialized;
    }

    public void Regenerate() {
        bIsInitialized = false;
    }

    // Update is called once per frame
    void Update()
    {

    }

    /* ==================================
     * Helper Functions
     * ==================================
     */

    public void SetSceneState(SceneState_t _sceneState)
    {
        sceneState = _sceneState;
    }

    public SceneState_t GetSceneState()
    {
        return sceneState;
    }
}
