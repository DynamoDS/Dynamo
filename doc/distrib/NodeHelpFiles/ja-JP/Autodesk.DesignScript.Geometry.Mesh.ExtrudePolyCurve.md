## 詳細
`Mesh.ExtrudePolyCurve` ノードは、指定された `polycurve` を `height` 入力で設定した距離に応じて、指定されたベクトル方向に押し出します。開いたポリカーブは、始点と終点を繋ぐことによって閉じます。元の `polycurve` が平面状で自己交差していない場合、結果として生成されるメッシュには、ソリッド メッシュを形成するためにキャップするオプションがあります。
次の例では、`Mesh.ExtrudePolyCurve` を使用して、閉じたポリカーブに基づいく閉じたメッシュを作成しています。

## サンプル ファイル

![Example](./Autodesk.DesignScript.Geometry.Mesh.ExtrudePolyCurve_img.jpg)
