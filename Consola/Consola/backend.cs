using System.Collections;

namespace Backend;

public class DoublyLinkedList<T> : IEnumerable<T>
{
    private readonly IComparer<T> _comparer;
    private Node<T>? _head;
    private Node<T>? _tail;
    private bool _ascending = true;

    public int Count { get; private set; }
    public bool IsEmpty => Count == 0;
    public bool IsAscending => _ascending;

    public DoublyLinkedList(IComparer<T>? comparer = null)
    {
        _comparer = comparer ?? Comparer<T>.Default;
    }

    // Compara respetando el orden vigente (ascendente o descendente)
    private int Compare(T a, T b) => _ascending ? _comparer.Compare(a, b) : _comparer.Compare(b, a);

    // ------------------------------------------------------------ Adicionar

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

        // Caso rápido: va al final
        if (Compare(_tail!.Value, value) <= 0)
        {
            node.Previous = _tail;
            _tail.Next = node;
            _tail = node;
            Count++;
            return;
        }

        // Caso general: primer nodo estrictamente mayor, e insertar antes de él
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

    // ------------------------------------------------------------ Recorridos

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

    // ------------------------------------------------------------ Ordenar

    public void SortDescending() => SetOrder(ascending: false);
    public void SortAscending() => SetOrder(ascending: true);

    private void SetOrder(bool ascending)
    {
        if (_ascending == ascending) return;
        ReverseLinks();
        _ascending = ascending;
    }

    // La lista siempre está ordenada: invertir los enlaces cambia su orden real
    private void ReverseLinks()
    {
        var current = _head;
        while (current is not null)
        {
            (current.Next, current.Previous) = (current.Previous, current.Next);
            current = current.Previous;
        }

        (_head, _tail) = (_tail, _head);
    }

    // ------------------------------------------------------------ Ocurrencias y moda

    // Los repetidos están contiguos: un solo recorrido
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

    public IReadOnlyList<T> Modes()
    {
        var occurrences = Occurrences().ToList();
        if (occurrences.Count == 0)
            return Array.Empty<T>();

        int max = occurrences.Max(o => o.Count);
        return occurrences.Where(o => o.Count == max).Select(o => o.Value).ToList();
    }

    // ------------------------------------------------------------ Existe / Eliminar

    public bool Exists(T value) => Find(value) is not null;

    // Primer nodo con ese valor; se detiene apenas se pasa
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

    public bool Remove(T value)
    {
        var node = Find(value);
        if (node is null) return false;

        Unlink(node);
        return true;
    }

    public int RemoveAll(T value)
    {
        int removed = 0;
        var node = Find(value);

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

internal sealed class Node<T>
{
    public T Value { get; set; }
    public Node<T>? Previous { get; set; }
    public Node<T>? Next { get; set; }

    public Node(T value)
    {
        Value = value;
    }
}