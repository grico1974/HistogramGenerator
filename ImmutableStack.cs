using System;
using System.Collections;
using System.Collections.Generic;

namespace HistogramGenerator
{
    public sealed class ImmutableStack<T>: IEnumerable<T>, IEnumerable
    {
        private readonly T head;
        private readonly ImmutableStack<T> tail;

        public readonly static ImmutableStack<T> Empty = new ImmutableStack<T>(default(T), null);

        private ImmutableStack(T head, ImmutableStack<T> tail)
        {
            this.head = head;
            this.tail = tail;
        }

        public int Count => this == Empty ? 0 : tail.Count + 1;

        public ImmutableStack<T> Push(T value)
        {
            if (this == Empty)
                return new ImmutableStack<T>(value, Empty);

            return new ImmutableStack<T>(value, new ImmutableStack<T>(head, tail));
        }

        public ImmutableStack<T> Pop()
        {
            if (this == Empty)
                throw new InvalidOperationException("Stack is empty.");

            return tail;
        }

        public T Peek()
        {
            if (this == Empty)
                throw new InvalidOperationException("Stack is empty.");

            return head;
        }

        public IEnumerator<T> GetEnumerator()
        {
            var current = this;
            
            while (current != Empty)
            {
                yield return current.head;
                current = current.tail;
            }            

        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
