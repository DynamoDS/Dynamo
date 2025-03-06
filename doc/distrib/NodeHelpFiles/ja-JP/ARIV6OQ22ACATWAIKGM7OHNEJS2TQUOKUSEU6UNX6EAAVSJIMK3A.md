<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.CompressIndexes --->
<!--- ARIV6OQ22ACATWAIKGM7OHNEJS2TQUOKUSEU6UNX6EAAVSJIMK3A --->
## 詳細
ノード `TSplineSurface.CompressIndexes` は、面の削除などのさまざまな操作の結果生じた、T スプライン サーフェスのエッジ、頂点、または面のインデックス番号のギャップを削除します。インデックスの順序は保持されます。

次の例では、形状のエッジ インデックスに影響を与えるクワッドボールのプリミティブ サーフェスから多数の面を削除します。形状のエッジ インデックスを修復するために `TSplineSurface.CompressIndexes` が使用され、その結果インデックス 1 のエッジが選択可能になります。

## サンプル ファイル

![Example](./ARIV6OQ22ACATWAIKGM7OHNEJS2TQUOKUSEU6UNX6EAAVSJIMK3A_img.jpg)
