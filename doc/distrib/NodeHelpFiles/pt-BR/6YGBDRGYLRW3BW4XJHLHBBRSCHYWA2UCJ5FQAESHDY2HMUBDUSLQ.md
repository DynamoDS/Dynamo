<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.AddReflections --->
<!--- 6YGBDRGYLRW3BW4XJHLHBBRSCHYWA2UCJ5FQAESHDY2HMUBDUSLQ --->
## In-Depth
`TSplineSurface.AddReflections` cria uma nova superfície da T-Spline aplicando uma ou várias reflexões à entrada `tSplineSurface`. A entrada booleana `weldSymmetricPortions` determina se as arestas dobradas geradas pela reflexão são suavizadas ou retidas.

O exemplo abaixo ilustra como adicionar várias reflexões a uma superfície da T-Spline usando o nó `TSplineSurface.AddReflections`. Duas reflexões são criadas: axial e radial. A geometria base é uma superfície da T-Spline na forma de uma varredura com o caminho de um arco. As duas reflexões são unidas em uma lista e usadas como entrada para o nó `TSplineSurface.AddReflections`, junto com a geometria base a ser refletida. As TSplineSurfaces são soldadas, resultando em uma TSplineSurface suave sem arestas dobradas. A superfície é alterada movendo-se um vértice usando o nó `TSplineSurface.MoveVertex`. Devido à reflexão que está sendo aplicada à superfície da T-Spline, o movimento do vértice é reproduzido 16 vezes.

## Arquivo de exemplo

![Example](./6YGBDRGYLRW3BW4XJHLHBBRSCHYWA2UCJ5FQAESHDY2HMUBDUSLQ_img.jpg)
