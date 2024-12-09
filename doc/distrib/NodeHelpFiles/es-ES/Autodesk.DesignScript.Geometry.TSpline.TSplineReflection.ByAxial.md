## In-Depth
`TSplineReflection.ByAxial` devuelve un objeto `TSplineReflection` que se puede utilizar como entrada para el nodo `TSplineSurface.AddReflections`.
La entrada del nodo `TSplineReflection.ByAxial` es un plano que sirve como plano de simetría. Al igual que TSplineInitialSymmetry, TSplineReflection, una vez establecida para la TSplineSurface, influye en todas las operaciones y las modificaciones posteriores.

En el ejemplo siguiente, `TSplineReflection.ByAxial` se utiliza para crear una TSplineReflection ubicada en la parte superior del cono de T-Spline. A continuación, la reflexión se utiliza como entrada para los nodos `TSplineSurface.AddReflections` a fin de reflejar el cono y devolver una nueva superficie de T-Spline.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineReflection.ByAxial_img.jpg)
