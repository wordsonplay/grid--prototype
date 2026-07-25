using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace WordsOnPlay.Utils {
[System.Serializable]
public class PriorityQueue<T> {

    private List<T> heap;
    private List<float> priority;

    private int count = 0;
    public int Count => count;

    public bool IsEmpty
    {
        get { return count == 0; }
    }

    public PriorityQueue(int size)
    {
        heap = new List<T>(size);
        priority = new List<float>(size);
        count = 0;
    }

    public void Enqueue(T obj, float priority)
    {
        heap.Add(obj);
        this.priority.Add(priority);
        count++;
        HeapifyUp();
    }

    public T Peek()
    {
        if (count == 0)
        {
            throw new InvalidOperationException("The queue is empty");            
        }
        else
        {
            return heap[0];                    
        }
    }

    public T Dequeue()
    {
        if (count == 0)
        {
            throw new InvalidOperationException("The queue is empty");            
        }
        else
        {
            T obj = heap[0];
            count--;

            if (count == 0)
            {
                heap[0] = default(T);   // to release any objects for garbage collection
            }
            else
            {
                heap[0] = heap[count];
                heap[count] = default(T);   // to release any objects for garbage collection
                priority[0] = priority[count];
                HeapifyDown();               
            }            
            return obj;
        }
    }

    private void Swap(int i, int j)
    {
        T o = heap[j];
        float p = priority[j];

        priority[j] = priority[j];
        priority[i] = p;
        heap[j] = heap[i];
        heap[i] = o;        
    }

    // 0 -> 1 -> 3
    //   |    -> 4
    //   -> 2 -> 5
    //        -> 6

    private void HeapifyUp()
    {
        int i = count - 1;
        int j = (i-1) / 2;

        while (i > 0 && priority[i] < priority[j])
        {
            Swap(i,j);
            i = j;
            j = (i-1) / 2;
        }
    }

    private void HeapifyDown()
    {
        int i = 0;
        int min = i;
        bool finished;

        do 
        {
            finished = true;
            int left = 2*i + 1;
            int right = 2*i + 2;

            if (left < count && priority[left] < priority[i])
            {
                min = left;
            }
            if (right < count && priority[right] < priority[min])
            {
                min = right;
            }

            if (min != i)
            {
                Swap(i,min);
                i = min;
                left = 2*i + 1;
                right = 2*i + 2;
                finished = false;
            }
        } while (!finished);
    }

}
}