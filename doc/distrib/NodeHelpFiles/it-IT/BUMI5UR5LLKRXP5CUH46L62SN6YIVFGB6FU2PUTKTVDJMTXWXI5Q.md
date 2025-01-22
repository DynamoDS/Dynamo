## In profondità

Nell'esempio seguente, una superficie T-Spline viene associata ad un bordo di una superficie BRep utilizzando un nodo `TSplineSurface.CreateMatch(tSplineSurface,tsEdges,brepEdges)`. L'input minimo richiesto per il nodo è il valore di base `tSplineSurface`, un gruppo di bordi della superficie fornito nell'input `tsEdges`, e un bordo o un elenco di bordi, forniti nell'input `brepEdges`. I seguenti input controllano i parametri della corrispondenza:
- `continuity` consente di impostare il tipo di continuità per la corrispondenza. L'input prevede valori 0, 1 o 2, corrispondenti alla continuità G0 posizionale, G1 tangente e G2 curvatura.
- `useArcLength` controlla le opzioni del tipo di allineamento. Se impostato su True, il tipo di allineamento utilizzato è lunghezza dell'arco. Questo allineamento riduce al minimo la distanza fisica tra ogni punto della superficie T-Spline e il punto corrispondente sulla curva. Quando viene fornito l'input False, il tipo di allineamento è parametrico, ossia ogni punto sulla superficie T-Spline viene abbinato ad un punto di distanza parametrica comparabile lungo la curva di destinazione di corrispondenza.
-`useRefinement`, quando è impostato su True, aggiunge punti di controllo alla superficie nel tentativo di corrispondere alla destinazione entro un determinato valore `refinementTolerance`
- `numRefinementSteps` is the maximum number of times that the base T-Spline surface is subdivided
while attempting to reach `refinementTolerance`. Both `numRefinementSteps` and `refinementTolerance` will be ignored if the `useRefinement` is set to False.
- `usePropagation` controls how much of the surface is affected by the match. When set to False, the surface is minimally affected. When set to True, the surface is affected within the provided `widthOfPropagation` distance.
- `scale` is the Tangency Scale which affects results for G1 and G2 continuity.
- `flipSourceTargetAlignment` reverses the alignment direction.


## File di esempio

![Example](./BUMI5UR5LLKRXP5CUH46L62SN6YIVFGB6FU2PUTKTVDJMTXWXI5Q_img.gif)
