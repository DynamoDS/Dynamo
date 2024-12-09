<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.SegmentsCount --->
<!--- GLVHD43IRWFTZKY7UVDJ7PNERQN5Z3PWTMFYVJ537HCGJCHCQQAQ --->
## In-Depth
`TSplineReflection.SegmentsCount` devuelve el número de segmentos de una reflexión radial. Si el tipo de TSplineReflection es axial, el nodo devuelve un valor de 0.

En el ejemplo siguiente, se crea una superficie de T-Spline con reflexiones añadidas. Más adelante en el gráfico, la superficie se interroga con el nodo `TSplineSurface.Reflections`. El resultado (una reflexión) se utiliza entonces como entrada para que `TSplineReflection.SegmentsCount` devuelva el número de segmentos de la reflexión radial utilizada para crear la superficie de T-Spline.

## Archivo de ejemplo

![Example](./GLVHD43IRWFTZKY7UVDJ7PNERQN5Z3PWTMFYVJ537HCGJCHCQQAQ_img.jpg)
