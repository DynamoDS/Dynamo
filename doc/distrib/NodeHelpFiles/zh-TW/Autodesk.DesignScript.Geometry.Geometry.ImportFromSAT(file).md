## 深入資訊
Geometry.ImportFromSAT 會從 SAT 檔案類型將幾何圖形匯入 Dynamo。此節點以 file 當作輸入，也接受檔案路徑有效的字串。在以下範例中，我們先前已將幾何圖形匯出至 SAT 檔案 (請參閱 ExportToSAT)。我們選擇的檔名為 example.sat，並將其匯出至使用者桌面上的資料夾。我們在範例中示範兩個不同的節點，用於從 SAT 檔案匯入幾何圖形。一個節點有 filePath 作為輸入類型，另一個節點有「file」作為輸入類型。filePath 是使用 File Path 節點建立的，按一下「瀏覽」按鈕可以選取檔案。在第二個範例中，我們使用字串元素手動指定檔案路徑。
___
## 範例檔案

![ImportFromSAT (file)](./Autodesk.DesignScript.Geometry.Geometry.ImportFromSAT(file)_img.jpg)

