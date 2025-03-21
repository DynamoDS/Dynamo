## 상세
This node will convert an object to a string. The second input `format specifier` controls how numeric inputs are converted to their string representations.
이 `format specifier` 입력은 c# 표준 형식 숫자 지정자 중 하나여야 합니다.

형식 지정자의 형식은 다음과 같아야 합니다.
`<specifier><precision>`. F1을 예로 들 수 있습니다.

일반적으로 사용되는 몇 가지 형식 지정자는 다음과 같습니다.
```
G: 일반 형식 G 1000.0 -> "1000"
F: 고정점 표기법 F4 1000.0 -> "1000.0000"
N: 번호 N2 1000 -> "1,000.00"
```

이 노드의 기본값은 'G'이며, 이는 간결하지만 가변적인 표현을 출력합니다.

[자세한 내용은 Microsoft 설명서를 참고하십시오.](https://learn.microsoft.com/ko-kr/dotnet/standard/base-types/standard-numeric-format-strings#standard-format-specifiers)
___
## 예제 파일

![Formatted String from Object](./CoreNodeModels.FormattedStringFromObject_img.jpg)
