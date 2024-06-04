
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;

namespace MineSweeper;

/// <summary>
/// A thread safe FIFO collection that removes duplicate values following a policy specified by the user via the <see cref="QueueOptimization"/> method parameter
/// </summary>
/// <typeparam name="TKey">The type of element used to determine wether the elements inserted are duplicates</typeparam>
/// <typeparam name="TValue">The type of the values stored in the queue</typeparam>
public class OptimizedQueue<TKey, TValue> where TKey : IComparable<TKey> where TValue : notnull
{
    // HOW IT WORKS
    //    This is a queue that keeps track of duplicate values inserted. When adding a new element to the queue,
    //    i can't remove duplicates already present without de-queue-ing all other elements.
    //    As a workaround i have a sorted list that keeps track of all latest elements added, one per key.
    //    When i de-queue an element, i keep it only if it is the latest added. If not, i discard it and de-queue the next
    //

    private struct QueueElement
    {
        public QueueElement() { }


        public QueueElement(TKey? key, TValue value)
        {
            OptimizationKey = key;
            IsOptimized = OptimizationKey is not null;
            Value = value;
        }

        public Guid ID = Guid.NewGuid();
        public TValue Value = default!;
        public TKey? OptimizationKey = default!;    // Null if not optimized
        public bool IsOptimized;
    }

    private Dictionary<TKey, Guid> _optimized = new();
    private Queue<QueueElement> _queue = new();
    private object _lock = new();

    private Func<TKey, TValue, TKey>? _keyGenerator;
    /// <summary>
    /// Creates a new thread safe optimized queue.<br/>
    /// It allows to pass an algorithm to generate key's used to recognize elements passed.
    /// </summary>
    /// <remarks><b>Note: </b> The keys generated should be unique for every possible value</remarks>
    /// <param name="keyGenerator">The algorithm used to generate keys from the values passed</param>
    public OptimizedQueue(Func<TKey, TValue, TKey>? keyGenerator = null)
    {
        _keyGenerator = keyGenerator;
    }


    public bool Enqueue(TValue value)
    {
        lock (_lock)
        {
            QueueElement element = new(default, value);
            _queue.Enqueue(element);
            return true;
        }
    }

    public bool Enqueue(TKey key, TValue value, QueueOptimization action = QueueOptimization.Newest)
    {
        lock (_lock)
        {
            QueueElement element;
            TKey finalKey;

            if (_keyGenerator is not null)
                finalKey = _keyGenerator.Invoke(key, value);
            else
                finalKey = key;

            switch (action)
            {
                case QueueOptimization.Oldest:
                    bool exists = _optimized.ContainsKey(finalKey);

                    if (exists)
                        return false;

                    element = new(finalKey, value);

                    _optimized.Add(finalKey, element.ID);
                    _queue.Enqueue(element);
                    break;

                case QueueOptimization.Newest:

                    // Remove any older elements so that when they come
                    // out of the queue they don't find themselves and dont get displayed
                    _optimized.Remove(finalKey);

                    // Add element
                    element = new(finalKey, value);
                    _optimized.Add(finalKey, element.ID);
                    _queue.Enqueue(element);
                    break;
            }

            //_handle.Set();
            return true;
        }
    }

    public bool TryPeek([NotNullWhen(true)] out TValue? value, out Guid uniqueID, out TKey? key)
    {
        while (true) lock (_lock)
            {
                // Exit if queue is empty
                if (!_queue.TryPeek(out QueueElement nextInQueue))
                {
                    value = default;
                    uniqueID = Guid.Empty;
                    key = default;
                    return false;
                }

                // Skip discarding elements if not optimized
                // (all elements will be processed without discarding any)
                if (nextInQueue.IsOptimized)
                {
                    if (nextInQueue.OptimizationKey is null)
                        throw new Exception("Impossible");

                    // Get the ID of the optimized element with same key 
                    Guid latestID = _optimized[nextInQueue.OptimizationKey];

                    // Check if next element in queue was discarded
                    // (the ID of the one NOT discarded is not the same of this one)
                    if (nextInQueue.ID != latestID)
                    {
                        _queue.Dequeue();
                        continue;
                    }
                }

                // Return this element if it was the one NOT discarded
                value = nextInQueue.Value!;
                uniqueID = nextInQueue.ID;
                key = nextInQueue.OptimizationKey;
                return true;
            }
    }

    public bool TryPeek([NotNullWhen(true)] out TValue? value, out Guid uniqueID)
    {
        return TryPeek(out value, out uniqueID, out _);
    }

    public bool TryRemove(Guid elementID)
    {
        lock (_lock)
        {
            if (!_queue.TryPeek(out QueueElement nextInQueue))
                return false;

            if (nextInQueue.ID != elementID)
                return false;

            // Remove this element from the queue
            _queue.Dequeue();

            // If this element was optimized
            if (nextInQueue.IsOptimized)
            {
                // Get the ID of the optimized element with same key
                Guid latestID = _optimized[nextInQueue.OptimizationKey];

                // If this was the optimized element to get, remove it and allow for more
                if (nextInQueue.ID == latestID)
                    _optimized.Remove(nextInQueue.OptimizationKey);
            }

            return true;
        }
    }
}
