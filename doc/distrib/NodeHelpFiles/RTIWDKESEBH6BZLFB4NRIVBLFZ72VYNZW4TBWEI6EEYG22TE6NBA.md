<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.NonManifoldEdges --->
<!--- RTIWDKESEBH6BZLFB4NRIVBLFZ72VYNZW4TBWEI6EEYG22TE6NBA --->
## In-Depth
 The `TSplineTopology.NonManifoldEdges` node identifies non-manifold edges from a T-Spline surface. A non-manifold surface can only be displayed in box mode until it is repaired. 

In the example below, a non-manifold T-Spline surface is created as a result of deleting faces on a plane surface. `TSplineTopology.NonManifoldEdges` and `TSplineUVNFrame.Position` nodes are used to identify and visualize non-manifold edges. 


## Example File

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineTopology.NonManifoldEdges_img.jpg)