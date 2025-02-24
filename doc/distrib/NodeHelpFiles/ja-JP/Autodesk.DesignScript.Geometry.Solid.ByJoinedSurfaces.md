## 詳細
Solid.ByJoinedSurfaces は、サーフェスのリストを入力として取得し、そのサーフェスによって定義される単一のソリッドを返します。サーフェスは閉じたサーフェスを定義する必要があります。次の例では、まず下部ジオメトリとして円を作成します。その円をパッチしてサーフェスを作成し、そのサーフェスを Z 方向に移動します。次に、円を押し出して側面を生成します。List.Create を使用して下部、側面、上部のサーフェスで構成されるリストを作成した後に、ByJoinedSurfaces を使用してそのリストを単一の閉じたソリッドに変換します。
___
## サンプル ファイル

![ByJoinedSurfaces](./Autodesk.DesignScript.Geometry.Solid.ByJoinedSurfaces_img.jpg)

