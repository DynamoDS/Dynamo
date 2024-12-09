<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.RemoveReflections --->
<!--- B6UBJT6X5TJMS4P6CSS7JRJI6HDOCJMIND4VHXATYF2L5IPVPQBQ --->
## In-Depth
`TSplineSurface.RemoveReflections` は、入力 `tSplineSurface` から反射を削除します。反射を削除しても形状は変更されませんが、ジオメトリの反射部分間の依存関係が解除されるため、個別に編集できるようになります。

次の例では、まず軸反射と放射状反射を適用することによって T スプライン サーフェスを作成します。次にサーフェスは `TSplineSurface.RemoveReflections` ノードに渡され、反射が削除されます。後の変更への影響を示すために、`TSplineSurface.MoveVertex` ノードを使用して頂点の 1 つが移動されます。サーフェスから反射が削除されているため、1 つの頂点のみが変更されます。

## サンプル ファイル

![Example](./B6UBJT6X5TJMS4P6CSS7JRJI6HDOCJMIND4VHXATYF2L5IPVPQBQ_img.jpg)
