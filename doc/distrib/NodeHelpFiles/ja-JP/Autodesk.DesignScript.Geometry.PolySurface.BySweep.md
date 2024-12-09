## 詳細
`PolySurface.BySweep (rail, crossSection)` は、レールに沿って接続された交差しない線分のリストをスイープして、PolySurface を返します。`crossSection` 入力が受け取る曲線のリストは、始点または終点で接続されている必要があります。接続されていない場合、ノードは PolySurface を返しません。`PolySurface.BySweep (rail, profile)` に似ていますが、唯一異なるのは、`profile` が 1 つの曲線のみを取得するのに対し、このノードの `crossSection` 入力は曲線のリストを取得する点です。

次の例では、円弧に沿ってスイープして PolySurface を作成します。


___
## サンプル ファイル

![PolySurface.BySweep](./Autodesk.DesignScript.Geometry.PolySurface.BySweep_img.jpg)
