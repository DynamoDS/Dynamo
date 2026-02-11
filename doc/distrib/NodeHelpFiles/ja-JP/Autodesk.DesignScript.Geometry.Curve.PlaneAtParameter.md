## 詳細
PlaneAtParameter は、指定されたパラメータの位置で、曲線に位置合わせされた平面を返します。結果の平面の法線ベクトルは、曲線の接線に対応します。曲線はパラメータ化されて、0 ～ 1 の範囲で計測され、0 が曲線の始点、1 が終点を表します。次の例では、まず ByControlPoints ノードで、ランダムに生成された点のセットを入力として使用して NurbsCurve を作成します。範囲が 0 ～ 1 に設定された数値スライダを使用して、PlaneAtParameter ノードのパラメータの入力をコントロールしています。
___
## サンプル ファイル

![PlaneAtParameter](./Autodesk.DesignScript.Geometry.Curve.PlaneAtParameter_img.jpg)

