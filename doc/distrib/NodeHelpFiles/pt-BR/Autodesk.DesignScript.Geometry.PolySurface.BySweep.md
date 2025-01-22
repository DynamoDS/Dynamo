## Em profundidade
`PolySurface.BySweep (rail, crossSection)` retorna um PolySurface varrendo uma lista de linhas conectadas e sem interseção ao longo de um trilho. A entrada `crossSection` pode receber uma lista de curvas conectadas que devem se encontrar em um ponto inicial ou final, ou o nó não retornará uma PolySurface. Esse nó é semelhante a `PolySurface.BySweep (rail, profile)` com a única diferença é que a entrada `crossSection` usa uma lista de curvas enquanto `profile` usa apenas uma curva.

No exemplo abaixo, uma PolySurface é criada executando a varredura ao longo de um arco.


___
## Arquivo de exemplo

![PolySurface.BySweep](./Autodesk.DesignScript.Geometry.PolySurface.BySweep_img.jpg)
