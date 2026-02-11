## Em profundidade
`List.RemoveIfNot` retorna uma lista que retém itens que correspondem ao tipo de elemento especificado e remove todos os outros itens da lista original.

Pode ser necessário usar o caminho completo do nó, como `Autodesk.DesignScript.Geometry.Surface`, na entrada `type` para remover itens. Para recuperar os caminhos para itens da lista, você pode inserir sua lista em um nó `Object.Type`.

No exemplo abaixo, `List.RemoveIfNot` retorna uma lista com uma linha, removendo os elementos de ponto da lista original porque eles não coincidem com o tipo especificado.
___
## Arquivo de exemplo

![List.RemoveIfNot](./List.RemoveIfNot_img.jpg)
