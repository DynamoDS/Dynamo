<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.UnweldVertices --->
<!--- D6ESVRBW5SSM4F7J65W7OKR5DTKJAFW5F5AP23FRRR3MGXKIFVOQ --->
## In-Depth
`TSplineSurface.UnweldEdges` と同様に、このノードは頂点のセットに対して連結解除操作を実行します。結果として、選択した頂点で結合するすべてのエッジが連結解除されます。接続を維持しながら頂点の周囲に急激な遷移が発生する折り目解除操作とは異なり、連結解除は不連続性を発生させます。

次の例では、T スプライン平面の選択した頂点の 1 つが `TSplineSurface.UnweldVertices` ノードを使用して連結解除されます。選択した頂点を囲むエッジに沿って不連続が発生します。これは、`TSplineSurface.MoveVertices` ノードで頂点を引き上げることで示されています。

## サンプル ファイル

![Example](./D6ESVRBW5SSM4F7J65W7OKR5DTKJAFW5F5AP23FRRR3MGXKIFVOQ_img.jpg)
