<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineInitialSymmetry.ByRadial --->
<!--- PK6P6YKREOU7DHO6OXJFT6PUF5LSO2W7ZW4IOTGWYPW3BJYASCOQ --->
## In-Depth
`TSplineInitialSymmetry.ByRadial` define si la geometría de T-Spline presenta simetría radial. La simetría radial solo se puede introducir para primitivas de T-Spline que lo permiten: cono, esfera, revolución y toroide. Una vez establecida en la creación de la geometría de T-Spline, la simetría radial influye en todas las operaciones y las modificaciones posteriores.

Es necesario definir un número deseado de `symmetricFaces` para aplicar la simetría, siendo 1 el mínimo. Independientemente del número de tramos radiales y de altura que tenga la superficie de T-Spline al principio, se dividirá en el número elegido de `symmetricFaces`.

En el ejemplo siguiente, se crea la superficie `TSplineSurface.ByConePointsRadii` y se aplica la simetría radial mediante el uso del nodo `TSplineInitialSymmetry.ByRadial`. A continuación, se utilizan los nodos `TSplineTopology.RegularFaces` y `TSplineSurface.ExtrudeFaces` para seleccionar y extruir respectivamente una cara de la superficie de T-Spline. La extrusión se aplica simétricamente y el control deslizante de número de caras simétricas muestra cómo se subdividen los tramos radiales.

## Archivo de ejemplo

![Example](./PK6P6YKREOU7DHO6OXJFT6PUF5LSO2W7ZW4IOTGWYPW3BJYASCOQ_img.gif)
