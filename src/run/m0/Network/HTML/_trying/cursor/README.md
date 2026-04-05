# Serwer Dokumentacyjny

Prosta strona HTML z panelem nawigacyjnym i funkcjonalnością ładowania dokumentów.

## Funkcjonalności

### Panel nawigacyjny (lewa strona)
- **Drzewiasta struktura** - spis treści z możliwością zagnieżdżania do 4 poziomów
- **Zwijanie/rozwijanie** - każdy poziom może być zwinięty lub rozwinięty
- **Przyciski globalne** - "Zwiń" i "Rozwiń" dla wszystkich elementów
- **Chowanie panelu** - przycisk do ukrywania większej części panelu
- **Zmiana rozmiaru** - możliwość przesuwania rozmiaru panelu za pomocą myszki

### Główna treść (prawa strona)
- **Dynamiczne ładowanie** - dokumenty HTML ładowane po kliknięciu w spis treści
- **Zachowanie stanu** - spis treści pozostaje w tym samym stanie po załadowaniu dokumentu
- **Obsługa błędów** - komunikat gdy dokument nie zostanie znaleziony

## Struktura plików

```
/
├── index.html          # Główna strona HTML
├── script.js           # Logika JavaScript
├── documents/          # Katalog z dokumentami HTML
│   ├── intro.html
│   ├── getting-started.html
│   ├── api.html
│   └── tutorials.html
└── README.md           # Ten plik
```

## Uruchomienie

1. Otwórz plik `index.html` w przeglądarce internetowej
2. Strona będzie działać lokalnie bez potrzeby serwera

## Użytkowanie

### Nawigacja
- Kliknij na elementy w drzewie, aby rozwinąć/zwinąć podsekcje
- Kliknij na pozycję w spisie treści, aby załadować dokument
- Użyj przycisków "Zwiń" i "Rozwiń" do globalnego zarządzania drzewem

### Panel nawigacyjny
- Kliknij przycisk ◀/▶ aby ukryć/pokazać panel
- Przeciągnij prawą krawędź panelu, aby zmienić jego szerokość
- Panel można zmniejszyć do 200px i zwiększyć do 400px

### Dokumenty
- Dokumenty są ładowane dynamicznie z plików HTML
- Każdy dokument zawiera tylko treść HTML (bez HEAD/BODY)
- Jeśli dokument nie istnieje, wyświetlany jest komunikat błędu

## Dodawanie nowych dokumentów

1. Utwórz plik HTML w katalogu `documents/`
2. Nazwij plik zgodnie z ID w spisie treści (np. `moj-dokument.html`)
3. Dodaj tylko treść HTML (bez tagów HEAD/BODY)
4. Dodaj pozycję do spisu treści w `script.js` (tablica `treeData`)

## Przykład dokumentu HTML

```html
<h1>Tytuł dokumentu</h1>
<p>Treść dokumentu...</p>
<h2>Podtytuł</h2>
<ul>
    <li>Element listy</li>
</ul>
```

## Technologie

- HTML5
- CSS3 (bez zewnętrznych bibliotek)
- Vanilla JavaScript (bez frameworków)
- Fetch API do ładowania dokumentów

## Kompatybilność

- Wszystkie nowoczesne przeglądarki
- Wymaga obsługi JavaScript
- Działa lokalnie bez serwera 