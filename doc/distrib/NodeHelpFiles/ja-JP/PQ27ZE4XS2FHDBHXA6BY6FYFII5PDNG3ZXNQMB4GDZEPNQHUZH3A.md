<!--- Autodesk.DesignScript.Geometry.Surface.BySweep(profile, path, cutEndOff) --->
<!--- PQ27ZE4XS2FHDBHXA6BY6FYFII5PDNG3ZXNQMB4GDZEPNQHUZH3A --->
## 詳細
`Surface.BySweep (profile, path, cutEndOff)` は、指定したパスに沿って入力された曲線をスイープして、サーフェスを作成します。`cutEndOff` 入力は、スイープの終点を切り取り、パスに対して垂直にするかどうかをコントロールします。

次の例では、プロファイル曲線として Y 方向の正弦曲線を使用します。この曲線をワールド座標系の Z 軸を中心に -90 度回転して、パス曲線として使用します。Surface.BySweep で、プロファイル曲線をパス曲線に沿って移動させ、サーフェスを作成します。


___
## サンプル ファイル

![Surface.BySweep](./PQ27ZE4XS2FHDBHXA6BY6FYFII5PDNG3ZXNQMB4GDZEPNQHUZH3A_img.jpg)
