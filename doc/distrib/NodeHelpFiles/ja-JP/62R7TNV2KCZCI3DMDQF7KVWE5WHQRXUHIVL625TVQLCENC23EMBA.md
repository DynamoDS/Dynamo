<!--- Autodesk.DesignScript.Geometry.Surface.ToNurbsSurface(surface, limitSurface) --->
<!--- 62R7TNV2KCZCI3DMDQF7KVWE5WHQRXUHIVL625TVQLCENC23EMBA --->
## 詳細
`Surface.ToNurbsSurface` は、サーフェスを入力として取得し、入力されたサーフェスに近似する NURBS サーフェスを返します。`limitSurface` 入力は、変換前にサーフェスを元のパラメータ範囲に戻すかどうかを決定します。たとえば、トリム操作を行うと、サーフェスのパラメータ範囲が制限されます。

次の例では、閉じた NURBS 曲線を入力として用いる `Surface.ByPatch` ノードを使用して、サーフェスを作成します。このサーフェスを `Surface.ToNurbsSurface` ノードの入力として使用すると、4 つの側面を持つトリムされていない NURBS サーフェスが作成されます。


___
## サンプル ファイル

![Surface.ToNurbsSurface](./62R7TNV2KCZCI3DMDQF7KVWE5WHQRXUHIVL625TVQLCENC23EMBA_img.jpg)
