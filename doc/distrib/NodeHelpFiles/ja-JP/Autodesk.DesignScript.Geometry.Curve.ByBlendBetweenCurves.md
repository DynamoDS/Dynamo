## 詳細
Curve.ByBlendBetweenCurves は、入力された 2 つの曲線を接続する新しい曲線を作成します。2 つの「endOrStart」入力により、接続するのが各曲線の終点か始点かを決定します。作成された曲線は、接続点で元の曲線の曲率と一致します。次の例では、まず 1 つの円弧と 1 つの線分を作成します。その円弧の始点と線分の終点の間でブレンドして、2 つの曲線を接続しています。2 つのブール値切り替えノードで、2 つの曲線についてどちらの端点をブレンドするかをコントロールできます。
___
## サンプル ファイル

![ByBlendBetweenCurves](./Autodesk.DesignScript.Geometry.Curve.ByBlendBetweenCurves_img.jpg)

