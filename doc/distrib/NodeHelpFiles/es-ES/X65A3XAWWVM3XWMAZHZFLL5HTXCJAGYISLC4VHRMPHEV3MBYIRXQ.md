<!--- Autodesk.DesignScript.Geometry.Solid.BySweep(profile, path, cutEndOff) --->
<!--- X65A3XAWWVM3XWMAZHZFLL5HTXCJAGYISLC4VHRMPHEV3MBYIRXQ --->
## En detalle:
`Solid.BySweep` crea un sólido mediante el barrido de una curva de perfil cerrada de entrada a lo largo de la trayectoria especificada.

En el ejemplo siguiente, utilizamos un rectángulo como curva de perfil base. La trayectoria se crea mediante una función coseno con una secuencia de ángulos para variar las coordenadas X de un conjunto de puntos. Los puntos se utilizan como entrada para el nodo `NurbsCurve.ByPoints`. A continuación, creamos un sólido barriendo el rectángulo a lo largo de la curva coseno creada.
___
## Archivo de ejemplo

![Solid.BySweep](./X65A3XAWWVM3XWMAZHZFLL5HTXCJAGYISLC4VHRMPHEV3MBYIRXQ_img.jpg)
