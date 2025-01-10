## 詳細
Surface.ByRevolve は、軸を中心に指定されたプロファイル曲線を回転してサーフェスを作成します。軸は、axisOrigin の点と axisDirection のベクトルで定義されます。startAngle はサーフェスを開始する位置(度単位で測定)を決定し、sweepAngle は軸を中心としてサーフェスが続く角度を決定します。次の例では、余弦関数で生成された曲線をプロファイル曲線として使用し、startAngle と sweepAngle をコントロールするために 2 つの数値スライダを使用します。axisOrigin と axisDirection は、既定値(このサンプルではワールド座標系の原点と Z 軸)のままになっています。
___
## サンプル ファイル

![ByRevolve](./Autodesk.DesignScript.Geometry.Surface.ByRevolve_img.jpg)

