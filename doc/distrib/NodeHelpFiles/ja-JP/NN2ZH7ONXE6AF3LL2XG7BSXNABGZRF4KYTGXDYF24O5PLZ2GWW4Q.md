<!--- Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.EnableSmoothMode --->
<!--- NN2ZH7ONXE6AF3LL2XG7BSXNABGZRF4KYTGXDYF24O5PLZ2GWW4Q --->
## 詳細
ボックス モードとスムーズ モードは、T スプライン サーフェスを表示する 2 つの方法です。スムーズ モードは、T スプライン サーフェスの実際の形状であり、モデルの外観と寸法をプレビューする場合に便利です。一方、ボックス モードは、サーフェスの構造に対する洞察を示して理解が深まるようにします。大きなジオメトリや複雑なジオメトリをより迅速にプレビューできるオプションでもあります。ノード `TSplineSurface.EnableSmoothMode` を使用すると、ジオメトリ開発のさまざまな段階で、この 2 つのプレビューの状態を切り替えることができます。

次の例では、T スプライン ボックス サーフェス上でベベル操作が実行されます。形状の構造を理解しやすくするために、結果はまずボックス モード(ボックス サーフェスの `inSmoothMode` 入力を false に設定)で視覚化されます。次に、`TSplineSurface.EnableSmoothMode` ノードを使用してスムーズ モードがアクティブ化され、両方のモードを同時にプレビューできるように右側に移動されます。
___
## サンプル ファイル

![TSplineSurface.EnableSmoothMode](./NN2ZH7ONXE6AF3LL2XG7BSXNABGZRF4KYTGXDYF24O5PLZ2GWW4Q_img.jpg)
