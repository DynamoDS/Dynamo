<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.SubdivideFaces --->
<!--- WKY3SVAE74IVMZW7MVT4F5TGIUFXAGA2W2FN6Q6PACG3NH6AMVFA --->
## En detalle:
En el ejemplo siguiente, se genera una superficie de T-Spline a través del nodo `TSplineSurface.ByBoxLengths`.
Se selecciona una cara mediante el nodo `TSplineTopology.FaceByIndex` y se subdivide mediante el nodo `TSplineSurface.SubdivideFaces`.
Este nodo divide las caras especificadas en caras más pequeñas: cuatro para las caras regulares, y tres, cinco o más para NGons.
Cuando la entrada booleana para `exact` se establece en el valor "True" (verdadero), el resultado es una superficie que intenta mantener exactamente la misma forma que el original mientras se añade la subdivisión. Se pueden añadir más isocurvas para conservar la forma. Cuando se establece en el valor "False" (falso), el nodo solo subdivide la única cara seleccionada, lo que suele dar como resultado una superficie distinta a la original.
Los nodos `TSplineFace.UVNFrame` y `TSplineUVNFrame.Position`se utilizan para resaltar el centro de la cara que se va a subdividir.
___
## Archivo de ejemplo

![TSplineSurface.SubdivideFaces](./WKY3SVAE74IVMZW7MVT4F5TGIUFXAGA2W2FN6Q6PACG3NH6AMVFA_img.jpg)
