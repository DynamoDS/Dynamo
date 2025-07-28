## 상세
`Geometry.ImportFromSATWithUnits`는 .SAT 파일 및 밀리미터에서 변환할 수 있는 `DynamoUnit.Unit`에서 형상을 Dynamo로 가져옵니다. 이 노드는 첫 번째 입력으로 파일 객체 또는 파일 경로를 사용하고 두 번째 입력으로 `dynamoUnit`을 사용합니다. `dynamoUnit` 입력을 null로 남겨 두는 경우 .SAT 형상은 단위 없이 가져오기되며, 단위 변환 없이 파일에서 형상 데이터를 가져오기만 합니다. 단위가 전달된 경우 .SAT 파일의 내부 단위가 지정된 단위로 변환됩니다.

Dynamo에는 단위가 없지만 Dynamo 그래프의 숫자 값에는 여전히 암시적 단위가 있을 수 있습니다. `dynamoUnit` 입력을 사용하여 .SAT 파일의 내부 형상 축척을 해당 단위 시스템으로 조정할 수 있습니다.

아래 예에서는 피트 단위를 사용하여 .SAT 파일에서 형상을 가져옵니다. 이 예제 파일이 컴퓨터에서 작동하도록 하려면 이 예제 SAT 파일을 다운로드하고 `File Path` 노드를 invalid.sat 파일로 가리킵니다.

___
## 예제 파일

![Geometry.ImportFromSATWithUnits](./GeometryUI.ImportFromSATWithUnits_img.jpg)
