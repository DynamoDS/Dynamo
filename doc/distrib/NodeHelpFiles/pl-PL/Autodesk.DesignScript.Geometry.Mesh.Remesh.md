## Informacje szczegółowe
Węzeł `Mesh.Remesh` tworzy nową siatkę, w której trójkąty w danym obiekcie są rozmieszczane równomierniej niezależnie od zmiany wartości wektorów normalnych trójkątów. Ta operacja może być przydatna w przypadku siatek o zmiennej gęstości trójkątów w celu przygotowania siatki do analizy wytrzymałościowej. Wielokrotne ponowne tworzenie siatki powoduje generowanie coraz bardziej jednorodnych siatek. W przypadku siatek, których wierzchołki są już równoodległościowe (na przykład siatki ikosfery), wynikiem węzła `Mesh.Remesh` jest ta sama siatka.
W poniższym przykładzie węzeł `Mesh.Remesh` jest stosowany do zaimportowanej siatki z dużą gęstością trójkątów w obszarach o wysokiej szczegółowości. Wynik węzła `Mesh.Remesh` jest przekształcany do położenia z boku, a do wizualizacji wyniku zostaje użyty węzeł `Mesh.Edges`.

`(Wykorzystany plik przykładowy podlega licencji Creative Commons)`

## Plik przykładowy

![Example](./Autodesk.DesignScript.Geometry.Mesh.Remesh_img.jpg)
