## En detalle:

En el ejemplo siguiente, una superficie de T-Spline se compara con una curva NURBS mediante
el nodo `TSplineSurface.CreateMatch(tSplineSurface,tsEdges,curves)`. La entrada mínima necesaria para el nodo
es la base `tSplineSurface`, un conjunto de aristas de la superficie, especificado en la entrada `tsEdges`, y una curva o
una lista de curvas.
Las siguientes entradas controlan los parámetros de la coincidencia:
- `continuity` permite establecer el tipo de continuidad para la coincidencia. La entrada espera los valores 0, 1 o 2, que corresponden a la continuidad posicional G0, tangente G1 y de curvatura G2. Sin embargo, para igualar una superficie con una curva, solo está disponible la G0 (valor de entrada 0).
- `useArcLength` controla las opciones de tipo de alineación. Si se establece en "True" (verdadero), se utiliza la longitud del arco
como tipo de alineación. Esta alineación minimiza la distancia física entre cada punto de la superficie de T-Spline y
el punto correspondiente en la curva. Si el valor de la entrada se establece en "False" (falso), se utiliza paramétrico como tipo de alineación;
cada punto de la superficie de T-Spline se compara con un punto de distancia paramétrica comparable a lo largo de la
curva objetivo de coincidencia.
- `useRefinement`: si se establece en "True" (verdadero), añade puntos de control a la superficie para intentar igualar el objetivo
dentro del valor especificado de `refinementTolerance`.
- `numRefinementSteps` es el número máximo de veces que se subdivide la superficie base de T-Spline
al intentar alcanzar `refinementTolerance`. Tanto `numRefinementSteps` como `refinementTolerance` se omitirán si `useRefinement` se establece en "False" (falso).
- `usePropagation` controla la cantidad de superficie que se ve afectada por la coincidencia. Si se establece en "False" (falso), la superficie se ve afectada de forma mínima. Si se establece en "True" (verdadero), la superficie se ve afectada dentro de la distancia de `widthOfPropagation` especificada.
- `scale` es la escala de tangencia que afecta a los resultados de la continuidad G1 y G2.
- `flipSourceTargetAlignment` invierte la dirección de la alineación.


## Archivo de ejemplo

![Example](./6ICXLN4V6DNK5KMYTY5LPCJBE27IRW5VOBKCCVFQGO3HST752ZNQ_img.gif)
