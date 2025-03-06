## In-Depth
El nodo `TSplineSurface.BevelEdges` desfasa una arista seleccionada o un grupo de aristas en ambas direcciones a lo largo de la cara, lo que sustituye la arista original por una secuencia de aristas que forman un canal.

En el ejemplo siguiente, se utiliza un grupo de aristas de una primitiva de cuadro de T-Spline como entrada para el nodo `TSplineSurface.BevelEdges`. En el ejemplo, se muestran cómo afectan las siguientes entradas al resultado:
- `percentage` controla la distribución de las aristas recién creadas a lo largo de las caras adyacentes; los valores próximos a cero posicionan las nuevas aristas más cerca de la arista original y los valores próximos a 1 lo hacen más lejos.
- `numberOfSegments` controla el número de caras nuevas en el canal.
- `keepOnFace` define si las aristas de biselado se colocan en el plano de la cara original. Si el valor se establece en "True" (verdadero), la entrada de redondez no tiene ningún efecto.
- `roundness` controla el grado de redondez del bisel y espera un valor comprendido entre 0 y 1, siendo 0 el resultado de un bisel recto y 1 el que devuelve un bisel redondeado.

El modo de cuadro se activa en ocasiones para comprender mejor la forma.


## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BevelEdges_img.gif)
