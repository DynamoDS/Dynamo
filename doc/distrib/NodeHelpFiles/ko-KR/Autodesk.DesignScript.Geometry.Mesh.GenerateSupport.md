## 상세
`Mesh.GenerateSupport` 노드는 3D 인쇄를 준비하기 위해 입력 메쉬 형상에 지지대를 추가하는 데 사용됩니다. 돌출부가 있는 형상을 성공적으로 인쇄하려면 지지대가 필요하며, 지지대는 적절한 레이어 접착을 보장하고 인쇄 과정에서 재료가 처지는 것을 방지합니다. `Mesh.GenerateSupport`는 돌출부를 감지하고 재료를 더 적게 소비하고 인쇄된 표면과의 접촉이 적어 더 쉽게 제거할 수 있는 트리 유형 지지대를 자동으로 생성합니다. 돌출부가 감지되지 않은 경우 `Mesh.GenerateSupport` 노드의 결과는 인쇄를 위한 최적의 방향으로 회전하고 XY 평면으로 변환된 동일한 메쉬입니다. 지지대 구성은 다음과 같은 입력으로 제어됩니다.
- baseHeight - 지지대의 가장 낮은 부분(밑면)의 두께를 정의합니다.
- baseDiameter는 지지대 밑면의 크기를 제어합니다.
- postDiameter 입력은 중간에 있는 각 지지대의 크기를 제어합니다.
- tipHeight 및 tipDiameter는 인쇄된 표면과 접촉하는 끝단의 지지대 크기를 제어합니다.
아래 예제에서는 `Mesh.GenerateSupport` 노드를 사용하여 문자 ‘T’ 모양의 메쉬에 지지대를 추가합니다.

## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.Mesh.GenerateSupport_img.jpg)
