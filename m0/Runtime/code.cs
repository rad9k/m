using System;
using System.IO;

class Program
{
    static void Main()
    {
        try
        {
            // Pobierz katalog w którym uruchomiono program
            string currentDirectory = Directory.GetCurrentDirectory();

            // Ścieżka do podkatalogu "autostart"
            string autostartPath = Path.Combine(currentDirectory, "autostart");

            // Sprawdź czy katalog "autostart" istnieje
            if (!Directory.Exists(autostartPath))
            {
                Console.WriteLine($"Katalog 'autostart' nie istnieje w: {currentDirectory}");
                return;
            }

            Console.WriteLine($"Rozpoczynam przetwarzanie plików w katalogu: {autostartPath}");

            // Rozpocznij rekurencyjne przetwarzanie
            ProcessDirectoryRecursively(autostartPath);

            Console.WriteLine("Przetwarzanie zakończone.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Wystąpił błąd: {ex.Message}");
        }
    }

    /// <summary>
    /// Rekurencyjnie przetwarza wszystkie pliki w podanym katalogu i jego podkatalogach
    /// </summary>
    /// <param name="directoryPath">Ścieżka do katalogu</param>
    static void ProcessDirectoryRecursively(string directoryPath)
    {
        try
        {
            // Pobierz wszystkie pliki w bieżącym katalogu
            string[] files = Directory.GetFiles(directoryPath);

            // Uruchom funkcję f dla każdego pliku
            foreach (string file in files)
            {
                f(file);
            }

            // Pobierz wszystkie podkatalogi
            string[] subdirectories = Directory.GetDirectories(directoryPath);

            // Rekurencyjnie przetwórz każdy podkatalog
            foreach (string subdirectory in subdirectories)
            {
                ProcessDirectoryRecursively(subdirectory);
            }
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"Brak dostępu do katalogu: {directoryPath} - {ex.Message}");
        }
        catch (DirectoryNotFoundException ex)
        {
            Console.WriteLine($"Katalog nie został znaleziony: {directoryPath} - {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd podczas przetwarzania katalogu {directoryPath}: {ex.Message}");
        }
    }

    /// <summary>
    /// Funkcja f - wykonywana dla każdego znalezionego pliku
    /// Zmodyfikuj tę funkcję zgodnie z potrzebami
    /// </summary>
    /// <param name="filePath">Pełna ścieżka do pliku</param>
    static void f(string filePath)
    {
        try
        {
            // Przykładowa implementacja - wyświetl informacje o pliku
            FileInfo fileInfo = new FileInfo(filePath);
            Console.WriteLine($"Przetwarzam plik: {fileInfo.Name}");
            Console.WriteLine($"  Ścieżka: {filePath}");
            Console.WriteLine($"  Rozmiar: {fileInfo.Length} bajtów");
            Console.WriteLine($"  Ostatnia modyfikacja: {fileInfo.LastWriteTime}");
            Console.WriteLine();

            // Tutaj dodaj własną logikę przetwarzania pliku
            // Na przykład:
            // - odczytanie zawartości pliku
            // - przetwarzanie danych
            // - kopiowanie pliku
            // - itp.
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Błąd podczas przetwarzania pliku {filePath}: {ex.Message}");
        }
    }
}