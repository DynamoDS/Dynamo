<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.SegmentAngle --->
<!--- M2WJT5G52MFWUUNWUZWTY2TSRSRY6GVVIAT4LLVJUC2VVLHVGW7A --->
## In-Depth
`TSplineReflection.SegmentAngle` devuelve el ángulo entre cada par de segmentos de reflexión radial. Si el tipo de TSplineReflection es axial, el nodo devuelve 0.

En el ejemplo siguiente, se crea una superficie de T-Spline con reflexiones añadidas. Más adelante en el gráfico, la superficie se interroga con el nodo `TSplineSurface.Reflections`. El resultado (una reflexión) se utiliza a continuación como entrada para que `TSplineReflection.SegmentAngle` devuelva el ángulo entre los segmentos de una reflexión radial.

## Archivo de ejemplo

![Example](./M2WJT5G52MFWUUNWUZWTY2TSRSRY6GVVIAT4LLVJUC2VVLHVGW7A_img.jpg)
