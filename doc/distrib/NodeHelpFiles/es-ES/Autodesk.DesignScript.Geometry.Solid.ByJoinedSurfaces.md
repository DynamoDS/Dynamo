## En detalle:
Solid.ByJoinedSurfaces utiliza una lista de superficies como entrada y devuelve un único sólido definido por las superficies. Las superficies deben definir una superficie cerrada. En el siguiente ejemplo, se comienza con un círculo como geometría base. Se aplica un parche al círculo para crear una superficie y esa superficie se traslada en la dirección Z. A continuación, se extruye el círculo para generar los lados. List.Create se utiliza para crear una lista que consta de la base, el lado y las superficies superiores. A continuación, se usa ByJoinedSurfaces para convertir la lista en un único sólido cerrado.
___
## Archivo de ejemplo

![ByJoinedSurfaces](./Autodesk.DesignScript.Geometry.Solid.ByJoinedSurfaces_img.jpg)

