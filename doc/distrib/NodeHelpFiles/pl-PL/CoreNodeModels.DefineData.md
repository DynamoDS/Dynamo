## Informacje szczegółowe

Węzeł Define Data sprawdza typ danych przychodzących. Za jego pomocą można zapewnić, że dane lokalne będą żądanego typu. Można też używać go jako węzła wejściowego lub wyjściowego, deklarując typ danych, których wykres oczekuje lub które dostarcza. Węzeł obsługuje wybór powszechnie używanych typów danych dodatku Dynamo, na przykład „String”, „Point” lub „Boolean”. Pełna lista obsługiwanych typów danych dostępna jest w menu rozwijanym węzła. Węzeł obsługuje dane w postaci pojedynczej wartości lub listy płaskiej. Nie są obsługiwane listy zagnieżdżone, słowniki ani replikacje.

### Zachowanie

Węzeł sprawdza poprawność danych przychodzących z portu wejściowego na podstawie ustawienia konfigurowanego za pomocą menu rozwijanego Typ i przełącznika Lista (szczegóły podano poniżej). Jeśli wynik weryfikacji jest pomyślny, dane wyjściowe węzła są takie same jak dane wejściowe. Jeśli wynik weryfikacji jest niepomyślny, węzeł przechodzi w stan ostrzeżenia z danymi wyjściowymi o wartości null.
Węzeł zawiera jedno wejście:

-   Wejście „**>**” - połącz z węzłem nadrzędnym, aby sprawdzić typ jego danych.
    Dodatkowo węzeł udostępnia trzy kontrolki użytkownika:
-   Przełącznik **Automatyczne wykrywanie typu** - gdy jest włączony, ten węzeł analizuje dane przychodzące i jeśli dane są obsługiwanego typu, węzeł ustawia wartości kontrolek Typ i Lista na podstawie typu danych przychodzących. Menu rozwijane Typ i przełącznik Lista są wyłączone i będą automatycznie aktualizowane na podstawie węzła wejściowego.

    Gdy przełącznik automatycznego wykrywania typu jest wyłączony, typ danych można określić za pomocą menu Typ i przełącznika Lista. Jeśli dane przychodzące są niezgodne z określonymi przez użytkownika, węzeł przejdzie w stan ostrzeżenia z danymi wyjściowymi null.
-   Menu rozwijane **Typ** - ustawia oczekiwany typ danych. Gdy ta kontrolka jest włączona, czyli przełącznik **Automatyczne wykrywanie typu** jest wyłączony, ustaw typ danych do zweryfikowania. Gdy ta kontrolka jest wyłączona, czyli przełącznik jest włączony, typ danych jest wybierany automatycznie na podstawie danych przychodzących. Dane są prawidłowe, jeśli ich typ jest dokładnie zgodny z wyświetlanym typem lub jeśli ich typ jest potomny wobec wyświetlanego typu (np. jeśli menu rozwijane Typ jest ustawione na „Curve”, obiekty typu „Rectangle”, „Line” itp., dane są poprawne).
-   Przełącznik **Lista** - gdy jest włączony, węzeł oczekuje, że dane przychodzące będą pojedynczą listą płaską zawierającą elementy poprawnego typu danych (patrz powyżej). Gdy jest wyłączony, węzeł oczekuje pojedynczego elementu poprawnego typu danych.

### Użyj jako węzła wejściowego

Gdy ta pozycja jest ustawiona jako dane wejściowe (za pomocą opcji w menu kontekstowym węzła), węzeł może opcjonalnie używać węzłów nadrzędnych, aby ustawiać domyślną wartość dla danych wejściowych. Uruchomienie wykresu spowoduje przechwycenie w pamięci podręcznej wartości węzła Define Data do użycia w przypadku zewnętrznego uruchamiania wykresu, na przykład za pomocą węzła Engine.

## Plik przykładowy

W poniższym przykładzie pierwsza grupa węzłów „DefineData” ma wyłączony przełącznik **Automatyczne wykrywanie typu**. Węzeł poprawnie weryfikuje wprowadzone dane wejściowe Number, odrzucając przy tym dane wejściowe String. Druga grupa zawiera węzeł z włączonym przełącznikiem **Automatyczne wykrywanie typu**. Węzeł automatycznie dopasowuje menu rozwijane Typ i przełącznik Lista do danych wejściowych, w tym przypadku do listy liczb całkowitych.

![Define_Data](./CoreNodeModels.DefineData_img.png)
