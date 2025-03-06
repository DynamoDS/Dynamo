## 詳細
ParameterAtPoint は、曲線上の指定された点のパラメータ値を返します。入力された点が曲線上にない場合、ParameterAtPoint は入力点に近い曲線上の点のパラメータを返します。次の例では、まず ByControlPoints ノードで、ランダムに生成された点のセットを入力として使用して NurbsCurve を作成します。Code Block で X 座標と Y 座標を指定して、もう 1 つ点を作成します。ParameterAtPoint ノードは、入力された点に最も近い点における曲線上のパラメータを返します。
___
## サンプル ファイル

![ParameterAtPoint](./Autodesk.DesignScript.Geometry.Curve.ParameterAtPoint_img.jpg)

