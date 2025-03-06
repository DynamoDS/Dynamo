## 深入資訊

以下範例使用 `TSplineSurface.CreateMatch(tSplineSurface,tsEdges,brepEdges)` 節點將 T 雲形線曲面與 BRep 曲面的邊相配。節點最少需要輸入基準 `tSplineSurface`，即 `tsEdges` 輸入提供的一組曲面邊，以及 `brepEdges` 輸入提供的邊或邊清單。以下輸入控制相配的參數:
- `continuity` 允許設定相配的連續性類型。輸入必須是 0、1 或 2，對應到 G0 位置、G1 切線和 G2 曲率連續性。
- `useArcLength` 控制對齊類型選項。如果設定為 True，使用的對齊類型為「弧長」。此對齊方式會讓 T 雲形線曲面每個點與曲線上對應點之間的實體距離變得最小。如果提供 False 輸入，對齊類型為「參數式」 - 表示 T 雲形線曲面上的每個點，都會與沿著配對目標曲線的可比較參數式距離的點相配。
-`useRefinement` 設定為 True 時，會在曲面中增加控制點，以嘗試在給定的 `refinementTolerance` 內與目標相配
- `numRefinementSteps` is the maximum number of times that the base T-Spline surface is subdivided
while attempting to reach `refinementTolerance`. Both `numRefinementSteps` and `refinementTolerance` will be ignored if the `useRefinement` is set to False.
- `usePropagation` controls how much of the surface is affected by the match. When set to False, the surface is minimally affected. When set to True, the surface is affected within the provided `widthOfPropagation` distance.
- `scale` is the Tangency Scale which affects results for G1 and G2 continuity.
- `flipSourceTargetAlignment` reverses the alignment direction.


## 範例檔案

![Example](./BUMI5UR5LLKRXP5CUH46L62SN6YIVFGB6FU2PUTKTVDJMTXWXI5Q_img.gif)
