## 詳細
次の例では、T スプライン サーフェスが無効になります。これはバックグラウンド プレビューで重なり合っている面が見られることで確認できます。サーフェスが無効であることは、`TSplineSurface.EnableSmoothMode` ノードを使用してスムーズ モードをアクティブにしようとして失敗することでも確認できます。もう 1 つの手がかりは、サーフェスで最初にスムーズ モードがアクティブになっていた場合でも、`TSplineSurface.IsInBoxMode` ノードが `true` を返すことです。

サーフェスを修復するには、`TSplineSurface.Repair` ノードを介してサーフェスを渡します。結果としてサーフェスが有効になり、スムーズ プレビュー モードを正常に有効にすることで確認できます。
___
## サンプル ファイル

![TSplineSurface.Repair](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.Repair_img.jpg)
