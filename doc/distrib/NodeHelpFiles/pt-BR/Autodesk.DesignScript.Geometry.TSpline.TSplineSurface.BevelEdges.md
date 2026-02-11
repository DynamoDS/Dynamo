## In-Depth
O nó `TSplineSurface.BevelEdges` desloca uma aresta selecionada ou um grupo de arestas em ambas as direções ao longo da face, substituindo a aresta original por uma sequência de arestas que formam um canal.

No exemplo abaixo, um grupo de arestas de um primitivo de caixa da T-Spline é usado como entrada para o nó `TSplineSurface.BevelEdges`. O exemplo ilustra como as seguintes entradas afetam o resultado:
- `percentage` controla a distribuição das arestas recém-criadas ao longo das faces vizinhas, posicionando novas arestas adjacentes com valores zero mais próximos da aresta original e os valores próximos a 1 mais distantes.
- `numberOfSegments` controla o número de novas faces no canal.
- `keepOnFace` define se as arestas de chanfro são colocadas no plano da face original. Se o valor estiver definido como True, a entrada de arredondamento não terá efeito.
- `roundness` controla o grau de arredondamento do chanfro e espera um valor no intervalo entre 0 e 1, com 0 resultando em um chanfro reto e 1 retornando um chanfro arredondado.

O modo de caixa é ocasionalmente ativado para obter uma melhor compreensão da forma.


## Arquivo de exemplo

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BevelEdges_img.gif)
