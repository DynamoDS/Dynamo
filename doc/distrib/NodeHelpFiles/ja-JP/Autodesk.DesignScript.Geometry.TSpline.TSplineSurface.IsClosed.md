## 詳細
閉じたサーフェスとは、開口部や境界のない完全な形状を形成するサーフェスです。
次の例では、`TSplineSurface.BySphereCenterPointRadius` を介して生成された T スプライン球体が開いているかどうかを確認するために `TSplineSurface.IsClosed` を使用して検査し、負の結果が返されます。これは、T スプライン球体が、閉じているように見えますが、実際には極で開いており、複数のエッジと頂点が 1 点で重なっているためです。

T スプライン球体内のギャップは `TSplineSurface.FillHole` ノードを使用して埋められます。これにより、サーフェスが埋められた場所でわずかな変形が生じます。`TSplineSurface.IsClosed` ノードを使用して再度チェックすると、閉じていることを意味する正の結果が返されます。
___
## サンプル ファイル

![TSplineSurface.IsClosed](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.IsClosed_img.jpg)
