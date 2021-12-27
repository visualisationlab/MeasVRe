# MeasVRe
By Jolly Chen, Robert Belleman, Visualisation Lab and Computational Science Lab, University of Amsterdam.

Contact: [Robert Belleman](mailto:R.G.Belleman_at_uva.nl).

MeasVRe is a measurement toolkit for Unity VR applications. With this toolkit, the user can measure
distances, angles, trace lengths, surface areas, and volumes. These measurements are taken by
placing a set of markers, which can be snapped to a mesh. New measurement types can be added by
inheriting from the ``Measurement<T>`` class. A template script is included in the Unity project
in `Assets/Scripts/Measurements`.

The data of the measurements and the snapshots can be saved locally, or they can be uploaded to a
logging server (MeasVReLog). The code for this is in the `LoggingServer` folder. For computing
the surface area and volume, the library
[Geometric Tools](https://github.com/davideberly/GeometricTools/tree/master/GTE) was used. This
library is written in C++ so native plugina were created to be able to use some methods from this
library in Unity. The folder `Plugins` contains the Visual Studio 2019 projects that were used to
build the plugins. The `MeasVRe` folder contains the Unity project with the MeasVre toolkit.

## Getting started
Download the `MeasVRe` folder and add the project in the Unity Hub via `Add` -> `Select Folder`.
The project was developed in Unity 2020.3.8f1 and tested on an Oculus Quest 2 device. The
required packages, XR Interaction Toolkit and TextMeshPro, should be automatically installed.

In `Assets/Scenes` a sample scene is included with the MeasVRe prefab and two meshes. To run the
sample scene on an Oculus device, switch the platform to Android in the build settings and press
build and run. For other devices, the device should be added to the `XR Plug-in Management` settings
in the `Project Settings` first.

## Default controls
The controls of the system can be edited in the Unity Editor in the ObjectScript component and
MainManager, but currently the controls are set to:

    * Left joy stick: move around.
    * Right joy stick: rotate the camera.
    * Right Trigger: place a marker.
    * Left grip: grab a menu to move it.
    * Right grab: grab a marker or label to move it.
    * A: select a marker or measurement when the right index finger touches the marker or
    * measurment label. Once a measurement is selected, a menu is opened for adding snapshots.
    * B + Y: delete a marker or measurement when the right index finger touches the marker or measurement label.

## Remarks
Markers can be snapped to the surface, a vertex, or the edge of a mesh using raycasts.
In order for snapping to work the meshes must have a MeshCollider component attached. Additionally,
read/write access must be enabled for imported models to use snapping to vertices and edges.

This project was developed by Jolly Chen for her UvA Bachelor Informatica (Computer Science) thesis project
[MeasVRe: measurement tools for Unity VR applications](https://scripties.uba.uva.nl/search?id=722538).
