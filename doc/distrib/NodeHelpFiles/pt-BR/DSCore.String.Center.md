## Em profundidade
Polygon Center localiza o centro de um polígono determinado tomando o valor médio dos cantos. Para polígonos côncavos, é possível que o centro esteja realmente fora do polígono. No exemplo abaixo, primeiro geramos uma lista de ângulos e raios aleatórios para usar como entradas para Point By Cylindrical Coordinates. Classificando os ângulos primeiro, garantimos que o polígono resultante será conectado em ordem crescente de ângulo e, portanto, não fará interseção. Em seguida, podemos usar o Center para usar a média dos pontos e encontrar o centro do polígono.
___
## Arquivo de exemplo

![Center](./DSCore.String.Center_img.jpg)

