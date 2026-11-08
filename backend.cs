using System.Collections;

namespace Backend;

/// <summary>
/// Lista doblemente ligada genérica que se mantiene SIEMPRE ordenada.
/// Por defecto el orden es ascendente; después de SortDescending() es descendente
/// y las nuevas inserciones respetan el orden vigente.
/// Admite elementos repetidos (quedan contiguos).
/// </summary>
/// <typeparam name="T">Cualquier tipo que se pueda comparar (o para el que se entregue un IComparer).</typeparam>
public class DoublyLinkedList<T> : IEnumerable<T>
{
    private readonly IComparer<T> _comparer;
    private Node<T>? _head;
    private Node<T>? _tail;
    private bool _ascending = true;

    public int Count { get; private set; }
    public bool IsEmpty => Count == 0;
    public bool IsAscending => _ascending;

    /// <param name="comparer">Criterio de orden. Si es null se usa el orden natural de T.</param>
    public DoublyLinkedList(IComparer<T>? comparer = null)
    {
        _comparer = comparer ?? Comparer<T>.Default;
    }

    // Compara respetando el orden vigente de la lista (ascendente o descendente).
    private int Compare(T a, T b) => _ascending ? _comparer.Compare(a, b) : _comparer.Compare(b, a);

    // ---------------------------------------------------------------- Adicionar

    /// <summary>Inserta el valor directamente en la posición que le corresponde.</summary>
    public void Add(T value)
    {
        var node = new Node<T>(value);

        // Lista vacía
        if (_head is null)
        {
            _head = _tail = node;
            Count = 1;
            return;
        }

        // Caso rápido: va al final (también deja los repetidos después de los existentes)
        if (Compare(_tail!.Value, value) <= 0)
        {
            node.Previous = _tail;
            _tail.Next = node;
            _tail = node;
            Count++;
            return;
        }

        // Caso general: buscar el primer nodo estrictamente mayor e insertar antes de él.
        // Siempre se encuentra, porque la cola ya es mayor que el valor.
        Node<T> current = _head;
        while (Compare(current.Value, value) <= 0)
            current = current.Next!;

        InsertBefore(current, node);
    }

    private void InsertBefore(Node<T> target, Node<T> node)
    {
        node.Next = target;
        node.Previous = target.Previous;

        if (target.Previous is null)
            _head = node;
        else
            target.Previous.Next = node;

        target.Previous = node;
        Count++;
    }

    // ---------------------------------------------------------------- Recorridos

    public IEnumerable<T> Forward()
    {
        for (var n = _head; n is not null; n = n.Next)
            yield return n.Value;
    }

    public IEnumerable<T> Backward()
    {
        for (var n = _tail; n is not null; n = n.Previous)
            yield return n.Value;
    }

    public IEnumerator<T> GetEnumerator() => Forward().GetEnumerator();
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    // ---------------------------------------------------------------- Ordenar

    /// <summary>
    /// Cambia el orden real de la lista a descendente (no solo la muestra al revés).
    /// Como la lista siempre está ordenada, basta con invertir los enlaces de cada nodo: O(n).
    /// </summary>
    public void SortDescending() => SetOrder(ascending: false);

    /// <summary>Vuelve a dejar la lista en orden ascendente.</summary>
    public void SortAscending() => SetOrder(ascending: true);

    private void SetOrder(bool ascending)
    {
        if (_ascending == ascending) return;
        ReverseLinks();
        _ascending = ascending;
    }

    private void ReverseLinks()
    {
        var current = _head;
        while (current is not null)
        {
            // Intercambia los enlaces del nodo...
            (current.Next, current.Previous) = (current.Previous, current.Next);
            // ...y avanza: el "siguiente" original ahora está en Previous.
            current = current.Previous;
        }

        (_head, _tail) = (_tail, _head);
    }

    // ---------------------------------------------------------------- Ocurrencias y moda

    /// <summary>
    /// Cada valor distinto con el número de veces que aparece, en el orden de la lista.
    /// Como los repetidos están contiguos, se resuelve en un solo recorrido, sin diccionarios.
    /// </summary>
    public IEnumerable<(T Value, int Count)> Occurrences()
    {
        var current = _head;
        while (current is not null)
        {
            T value = current.Value;
            int count = 0;

            while (current is not null && _comparer.Compare(current.Value, value) == 0)
            {
                count++;
                current = current.Next;
            }

            yield return (value, count);
        }
    }

    /// <summary>La(s) moda(s): los valores que más veces aparecen (todos, si hay empate).</summary>
    public IReadOnlyList<T> Modes()
    {
        var occurrences = Occurrences().ToList();
        if (occurrences.Count == 0)
            return Array.Empty<T>();

        int max = occurrences.Max(o => o.Count);
        return occurrences.Where(o => o.Count == max).Select(o => o.Value).ToList();
    }

    // ---------------------------------------------------------------- Existe / Eliminar

    /// <summary>¿Hay al menos un elemento con este valor?</summary>
    public bool Exists(T value) => Find(value) is not null;

    // Primer nodo con ese valor. Como la lista está ordenada, se detiene apenas se pasa.
    private Node<T>? Find(T value)
    {
        for (var n = _head; n is not null; n = n.Next)
        {
            int c = Compare(n.Value, value);
            if (c == 0) return n;
            if (c > 0) return null;
        }
        return null;
    }

    /// <summary>Elimina la primera ocurrencia encontrada. Devuelve false si no existía.</summary>
    public bool Remove(T value)
    {
        var node = Find(value);
        if (node is null) return false;

        Unlink(node);
        return true;
    }

    /// <summary>Elimina todas las ocurrencias. Devuelve cuántas eliminó.</summary>
    public int RemoveAll(T value)
    {
        int removed = 0;
        var node = Find(value);

        // Los repetidos son contiguos: se borra mientras el valor siga siendo el mismo.
        while (node is not null && Compare(node.Value, value) == 0)
        {
            var next = node.Next;
            Unlink(node);
            removed++;
            node = next;
        }

        return removed;
    }

    private void Unlink(Node<T> node)
    {
        if (node.Previous is null)
            _head = node.Next;
        else
            node.Previous.Next = node.Next;

        if (node.Next is null)
            _tail = node.Previous;
        else
            node.Next.Previous = node.Previous;

        node.Previous = null;
        node.Next = null;
        Count--;
    }
}