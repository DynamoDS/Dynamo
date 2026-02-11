## 詳細
Extend は、入力された曲線を入力された距離だけ延長します。pickSide 入力は、曲線の始点または終点を入力として取得し、曲線のどちらの端点を延長するかを決定します。次の例では、まず ByControlPoints ノードで、ランダムに生成された点のセットを入力として使用して NurbsCurve を作成します。Query のノードである Curve.EndPoint を使用し、「pickSide」入力として使用する曲線の終点を求めます。数値スライダで、延長の距離をコントロールできます。
___
## サンプル ファイル

![Extend](./Autodesk.DesignScript.Geometry.Curve.Extend_img.jpg)

