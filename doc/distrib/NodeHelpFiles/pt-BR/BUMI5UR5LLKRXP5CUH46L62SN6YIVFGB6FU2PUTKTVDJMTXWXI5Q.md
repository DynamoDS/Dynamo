## Em profundidade

No exemplo abaixo, uma superfície da T-Spline é correspondida com uma aresta de uma superfície BRep usando um nó `TSplineSurface.CreateMatch(tSplineSurface,tsEdges,brepEdges)`. A entrada mínima necessária para o nó é a base `tSplineSurface`, um conjunto de arestas da superfície fornecido na entrada `tsEdges` e uma aresta ou uma lista de arestas, fornecidas na entrada `brepEdges`. As seguintes entradas controlam os parâmetros da correspondência:
- `continuity` permite definir o tipo de continuidade para a correspondência. A entrada espera valores 0, 1 ou 2, correspondentes à continuidade da posição G0, da tangente G1 e da curvatura G2.
- `useArcLength` controla as opções de tipo de alinhamento. Se definido como True, o tipo de alinhamento usado será Comprimento do arco. Esse alinhamento minimiza a distância física entre cada ponto da superfície da T-Spline e o ponto correspondente na curva. Quando a entrada False é fornecida, o tipo de alinhamento é Paramétrico – cada ponto na superfície da T-Spline coincide com um ponto de distância paramétrica comparável ao longo da curva-alvo correspondente.
-`useRefinement` quando definido como True, adiciona pontos de controle à superfície na tentativa de corresponder ao destino dentro de uma `refinementTolerance` determinada
- `numRefinementSteps` is the maximum number of times that the base T-Spline surface is subdivided
while attempting to reach `refinementTolerance`. Both `numRefinementSteps` and `refinementTolerance` will be ignored if the `useRefinement` is set to False.
- `usePropagation` controls how much of the surface is affected by the match. When set to False, the surface is minimally affected. When set to True, the surface is affected within the provided `widthOfPropagation` distance.
- `scale` is the Tangency Scale which affects results for G1 and G2 continuity.
- `flipSourceTargetAlignment` reverses the alignment direction.


## Arquivo de exemplo

![Example](./BUMI5UR5LLKRXP5CUH46L62SN6YIVFGB6FU2PUTKTVDJMTXWXI5Q_img.gif)
