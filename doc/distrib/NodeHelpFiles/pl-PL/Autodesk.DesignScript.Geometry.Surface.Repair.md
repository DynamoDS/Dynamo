## Informacje szczegółowe
Węzeł Repair próbuje naprawić powierzchnie lub powierzchnie PolySurface, które mają nieprawidłową geometrię, jak również potencjalnie wykonać optymalizację. Węzeł Repair zwraca nowy obiekt powierzchni.
Ten węzeł jest przydatny w przypadku wystąpienia błędów podczas wykonywania operacji na zaimportowanej lub przekonwertowanej geometrii.

Jeśli na przykład dane zostaną zaimportowane z kontekstu nadrzędnego, takiego jak program **Revit**, lub z pliku **.SAT** i okaże się, że nieoczekiwanie nie można wykonać operacji logicznej lub operacji przycięcia, operacja naprawy może oczyścić *nieprawidłową geometrię*, która powoduje błąd.

Z reguły nie trzeba używać tej funkcji w przypadku geometrii utworzonej w dodatku Dynamo, a jedynie w przypadku geometrii ze źródeł zewnętrznych. Jeśli okaże się, że tak nie jest, zgłoś błąd zespołowi dodatku Dynamo w serwisie GitHub.
___


