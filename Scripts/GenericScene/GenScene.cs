/* Copyright (C) 2021 Martin Scheiber, Alexander Hardt-Stremayr, and others
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

/// <summary>
///
/// </summary>
public class GenScene : MonoBehaviour
{
    // Parameters
    public GameObject heightNoisePrefab;
    public GameObject featuresNoisePrefab;
    [SerializeField]
    private GameObject internalCamera;

    // Game Objects
    private GameObject groundObject;
    private GameObject prefabsObject;
    private GameObject illuminationObject;
    private GameObject heightNoiseObject;
    private GameObject featuresNoiseObject;

    private GenGround ground;
    private GenPrefabs prefabs;
    private GenIllumination illumination;
    private GenPerlinNoise heightNoise;
    private GenPerlinNoise featuresNoise;

    // Camera and Scene Objects
    public SceneState_t sceneState { get; set; }
    private CameraController cameraController;
    public SceneState_t controlSceneState { get; set; }
    private bool bHaveCamController = false;
    private GameObject cameraMinimap;

    private bool generated_scene = false;

    // Noise Interpretation Function Parameters
    [SerializeField]
    private float clutterFxMultiplier = 1.0f/9.0f;
    [SerializeField]
    private float featuresFxMultiplier = 1.0f/10.0f;
    [SerializeField]
    public float timeToSwitchIllumination = 30.0f;

    [Space(10),Header("Scene Generation Settings")]

    // test variables for scene generation
    [SerializeField]
    private bool PERFORM_SCENE_TEST = false;
    [Tooltip("Parameters used for scene generation when PERFORM_SCENE_TEST is active, or if no parameters were transferred.")]
    public SceneParams sceneParameters;

    /**========================
     * UNITY PLAYER EVENT HOOKS
     * ========================
     */
    public void Start()
    {
        // get application version
        var vio_eval_version = Application.version;
        Debug.Log("[SCENE  ] VIO Evaluation version: " + vio_eval_version);

        // load camera and scene state
        try {
            GameObject cameraObject = GameObject.Find("Camera_Controller");
            cameraController = cameraObject.GetComponent<CameraController>();
            if (!cameraController) {
                Debug.LogError("[SCENE  ] Could not retrieve camera controller of control object.");
                Application.Quit();
            } else {
                Debug.Log("[SCENE  ] Found camera controller.");
                controlSceneState = cameraController.GetSceneState();
                bHaveCamController = true;
            }
        }
        // in case of editor testing catch the camera not found exception and generate scene from internal values
        catch (System.NullReferenceException err) {
            Debug.LogException(err);
            Debug.LogWarning("[SCENE  ] Did not find camera controller. Using default scene params");
            controlSceneState = new SceneState_t();
            controlSceneState.size = new Vector2Int(60, 60);
            PERFORM_SCENE_TEST = true;

            // generate camera
            cameraMinimap = Instantiate(internalCamera, new Vector3(0,10,0), new Quaternion(0.7f,0f,0f,0.7f).normalized, gameObject.transform);
        }

        // set noise random seed
        // UnityEngine.Random.seed = 5;
        // UnityEngine.Random.InitState(5);

        // load noise prefabs, do this first
        heightNoiseObject = Instantiate(heightNoisePrefab, new Vector3(0,0,0), Quaternion.identity, gameObject.transform);
        heightNoiseObject.name = "HeightNoise";
        heightNoise = heightNoiseObject.GetComponent<GenPerlinNoise>();
        featuresNoiseObject = Instantiate(featuresNoisePrefab, new Vector3(0,0,0), Quaternion.identity, gameObject.transform);
        featuresNoiseObject.name = "FeaturesNoise";
        featuresNoise = featuresNoiseObject.GetComponent<GenPerlinNoise>();

        // interpret the scene
        InterpretSceneState();

        // create objects
        groundObject = new GameObject("GenGround", typeof(GenGround));
        groundObject.transform.parent = gameObject.transform;
        ground = groundObject.GetComponent<GenGround>();
        prefabsObject = new GameObject("GenPrefabs", typeof(GenPrefabs));
        prefabsObject.transform.parent = gameObject.transform;
        prefabs = prefabsObject.GetComponent<GenPrefabs>();
        illuminationObject = new GameObject("GenIllumination", typeof(GenIllumination));
        illuminationObject.transform.parent = gameObject.transform;
        illumination = illuminationObject.GetComponent<GenIllumination>();
    }

    // Update is called once per frame
    public void Update() {
        // generate scene if needed
        if (!generated_scene) {
            generated_scene = true & ground.Generate();
            generated_scene &= prefabs.Generate();
            generated_scene &= illumination.Generate();
        }
        else {
            if (bHaveCamController) {
                // update scene states
                controlSceneState = cameraController.GetSceneState();

                // regenerate scene if change was detected
                // Debug.Log("[SCENE  ] Param check: " + (sceneParameters != controlSceneState.sceneParams));
                if ((PERFORM_SCENE_TEST && sceneParameters != controlSceneState.sceneParams) || controlSceneState.sceneParams.reinitialize) {
                    Debug.LogWarning("[SCENE  ] Received change scene parameters in active run.");
                    InterpretSceneState();
                    ground.Regenerate();
                    prefabs.Regenerate();
                    illumination.Regenerate();
                    generated_scene = false;
                }
            }
        }

        // handle inputs
        HandleUserInput();
    }

    private void InterpretSceneState() {
        sceneState = controlSceneState;

        if (!PERFORM_SCENE_TEST) {
            Debug.Log("[SCENE  ] Interpreting scene state.");
            InterpretFeatureNoise();
            InterpretHeightNoise();
        }
        else {
            Debug.Log("[SCENE  ] Performing scene test with bridge parameters.");
            // TODO(scm): create scene from transmitted scene parameters
            if (bHaveCamController) {
                // sceneParameters.objectsDensityMultiplier    = sceneState.sceneParams.objectsDensityMultiplier;
                // sceneParameters.objectsDistributionScale    = sceneState.sceneParams.objectsDistributionScale;
                // sceneParameters.objectsPlacementProbability = sceneState.sceneParams.objectsPlacementProbability;
                // sceneParameters.objectsPlacementMinFactor   = sceneState.sceneParams.objectsPlacementMinFactor;
                // sceneParameters.objectsPlacementMaxFactor   = sceneState.sceneParams.objectsPlacementMaxFactor;
                // sceneParameters.objectsDistUniformity       = sceneState.sceneParams.objectsDistUniformity;
                // sceneParameters.objectsDistOffset           = sceneState.sceneParams.objectsDistOffset;
                // sceneParameters.objectsDistMultiplier       = sceneState.sceneParams.objectsDistMultiplier;
                // sceneParameters.objectOriginRadius          = sceneState.sceneParams.objectOriginRadius;
                // sceneParameters.heightMeshFactor            = sceneState.sceneParams.heightMeshFactor;
                // sceneParameters.heightDistributionScale     = sceneState.sceneParams.heightDistributionScale;
                // sceneParameters.heightDistUniformity        = sceneState.sceneParams.heightDistUniformity;
                // sceneParameters.heightDistOffset            = sceneState.sceneParams.heightDistOffset;
                // sceneParameters.heightDistMultiplier        = sceneState.sceneParams.heightDistMultiplier;
                sceneParameters = sceneState.sceneParams;
            }
        }
    }

    private void InterpretFeatureNoise() {
	int maxLevel = 9;

        sceneParameters.objectsDensityMultiplier    = 1.0f; //0.25f;
        sceneParameters.objectsDistributionScale    = 8.0f;
        sceneParameters.objectsPlacementProbability = 1.0f-(0.95f*controlSceneState.noiseFeatures/(controlSceneState.maxLevel));
        sceneParameters.objectsPlacementMinFactor   = (10.0f/controlSceneState.maxLevel)*controlSceneState.noiseFeatures;
        sceneParameters.objectsPlacementMaxFactor   = Normal(65.0f, ((4.0f/controlSceneState.maxLevel)*controlSceneState.noiseFeatures+1.0f));
        sceneParameters.objectsDistUniformity       = HalfNormal(0.5f, ((0.3f/controlSceneState.maxLevel)*controlSceneState.noiseFeatures+0.1f), 0.9f, 0.1f);
        sceneParameters.objectsDistOffset           = 0.0f; //0.3f;
        sceneParameters.objectsDistMultiplier       = 1.0f;
        sceneParameters.objectOriginRadius          = 0.75f;
        sceneParameters.heightMeshFactor            = 10;
        sceneParameters.heightDistributionScale     = 4.0f;
        sceneParameters.heightDistUniformity        = 0.0f;
        sceneParameters.heightDistOffset            = 0.0f;
        sceneParameters.heightDistMultiplier        = 0.5f;

        Debug.Log("Params:\n\tplacement_prob: " + sceneParameters.objectsPlacementProbability +
            "\n\tplacement_min: " + sceneParameters.objectsPlacementMinFactor +
            "\n\tplacement_max: " + sceneParameters.objectsPlacementMaxFactor +
            "\n\tuniformity: " + sceneParameters.objectsDistUniformity);
    }

    private void InterpretHeightNoise() {
        // TODO(scm): this has to be implemented
        // sceneState.noiseClutter = clutterFxMultiplier * (controlSceneState.noiseClutter - 1.0f);
        //     heightNoise.uniform = sceneState.noiseClutter;
            // clutterNoise.CalcNoise();
    }

    // TODO(alf): move the following functions on appropriate location
    private float Normal(float mean, float stdDev) {

    // INFO(martin): Currently we only need pseudorandomness
	// Instantiate random number generator.
   	// Random rand = new Random();

	// Get 2 uniform random variable u1 and u2 in (0,1]
	float u1 = 1.0f - Random.Range(0.0f, 1.0f); //rand.NextFloat();
	float u2 = 1.0f - Random.Range(0.0f, 1.0f); //rand.NextFloat();

	// Get a gaussian random varibale N(0,1)
	float randStdNormal = Mathf.Sqrt(-2.0f * Mathf.Log(u1)) * Mathf.Sin(2.0f * Mathf.PI * u2);

	// Get a gaussian random varibale N(mean,stdDev)
	float randNormal = mean + stdDev * randStdNormal;

	return randNormal;
    }

    private float HalfNormal(float mean, float stdDev, float upperBound, float lowerBound) {

	float randNormal = Normal(mean, stdDev);
	float halfRandNormal;

	if (randNormal > mean && randNormal < upperBound) {
	    halfRandNormal = mean - randNormal;
	} else if (randNormal > upperBound || randNormal < lowerBound) {
	    halfRandNormal = lowerBound;
	} else {
	    halfRandNormal = randNormal;
	}

	return randNormal;
    }


    /**========================
     * INPUT HANDLING FUNCTIONS
     * ========================
     */

    private void HandleUserInput() {
        // only care for input in test environment
        if (PERFORM_SCENE_TEST) {
            Vector3 camMinimapPos = cameraMinimap.transform.position;

            // move cam down
            if (Input.GetKey(KeyCode.DownArrow)) {
                Debug.Log("Handle input " + camMinimapPos);
                if (camMinimapPos.y > 1.0f) {
                    camMinimapPos.y -= 1.0f;
                    Debug.Log("Changed minimap camera position to " + camMinimapPos);
                }
            }
            else if (Input.GetKeyDown(KeyCode.UpArrow)) {
                if (camMinimapPos.y < 25.0f) {
                    camMinimapPos.y += 1.0f;
                    Debug.Log("Changed minimap camera position to " + camMinimapPos);
                }
            }
        }
    }
}

