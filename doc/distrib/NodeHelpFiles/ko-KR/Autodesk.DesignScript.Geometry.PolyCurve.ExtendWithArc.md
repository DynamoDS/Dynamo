## 상세
Extend With Arc는 입력 PolyCurve의 시작 또는 끝에 원형 호를 추가하고 결합된 단일 PolyCurve를 반환합니다. 반지름 입력은 원의 반지름을 결정하는 반면 길이 입력은 원을 따라 호에 대한 거리를 결정합니다. 총 길이는 지정된 반지름을 가진 완전한 원의 길이보다 작거나 같아야 합니다. 생성된 호는 입력 PolyCurve의 끝에 접합니다. endOrStart에 대한 부울 입력은 호가 작성될 PolyCurve의 끝을 제어합니다. 'true' 값은 PolyCurve 끝에서 호를 작성하고, 'false'는 PolyCurve의 시작 부분에서 호를 작성합니다. 아래 예에서는 먼저 임의의 점 세트와 PolyCurve By Points를 사용하여 PolyCurve를 작성합니다. 그런 다음 두 개의 숫자 슬라이더와 부울 토글을 사용하여 ExtendWithArc에 대한 매개변수를 설정합니다.
___
## 예제 파일

![ExtendWithArc](./Autodesk.DesignScript.Geometry.PolyCurve.ExtendWithArc_img.jpg)

