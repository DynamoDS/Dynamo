## 상세
`Mesh.ByGeometry`는 Dynamo 형상 객체(표면 또는 솔리드)를 입력으로 사용하여 메쉬로 변환합니다. 점과 곡선에는 메쉬 표현이 없으므로 점과 곡선은 유효한 입력이 아닙니다. 변환에서 생성된 메쉬의 해상도는 두 입력(`tolerance`및 `maxGridLines`)에 의해 제어됩니다. `tolerance`는 원래 형상에서 허용 가능한 메쉬 편차를 설정하며 메쉬 크기의 영향을 받습니다. `tolerance` 값을 -1로 설정하면 Dynamo가 적절한 공차를 선택합니다. `maxGridLines` 입력은 U 또는 V 방향으로 생성할 그리드 선의 최대 수를 설정합니다. 그리드 선 수가 많을수록 테셀레이션의 부드러움이 향상됩니다.

## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.Mesh.ByGeometry_img.jpg)
