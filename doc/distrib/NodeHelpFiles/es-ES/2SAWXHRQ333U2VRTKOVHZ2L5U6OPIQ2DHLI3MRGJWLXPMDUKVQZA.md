<!--- Autodesk.DesignScript.Geometry.NurbsSurface.ByControlPointsWeightsKnots --->
<!--- 2SAWXHRQ333U2VRTKOVHZ2L5U6OPIQ2DHLI3MRGJWLXPMDUKVQZA --->
## En detalle:
Crea una NurbsSurface según las especificaciones de vértices, nudos, grosores y grados para U y V. Hay diversas restricciones en los datos que, de ser ignoradas, pueden hacer que la función produzca un error y genere una excepción. Grado: el grado para U y V debe ser >= 1 (spline lineal por segmentos) e inferior a 26 (el máximo de grados base admitido para B-splines por ASM). Grosores: todos los valores de grosor (si se suministran) deben ser positivos. Los grosores inferiores a 1e-11 se rechazarán y la función producirá un error. Nudos: los dos vectores de nudo deben tener secuencias no decrecientes. La multiplicidad de nudos interior no debe ser mayor que su grado más 1 en el nudo inicial/final ni mayor que el grado en el caso de nudos internos (esto permite representar superficies con discontinuidades G1). Tenga en cuenta que se admiten los vectores de nudos no bloqueados, pero que estos se convertirán a nudos bloqueados, con los cambios correspondientes aplicados al punto de control y a los datos de grosor.
___
## Archivo de ejemplo



