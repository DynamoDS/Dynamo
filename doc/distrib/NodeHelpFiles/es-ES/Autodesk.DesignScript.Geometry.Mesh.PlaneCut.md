## En detalle
`Mesh.PlaneCut` devuelve una malla que se ha cortado por un plano especificado. El resultado del corte es la parte de la malla que se encuentra en el lado del plano en la dirección de la normal de la entrada `plane`. El parámetro `makeSolid` controla si la malla se trata como un sólido, en cuyo caso el corte se rellena con el menor número posible de triángulos para cubrir cada agujero.

En el ejemplo siguiente, una malla hueca obtenida de una operación `Mesh.BooleanDifference` se corta por un plano en ángulo.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.PlaneCut_img.jpg)