[System.Serializable]
public class SceneParams
{
    [Header("Object Distribution Parameters")]
    // placement probabilities
    [Tooltip("Object placement density multiplier."),Range(0.01f,1f)]
    public float objectsDensityMultiplier = 0.5f;

    [Tooltip("Octave of Perlin noise distribution"),Range(1f,16f)]
    public float objectsDistributionScale = 4f;

    [Tooltip("Object placement probability"),Range(0f,1f)]
    public float objectsPlacementProbability = 0.2f;

    [Tooltip("Min object size factor needed for placement."),Min(0)]
    public float objectsPlacementMinFactor = 2f;

    [Tooltip("Max density factor needed for placement"),Min(0)]
    public float objectsPlacementMaxFactor = 4f;

    [Tooltip("Uniformity of distirbution"),Range(0,1)]
    public float objectsDistUniformity = 0f;

    [Tooltip("Offest of object distirbution"),Min(0)]
    public float objectsDistOffset = 0f;

    [Tooltip("Distribution multiplier for objects"),Min(0)]
    public float objectsDistMultiplier = 1f;

    [Tooltip("Radius of region where no objects shall be located."),Min(0)]
    public float objectOriginRadius = 3f;

    [Space(5),Header("Ground Plane Height Distribution Parameters")]
    [Tooltip("Plane division factor, or number of mesh planes generated."),Range(1,100)]
    public int heightMeshFactor = 10;

