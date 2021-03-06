using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//using UnityEngine.PostProcessing;

//[RequireComponent(typeof(PostProcessingBehaviour))]

// Array ops
using System.Linq;

namespace MessageSpec
{

    // =============================
    // INTERNAL Message definitions
    // =============================
    // For storing unity's internal state
    // E.g. are objects initialized, should this frame be rendered, etc.

    public class UnityState_t
    {
        private Dictionary<string, ObjectState_t> objects;

        // Initialization status
        public int initializationStep { get; set; } = 0;
        public int screenSkipFrames { get; set; } = 0;

        // Scene variable
        public SceneState_t scene;

        // List of landmark objects in the environment
        public Dictionary<string, GameObject> landmarkObjects;
        public Vector3 uavCenterOfMass;
        
        // Convenience getter function.
        public bool initialized { get { return (initializationStep < 0); } }
        public bool readyToRender { get { return (initialized && (screenSkipFrames == 0)); } }

        // Advanced getters/setters

        // Get Wrapper object, defaulting to a passed in template if it does not exist.
        public ObjectState_t getWrapperObject(string ID, GameObject template)
        {
            if (!objects.ContainsKey(ID))
            {
                // Create and save object from template
                objects[ID] = new ObjectState_t(template);
            }
            return objects[ID];
        }

        // Get Wrapper object, defaulting to a passed in template if it does not exist.
        public ObjectState_t getWrapperObject(string ID, string prefab_ID)
        {
            if (!objects.ContainsKey(ID))
            {
                // Create and save object from template
                GameObject template = Resources.Load(prefab_ID) as GameObject;
                objects[ID] = new ObjectState_t(template);
            }
            return objects[ID];
        }

       // Get gameobject from wrapper object
        public GameObject getGameobject(string ID, GameObject template)
        {
            return getWrapperObject(ID, template).gameObj;
        }
        public GameObject getGameobject(string ID, string templateID)
        {
            return getWrapperObject(ID, templateID).gameObj;
        }

        // Check if object is initialized
        public bool isInitialized(string ID)
        {
            bool isInitialized = false;
            if (objects.ContainsKey(ID))
            {
                isInitialized = objects[ID].initialized;
            }
            return isInitialized;
        }
        // Constructor
        public UnityState_t()
        {
            objects = new Dictionary<string, ObjectState_t>() { };
        }
    }

    // Keeps track of gameobjects and their initialization and instantiation.
    public class ObjectState_t
    {
        public bool initialized { get; set; } = false;
        public GameObject gameObj { get; set; }
        public GameObject template { get; set; }
        // public PostProcessingProfile postProcessingProfile { get; set; }
        // Constructor
        public ObjectState_t(GameObject template)
        {
            this.gameObj = GameObject.Instantiate(template);
            this.template = template;
        }

    }

    // Keeps track of (generic) scene parameters
    public class SceneState_t 
    {
        public string scene_name { get; set; } = "gen3dscene";
        public double simTime { get; set; } = 0.0;
        public Vector2Int size { get; set; } = new Vector2Int(60, 60);
        public float noiseFeatures { get; set; } = 1.0f;
        public float noiseClutter { get; set; } = 1.0f;
        public float curIllumination { get; set; } = 1.0f;
	    public int maxLevel { get; set; } = 9;

        public Scene_t sceneParams { get; set; }
    }

    // =============================
    // INCOMING Message definitions
    // =============================
    public class StateMessage_t
    {
        // Startup parameters. They should only change once.
        public bool sceneIsInternal { get; set; }
        public string sceneFilename { get; set; }

        // Scene Metadata
        public float illumination { get; set; }
        public float objectClutter { get; set; }
        public float objectFeatures { get; set; }

        // Frame Metadata
        public Int64 ntime { get; set; }
        public int camWidth { get; set; }
        public int camHeight { get; set; }
        public float camFOV   { get; set; }
        public float camDepthScale { get; set; }
        public float camMotionBlur { get; set; }
        
