## In Depth

Box mode and smooth mode are two ways of viewing a T-Spline surface. Smooth mode is the true shape of a T-Spline surface and is useful for previewing the aesthetics and dimensions of the model. Box mode, on the other hand, can cast an insight into surface structure and give a better understanding of it, as well as being a faster option for previewing large or complex geometry. The node `TSplineSurface.EnableSmoothMode` allows to switch between these two preview states at various stages of geometry development. 

In the example below, Bevel operation is perfromed on a T-Spline Box surface. The result is first visualized in box mode (the `inSmoothMode` input of the box surface set to false) for a better understanding of the structure of the shape. Smooth mode is then is activated through `TSplineSurface.EnableSmoothMode` node and the result is translated to the right for previewing both modes at the same time.
___
## Example File

![TSplineSurface.EnableSmoothMode](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.EnableSmoothMode_img.jpg)