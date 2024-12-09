## 詳細
次の例では、指定された `path` を中心に `profile` をスイープして T スプライン サーフェスを作成します。`parallel` 入力は、プロファイル スパンがパス方向と平行のままになるか、パス方向に沿って回転するかをコントロールします。形状の定義は、`pathSpans` と `radialSpans` で設定します。`pathUniform` 入力は、パス スパンが均等に配分されるか、曲率を考慮するかを定義します。同様の設定の `profileUniform` はプロファイルに沿ったスパンをコントロールします。形状の初期対称性は `symmetry` 入力で指定します。最後に、`inSmoothMode` 入力を使用して、T スプライン サーフェスのスムーズ モードとボックス モードのプレビューを切り替えます。

## サンプル ファイル

![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.BySweep_img.jpg)
