## 상세
Solid by Joined Surfaces는 표면 리스트를 입력으로 사용하고 표면에 의해 정의된 단일 솔리드를 반환합니다. 이 표면은 닫힌 표면을 정의해야 합니다. 아래 예에서는 원을 베이스 형상으로 시작합니다. 이 원이 패치되어 표면을 작성하고 해당 표면은 z 방향으로 변환됩니다. 그런 다음 원을 돌출시켜 변을 생성합니다. List.Create는 밑변, 변, 및 상단 표면으로 구성된 리스트를 만드는 데 사용됩니다. 그런 다음 ByJoinedSurfaces를 사용하여 리스트를 단일한 닫힌 솔리드로 전환합니다.
___
## 예제 파일

![ByJoinedSurfaces](./Autodesk.DesignScript.Geometry.Solid.ByJoinedSurfaces_img.jpg)

