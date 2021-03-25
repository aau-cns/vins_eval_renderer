using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ImagePerlinScript : MonoBehaviour
{
    public GameObject spriteSource;

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        gameObject.GetComponent<RawImage>().texture = spriteSource.GetComponent<Renderer>().material.mainTexture;
    }
}
