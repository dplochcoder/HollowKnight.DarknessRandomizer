using System;
using System.Collections.Generic;
using System.Linq;

namespace DarknessRandomizer.Lib;

// A sorted, weighted heap which supports O(log(N)) random removal.
//
// Sorted for determinism. This class may be changed in the future for performance or other reasons, but it should always
// produce the same removal for the same-seeded Random input.
public class WeightedHeap<T>
{
    private int size = 0;
    private int weight = 0;

    private T pivot = default;
    private int pivotWeight = 0;
    private WeightedHeap<T> left;
    private WeightedHeap<T> right;

    public WeightedHeap() { }

    private WeightedHeap(IEnumerable<(T, int)> sorted)
    {
        var list = sorted.ToList();
        if (list.Count > 0)
        {
            int mid = list.Count / 2;
            int lw = 0;
            if (mid > 0)
            {
                left = new(list.GetRange(0, mid));
                lw = left.Weight();
            }
            (pivot, pivotWeight) = list[mid];
            int rw = 0;
            if (mid < list.Count - 1)
            {
                right = new(list.GetRange(mid + 1, list.Count - mid - 1));
                rw = right.Weight();
            }

            size = list.Count;
            weight = lw + pivotWeight + rw;
        }
    }

    public int Size() => size;

    public int Weight() => weight;

    public bool IsEmpty() => size == 0;

    public bool Contains(T t)
    {
        if (size == 0)
        {
            return false;
        }

        int cmp = Comparer<T>.Default.Compare(t, pivot);
        if (cmp < 0)
        {
            return left?.Contains(t) ?? false;
        }
        else if (cmp > 0)
        {
            return right?.Contains(t) ?? false;
        }
        else
        {
            return true;
        }
    }

    public void Clear()
    {
        size = 0;
        weight = 0;
        pivot = default;
        pivotWeight = 0;
        left = null;
        right = null;
    }

    public void Add(T t, int w)
    {
        if (w <= 0)
        {
            throw new ArgumentException(string.Format("Weight (%d) must be a positive integer", w));
        }

        if (size == 0)
        {
            pivot = t;
            pivotWeight = w;
        }
        else
        {
            int cmp = Comparer<T>.Default.Compare(t, pivot);
            if (cmp < 0)
            {
                (left ??= new()).Add(t, w);
            }
            else if (cmp > 0)
            {
                (right ??= new()).Add(t, w);
            }
            else
            {
                throw new ArgumentException(string.Format("Element %s already in heap", t));
            }
        }

        ++size;
        weight += w;
        MaybeRebalance();
    }

    public T Remove(Random r) => Remove(r.Next(0, weight)).Item1;

    private (T, int) Remove(int i)
    {
        if (size == 0)
        {
            throw new InvalidOperationException("WeightedHeap is empty");
        }

        int lw = left?.Weight() ?? 0;
        (T, int) ret;
        if (i < lw)
        {
            ret = left.Remove(i);
        } else if (i < lw + pivotWeight)
        {
            ret = RemovePivotNoAccounting();
        } else
        {
            ret = right.Remove(i - lw - pivotWeight);
        }

        --size;
        weight -= ret.Item2;
        MaybeRebalance();
        return ret;
    }

    private (T, int) RemovePivotNoAccounting()
    {
        var ret = (pivot, pivotWeight);
        pivot = default;
        pivotWeight = 0;
        return ret;
    }

    private (T, int) RemoveFirst()
    {
        if (size == 0)
        {
            throw new InvalidOperationException("Cannot remove from an empty heap");
        }

        (T, int) ret = left?.RemoveFirst() ?? RemovePivotNoAccounting();

        --size;
        weight -= ret.Item2;
        MaybeRebalance();
        return ret;
    }

    private (T, int) RemoveLast()
    {
        if (size == 0)
        {
            throw new InvalidOperationException("Cannot remove from an empty heap");
        }

        (T, int) ret = right?.RemoveLast() ?? RemovePivotNoAccounting();

        --size;
        weight -= ret.Item2;
        MaybeRebalance();
        return ret;
    }
    public IEnumerable<(T, int)> EnumerateSorted()
    {
        if (left != null)
        {
            foreach (var e in left.EnumerateSorted())
            {
                yield return e;
            }
        }

        if (pivotWeight > 0)
        {
            yield return (pivot, pivotWeight);
        }

        if (right != null)
        {
            foreach (var e in right.EnumerateSorted())
            {
                yield return e;
            }
        }
    }

    private void Copy(WeightedHeap<T> that)
    {
        size = that.size;
        weight = that.weight;
        pivot = that.pivot;
        pivotWeight = that.pivotWeight;
        left = that.left;
        right = that.right;
    }

    private void MaybeRebalance()
    {
        if (size == 0)
        {
            return;
        }

        int ls = left?.Size() ?? 0;
        int rs = right?.Size() ?? 0;
        if (pivotWeight == 0)
        {
            if (ls < rs)
            {
                (pivot, pivotWeight) = right.RemoveFirst();
            }
            else
            {
                (pivot, pivotWeight) = left.RemoveLast();
            }
        }

        ls = left?.Size() ?? 0;
        rs = right?.Size() ?? 0;
        if (ls == 0) left = null;
        if (rs == 0) right = null;
        if (size > 10 && 3*Math.Abs(ls - rs) > size)
        {
            Copy(new WeightedHeap<T>(EnumerateSorted().ToList()));
        }
    }
}
