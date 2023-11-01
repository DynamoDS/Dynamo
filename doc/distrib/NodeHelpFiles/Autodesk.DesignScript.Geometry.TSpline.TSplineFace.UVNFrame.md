## In-Depth
A UVNFrame of a face provides useful information about the face position and orientation by returning the normal vector and UV directions.
In the example below, `TSplineFace.UVNFrame` node is used to visualize the distribution of faces on a quadball primitive. `TSplineTopology.DecomposedFaces` is used to query all faces and `TSplineFace.UVNFrame` node is then used to retrieve the positions of face centroids as points. The points are visualized using `TSplineUVNFrame.Position` node. Labels are displayed in the background preview by enabling `Show Labels` in the node`s right-click menu. 

## Example File

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineFace.UVNFrame_img.jpg)