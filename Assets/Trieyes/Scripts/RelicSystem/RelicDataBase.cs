using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using UnityEngine;
using System.IO;

namespace RelicSystem
{
    public class RelicDataBase
    {
        private static Dictionary<int, RelicSystem.RelicDataSO> relicDict = new();
        private static bool initialized = false;

        /// <summary>
        /// 반드시 게임 시작 전에서 1번 비동기로 호출해야 합니다!
        /// 호출 예시: await RelicDataBase.InitializeAsync();
        /// 또한 호출 되는 함수는 반드시 비동기로 선언되어야 합니다.
        /// ex) private async void Start()
        /// </summary>
        public static async Task InitializeAsync()
        {
            // 이미 한 번 초기화되었다면 다시 실행하지 않음
            if (initialized) return;

            // Addressables API:
            // "RelicDataSO" 라벨이 붙은 모든 RelicDataSO를 비동기로 로드한다.
            // 두 번째 인자인 람다 함수(so => { ... })는 각각의 SO를 불러올 때마다 실행됨.
            var handle = Addressables.LoadAssetsAsync<RelicSystem.RelicDataSO>("RelicDataSO", so =>
            {
                relicDict[so.id] = so;
            });
            
            // await handle.Task:
            // 모든 SO의 로드가 끝날 때까지 대기 (비동기적으로 완료를 기다림)
            await handle.Task;
            
            // 한 번만 초기화되도록 플래그 설정
            initialized = true;
        }

        public static RelicDataSO GetRelicDataSO(int id)
        {
            if (!initialized)
            {
                Debug.LogWarning("RelicDataBase.InitializeAsync()를 먼저 await으로 호출 해주세요!");
                return null;
            }
            return relicDict.TryGetValue(id, out var so) ? so : null;
        }
    }
}