## 상세
IsNull은 객체가 null인지 여부를 기준으로 부울 값을 반환합니다. 아래 예에서는 비트맵의 빨간색 레벨을 기준으로 원 그리드가 다양한 반지름으로 그려집니다. 빨간색 값이 없는 경우 원이 그려지지 않고 원 리스트에서 null을 반환합니다. 이 리스트를 IsNull을 통해 전달하면 부울 값 리스트가 반환되며, true는 null 값의 모든 위치를 나타냅니다. 이 부울 리스트를 List.FilterByBoolMask와 함께 사용하여 null이 없는 리스트를 반환할 수 있습니다.
___
## 예제 파일

![IsNull](./DSCore.Object.IsNull_img.jpg)

