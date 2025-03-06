## En detalle:
`PolyCurve.Heal` utiliza una PolyCurve que se interseca consigo misma y devuelve una PolyCurve nueva que no se interseca consigo misma. La PolyCurve de entrada no puede tener más de tres autointersecciones. En otras palabras, si un solo segmento de la PolyCurve se encuentra o se interseca con más de dos segmentos, la reparación no funcionará. Introduzca un valor de `trimLength` mayor que 0; los segmentos finales con una longitud superior a `trimLength` no se recortarán.

En el ejemplo siguiente, una PolyCurve autointersecante se repara mediante `PolyCurve.Heal`.
___
## Archivo de ejemplo

![PolyCurve.Heal](./Autodesk.DesignScript.Geometry.PolyCurve.Heal_img.jpg)
