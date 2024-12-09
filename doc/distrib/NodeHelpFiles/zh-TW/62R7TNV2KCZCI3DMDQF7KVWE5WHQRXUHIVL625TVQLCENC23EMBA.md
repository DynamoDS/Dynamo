<!--- Autodesk.DesignScript.Geometry.Surface.ToNurbsSurface(surface, limitSurface) --->
<!--- 62R7TNV2KCZCI3DMDQF7KVWE5WHQRXUHIVL625TVQLCENC23EMBA --->
## 深入資訊
`Surface.ToNurbsSurface` 以曲面當作輸入，並傳回一個近似輸入曲面的 NurbsSurface。`limitSurface` 輸入決定曲面在轉換之前是否應還原為其原始參數範圍，例如，曲面的參數範圍在修剪作業之後受限。

在以下範例中，我們使用 `Surface.ByPatch` 節點和一條封閉的 NurbsCurve 作為輸入來建立曲面。請注意，當我們使用此曲面作為 `Surface.ToNurbsSurface` 節點的輸入時，結果會是一個未修剪而有四條邊的 NurbsSurface。


___
## 範例檔案

![Surface.ToNurbsSurface](./62R7TNV2KCZCI3DMDQF7KVWE5WHQRXUHIVL625TVQLCENC23EMBA_img.jpg)
