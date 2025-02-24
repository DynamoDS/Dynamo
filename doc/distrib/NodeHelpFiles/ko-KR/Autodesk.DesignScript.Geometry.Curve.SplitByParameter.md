## 상세
`Curve.SplitByParameter (curve, parameters)`는 곡선과 매개변수 리스트를 입력으로 사용합니다. 이 노드는 지정된 매개변수에서 곡선을 분할하고 결과 곡선의 리스트를 반환합니다.

아래 예에서는 먼저 임의로 생성된 점 세트를 입력으로 하여 `NurbsCurve.ByControlPoints` 노드를 사용해 NurbsCurve를 만듭니다. 곡선이 분할되는 매개변수 리스트로 사용할 0과 1 사이의 일련의 숫자를 만드는 데 코드 블록을 사용합니다.

___
## 예제 파일

![SplitByParameter](./Autodesk.DesignScript.Geometry.Curve.SplitByParameter_img.jpg)

