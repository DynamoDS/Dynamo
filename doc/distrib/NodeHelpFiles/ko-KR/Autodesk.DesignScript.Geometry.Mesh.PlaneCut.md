## 상세
`Mesh.PlaneCut`은 지정된 평면으로 절단된 메쉬를 반환합니다. 절단 결과는 `plane` 입력의 법선 방향으로 평면의 한 쪽에 위치한 메쉬의 부분입니다. `makeSolid` 매개변수는 메쉬를 `Solid`로 처리할지 여부를 제어하며, 이 경우 절단부는 각 구멍을 덮을 수 있는 최소한의 삼각형으로 채워집니다.

아래 예제에서 `Mesh.BooleanDifference` 연산을 통해 구한 속이 빈 메쉬는 평면에 의해 특정 각도로 절단됩니다.

## 예제 파일

![Example](./Autodesk.DesignScript.Geometry.Mesh.PlaneCut_img.jpg)
