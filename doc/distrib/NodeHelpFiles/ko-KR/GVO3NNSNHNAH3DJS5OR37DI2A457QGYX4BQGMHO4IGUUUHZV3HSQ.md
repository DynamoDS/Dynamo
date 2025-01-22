## In-Depth
아래 예에서는 T-Spline 원추 원형이 `TSplineSurface.ByConePointsRadius` 노드를 사용하여 작성됩니다. 원추의 위치와 높이는 `startPoint` 및 `endPoint`의 두 입력으로 제어됩니다. 기본 반지름만 `radius` 입력을 사용하여 조정할 수 있고, 상한 반지름은 항상 0입니다. `radialSpans` 및 `heightSpans`는 방사형 및 높이 스팬을 결정합니다. 모양의 초기 대칭은 `symmetry` 입력에 의해 지정됩니다. X 또는 Y 대칭을 True로 설정하면 방사형 스팬의 값이 4의 배수여야 합니다. 마지막으로 'InSmoothMode` 입력이 T-Spline 표면의 매끄럽게 모드 미리보기와 상자 모드 미리보기 사이를 전환하는 데 사용됩니다.

## 예제 파일

![Example](./GVO3NNSNHNAH3DJS5OR37DI2A457QGYX4BQGMHO4IGUUUHZV3HSQ_img.jpg)
