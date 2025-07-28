<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.AddReflections --->
<!--- 6YGBDRGYLRW3BW4XJHLHBBRSCHYWA2UCJ5FQAESHDY2HMUBDUSLQ --->
## In-Depth
`TSplineSurface.AddReflections` は、入力 `tSplineSurface` に 1 つまたは複数の反射を適用して、新しい T スプライン サーフェスを作成します。ブール値入力 `weldSymmetricPortions` は、反射によって生成された折り目が付いたエッジをスムーズにするのか、保持するのかを決定します。

次の例では、`TSplineSurface.AddReflections` ノードを使用して T スプライン サーフェスに複数の反射を追加する方法を示します。軸と放射状の 2 つの反射が作成されます。基本ジオメトリは、円弧のパスを持つスイープの形状の T スプライン サーフェスです。2 つの反射は、リストに結合され、反射する基本ジオメトリとともに、`TSplineSurface.AddReflections` ノードの入力として使用されます。T スプライン サーフェスは連結され、折り目が付けられたエッジがないスムーズな T スプライン サーフェスになります。サーフェスは、`TSplineSurface.MoveVertex` ノードを使用して 1 つの頂点を移動することによってさらに変更されます。T スプライン サーフェスに適用される反射により、頂点の移動は 16 回再現されます。

## サンプル ファイル

![Example](./6YGBDRGYLRW3BW4XJHLHBBRSCHYWA2UCJ5FQAESHDY2HMUBDUSLQ_img.jpg)
