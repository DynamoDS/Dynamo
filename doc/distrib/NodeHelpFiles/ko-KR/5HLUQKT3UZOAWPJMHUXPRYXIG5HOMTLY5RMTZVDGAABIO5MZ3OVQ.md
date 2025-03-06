<!--- Autodesk.DesignScript.Geometry.Surface.Thicken(surface, thickness, both_sides) --->
<!--- 5HLUQKT3UZOAWPJMHUXPRYXIG5HOMTLY5RMTZVDGAABIO5MZ3OVQ --->
## 상세
`Surface.Thicken (surface, thickness, both_sides)`은 `thickness` 입력에 따라 표면을 간격띄우기하고 끝을 캡핑하여 솔리드를 닫는 방식으로 솔리드를 만듭니다. 이 노드에는 양쪽 면을 두껍게 할지 여부를 지정하는 추가 입력이 있습니다. 'both_sides' 입력의 경우 부울 값 True를 사용하여 양쪽 면을 두껍게 하고 False를 사용하여 한쪽 면을 두껍게 합니다. `thickness` 매개변수가 최종 솔리드의 총 두께를 결정하므로 `both_sides` 값을 True로 설정하면 양쪽 모두의 입력 두께의 절반만큼 원래 표면에서 간격띄우기됩니다.

아래 예에서는 먼저 `Surface.BySweep2Rails`를 사용하여 표면을 만듭니다. 그런 다음 숫자 슬라이더로 `Surface.Thicken` 노드의 `thickness` 입력을 결정하여 솔리드를 작성합니다. 부울 토글은 양쪽 면을 두껍게 할지 아니면 한쪽 면만 두껍게 할지 여부를 제어합니다.

___
## 예제 파일

![Surface.Thicken](./5HLUQKT3UZOAWPJMHUXPRYXIG5HOMTLY5RMTZVDGAABIO5MZ3OVQ_img.jpg)