    [Tooltip("Octave of Perlin noise distribution"),Range(1f,16f)]
    public float heightDistributionScale = 4f;

    [Tooltip("Uniformity of distirbution"),Range(0,1)]
    public float heightDistUniformity = 0f;

    [Tooltip("Offest of object distirbution"),Min(0)]
    public float heightDistOffset = 0f;

    [Tooltip("Distribution multiplier for objects"),Min(0)]
    public float heightDistMultiplier = 1f;

    public static bool operator== (SceneParams obj1, SceneParams obj2) {
        return (obj1.objectsDensityMultiplier       == obj2.objectsDensityMultiplier
                && obj1.objectsDistributionScale    == obj2.objectsDistributionScale
                && obj1.objectsPlacementProbability == obj2.objectsPlacementProbability
                && obj1.objectsPlacementMinFactor   == obj2.objectsPlacementMinFactor
                && obj1.objectsPlacementMaxFactor   == obj2.objectsPlacementMaxFactor
                && obj1.objectsDistUniformity       == obj2.objectsDistUniformity
                && obj1.objectsDistOffset           == obj2.objectsDistOffset
                && obj1.objectsDistMultiplier       == obj2.objectsDistMultiplier
                && obj1.objectOriginRadius          == obj2.objectOriginRadius
                && obj1.heightMeshFactor            == obj2.heightMeshFactor
                && obj1.heightDistributionScale     == obj2.heightDistributionScale
                && obj1.heightDistUniformity        == obj2.heightDistUniformity
                && obj1.heightDistOffset            == obj2.heightDistOffset
                && obj1.heightDistMultiplier        == obj2.heightDistMultiplier);
    }

