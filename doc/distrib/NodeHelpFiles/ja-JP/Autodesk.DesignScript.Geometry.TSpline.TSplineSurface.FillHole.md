## 詳細
次の例では、`TSplineSurface.FillHole` ノードを使用して T スプライン円柱サーフェスのギャップが埋められます。これには次の入力が必要です。
- `edges`: 埋める T スプライン サーフェスから選択された複数の境界エッジ
- `fillMethod`: 埋める方法を示す 0 ～ 3 の数値:
    * 0 はテッセレーションで穴を埋めます
    * 1 は、単一の多角形の面で穴を埋めます
    * 2 を指定すると、穴の中心に点が作成され、そこから三角形の面がエッジに向かって放射状に広がります
    * 3 は方法 2 と似ていますが、中心の頂点が重なるのではなく、1 つの頂点に連結されるという違いがあります。
- `keepSubdCreases`: 再分割された折り目を保持するかどうかを示すブール値です。
___
## サンプル ファイル

![TSplineSurface.FillHole](./Autodesk.DesignScript.Geometry.TSpline.TSplineSurface.FillHole_img.gif)
