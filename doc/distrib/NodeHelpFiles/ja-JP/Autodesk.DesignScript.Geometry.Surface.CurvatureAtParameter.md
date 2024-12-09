## 詳細
CurvatureAtParameter は、入力された U パラメータと V パラメータを使用して、サーフェス上の UV 位置の法線、U 方向、V 方向に基づく座標系を返します。Normal ベクトルは Z 軸を決定し、U 方向と Y 方向は X 軸と Y 軸を決定します。軸の長さは U と V の曲率で決まります。次の例では、まず BySweep2Rails を使用してサーフェスを作成します。次に、CurvatureAtParameter ノードを使用して、2 つの数値スライダで U パラメータと V パラメータを決定し、CoordinateSystem を作成しています。
___
## サンプル ファイル

![CurvatureAtParameter](./Autodesk.DesignScript.Geometry.Surface.CurvatureAtParameter_img.jpg)

