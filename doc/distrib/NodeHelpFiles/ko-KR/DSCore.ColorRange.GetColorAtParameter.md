## 상세
GetColorAtParameter는 입력 2D 색상 범위를 가져와 지정된 UV 매개변수에서 범위가 0~1인 색상 리스트를 반환합니다. 아래 예에서는 먼저 매개변수 리스트 및 색상 리스트와 함께 ByColorsAndParameters 노드를 사용하여 2D 색상 범위를 작성하여 범위를 설정합니다. 코드 블록은 0~1 사이의 숫자 범위를 생성하는 데 사용되고, 이러한 범위는 UV.ByCoordinates 노드에서 u 및 v 입력으로 사용됩니다. 이 노드의 레이싱은 외적으로 설정됩니다. 외적 레이싱이 있는 Point.ByCoordinates 노드가 정육면체 배열을 작성하는 데 사용되는 것과 유사한 방식으로 정육면체 세트가 작성됩니다. 그런 다음 GetColorAtParameter 노드에서 얻은 정육면체 배열과 색상 리스트와 함께 Display.ByGeometryColor 노드를 사용합니다.
___
## 예제 파일

![GetColorAtParameter](./DSCore.ColorRange.GetColorAtParameter_img.jpg)

