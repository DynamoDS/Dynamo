<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByPlaneOriginXAxisYAxis --->
<!--- JDRXXB3ZLF7RXZJRV66VKV5ZDAZGN5YCY7ZLVWABJQNDVHNU4QKA --->
## In-Depth
`TSplineSurface.ByPlaneOriginXAxisYAxis` genera una superficie del piano della primitiva T-Spline utilizzando un punto di origine e due vettori che rappresentano gli assi X e Y del piano. Per creare il piano T-Spline, il nodo utilizza i seguenti input:
- `origin`: a point defining the origin of the plane.
- `xAxis` e `yAxis`: vettori che definiscono la direzione degli assi X e Y del piano creato.
- `minCorner` and `maxCorner`: the corners of the plane, represented as Points with X and Y values (Z coordinates will be ignored). These corners represent the extents of the output T-Spline surface if it is translated onto the XY plane. The `minCorner` and `maxCorner` points do not have to coincide with the corner vertices in 3D. For example, when a `minCorner` is set to (0,0) and `maxCorner` is (5,10), the plane width and length will be 5 and 10 respectively.
- `xSpans` and `ySpans`: number of width and length spans/divisions of the plane
- `symmetry`: whether the geometry is symmetrical with respect to its X, Y and Z axes
- `inSmoothMode`: whether the resulting geometry will appear with smooth or box mode

Nell'esempio seguente, viene creata una superficie T-Spline piana utilizzando il punto di origine fornito e due vettori che fungono da direzioni X e Y. La dimensione della superficie è controllata dai due punti utilizzati come input `minCorner` e `maxCorner`.

## File di esempio

![Example](./JDRXXB3ZLF7RXZJRV66VKV5ZDAZGN5YCY7ZLVWABJQNDVHNU4QKA_img.jpg)
