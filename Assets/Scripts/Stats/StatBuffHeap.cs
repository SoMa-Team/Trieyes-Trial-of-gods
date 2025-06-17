using System.Collections.Generic;
using CombatSystem;

namespace Stats
{
    /// <summary>
    /// StatBuff를 끝나는 시간 순으로 관리하는 최소 힙
    /// </summary>
    public class StatBuffHeap
    {
        private List<StatBuff> heap = new List<StatBuff>();

        /// <summary>
        /// 버프를 힙에 추가합니다.
        /// </summary>
        public void Push(StatBuff buff)
        {
            heap.Add(buff);
            int current = heap.Count - 1;
            
            // 힙 속성 유지 (부모보다 자식이 더 큰 endTime을 가져야 함)
            while (current > 0)
            {
                int parent = (current - 1) / 2;
                if (heap[parent].endTime <= heap[current].endTime)
                    break;

                // 부모와 자식 교환
                var temp = heap[parent];
                heap[parent] = heap[current];
                heap[current] = temp;
                current = parent;
            }
        }

        /// <summary>
        /// 가장 빨리 끝나는 버프를 제거하고 반환합니다.
        /// </summary>
        public StatBuff? Pop()
        {
            if (heap.Count == 0)
                return null;

            var result = heap[0];
            heap[0] = heap[heap.Count - 1];
            heap.RemoveAt(heap.Count - 1);

            if (heap.Count > 0)
                Heapify(0);

            return result;
        }

        /// <summary>
        /// 가장 빨리 끝나는 버프를 반환합니다.
        /// </summary>
        public StatBuff? Peek()
        {
            if (heap.Count == 0)
                return null;

            return heap[0];
        }

        /// <summary>
        /// 힙에서 만료된 버프들을 제거합니다.
        /// </summary>
        public void RemoveExpiredBuffs()
        {
            float currentTime = CombatStageManager.Instance.GetTime();
            while (heap.Count > 0 && !heap[0].isPermanent && heap[0].endTime <= currentTime)
            {
                Pop();
            }
        }
        /// <summary>
        /// 힙의 크기를 반환합니다.
        /// </summary>
        public int Count()
        {
            return heap.Count;
        }

        /// <summary>
        /// 힙을 비웁니다.
        /// </summary>
        public void Clear()
        {
            heap.Clear();
        }

        /// <summary>
        /// 힙 속성을 유지하도록 재정렬합니다.
        /// </summary>
        private void Heapify(int index)
        {
            int smallest = index;
            int left = 2 * index + 1;
            int right = 2 * index + 2;

            if (left < heap.Count && heap[left].endTime < heap[smallest].endTime)
                smallest = left;

            if (right < heap.Count && heap[right].endTime < heap[smallest].endTime)
                smallest = right;

            if (smallest != index)
            {
                var temp = heap[index];
                heap[index] = heap[smallest];
                heap[smallest] = temp;
                Heapify(smallest);
            }
        }
    }
} 