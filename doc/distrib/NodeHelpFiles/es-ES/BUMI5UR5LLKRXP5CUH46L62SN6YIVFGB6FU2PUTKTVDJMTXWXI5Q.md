## En detalle:

En el ejemplo siguiente, una superficie de T-Spline se compara con una arista de una superficie de BRep mediante un nodo `TSplineSurface.CreateMatch(tSplineSurface,tsEdges,brepEdges)`. La entrada mínima necesaria para el nodo es la base `tSplineSurface`, un conjunto de aristas de la superficie especificadas en la entrada `tsEdges` y una arista o una lista de aristas, especificadas en la entrada `brepEdges`. Las siguientes entradas controlan los parámetros de la coincidencia:
- `continuity` permite establecer el tipo de continuidad de la coincidencia. La entrada espera los valores 0, 1 o 2, que corresponden a la continuidad posicional G0, tangente G1 y de curvatura G2.
- `useArcLength` controla las opciones del tipo de alineación. Si se establece en "True" (verdadero), se utiliza la longitud de arco como tipo de alineación. Esta alineación minimiza la distancia física entre cada punto de la superficie de T-Spline y el punto correspondiente de la curva. Si se especifica "False" (falso), se utiliza el tipo de alineación paramétrico; cada punto de la superficie de T-Spline se ajusta a un punto de distancia paramétrica comparable a lo largo de la curva objetivo de ajuste.
-`useRefinement`: cuando se establece en "True" (verdadero), añade puntos de control a la superficie en un intento de igualar el objetivo dentro de un determinado valor de `refinementTolerance`.
- `numRefinementSteps` is the maximum number of times that the base T-Spline surface is subdivided
while attempting to reach `refinementTolerance`. Both `numRefinementSteps` and `refinementTolerance` will be ignored if the `useRefinement` is set to False.
- `usePropagation` controls how much of the surface is affected by the match. When set to False, the surface is minimally affected. When set to True, the surface is affected within the provided `widthOfPropagation` distance.
- `scale` is the Tangency Scale which affects results for G1 and G2 continuity.
- `flipSourceTargetAlignment` reverses the alignment direction.


## Archivo de ejemplo

![Example](./BUMI5UR5LLKRXP5CUH46L62SN6YIVFGB6FU2PUTKTVDJMTXWXI5Q_img.gif)
