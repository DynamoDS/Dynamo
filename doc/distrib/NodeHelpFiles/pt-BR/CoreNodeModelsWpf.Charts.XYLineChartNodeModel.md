## Em profundidade

O gráfico de linha XY cria um gráfico com uma ou mais linhas plotadas por seus valores x e y. Rotule as linhas ou altere o número de linhas inserindo uma lista de legendas de sequência de caracteres na entrada de legendas. Cada legenda cria uma nova linha codificada por cores. Se você inserir apenas um valor de sequência de caracteres, apenas uma linha será criada.

Para determinar o posicionamento de cada ponto ao longo de cada linha, use uma lista de listas contendo valores duplos para as entradas de valores x e y. Deve haver um número igual de valores nas entradas de valores x e y. O número de sublistas também deve corresponder ao número de valores de sequência de caracteres na entrada de legendas.
Por exemplo, se você desejar criar 3 linhas, cada uma com 5 pontos, forneça uma lista com 3 valores de sequência de caracteres na entrada de legendas para nomear cada linha e forneça 3 sublistas com 5 valores duplos em cada uma para os valores x e y.

Para atribuir uma cor para cada linha, insira uma lista de cores na entrada de cores. Ao atribuir cores personalizadas, o número de cores deve corresponder ao número de valores de sequência de caracteres na entrada de legendas. Se nenhuma cor for atribuída, serão usadas cores aleatórias.

___
## Arquivo de exemplo

![XY Line Plot](./CoreNodeModelsWpf.Charts.XYLineChartNodeModel_img.jpg)

