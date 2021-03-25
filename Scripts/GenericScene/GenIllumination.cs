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

public class GenIllumination : MonoBehaviour {

    // GameObjects
    private Light lightObject;
    // private GameObject lightObject;

    // Game Scene Objects
    private SceneState_t sceneState;
    private SceneState_t controlState;

    // Code State Flags
    private bool bIsStarted = false;
    private bool bIsInitialized = false;

    // time to switch illumination
    public double timeToSwitch = 45.0;

    // Time objects
    private double deltaTime;
    private double startTime;
    private bool bIsIlluminated = false;
    private bool bPerformedIllumination = false;
    private float defaultIllumination = 1.0f; //1.8f;

    void Start() {
        // set lightObject
        lightObject = GameObject.FindObjectOfType<Light>();
        // lightObject = GameObject.Find("MainLight");

        bIsStarted = true;
    }

    public bool Generate() {
        if (!bIsInitialized && bIsStarted) {
            Debug.Log("[ILLU   ] Generating illumination.");

            // get parent scene state
            // sceneState = gameObject.GetComponentInParent<GenScene>().sceneState;

            // update time to switch
            timeToSwitch = gameObject.GetComponentInParent<GenScene>().timeToSwitchIllumination;
            lightObject.intensity = defaultIllumination;

            // set start time
            deltaTime = 0.0;
            startTime = gameObject.GetComponentInParent<GenScene>().sceneState.simTime;
            bIsIlluminated = false;
            bPerformedIllumination = false;

            bIsInitialized = true;
        }

        return bIsInitialized;
    }

    void Update() {
        if (bIsInitialized) {
            controlState = gameObject.GetComponentInParent<GenScene>().controlSceneState;
            deltaTime = controlState.simTime - startTime;

            if (!bIsIlluminated) {
                // deltaTime += Time.deltaTime;
                // Debug.Log("Increasing delta: " + deltaTime + " vs " + timeToSwitch);
                // switch illumination
                if (deltaTime > timeToSwitch) {
                    Debug.Log("[ILLU   ] Changing light intensity.");
                    // update parent scene state
                    sceneState = gameObject.GetComponentInParent<GenScene>().sceneState;
                    lightObject.intensity = sceneState.curIllumination;
                    // lightObject.GetComponent<Light>().intensity = sceneState.curIllumination;
                    startTime = controlState.simTime;
                    bIsIlluminated = true;
                }
            }
            else if (!bPerformedIllumination) {
                // deltaTime += Time.deltaTime;

                // switch illumination
                if (deltaTime > 15.0f) {
                    Debug.Log("[ILLU   ] Resetting light intensity.");
                    lightObject.intensity = defaultIllumination;
                    // lightObject.GetComponent<Light>().intensity = 1.0f;
                    deltaTime = 0.0f;
                    bPerformedIllumination = true;
                }
            }
        }
    }

    public void Regenerate() {
        bIsInitialized = false;
    }
}
