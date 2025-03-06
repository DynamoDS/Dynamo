## 상세
`PolyCurve.Heal`은 자체 교차하는 PolyCurve를 가져와 자체 교차하지 않는 새 PolyCurve를 반환합니다. 입력 PolyCurve는 3개 이상의 자체 점을 가질 수 없습니다. 즉, PolyCurve의 단일 세그먼트가 2개 이상의 다른 세그먼트를 만나거나 교차하는 경우 수정이 작동하지 않습니다. 0보다 큰 `trimLength`를 입력하면 `trimLength` 보다 긴 끝 세그먼트는 잘리지 않습니다.

아래 예에서는 자체 교차하는 PolyCurve가 `PolyCurve.Heal` 참조를 사용하여 수정됩니다.
___
## 예제 파일

![PolyCurve.Heal](./Autodesk.DesignScript.Geometry.PolyCurve.Heal_img.jpg)
