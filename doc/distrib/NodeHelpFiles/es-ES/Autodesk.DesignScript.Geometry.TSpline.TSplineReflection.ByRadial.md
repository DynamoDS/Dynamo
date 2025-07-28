## In-Depth
`TSplineReflection.ByRadial` devuelve un objeto `TSplineReflection` que se puede utilizar como entrada para el nodo `TSplineSurface.AddReflections`. El nodo utiliza un plano como entrada y la normal del plano actúa como eje para rotar la geometría. Al igual que TSplineInitialSymmetry, TSplineReflection, una vez establecida en la creación de la TSplineSurface, influye en todas las operaciones y las modificaciones posteriores.

En el ejemplo siguiente, se utiliza `TSplineReflection.ByRadial` para definir la reflexión de una superficie de T-Spline. Las entradas `segmentsCount` y `segmentAngle` se utilizan para controlar la forma en que la geometría se refleja alrededor de la normal de un plano determinado. La salida del nodo se utiliza como entrada para el nodo `TSplineSurface.AddReflections` para crear una nueva superficie de T-Spline.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.ByRadial_img.gif)
