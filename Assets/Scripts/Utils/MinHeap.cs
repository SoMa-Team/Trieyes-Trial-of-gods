using System;
using System.Collections.Generic;

namespace Utils
{
    /// <summary>
    /// 제네릭 최소 힙(Min Heap) 구현체입니다.
    /// IComparable 인터페이스를 구현하는 모든 타입을 지원합니다.
    /// </summary>
    /// <typeparam name="T">힙에 저장될 요소의 타입 (IComparable 구현 필요)</typeparam>
    public class MinHeap<T> where T : IComparable<T>
    {
        // --- 필드 ---

        private List<T> _heap;

        // --- 프로퍼티 ---

        /// <summary>
        /// 힙의 현재 요소 개수를 반환합니다.
        /// </summary>
        public int Count => _heap.Count;

        /// <summary>
        /// 힙이 비어있는지 확인합니다.
        /// </summary>
        public bool IsEmpty => _heap.Count == 0;

        // --- 생성자 ---

        /// <summary>
        /// 기본 생성자
        /// </summary>
        public MinHeap()
        {
            _heap = new List<T>();
        }

        /// <summary>
        /// 초기 용량을 지정하는 생성자
        /// </summary>
        /// <param name="capacity">초기 용량</param>
        public MinHeap(int capacity)
        {
            _heap = new List<T>(capacity);
        }

        // --- 메서드 ---

        /// <summary>
        /// 힙에 새로운 요소를 추가합니다.
        /// </summary>
        /// <param name="item">추가할 요소</param>
        public void Push(T item)
        {
            _heap.Add(item);
            HeapifyUp(_heap.Count - 1);
        }

        /// <summary>
        /// 힙의 최소값을 제거하고 반환합니다.
        /// </summary>
        /// <returns>힙의 최소값</returns>
        /// <exception cref="InvalidOperationException">힙이 비어있을 때 발생</exception>
        public T Pop()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("힙이 비어있습니다.");
            }

            T min = _heap[0];
            int lastIndex = _heap.Count - 1;

            // 마지막 요소를 루트로 이동
            _heap[0] = _heap[lastIndex];
            _heap.RemoveAt(lastIndex);

            // 힙이 비어있지 않다면 힙 속성을 복원
            if (!IsEmpty)
            {
                HeapifyDown(0);
            }

            return min;
        }

        /// <summary>
        /// 힙의 최소값을 반환하되 제거하지는 않습니다.
        /// </summary>
        /// <returns>힙의 최소값</returns>
        /// <exception cref="InvalidOperationException">힙이 비어있을 때 발생</exception>
        public T Peek()
        {
            if (IsEmpty)
            {
                throw new InvalidOperationException("힙이 비어있습니다.");
            }

            return _heap[0];
        }

        /// <summary>
        /// 힙의 모든 요소를 제거합니다.
        /// </summary>
        public void Clear()
        {
            _heap.Clear();
        }

        /// <summary>
        /// 힙의 모든 요소를 배열로 반환합니다.
        /// </summary>
        /// <returns>힙의 모든 요소 배열</returns>
        public T[] ToArray()
        {
            return _heap.ToArray();
        }

        /// <summary>
        /// 힙의 모든 요소를 리스트로 반환합니다.
        /// </summary>
        /// <returns>힙의 모든 요소 리스트</returns>
        public List<T> ToList()
        {
            return new List<T>(_heap);
        }

        // --- private 메서드 ---

        /// <summary>
        /// 새로운 요소를 추가한 후 힙 속성을 복원합니다 (위로 올라가며 정렬).
        /// </summary>
        /// <param name="index">정렬할 요소의 인덱스</param>
        private void HeapifyUp(int index)
        {
            int parentIndex = GetParentIndex(index);

            // 부모가 존재하고 현재 요소가 부모보다 작다면 교환
            while (index > 0 && _heap[index].CompareTo(_heap[parentIndex]) < 0)
            {
                Swap(index, parentIndex);
                index = parentIndex;
                parentIndex = GetParentIndex(index);
            }
        }

        /// <summary>
        /// 힙 속성을 복원합니다 (아래로 내려가며 정렬).
        /// </summary>
        /// <param name="index">정렬할 요소의 인덱스</param>
        private void HeapifyDown(int index)
        {
            int smallest = index;
            int leftChild = GetLeftChildIndex(index);
            int rightChild = GetRightChildIndex(index);

            // 왼쪽 자식이 존재하고 현재 요소보다 작다면
            if (leftChild < _heap.Count && _heap[leftChild].CompareTo(_heap[smallest]) < 0)
            {
                smallest = leftChild;
            }

            // 오른쪽 자식이 존재하고 가장 작은 요소보다 작다면
            if (rightChild < _heap.Count && _heap[rightChild].CompareTo(_heap[smallest]) < 0)
            {
                smallest = rightChild;
            }

            // 가장 작은 요소가 현재 요소가 아니라면 교환하고 재귀
            if (smallest != index)
            {
                Swap(index, smallest);
                HeapifyDown(smallest);
            }
        }

        /// <summary>
        /// 두 요소의 위치를 교환합니다.
        /// </summary>
        /// <param name="index1">첫 번째 요소의 인덱스</param>
        /// <param name="index2">두 번째 요소의 인덱스</param>
        private void Swap(int index1, int index2)
        {
            T temp = _heap[index1];
            _heap[index1] = _heap[index2];
            _heap[index2] = temp;
        }

        /// <summary>
        /// 부모 노드의 인덱스를 반환합니다.
        /// </summary>
        /// <param name="index">자식 노드의 인덱스</param>
        /// <returns>부모 노드의 인덱스</returns>
        private int GetParentIndex(int index)
        {
            return (index - 1) / 2;
        }

        /// <summary>
        /// 왼쪽 자식 노드의 인덱스를 반환합니다.
        /// </summary>
        /// <param name="index">부모 노드의 인덱스</param>
        /// <returns>왼쪽 자식 노드의 인덱스</returns>
        private int GetLeftChildIndex(int index)
        {
            return 2 * index + 1;
        }

        /// <summary>
        /// 오른쪽 자식 노드의 인덱스를 반환합니다.
        /// </summary>
        /// <param name="index">부모 노드의 인덱스</param>
        /// <returns>오른쪽 자식 노드의 인덱스</returns>
        private int GetRightChildIndex(int index)
        {
            return 2 * index + 2;
        }
    }
}
