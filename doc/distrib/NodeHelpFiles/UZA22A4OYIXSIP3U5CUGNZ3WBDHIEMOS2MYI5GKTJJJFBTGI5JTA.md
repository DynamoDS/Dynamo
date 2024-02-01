<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.WeldCoincidentVertices --->
<!--- UZA22A4OYIXSIP3U5CUGNZ3WBDHIEMOS2MYI5GKTJJJFBTGI5JTA --->
## In Depth
In the example below, two halves of a T-Spline surface are joined into one using the `TSplineSurface.ByCombinedTSplineSurfaces` node. The vertices along the mirror plane are overlapping, which becomes visible when one of the vertices is moved using `TSplineSurface.MoveVertices` node. To repair this, welding is performed by using the `TSplineSurface.WeldCoincidentVertices` node. The result of moving a vertex is now different, translated to the side for a better preview. 
___
## Example File

![TSplineSurface.WeldCoincidentVertices](./UZA22A4OYIXSIP3U5CUGNZ3WBDHIEMOS2MYI5GKTJJJFBTGI5JTA_img.jpg)