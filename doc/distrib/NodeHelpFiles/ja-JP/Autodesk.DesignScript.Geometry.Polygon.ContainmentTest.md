## 詳細
ContainmentTest は、指定された点が指定されたポリゴン内に含まれるかどうかに応じて、ブール値を返します。この操作を行うには、ポリゴンが平面状になっていて、自己交差しない必要があります。次の例では、ByCylindricalCoordinates で作成された一連の点を使用してポリゴンを作成します。高さを一定にして、角度を並べ替えると、平面状で自己交差しないポリゴンになります。次に、テストする点を作成し、ContainmentTest を使用してその点がポリゴンの内側にあるか外側にあるかを確認します。
___
## サンプル ファイル

![ContainmentTest](./Autodesk.DesignScript.Geometry.Polygon.ContainmentTest_img.jpg)

