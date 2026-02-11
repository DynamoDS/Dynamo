## Em profundidade
`Cuboid.Height` devolve a altura do cuboide de entrada. Observação: Se o cuboide tiver sido transformado para um sistema de coordenadas diferente com um fator de escala, isso devolverá as dimensões originais do cuboide, não as dimensões do espaço universal. Em outras palavras, se você criar um cuboide com uma largura (eixo X) de 10 e transformá-lo em um CoordinateSystem com duas vezes o dimensionamento em X, a largura ainda será 10.

No exemplo abaixo, geramos um cuboide pelos cantos e, em seguida, usamos um nó `Cuboid.Height` para encontrar sua altura.

___
## Arquivo de exemplo

![Height](./Autodesk.DesignScript.Geometry.Cuboid.Height_img.jpg)