    public static bool operator!= (SceneParams obj1, SceneParams obj2) {
        return !(obj1.objectsDensityMultiplier      == obj2.objectsDensityMultiplier
                && obj1.objectsDistributionScale    == obj2.objectsDistributionScale
                && obj1.objectsPlacementProbability == obj2.objectsPlacementProbability
                && obj1.objectsPlacementMinFactor   == obj2.objectsPlacementMinFactor
                && obj1.objectsPlacementMaxFactor   == obj2.objectsPlacementMaxFactor
                && obj1.objectsDistUniformity       == obj2.objectsDistUniformity
                && obj1.objectsDistOffset           == obj2.objectsDistOffset
                && obj1.objectsDistMultiplier       == obj2.objectsDistMultiplier
                && obj1.objectOriginRadius          == obj2.objectOriginRadius
                && obj1.heightMeshFactor            == obj2.heightMeshFactor
                && obj1.heightDistributionScale     == obj2.heightDistributionScale
                && obj1.heightDistUniformity        == obj2.heightDistUniformity
                && obj1.heightDistOffset            == obj2.heightDistOffset
                && obj1.heightDistMultiplier        == obj2.heightDistMultiplier);
    }

    public static implicit operator SceneParams(Scene_t s) {
        SceneParams sp = new SceneParams();
        sp.objectsDensityMultiplier     = s.objectsDensityMultiplier;
        sp.objectsDistributionScale     = s.objectsDistributionScale;
        sp.objectsPlacementProbability  = s.objectsPlacementProbability;
        sp.objectsPlacementMinFactor    = s.objectsPlacementMinFactor;
        sp.objectsPlacementMaxFactor    = s.objectsPlacementMaxFactor;
        sp.objectsDistUniformity        = s.objectsDistUniformity;
        sp.objectsDistOffset            = s.objectsDistOffset;
        sp.objectsDistMultiplier        = s.objectsDistMultiplier;
        sp.objectOriginRadius           = s.objectOriginRadius;
        sp.heightMeshFactor             = s.heightMeshFactor;
        sp.heightDistributionScale      = s.heightDistributionScale;
        sp.heightDistUniformity         = s.heightDistUniformity;
        sp.heightDistOffset             = s.heightDistOffset;
        sp.heightDistMultiplier         = s.heightDistMultiplier;
        return sp;
    }
}
