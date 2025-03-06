## In-Depth
`TSplineSurface.BuildPipes`는 곡선 네트워크를 사용하여 T-Spline 파이프 표면을 생성합니다. 개별 파이프는 끝점이 `snappingTolerance` 입력에 의해 설정된 최대 공차 내에 있는 경우 결합된 것으로 간주됩니다. 이 노드의 결과는 입력이 파이프 수와 동일한 길이의 리스트인 경우 모든 파이프에 대해 또는 개별적으로 값을 설정할 수 있는 입력 세트를 사용하여 미세 조정할 수 있습니다. 다음의 입력을 이러한 방식으로 사용할 수 있습니다. `segmentsCount`, `startRotations`, `endRotations`, `startRadii`, `endRadii`, `startPositions` 및 `endPositions`.

아래 예에서는 끝점에서 결합된 세 개의 곡선이 `TSplineSurface.BuildPipes` 노드의 입력으로 제공됩니다. 이 경우 `defaultRadius`는 3개의 파이프 모두에 대한 단일 값이며, 시작 및 끝 반지름을 제공하지 않는 한 기본적으로 파이프의 반지름을 정의합니다.
다음으로 `segmentsCount`는 각 개별 파이프에 대해 3개의 서로 다른 값을 설정합니다. 입력은 3개의 값 리스트이며 각각 하나의 파이프에 해당합니다.

`autoHandleStart` 및 `autoHandleEnd`가 False로 설정되면 추가 조정을 사용할 수 있게 됩니다. 그러면 `startRadii` 및 `EndRadii`를 지정하여 각 파이프의 시작 및 끝 회전(`startRotations` 및 `endRotations` 입력)뿐만 아니라 각 파이프 끝 및 시작의 반지름을 제어할 수 있습니다. 마지막으로 `startPositions` 및 `endPositions`는 각 곡선의 시작 또는 끝에서 각각 세그먼트의 간격띄우기를 허용합니다. 이 입력에는 세그먼트가 시작되거나 끝나는 곡선의 매개변수(0에서 1 사이의 값)에 해당하는 값이 필요합니다.

## 예제 파일
![Example](./M3VFMWB2QNLX6WXZAGO7A2KLFVYNTV3P6QYWKGHMXCJ2TEDO3KZQ_img.gif)
