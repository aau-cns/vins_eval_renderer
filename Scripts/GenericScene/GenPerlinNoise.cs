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

public class GenPerlinNoise : MonoBehaviour
{
    public float scale = 4.0f;      // number of octaves
    public float uniform = 0.0f;    // multiplier * ( noise *(1-u) + mean *u + offset ) mainly for height
    public float offset = 0.0f;
    public float multiplier = 1.0f; // mainly for height to multify the effect


    // Start is called before the first frame update
    void Start() {
    }

    //x, y between 0 and 1
    /// <summary>
    /// Calculates the PerlinNoise for the coordinate <c>x</c> and <c>y</c>
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns>The PerlinNoise value distributed according to our distribution</returns>
    public float CalcNoise(float x, float y) {
        return (offset + Mathf.PerlinNoise(x * scale, y * scale) * (1 - uniform) + 0.5f * uniform) * multiplier;
    }

    // Update is called once per frame
    void Update() {
    }
}
