<!--- Autodesk.DesignScript.Geometry.Curve.SweepAsSurface(curve, path, cutEndOff) --->
<!--- DUHOUAQLX67Z6VGX2F6TGNPE2PGYDN7VGCOK6UW3D5GYILRXG3KA --->
## 詳細
`Curve.SweepAsSurface` は、入力された曲線を指定されたパスに沿ってスイープしてサーフェスを作成します。次の例では、Code Block を使用して `Arc.ByThreePoints` ノードの 3 点を作成することで、スイープする曲線を作成します。パス曲線は、X 軸に沿った単純な線分として作成されます。`Curve.SweepAsSurface` は、プロファイル曲線をパス曲線に沿って移動して、サーフェスを作成します。`cutEndOff` パラメータは、スイープ サーフェスの末端処理をコントロールするブール値です。`true` に設定すると、サーフェスの終端がパス曲線に対して垂直(法線)にカットされ、平坦で整った終端になります。`false`(既定)に設定すると、サーフェスの終端は、トリミングを行わずにプロファイル曲線の自然な形状に従います。このため、パスの曲率によっては、終端に角度がついたり、不均一になることがあります。
___
## サンプル ファイル

![Example](./DUHOUAQLX67Z6VGX2F6TGNPE2PGYDN7VGCOK6UW3D5GYILRXG3KA_img.jpg)

