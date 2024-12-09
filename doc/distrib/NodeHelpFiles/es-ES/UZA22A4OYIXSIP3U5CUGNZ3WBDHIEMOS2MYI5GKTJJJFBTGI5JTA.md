<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.WeldCoincidentVertices --->
<!--- UZA22A4OYIXSIP3U5CUGNZ3WBDHIEMOS2MYI5GKTJJJFBTGI5JTA --->
## En detalle:
En el ejemplo siguiente, dos mitades de una superficie de T-Spline se unen en una mediante el nodo `TSplineSurface.ByCombinedTSplineSurfaces`. Los vértices a lo largo del plano de simetría se solapan, lo que se hace visible cuando se desplaza uno de los vértices mediante el nodo `TSplineSurface.MoveVertices`. Para reparar esto, se realiza una soldadura mediante el nodo `TSplineSurface.WeldCoincidentVertices`. El resultado de desplazar un vértice es ahora diferente; se traslada hacia un lado para obtener una mejor vista preliminar.
___
## Archivo de ejemplo

![TSplineSurface.WeldCoincidentVertices](./UZA22A4OYIXSIP3U5CUGNZ3WBDHIEMOS2MYI5GKTJJJFBTGI5JTA_img.jpg)
