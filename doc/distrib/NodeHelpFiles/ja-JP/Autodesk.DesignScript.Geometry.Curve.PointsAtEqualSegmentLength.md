## 詳細
PointsAtEqualSegmentLength は、入力された曲線をセグメントの長さが等しくなるように分割して、その曲線上の点のリストを返します。使用する分割数は整数として入力され、曲線の内部を均等に分割する数の点が生成されます。このノードには、曲線の端点は含まれません。次の例では、ランダムな数値のセットを 2 つ使用して、点のリストを生成します。この点群を使用して、ByControlPoints で NurbsCurve を作成します。整数スライダを PointsAtEqualSegmentLength ノードの divisions 入力に使用します。
___
## サンプル ファイル

![PointsAtEqualSegmentLength](./Autodesk.DesignScript.Geometry.Curve.PointsAtEqualSegmentLength_img.jpg)

