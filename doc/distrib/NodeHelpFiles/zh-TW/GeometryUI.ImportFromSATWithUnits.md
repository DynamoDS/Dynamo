## 深入資訊
`Geometry.ImportFromSATWithUnits` 會從 .SAT 檔案和可從公釐轉換的 `DynamoUnit.Unit` 將幾何圖形匯入到 Dynamo 中。此節點使用檔案物件或檔案路徑作為第一個輸入，使用 `dynamoUnit` 作為第二個輸入。如果 `dynamoUnit` 輸入保留為空值，則會以無單位匯入 .SAT 幾何圖形，只匯入檔案中的幾何資料，而不做任何單位轉換。如果傳入單位，則 .SAT 檔的內部單位會轉換為指定的單位。

Dynamo 沒有單位，但 Dynamo 圖表中的數值可能還是有某種隱含單位。您可以使用 `dynamoUnit` 輸入，將 .SAT 檔案的內部幾何圖形調整為該單位系統。

在以下範例中，從以英尺為單位的 .SAT 檔匯入幾何圖形。若要讓此範例檔案能在您的電腦上運作，請下載此範例 SAT 檔，並將 `File Path` 節點指向 invalid.sat 檔。

___
## 範例檔案

![Geometry.ImportFromSATWithUnits](./GeometryUI.ImportFromSATWithUnits_img.jpg)
