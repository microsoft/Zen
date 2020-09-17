namespace ZenLib.SymbolicExecution
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    /// A class to efficienty enumerate items recursively.
    /// </summary>
    public sealed class NestedEnumerable<T> : IEnumerable<T>
    {
        /// <summary>
        /// Nested enumerator type.
        /// </summary>
        public sealed class NestedEnumerator : IEnumerator<T>
        {
            /// <summary>
            /// Reference to the parent enumerable.
            /// </summary>
            private List<IEnumerator<T>> enumerators;

            /// <summary>
            /// The index of the current enumerable.
            /// </summary>
            private int index;

            /// <summary>
            /// Create a new instance of the <see cref="NestedEnumerator"/> class.
            /// </summary>
            /// <param name="enumerable">The parent enumerable.</param>
            internal NestedEnumerator(NestedEnumerable<T> enumerable)
            {
                this.enumerators = new List<IEnumerator<T>>(enumerable.enumerables.Count);

                foreach (var e in enumerable.enumerables)
                {
                    enumerators.Add(e.GetEnumerator());
                }

                this.index = this.enumerators.Count - 1;
            }

            /// <summary>
            /// Get the current element.
            /// </summary>
            public T Current
            {
                get
                {
                    return this.enumerators[this.index].Current;
                }
            }

            /// <summary>
            /// Get the current element.
            /// </summary>
            [ExcludeFromCodeCoverage]
            object IEnumerator.Current
            {
                get
                {
                    return this.enumerators[this.index].Current;
                }
            }

            /// <summary>
            /// Disposes the enumerator.
            /// </summary>
            public void Dispose()
            {
                this.Dispose(true);
            }

            /// <summary>
            /// Disposes the enumerator.
            /// </summary>
            /// <param name="disposing"></param>
            [ExcludeFromCodeCoverage]
            void Dispose(bool disposing)
            {
                if (!disposing)
                {
                    return;
                }

                foreach (var enumerator in this.enumerators)
                {
                    enumerator.Dispose();
                }

                this.enumerators.Clear();
            }

            /// <summary>
            /// Move to the next element.
            /// </summary>
            /// <returns>Whether there is a next element.</returns>
            public bool MoveNext()
            {
                while (this.index >= 0)
                {
                    var current = this.enumerators[this.index];

                    if (current.MoveNext())
                    {
                        return true;
                    }
                    else
                    {
                        this.index--;
                    }
                }

                return false;
            }

            /// <summary>
            /// Resets the enumerator.
            /// </summary>
            public void Reset()
            {
                foreach (var enumerator in this.enumerators)
                {
                    enumerator.Reset();
                }

                this.index = this.enumerators.Count - 1;
            }
        }

        /// <summary>
        /// A stack of enumerators.
        /// </summary>
        private List<IEnumerable<T>> enumerables;

        /// <summary>
        /// Creates a new instance of the <see cref="NestedEnumerable{T}"/> class.
        /// </summary>
        public NestedEnumerable()
        {
            this.enumerables = new List<IEnumerable<T>>();
        }

        /// <summary>
        /// Creates a new instance of the <see cref="NestedEnumerable{T}"/> class.
        /// </summary>
        /// <param name="elt">A single element.</param>
        public NestedEnumerable(T elt)
        {
            this.enumerables = new List<IEnumerable<T>>() { Enumerable.Repeat(elt, 1) };
        }

        /// <summary>
        /// Adds a new IEnumerable to iterate.
        /// </summary>
        /// <param name="enumerable">The enumerable.</param>
        public void Add(IEnumerable<T> enumerable)
        {
            CommonUtilities.ValidateNotNull(enumerable);
            this.enumerables.Add(enumerable);
        }

        /// <summary>
        /// Adds all the elements of the nested enumerable to this one.
        /// </summary>
        /// <param name="nestedEnumerable"></param>
        public void AddNested(NestedEnumerable<T> nestedEnumerable)
        {
            CommonUtilities.ValidateNotNull(nestedEnumerable);
            this.enumerables.AddRange(nestedEnumerable.enumerables);
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>The enumerator.</returns>
        public IEnumerator<T> GetEnumerator()
        {
            return new NestedEnumerator(this);
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns>The enumerator.</returns>
        [ExcludeFromCodeCoverage]
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new NestedEnumerator(this);
        }
    }
}
