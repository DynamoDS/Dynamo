<!--- Autodesk.DesignScript.Geometry.Curve.SweepAsSolid(curve, path, cutEndOff) --->
<!--- LUVHU25JWECNEBKIBZFH6N5EUAM42XM3BSEOTMCI3TQDNS5EKLXA --->
## En detalle:
`Curve.SweepAsSolid` crea un sólido mediante el barrido de una curva de perfil cerrada de entrada a lo largo de la trayectoria especificada.

En el ejemplo siguiente, utilizamos un rectángulo como curva de perfil base. La trayectoria se crea mediante una función coseno con una secuencia de ángulos para variar las coordenadas X de un conjunto de puntos. Los puntos se utilizan como entrada para el nodo `NurbsCurve.ByPoints`. A continuación, creamos un sólido barriendo el rectángulo a lo largo de la curva coseno creada con el nodo `Curve.SweepAsSolid`.
___
## Archivo de ejemplo

![Curve.SweepAsSolid(curve, path, cutEndOff)](./LUVHU25JWECNEBKIBZFH6N5EUAM42XM3BSEOTMCI3TQDNS5EKLXA_img.jpg)
