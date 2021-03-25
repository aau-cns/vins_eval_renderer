using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SliderCallback : MonoBehaviour
{
    public Slider densitySlider;
    public Slider clusterSlider;
    public Slider bumpySlider;
    public Slider lightSlider;

    public GameObject genObject;
    public GameObject distNoiseObject;
    public GameObject heightNoiseObject;
    public Light lightObject;

    private GenPrefabs genPrefabsScript;
    private GenGround genGroundScript;
    private GenPerlinNoise distNoiseScript;
    private GenPerlinNoise heightNoiseScript;

    // Start is called before the first frame update
    void Start()
    {
	genPrefabsScript = genObject.GetComponent<GenPrefabs>();
	genGroundScript = genObject.GetComponent<GenGround>();
	distNoiseScript = distNoiseObject.GetComponent<GenPerlinNoise>();
	heightNoiseScript = heightNoiseObject.GetComponent<GenPerlinNoise>();

        densitySlider.onValueChanged.AddListener(delegate {ValueChangeCheckDensity(); });
        clusterSlider.onValueChanged.AddListener(delegate {ValueChangeCheckCluster(); });
        bumpySlider.onValueChanged.AddListener(delegate {ValueChangeCheckHeight(); });
        lightSlider.onValueChanged.AddListener(delegate {ValueChangeCheckLight(); });
    }

    void ValueChangeCheckDensity()
    {
	genPrefabsScript.density = densitySlider.value;
	genPrefabsScript.Generate();
    }

    void ValueChangeCheckCluster()
    {
        distNoiseScript.uniform = clusterSlider.value;
        // distNoiseScript.CalcNoise();
        genPrefabsScript.Generate();
    }

    void ValueChangeCheckHeight()
    {
        heightNoiseScript.multiplier = bumpySlider.value;
	genGroundScript.Generate();
	genPrefabsScript.Generate();
    }


    void ValueChangeCheckLight()
    {
        lightObject.intensity = lightSlider.value;
    }

    // Update is called once per frame
    void Update()
    {
    }
}
