namespace MineSweeper;

/// <summary>
/// Describes the behaviour of an <see cref="OptimizedQueue{TKey, TValue}"/> when adding an element
/// </summary>
public enum QueueOptimization
{
    /// <summary>
    /// New elements will be added to the queue. Duplicates already present will be removed
    /// </summary>
    Newest,
    /// <summary>
    /// New elements will be added only if there aren't any already present
    /// </summary>
    Oldest
}