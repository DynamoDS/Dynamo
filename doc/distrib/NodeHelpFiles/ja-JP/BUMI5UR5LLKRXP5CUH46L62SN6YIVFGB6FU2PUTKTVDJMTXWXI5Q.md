## 詳細

次の例では、`TSplineSurface.CreateMatch (tSplineSurface, tsEdges, brepEdges)` ノードを使用して、T スプライン サーフェスを BRep サーフェスのエッジと一致させます。このノードに必要な最小限の入力は、ベース `tSplineSurface`、`tsEdges` 入力に指定されるサーフェスのエッジのセット、`brepEdges` 入力で指定されるエッジまたはエッジのリストです。次の入力が一致のパラメータをコントロールします。
- `continuity` では、一致の連続性タイプを設定できます。入力には、G0 位置連続性、G1 接線連続性、G2 曲率連続性に対応する値 0、1、2 を使用します。
- `useArcLength` は位置合わせタイプのオプションをコントロールします。True に設定すると、使用される位置合わせタイプは弧長になります。この位置合わせでは、T スプライン サーフェスの各点と曲線上の対応する点との間の物理的な距離が最小化されます。False 入力が指定されると、位置合わせタイプはパラメータ制御になり、T スプライン サーフェス上の各点は、一致ターゲット曲線上の、互換性のあるパラメータで指定した距離の点と一致します。
-`useRefinement` を True に設定すると、指定した `refinementTolerance` 内でターゲットに一致するように、サーフェスに制御点が追加されます
- `numRefinementSteps` is the maximum number of times that the base T-Spline surface is subdivided
while attempting to reach `refinementTolerance`. Both `numRefinementSteps` and `refinementTolerance` will be ignored if the `useRefinement` is set to False.
- `usePropagation` controls how much of the surface is affected by the match. When set to False, the surface is minimally affected. When set to True, the surface is affected within the provided `widthOfPropagation` distance.
- `scale` is the Tangency Scale which affects results for G1 and G2 continuity.
- `flipSourceTargetAlignment` reverses the alignment direction.


## サンプル ファイル

![Example](./BUMI5UR5LLKRXP5CUH46L62SN6YIVFGB6FU2PUTKTVDJMTXWXI5Q_img.gif)
