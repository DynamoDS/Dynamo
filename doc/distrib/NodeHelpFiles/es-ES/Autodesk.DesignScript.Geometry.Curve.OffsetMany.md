## En detalle:
`Curve.OffsetMany` crea una o más curvas desfasando una curva plana según la distancia especificada en un plano definido por la normal del plano. Si hay huecos entre las curvas del componente de desfase, se rellenan alargando las curvas de desfase.

La entrada `planeNormal` se establece por defecto en la normal del plano que contiene la curva, pero se puede proporcionar una normal explícita paralela a la normal de la curva original para controlar de forma más eficaz la dirección del desfase.

Por ejemplo, si se necesita una dirección de desfase coherente para varias curvas que comparten el mismo plano, se puede utilizar la entrada `planeNormal` para modificar normales de curva individuales y forzar que todas las curvas se desfasen en la misma dirección. Al invertir la normal, se invierte la dirección del desfase.

En el ejemplo siguiente, se desfasa una PolyCurve con una distancia de desfase negativa, que se aplica en la dirección opuesta del producto vectorial entre la tangente de la curva y el vector normal del plano.
___
## Archivo de ejemplo

![Curve.OffsetMany](./Autodesk.DesignScript.Geometry.Curve.OffsetMany_img.jpg)
