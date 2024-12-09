<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.SegmentAngle --->
<!--- M2WJT5G52MFWUUNWUZWTY2TSRSRY6GVVIAT4LLVJUC2VVLHVGW7A --->
## In-Depth
`TSplineReflection.SegmentAngle` restituisce l'angolo tra ogni coppia di segmenti di riflesso radiale. Se il tipo di TSplineReflection è assiale, il nodo restituisce 0.

Nell'esempio seguente, viene creata una superficie T-Spline con riflessi aggiunti. Successivamente, nel grafico, la superficie viene esaminata con il nodo `TSplineSurface.Reflections`. Il risultato (un riflesso) viene quindi utilizzato come input per `TSplineReflection.SegmentAngle` per restituire l'angolo tra i segmenti di un riflesso radiale.

## File di esempio

![Example](./M2WJT5G52MFWUUNWUZWTY2TSRSRY6GVVIAT4LLVJUC2VVLHVGW7A_img.jpg)
