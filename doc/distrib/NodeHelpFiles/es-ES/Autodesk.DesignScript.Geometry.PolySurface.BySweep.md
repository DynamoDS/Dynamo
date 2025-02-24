## En detalle:
`PolySurface.BySweep (rail, crossSection)` devuelve una PolySurface mediante el barrido de una lista de líneas conectadas no intersecantes a lo largo de un carril. La entrada `crossSection` puede recibir una lista de curvas conectadas que deben encontrarse en un punto inicial o final, o el nodo no devolverá una PolySurface. Este nodo es similar a `PolySurface.BySweep (rail, profile)`, con la única diferencia de que la entrada `crossSection` utiliza una lista de curvas, mientras que `profile` solo utiliza una curva.

En el ejemplo siguiente, se crea una PolySurface mediante el barrido a lo largo de un arco.


___
## Archivo de ejemplo

![PolySurface.BySweep](./Autodesk.DesignScript.Geometry.PolySurface.BySweep_img.jpg)
