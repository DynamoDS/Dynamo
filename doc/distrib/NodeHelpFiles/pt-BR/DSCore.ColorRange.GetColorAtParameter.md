## Em profundidade
GetColorAtParameter obtém um intervalo de cores 2D de entrada e retorna uma lista de cores em parâmetros UV especificados no intervalo de 0 a 1. No exemplo abaixo, primeiro criamos um intervalo de cores 2D usando um nó ByColorsAndParameters com uma lista de cores e uma lista de parâmetros para definir o intervalo. Um bloco de código é usado para gerar um intervalo de números entre 0 e 1, que é usado como entradas u e v em um nó UV.ByCoordinates. A treliça desse nó é definida como produto cartesiano. Um conjunto de cubos é criado de forma semelhante, na qual um nó Point.ByCoordinates com treliça do produto cartesiano é usado para criar uma matriz de cubos. Em seguida, usamos um nó Display.ByGeometryColor com a matriz de cubos e a lista de cores obtidas do nó GetColorAtParameter.
___
## Arquivo de exemplo

![GetColorAtParameter](./DSCore.ColorRange.GetColorAtParameter_img.jpg)

