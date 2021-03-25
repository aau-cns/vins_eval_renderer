# VINSEval EvaluationRenderer v1.0.0
The VINSEval Renderer developed in Unity3D. This is an extension of the [FlightGogglesRenderer](https://github.com/mit-fast/FlightGogglesRenderer).

## Usage
Download the pre-compiled binary and run its contents. You can also put this inside your [`vinseval_cws`](https://github.com/aau-cns/vins_eval) under `vinseval_cws/devel/lib/flightgoggles/`.

## Development

### Prequesites
This project was developed in [Unity3D 2019.4.12f1](https://unity3d.com/unity/qa/lts-releases).

- Unity2019.4.12f1
- ca. 10 GB disk space
- [VINSEval ROS Workspace](https://github.com/aau-cns/vins_eval)

### Installation

1. Create a new Unity3D project.
1. Create a `Asset` folder inside your project folder.
1. Clone this repository into the `Assets` folder.
1. Load the `TopLevelScene`, found in `Assets/Scenes/`.
1. Set the game camera resolution to `640x480` (VGA).

---

## License
This repository is made available to the public to use (_source-available_),
licensed under the terms of the BSD-2-Clause-License with no commercial use allowed, the full terms of which are made available in the LICENSE file. No license in patents is granted.

This work also extends the [FlightGogglesRenderer](https://github.com/mit-fast/FlightGogglesRenderer), which was originally published under the MIT License.
Please refer to the `Scripts/FlightGoggles/LICENSE` for further information on this part of the software.
**All parts of the code, not originally present in the [FlightGogglesRenderer](https://github.com/mit-fast/FlightGogglesRenderer) repository are licensed under this repository's LICENSE file.**

### Usage for academic purposes
If you use this software in an academic research setting, please cite the
corresponding paper and consult the `LICENSE` file for a detailed explanation.

```latex
@inproceedings{vinseval,
   author   = {Alessandro Fornasier and Martin Scheiber and Alexander Hardt-Stremayr and Roland Jung and Stephan Weiss},
   journal  = {2021 Proceedings of the IEEE International Conference on Robotics and Automation (ICRA21 - accepted)},
   title    = {VINSEval: Evaluation Framework for Unified Testing of Consistency and Robustness of Visual-Inertial Navigation System Algorithms},
   year     = {2021},
}
```
