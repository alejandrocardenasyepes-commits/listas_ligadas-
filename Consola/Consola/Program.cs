using System.Text;
using Backend;

Console.OutputEncoding = Encoding.UTF8;

// Misma lista genérica, ahora de texto (colores). No distingue mayúsculas.
var list = new DoublyLinkedList<string>(StringComparer.CurrentCultureIgnoreCase);

while (true)
{
    ShowMenu();
    string? option = Console.ReadLine()?.Trim();
    Console.WriteLine();

    switch (option)
    {
        case "1": Add(); break;
        case "2": ShowForward(); break;
        case "3": ShowBackward(); break;
        case "4": SortDescending(); break;
        case "5": ShowModes(); break;
        case "6": ShowChart(); break;
        case "7": CheckExists(); break;
        case "8": RemoveOne(); break;
        case "9": RemoveAll(); break;
        case "0":
        case null: return;
        default: Console.WriteLine("Opción inválida."); break;
    }

    Console.WriteLine();
}

void ShowMenu()
{
    Console.WriteLine("1. Adicionar.                  6. Mostrar gráfico.");
    Console.WriteLine("2. Mostrar hacia adelante.     7. Existe.");
    Console.WriteLine("3. Mostrar hacia atrás.        8. Eliminar una ocurrencia.");
    Console.WriteLine("4. Ordenar descendentemente.   9. Eliminar todas las ocurrencias.");
    Console.WriteLine("5. Mostrar la(s) moda(s).      0. Salir.");
    Console.Write("Opción: ");
}

void Add()
{
    Console.Write("Valor(es) a adicionar, separados por coma: ");
    string? line = Console.ReadLine();

    foreach (string part in (line ?? "").Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        list.Add(part);
}

void ShowForward()
{
    if (list.IsEmpty) { Console.WriteLine("La lista está vacía."); return; }
    Console.WriteLine("Hacia adelante: " + string.Join(", ", list.Forward()));
}

void ShowBackward()
{
    if (list.IsEmpty) { Console.WriteLine("La lista está vacía."); return; }
    Console.WriteLine("Hacia atrás: " + string.Join(", ", list.Backward()));
}

void SortDescending()
{
    list.SortDescending();
    Console.WriteLine("La lista quedó ordenada descendentemente.");
}

void ShowModes()
{
    var modes = list.Modes();

    if (modes.Count == 0)
        Console.WriteLine("La lista está vacía.");
    else if (modes.Count == 1)
        Console.WriteLine($"La moda es {modes[0]}.");
    else
        Console.WriteLine($"Las modas son {string.Join(", ", modes.Take(modes.Count - 1))} y {modes[^1]}.");
}

void ShowChart()
{
    var occurrences = list.Occurrences().ToList();

    if (occurrences.Count == 0) { Console.WriteLine("La lista está vacía."); return; }

    int width = occurrences.Max(o => o.Value.Length);

    foreach (var (value, count) in occurrences)
        Console.WriteLine($"{value.PadRight(width)} {new string('*', count)}");
}

void CheckExists()
{
    if (!ReadText("Valor a buscar: ", out string value)) return;

    Console.WriteLine(list.Exists(value) ? $"Sí existe el {value}." : $"No existe el {value}.");
}

void RemoveOne()
{
    if (!ReadText("Valor a eliminar (una ocurrencia): ", out string value)) return;

    Console.WriteLine(list.Remove(value)
        ? $"Se eliminó una ocurrencia del {value}."
        : $"No existe el {value}.");
}

void RemoveAll()
{
    if (!ReadText("Valor a eliminar (todas las ocurrencias): ", out string value)) return;

    int removed = list.RemoveAll(value);
    Console.WriteLine(removed > 0
        ? $"Se eliminaron {removed} ocurrencia(s) del {value}."
        : $"No existe el {value}.");
}

bool ReadText(string prompt, out string value)
{
    Console.Write(prompt);
    value = Console.ReadLine()?.Trim() ?? "";

    if (value.Length > 0)
        return true;

    Console.WriteLine("Debes escribir un valor.");
    return false;
}