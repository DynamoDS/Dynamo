## En detalle
`Mesh.MakeWatertight` genera una malla sólida, estanca e imprimible en 3D mediante el muestreo de la malla original. Proporciona una forma rápida de resolver una malla con numerosos problemas como autointersecciones, solapamientos y geometría no múltiple. El método calcula un campo de distancia de banda fina y genera una nueva malla mediante el algoritmo de cubos de marcha, pero no vuelve a proyectarse sobre la malla original. Este método es más adecuado para objetos de malla que tienen numerosos defectos o problemas difíciles como las autointersecciones.
En el ejemplo siguiente, se muestra un jarrón no hermético y su equivalente hermético.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.MakeWatertight_img.jpg)
