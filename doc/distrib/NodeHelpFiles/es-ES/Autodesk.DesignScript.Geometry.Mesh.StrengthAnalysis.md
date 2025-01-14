## En detalle
 El nodo `Mesh.StrengthAnalysis`devuelve una lista de colores representativos para cada vértice. El resultado se puede utilizar junto con el nodo`Mesh.ByMeshColor`. Las áreas más fuertes de la malla se muestran de color verde, mientras que las áreas más débiles se indican mediante un mapa de calor de amarillo a rojo. El análisis puede dar lugar a falsos positivos si la malla es demasiado gruesa o irregular (es decir, si tiene muchos triángulos largos y finos). Puede probar a utilizar`Mesh.Remesh` para generar una malla normal antes de llamar a`Mesh.StrengthAnalysis` para generar mejores resultados.

En el ejemplo siguiente, se utiliza `Mesh.StrengthAnalysis` para codificar por colores la resistencia estructural de una malla en forma de rejilla. El resultado es una lista de colores que coinciden con la longitud de los vértices de la malla. Esta lista puede utilizarse con el nodo `Mesh.ByMeshColor` para colorear la malla.

## Archivo de ejemplo

![Example](./Autodesk.DesignScript.Geometry.Mesh.StrengthAnalysis_img.jpg)
