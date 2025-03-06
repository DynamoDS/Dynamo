<!--- Autodesk.DesignScript.Geometry.Curve.TrimSegmentsByParameter(curve, parameters, discardEvenSegments) --->
<!--- BZCTQI2SIMCNMSCEHGSQLE6G74ND4ZQRICVGQCLVQ3OGHPBNX5NQ --->
## 상세
`Curve.TrimSegmentsByParameter (parameters, discardEvenSegments)`는 먼저 매개변수의 입력 리스트에 의해 결정된 점에서 곡선을 분할합니다. 그런 다음 'discardEvenSegments' 입력의 부울 값에 의해 결정된 대로 홀수 세그먼트 또는 짝수 세그먼트를 반환합니다.

아래 예에서는 먼저 임의로 생성된 점 세트를 입력으로 하여 `NurbsCurve.ByControlPoints` 노드를 사용해 NurbsCurve를 만듭니다. 0에서 1 사이의 숫자 범위를 0.1씩 단계적으로 생성하는 데 `code block`을 사용합니다. 이를 `Curve.TrimSegmentsByParameter_results 노드의 입력 매개변수로 사용하면 실질적으로 원래 곡선의 파형선 버전인 곡선 리스트가 생성됩니다.
___
## 예제 파일

![Curve.TrimSegmentsByParameter(parameters, discardEvenSegments)](./BZCTQI2SIMCNMSCEHGSQLE6G74ND4ZQRICVGQCLVQ3OGHPBNX5NQ_img.jpg)
