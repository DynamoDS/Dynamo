<!--- Autodesk.DesignScript.Geometry.Surface.TrimWithEdgeLoops(surface, loops, tolerance) --->
<!--- IHQBNPJ223NVYG6Y4542YTEX7XGP53QRWLFA6633XPAJMTTLNO7A --->
## 상세
`Surface.TrimWithEdgeLoops`는 지정된 공차 내에서 표면에 모두 있어야 하는 하나 이상의 닫힌 PolyCurve 모음을 사용하여 표면을 자릅니다. 입력 표면에서 하나 이상의 구멍을 잘라야 하는 경우 표면의 경계에 대해 하나의 외부 루프가 지정되고 각 구멍에 대해 하나의 내부 루프가 지정되어야 합니다. 표면 경계와 구멍 사이의 영역을 잘라야 하는 경우 각 구멍의 루프만 제공되어야 합니다. 구형 표면과 같이 외부 루프가 없는 주기적 표면의 경우, 잘린 영역은 루프 곡선의 방향을 반전하여 제어할 수 있습니다.

공차는 곡선 끝이 일치하는지 여부 및 곡선과 표면이 일치하는지 여부를 결정할 때 사용되는 공차입니다. 제공된 공차는 입력 PolyCurve를 만들 때 사용되는 공차보다 작을 수 없습니다. 기본값 0.0은 입력 PolyCurve를 만들 때 사용된 가장 큰 공차가 사용된다는 의미입니다.

아래 예에서는 두 루프가 표면에서 잘려 파란색으로 강조 표시된 두 개의 새 표면을 반환합니다. 숫자 슬라이더를 사용하여 새 표면의 모양을 조정할 수 있습니다.

___
## 예제 파일

![Surface.TrimWithEdgeLoops](./IHQBNPJ223NVYG6Y4542YTEX7XGP53QRWLFA6633XPAJMTTLNO7A_img.jpg)
