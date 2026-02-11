## En detalle
La operación `Mesh.MakeHollow` puede utilizarse para vaciar un objeto de malla como preparación para la impresión en 3D. El vaciado de una malla puede reducir considerablemente la cantidad de material de impresión necesario, el tiempo de impresión y los costes. La entrada `wallThickness` define el grosor de las paredes del objeto de malla. De forma opcional, `Mesh.MakeHollow` puede generar agujeros de escape para eliminar el exceso de material durante el proceso de impresión. El tamaño y el número de agujeros se controlan mediante las entradas `holeCount` y `holeRadius`. Por último, las entradas `meshResolution` y `solidResolution` afectan a la resolución del resultado de la malla. Un valor superior de `meshResolution` mejora la precisión con la que la parte interior de la malla se desfasa respecto a la malla original, pero generará más triángulos. Un valor superior de `solidResolution` mejora la conservación de los detalles más finos de la malla original en la parte interior de la malla vaciada.
En el ejemplo siguiente, se utiliza `Mesh.MakeHollow` en una malla con forma de cono. Se añaden cinco agujeros de escape en su base.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.MakeHollow_img.jpg)
