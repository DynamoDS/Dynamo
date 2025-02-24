## Em profundidade

No exemplo abaixo, uma superfície da T-Spline é correspondida com uma curva NURBS usando
o nó `TSplineSurface.CreateMatch(tSplineSurface,tsEdges,curves)`. A entrada mínima necessária para o
nó é o `tSplineSurface` base, um conjunto de arestas da superfície, fornecido na entrada `tsEdges`, e uma curva ou
lista de curvas.
As seguintes entradas controlam os parâmetros da correspondência:
- `continuity` permite definir o tipo de continuidade da correspondência. A entrada espera valores 0, 1 ou 2, correspondentes à posição G0, tangente G1 e continuidade de curvatura G2. No entanto, para corresponder uma superfície com uma curva, somente G0 (valor de entrada 0) está disponível.
- `useArcLength` controla as opções de tipo de alinhamento. Se definido como True, o tipo de alinhamento usado é comprimento do
arco. Esse alinhamento minimiza a distância física entre cada ponto da superfície da T-Spline e
o ponto correspondente na curva. Quando a entrada False é fornecida, o tipo de alinhamento é Paramétrico -
cada ponto na superfície da T-Spline é comparado a um ponto de distância paramétrico comparável ao longo da
curva-alvo correspondente.
- `useRefinement` quando definido como True, adiciona pontos de controle à superfície na tentativa de corresponder ao destino
dentro de uma `refinementTolerance` fornecida
- `numRefinementSteps` é o número máximo de vezes que a superfície da T-Spline base é subdividida
ao tentar atingir `refinementTolerance`. `numRefinementSteps` e `refinementTolerance` serão ignorados se `useRefinement` estiver definido como False.
- `usePropagation` controla quanto da superfície é afetada pela correspondência. Quando definido como False, a superfície é minimamente afetada. Quando definido como True, a superfície é afetada dentro da distância `widthOfPropagation` fornecida.
- `scale` é a escala de tangência que afeta os resultados para a continuidade G1 e G2.
- `flipSourceTargetAlignment` reverte a direção do alinhamento.


## Arquivo de exemplo

![Example](./6ICXLN4V6DNK5KMYTY5LPCJBE27IRW5VOBKCCVFQGO3HST752ZNQ_img.gif)
