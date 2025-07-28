<!--- Autodesk.DesignScript.Geometry.NurbsCurve.ByControlPointsWeightsKnots --->
<!--- T6GEU2COB3ZCMHPIT6WYQEY7NOLFALMOFIPSGLNKU5GNGESBEB7Q --->
## Em profundidade
`NurbsCurve.ByControlPointsWeightsKnots` nos permite controlar manualmente os pesos e os nós de uma NurbsCurve. A lista de pesos deve ter o mesmo comprimento que a lista de pontos de controle. O tamanho da lista de nós deve ser igual ao número de pontos de controle mais o grau mais 1.

No exemplo abaixo, primeiro criamos uma NurbsCurve interpolando entre uma série de pontos aleatórios. Usamos nós, pesos e pontos de controle para encontrar as partes correspondentes dessa curva. Podemos usar `List.ReplaceItemAtIndex` para modificar a lista de pesos. Finalmente, usamos `NurbsCurve.ByControlPointsWeightsKnots` para recriar uma NurbsCurve com os pesos modificados.

___
## Arquivo de exemplo

![ByControlPointsWeightsKnots](./T6GEU2COB3ZCMHPIT6WYQEY7NOLFALMOFIPSGLNKU5GNGESBEB7Q_img.jpg)

