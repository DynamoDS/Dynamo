## Em profundidade
`List.IsUniformDepth` retorna um valor booleano baseado em se a profundidade da lista é consistente ou não, o que significa que cada lista tem o mesmo número de listas aninhadas dentro dela.

No exemplo abaixo, duas listas são comparadas, uma de profundidade uniforme e outra de profundidade não uniforme, para mostrar a diferença. A lista uniforme contém três listas sem listas aninhadas. A lista não uniforme contém duas listas. A primeira lista não tem listas aninhadas, mas a segunda tem duas listas aninhadas. As listas em [0] e [1] não são iguais em profundidade; portanto,`List.IsUniformDepth` retorna False.
___
## Arquivo de exemplo

![List.IsUniformDepth](./DSCore.List.IsUniformDepth_img.jpg)
