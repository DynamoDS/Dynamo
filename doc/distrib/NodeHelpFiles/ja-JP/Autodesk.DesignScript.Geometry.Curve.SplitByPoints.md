## 詳細
SplitByPoints は、入力された曲線を指定された点で分割し、結果のセグメントのリストを返します。指定された点が曲線上にない場合、このノードは入力された点に最も近い曲線上の点を求めて、その結果の点で曲線を分割します。次の例では、まず ByPoints ノードで、ランダムに生成された点のセットを入力として使用して NurbsCurve を作成します。同じ点のセットを、SplitByPoints ノードの点のリストとして使用します。結果は、生成された点間の曲線セグメントのリストです。
___
## サンプル ファイル

![SplitByPoints](./Autodesk.DesignScript.Geometry.Curve.SplitByPoints_img.jpg)

