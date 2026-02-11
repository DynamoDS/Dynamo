## In-Depth
次の例では、T スプライン サーフェスは指定したプロファイル `curve` の押し出しとして作成されます。曲線は開いていても閉じていてもかまいません。押し出しは指定した `direction` の方向に実行され、`frontDistance` と `backDistance` によるコントロールで両方向に実行できます。スパンは指定した `frontSpans` と `backSpans` を使用して、押し出しの 2 つの方向に対して個別に設定できます。曲線に沿ったサーフェスの定義を設定するには、`profileSpans` で面の数をコントロールし、`uniform` で、均等に分配するか、曲率を考慮するかを設定します。最後に、`inSmoothMode` は、サーフェスがスムーズ モードとボックス モードのどちらで表示されるかをコントロールします。

## サンプル ファイル
![Example](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.ByExtrude_img.gif)
