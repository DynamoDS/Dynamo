## 상세
`Geometry.DeserializeFromSABWithUnits`는 .SAB(표준 ACIS 바이너리) 바이트 배열 및 밀리미터에서 변환할 수 있는 `DynamoUnit.Unit`에서 형상을 Dynamo로 가져옵니다. 이 노드는 첫 번째 입력으로 byte[]를 사용하고 두 번째 입력으로 `dynamoUnit`을 사용합니다. `dynamoUnit` 입력을 null로 남겨 두는 경우 .SAB 형상이 단위 없이 가져오기되며, 단위 변환 없이 배열의 형상 데이터를 가져옵니다. 단위가 제공된 경우 .SAB 배열의 내부 단위가 지정된 단위로 변환됩니다.

Dynamo에는 단위가 없지만 Dynamo 그래프의 숫자 값에는 여전히 암시적 단위가 있을 수 있습니다. `dynamoUnit` 입력을 사용하여 .SAB의 내부 형상의 축척을 해당 단위 시스템으로 조정할 수 있습니다.

아래 예에서는 직육면체가 2개의 측정 단위(단위 없음)를 사용하여 SAB에서 생성되었습니다. `dynamoUnit` 입력은 다른 소프트웨어에서 사용할 수 있도록 선택한 단위의 축척을 조정합니다.

___
## 예제 파일

![Geometry.DeserializeFromSABWithUnits](./GeometryUI.DeserializeFromSABWithUnits_img.jpg)
