using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Disables realtime reflection probes if not supported.

public class realtimeProbeDisabler : MonoBehaviour
{
    
    // Start is called before the first frame update
    void Start()
    {
        // Check if realtime reflection probes are disabled.
        // Quality levels:
        // - VeryLow                            (0)
        // - Low                                (1)
        // - Medium                             (2)
        // - High - realtime reflection probes  (3)
        // - Ultra - realtime reflection probes (4)

        Debug.Log("Using quality level: " + QualitySettings.GetQualityLevel().ToString());

        if (QualitySettings.GetQualityLevel() <= 2){
            Debug.Log("Disabling realtime reflection probes.");
            // Disable this realtime probe.
            this.gameObject.SetActive(false);
        }
    }

}
