## 상세
ByColorsAndParameters는 입력 색상 리스트와 범위가 0~1인 지정된 해당 UV 매개변수 리스트에서 2D 색상 범위를 작성합니다. 아래 예에서는 코드 블록을 사용하여 3가지 다른 색상(이 경우 녹색, 빨간색 및 파란색)을 작성하고 리스트로 결합합니다. 개별 코드 블록을 사용하여 각 색상에 하나씩, 세 개의 UV 매개변수를 작성합니다. 이러한 리스트 2개는 ByColorsAndParameters 노드에 대한 입력으로 사용됩니다. Display.ByGeometryColor 노드와 함께 후속 GetColorAtParameter 노드를 사용하여 정육면체 세트에서 2D 색상 범위를 시각화합니다.
___
## 예제 파일

![ByColorsAndParameters](./DSCore.ColorRange.ByColorsAndParameters_img.jpg)