        // Object state update
        public Scene_t scene { get; set; }
        public List<Camera_t> cameras { get; set; }
        public List<Object_t> objects { get; set; }
        public List<Landmark_t> landmarksInView { get; set; } = new List<Landmark_t>(); // Must be initialized or will segfault.
        public float lidarReturn { get; set; } 


        // Private state members used by Unity3d.
        public bool hasCameraCollision { get; set; } = false;


        // Additional getters (for convenience)
        public int numCameras { get { return cameras.Count(); } }
        public int screenWidth { get { return camWidth; } }
        public int screenHeight { get { return camHeight * numCameras; } }
        public bool sceneIsDefault { get { return sceneFilename.Length == 0; } }
        
    }

    // Camera class for decoding the ZMQ messages.
    public class Camera_t
    {
        public string ID { get; set; }
        public List<float> position { get; set; }
        public List<float> rotation { get; set; }
        // Metadata
        public int channels { get; set; }
        public bool isDepth { get; set; }
        public int outputIndex { get; set; }

        // Should this camera do collision or landmark visibility checks?
        public bool hasCollisionCheck { get; set; }
        public bool doesLandmarkVisCheck { get; set; }

        // Additional getters
        public bool isGrayscale { get { return (channels == 1) && (!isDepth); } }

    }

    // Generic object class for decoding the ZMQ messages.
    public class Object_t
    {
        public string prefabID { get; set; }
        public string ID { get; set; } // The instance ID
        public List<float> position { get; set; }
        public List<float> rotation { get; set; }
        // Metadata
        public List<float> size { get; set; }
    }

    // Generic landmark point class for visibility checks
    public class Landmark_t
    {
        public string ID { get; set; }
        public List<float> position { get; set; }
    }


    // generic scene parameters
    public class Scene_t 
    {
        // placement probabilities
        public bool reinitialize { get; set; }

        public float objectsDensityMultiplier { get; set; }
        public float objectsDistributionScale { get; set; }
        public float objectsPlacementProbability { get; set; }
        public float objectsDensity { get; set; }
        public float objectsPlacement { get; set; }
        public float objectsPlacementMinFactor { get; set; }
        public float objectsPlacementMaxFactor { get; set; }
        public float objectsDistUniformity { get; set; }
        public float objectsDistOffset { get; set; } 
        public float objectsDistMultiplier { get; set; }
        public float objectOriginRadius { get; set; }

        public int heightMeshFactor { get; set; }
        public float heightDistributionScale { get; set; }
        public float heightDistUniformity { get; set; }
        public float heightDistOffset { get; set; }
        public float heightDistMultiplier { get; set; }
    }

    // =============================
    // OUTGOING Message definitions
    // =============================

    public class RenderMetadata_t
    {
        // Metadata
        public Int64 ntime { get; set; }
        public int camWidth { get; set; }
        public int camHeight { get; set; }
        public float camDepthScale { get; set; }
        
        // Additional metadata for helping with the deserialization process.
        public List<string> cameraIDs { get; set; }
        public List<int> channels { get; set; }
        
        public bool hasCameraCollision { get; set; }
        public List<Landmark_t> landmarksInView { get; set; }
        public float lidarReturn { get; set; }

        public RenderMetadata_t(StateMessage_t state)
        {
            ntime = state.ntime;
            camWidth = state.camWidth;
            camHeight = state.camHeight;
            camDepthScale = state.camDepthScale;
            cameraIDs = state.cameras.Select(obj => obj.ID).ToList();
            channels = state.cameras.Select(obj => obj.channels).ToList();
            // These come from private vars generated by unity on every frame.
            hasCameraCollision = state.hasCameraCollision;
            landmarksInView = state.landmarksInView.ToList();
            lidarReturn = state.lidarReturn;
        }
    }

}
