<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BridgeFacesToFaces --->
<!--- MQJ667AXSQFCK2Z2B7G2MNL35OIFJYLSADFLGXFJUJCA27FCHVHQ --->
## En detalle:
`TSplineSurface.BridgeEdgesToFaces` conecta dos conjuntos de caras, ya sean de la misma superficie o de dos superficies diferentes. El nodo requiere las entradas que se describen a continuación. Las tres primeras entradas son suficientes para generar el puente; el resto son opcionales. La superficie resultante procede de la superficie a la que pertenece el primer grupo de aristas.

En el ejemplo siguiente, se crea una superficie de toroide con `TSplineSurface.ByTorusCenterRadii`. Dos de sus caras se seleccionan y se utilizan como entrada para el nodo `TSplineSurface.BridgeFacesToFaces`, junto con la superficie de toroide. El resto de las entradas muestran cómo se puede ajustar de forma adicional el puente:
- `followCurves`: (optional) a curve for the bridge to follow. In the absence of this input, the bridge follows a straight line
- `frameRotations`: (optional) number of rotations of the bridge extrusion that connects the chosen edges.
- `spansCounts`: (optional) number of spans/segments of the bridge extrusion that connects the chosen edges. If the number of spans is too low, certain options might not be available until it is increased.
- `cleanBorderBridges`: (opcional) suprime los puentes entre los puentes de borde para evitar pliegues.
- `keepSubdCreases`: (opcional) conserva los pliegues subdivididos de la topología de entrada, lo que da como resultado un tratamiento plegado del inicio y el final del puente. La superficie de toroide no tiene aristas plegadas, por lo que esta entrada no tiene ningún efecto en la forma.
- `firstAlignVertices`(opcional) y `secondAlignVertices`: al especificar un par de vértices desplazados, el puente adquiere una ligera rotación.
- `flipAlignFlags`: (optional) reverses the direction of vertices to align

## Archivo de ejemplo

![Example](./MQJ667AXSQFCK2Z2B7G2MNL35OIFJYLSADFLGXFJUJCA27FCHVHQ_img.gif)
