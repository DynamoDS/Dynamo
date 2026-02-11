<!--- Autodesk.DesignScript.Geometry.CoordinateSystem.Scale(coordinateSystem, basePoint, from, to) --->
<!--- DXEUCQG4Y4M6N76367ZJYZYJJOLPKVDC5UFPTIBMDW6CC5ORFAXQ --->
## 詳細
`CoordinateSystem.Scale (coordinateSystem, basePoint, from, to)` は、スケール変更の基準点、スケール変更元の点、スケール変更先の点に基づいてスケールされた座標系を返します。`basePoint` の入力値は、スケールの開始位置(座標系の移動量)を定義します。`from` と `to` の間の距離によって、スケールを変更する量が定義されます。

次の例では、(-1, 2, 0)の `basePoint` によってスケールの開始位置が定義されます。`from` 点(1, 1, 0)と `to` 点(6, 6, 0)の距離によって、スケールを変更する量が決まります。

___
## サンプル ファイル

![CoordinateSystem.Scale](./DXEUCQG4Y4M6N76367ZJYZYJJOLPKVDC5UFPTIBMDW6CC5ORFAXQ_img.jpg)
