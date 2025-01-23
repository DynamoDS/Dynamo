## 深入資訊
`Geometry.DeserializeFromSABWithUnits` 會從 .SAB (標準 ACIS 二進位) 位元組陣列和可從公釐轉換的 `DynamoUnit.Unit` 將幾何圖形匯入到 Dynamo 中。此節點使用 byte[] 作為第一個輸入，使用 `dynamoUnit` 作為第二個輸入。如果 `dynamoUnit` 輸入保留為空值，則會以無單位匯入 .SAB 幾何圖形，匯入陣列中的幾何資料，而不做任何單位轉換。如果提供單位，則 .SAB 陣列的內部單位會轉換為指定的單位。

Dynamo 沒有單位，但 Dynamo 圖表中的數值可能還是有某種隱含單位。您可以使用 `dynamoUnit` 輸入，將 .SAB 的內部幾何圖形調整為該單位系統。

在以下範例中，從有 2 個測量單位 (無單位) 的 SAB 產生立方體。`dynamoUnit` 輸入會調整所選單位的比例，以在其他軟體中使用。

___
## 範例檔案

![Geometry.DeserializeFromSABWithUnits](./GeometryUI.DeserializeFromSABWithUnits_img.jpg)
