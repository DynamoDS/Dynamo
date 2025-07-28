## 詳細
PullOntoSurface は、入力された曲線を入力されたサーフェスに投影して新しい曲線を作成します。そのサーフェスの法線ベクトルを投影方向として使用します。次の例では、まず Surface.BySweep ノードで、正弦曲線に基づいて生成された曲線を使用してサーフェスを作成します。このサーフェスは、PullOntoSurface ノードでプル先の基準サーフェスとして使用されます。曲線については、Code Block を使用して中心点の座標を指定し、数値スライダを使用して半径をコントロールすることで、円を作成します。結果として、円がサーフェスに投影されます。
___
## サンプル ファイル

![PullOntoSurface](./Autodesk.DesignScript.Geometry.Curve.PullOntoSurface_img.jpg)

