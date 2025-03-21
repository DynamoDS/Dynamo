## 詳細
`Solid.Repair` attempts to repair solids which have invalid geometry, as well as potentially performing optimizations. `Solid.Repair` will return a new solid object.

このノードは、読み込まれたジオメトリまたは変換されたジオメトリの操作でエラーが発生した場合に役に立ちます。

In the example below, `Solid.Repair` is used to repair geometry from an **.SAT** file. The geometry in the file fails to boolean or trim, and `Solid.Repair` cleans up any *invalid geometry* that is causing the failure.

In general, you should not need to use this functionality on geometry you create in Dynamo, only on geometry from external sources. If you find that is not the case, please report a bug to the Dynamo team Github
___
## サンプル ファイル

![Solid.Repair](./Autodesk.DesignScript.Geometry.Solid.Repair_img.jpg)
