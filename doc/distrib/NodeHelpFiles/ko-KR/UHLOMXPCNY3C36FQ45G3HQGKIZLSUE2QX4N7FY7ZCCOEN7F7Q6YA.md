## 상세
`TSplineSurface.Thicken(distance, softEdges)`은 해당하는 면 법선을 따라 지정된 `distance`로 T-Spline 표면을 바깥쪽으로(또는 음수 `distance` 값이 제공되는 경우 내부적으로) 두껍게 합니다. `softEdges` 부울 입력은 결과 모서리를 매끄럽게 할지(true), 각지게 할지(false) 제어합니다.

아래 예에서는 T-Spline 원통 표면이 `TSplineSurface.Thicken(distance, softEdges)` 노드를 사용하여 두꺼워집니다. 결과 표면은 더 나은 시각화를 위해 측면으로 이동됩니다.
___
## 예제 파일

![TSplineSurface.Thicken](./UHLOMXPCNY3C36FQ45G3HQGKIZLSUE2QX4N7FY7ZCCOEN7F7Q6YA_img.jpg)
