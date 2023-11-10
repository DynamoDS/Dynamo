## In Depth
In the example below, a T-Spline surface becomes invalid, which can be observed by noticing overlapping faces in background preview. The fact that the surface is invalid can be confirmed by failure in activating smooth mode using the `TSplineSurface.EnableSmoothMode` node. Another clue is `TSplineSurface.IsInBoxMode` node returning `true`, even if the surface has initially activated smooth mode. 

To repair the surface, it is passed through `TSplineSurface.Repair` node. The result is a valid surface, which can be confirmed by successfully enabling smooth preview mode. 
___
## Example File

![TSplineSurface.Repair](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Repair_img.jpg)