<!--- Autodesk.DesignScript.Geometry.Solid.BySweep(profile, path, cutEndOff) --->
<!--- X65A3XAWWVM3XWMAZHZFLL5HTXCJAGYISLC4VHRMPHEV3MBYIRXQ --->
## 상세
`Solid.BySweep`은 지정된 경로를 따라, 입력된 닫힌 프로파일 곡선을 스윕하여 솔리드를 만듭니다.

아래 예에서는 직사각형을 기준 프로파일 곡선으로 사용합니다. 경로는 일련의 각도와 함께 코사인 함수를 사용해 점 세트의 X 좌표를 변경하여 만듭니다. 점은 `NurbsCurve.ByPoints` 노드에 대한 입력으로 사용됩니다. 이제 생성된 코사인 곡선을 따라 직사각형을 스윕하여 솔리드를 만듭니다.
___
## 예제 파일

![Solid.BySweep](./X65A3XAWWVM3XWMAZHZFLL5HTXCJAGYISLC4VHRMPHEV3MBYIRXQ_img.jpg)
