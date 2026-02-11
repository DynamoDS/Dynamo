## Em profundidade
O intervalo de cores criará um gradiente entre um conjunto de cores de entrada e permitirá que as cores desse gradiente sejam selecionadas por uma lista de valores de entrada. A primeira entrada, cores, é uma lista de cores a serem usadas no gradiente. A segunda entrada, índices, determinará a localização relativa das cores de entrada no gradiente. Essa lista deve corresponder à lista de cores, cada valor estando no intervalo de 0 a 1. O valor exato não é importante, apenas a posição relativa dos valores. A cor correspondente ao valor mais baixo estará à esquerda do gradiente e a cor correspondente ao valor mais alto estará à direita do gradiente. A entrada do valor final permite ao usuário selecionar pontos ao longo do gradiente no intervalo de 0 a 1 para a saída. No exemplo abaixo, primeiro criamos duas cores: vermelho e azul. A ordem dessas cores no gradiente é determinada por uma lista que criamos com um bloco de código. Um terceiro bloco de código é usado para criar um intervalo de números entre 0 e 1 que determinará as cores de saída do gradiente. Um conjunto de cubos é gerado ao longo do eixo x e esses cubos são finalmente coloridos de acordo com o gradiente usando um nó Display.ByGeometryColor.
___
## Arquivo de exemplo

![Color Range](./CoreNodeModels.ColorRange_img.jpg)

