<!--- Autodesk.DesignScript.Geometry.NurbsSurface.ByControlPointsWeightsKnots --->
<!--- 2SAWXHRQ333U2VRTKOVHZ2L5U6OPIQ2DHLI3MRGJWLXPMDUKVQZA --->
## 深入資訊
使用指定的控制頂點、節點、權重和 U V 次數，建立一個 NurbsSurface。資料有幾個限制，如果違反，將導致函數失敗並擲出例外。次數: u 次數和 v 次數應 >= 1 (分段線性雲形線)，且小於 26 (ASM 支援的最大 B 雲形線基礎次數)。權重: 所有權重值 (如果提供) 應為絕對正數。權重若小於 1e-11，將會被拒絕，而且函數將失敗。節點: 兩個節點向量都應為非遞減數列。內部節點重數不得大於開始/結束節點處的次數加 1，和內部節點處的次數 (這允許曲面以 G1 不連續性表示)。請注意，支援非固定邊界的節點向量，但是將轉換為固定邊界的節點向量，而且對應的變更會套用至控制點/權重資料。
___
## 範例檔案



