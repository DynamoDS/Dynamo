## 深入資訊

以下範例使用 `TSplineSurface.CreateMatch(tSplineSurface,tsEdges,curves)` 節點
將 T 雲形線曲面與 NURBS 曲線相配。
節點最少需要輸入基準 `tSplineSurface`，即 `tsEdges` 輸入提供的一組曲面邊，以及曲線或
曲線清單。
以下輸入控制相配的參數:
- `continuity` 允許設定相配的連續性類型。輸入必須是值 0、1 或 2，對應到 G0 位置、G1 切線和 G2 曲率連續性。但是，如果是將曲面與曲線相配，只能使用 G0 (輸入值 0)。
- `useArcLength` 控制對齊類型選項。如果設定為 True，使用的對齊類型為「弧長」。
此對齊方式會讓 T 雲形線曲面每個點與
曲線上對應點之間的實體距離變得最小。如果提供 False 輸入，對齊類型為「參數式」-
表示 T 雲形線曲面上的每個點，都會與沿著配對目標曲線的可比較參數式距離
的點相配。
- `useRefinement` 設定為 True 時，會在曲面中增加控制點，以嘗試在給定的 `refinementTolerance` 內
與目標相配
- `numRefinementSteps` 是嘗試達到 `refinementTolerance` 時，細分基準 T 雲形線曲面
的最大次數。如果將 `useRefinement` 設定為 False，則會同時忽略 `numRefinementSteps` 和 `refinementTolerance`。
- `usePropagation` 控制曲面受到相配影響的程度。設定為 False 時，曲面受到的影響最小。設定為 True 時，曲面則受到提供的 `widthOfPropagation` 距離內的影響。
- `scale` 是相切比例，會影響 G1 和 G2 連續性的結果。
- `flipSourceTargetAlignment` 會反轉對齊方向。


## 範例檔案

![Example](./6ICXLN4V6DNK5KMYTY5LPCJBE27IRW5VOBKCCVFQGO3HST752ZNQ_img.gif)
