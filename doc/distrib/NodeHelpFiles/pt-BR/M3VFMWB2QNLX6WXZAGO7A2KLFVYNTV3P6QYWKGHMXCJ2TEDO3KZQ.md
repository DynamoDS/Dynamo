## In-Depth
`TSplineSurface.BuildPipes` gera uma superfície de tubulação da T-Spline usando uma rede de curvas. As tubulações individuais serão consideradas unidas se seus pontos finais estiverem dentro da tolerância máxima definida pela entrada `snappingTolerance`. O resultado desse nó poderá ser ajustado com um conjunto de entradas que permitem definir valores para todas as tubulações ou individualmente se a entrada for uma lista com o mesmo comprimento do número de tubulações. As seguintes entradas podem ser usadas dessa forma: `segmentsCount`, `startRotations`, `endRotations`, `startRadii`, `endRadii`, `startPositions` e `endPositions`.

No exemplo abaixo, três curvas unidas em pontos finais são fornecidas como entrada para o nó `TSplineSurface.BuildPipes`. `defaultRadius`, neste caso, é um valor único para todas as três tubulações, definindo o raio das tubulações por padrão, a não ser que os raios inicial e final sejam fornecidos.
Em seguida, `segmentsCount` define três valores diferentes para cada tubulação individual. A entrada é uma lista de três valores, cada um correspondente a uma tubulação.

Mais ajustes ficarão disponíveis se `autoHandleStart` e `autoHandleEnd` estiverem definidos como False. Isso permite o controle sobre as rotações inicial e final de cada tubulação (entradas`startRotations` e `endRotations`), bem como os raios no final e no início de cada tubulação, especificando `startRadii` e `endRadii`. Por fim, `startPositions` e `endPositions` permitem o deslocamento dos segmentos no início ou no fim de cada curva, respectivamente. Essa entrada espera um valor correspondente ao parâmetro da curva em que os segmentos começam ou terminam (valores entre 0 e 1).

## Arquivo de exemplo
![Example](./M3VFMWB2QNLX6WXZAGO7A2KLFVYNTV3P6QYWKGHMXCJ2TEDO3KZQ_img.gif)
