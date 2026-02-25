<!--- Autodesk.DesignScript.Geometry.Curve.SweepAsSurface(curve, path, cutEndOff) --->
<!--- DUHOUAQLX67Z6VGX2F6TGNPE2PGYDN7VGCOK6UW3D5GYILRXG3KA --->
## En detalle:
`Curve.SweepAsSurface` creará una superficie mediante el barrido de una curva de entrada a lo largo de una trayectoria especificada. En el ejemplo siguiente, creamos una curva para barrer mediante un bloque de código a fin de crear tres puntos de un nodo `Arc.ByThreePoints`. Se crea una curva de trayectoria como una línea simple a lo largo del eje X. `Curve.SweepAsSurface` mueve la curva de perfil a lo largo de la curva de trayectoria para crear una superficie. El parámetro `cutEndOff` es un valor booleano que controla el tratamiento de los extremos de la superficie barrida. Si se establece en `true`, los extremos de la superficie se cortan perpendicularmente (normal) a la curva de trayectoria, lo que genera terminaciones limpias y planas. Si se establece en `false` (el valor por defecto), los extremos de la superficie siguen la forma natural de la curva de perfil sin ningún recorte, lo que puede dar lugar a extremos angulosos o irregulares en función de la curvatura de la trayectoria.
___
## Archivo de ejemplo

![Example](./DUHOUAQLX67Z6VGX2F6TGNPE2PGYDN7VGCOK6UW3D5GYILRXG3KA_img.jpg)

