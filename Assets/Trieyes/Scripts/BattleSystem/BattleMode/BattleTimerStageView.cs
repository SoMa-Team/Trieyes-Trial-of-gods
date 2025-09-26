using UnityEngine;

namespace BattleSystem
{
    /// <summary>
    /// 타이머 기반 전투 스테이지 뷰 컴포넌트
    /// BattleTimerStage 데이터와 Unity GameObject를 연결하는 역할을 합니다.
    /// </summary>
    public class BattleTimerStageView : BattleStageView
    {
        // TODO: 타이머 기반 전투 스테이지 전용 뷰 로직 구현

        [SerializeField] private GameObject beaconPrefab;
        private GameObject _beacon;

        [SerializeField] private GameObject _PilliarPrefab;
        private GameObject _pillar;

        private float duration = 999f;
        private float beaconEdgeOffset = 11f;

        public void Awake()
        {
            TopLeft = new Vector2(-20f, 16f);
            BottomRight = new Vector2(20f, -16f);
        }

        public void CreateBeacon()
        {
            _beacon = Instantiate(beaconPrefab);
            _beacon.gameObject.SetActive(false);
            _beacon.transform.position = GetSubsystemPosition();

            // Particlesystem duration, startLifetime 설정
            var particleSystem = _beacon.GetComponentInChildren<ParticleSystem>();
            var main = particleSystem.main;
            main.duration = duration;
            main.startLifetime = duration;

            // Beacon 컴포넌트 설정
            var beaconComponent = _beacon.GetComponent<Beacon>();
            if (beaconComponent != null)
            {
                // 비콘 초기화
                beaconComponent.Initialize(10, 2);
                
                // BattleTimerStage에 콜백 연결
                beaconComponent.OnBeaconActivated += (BattleStage.now as BattleTimerStage).OnBeaconActivated;
            }

            _beacon.gameObject.SetActive(true);

            // vfx play
            particleSystem.Play();
        }

        internal void CreatePilliar()
        {
            Instantiate(_PilliarPrefab, GetSubsystemPosition(), Quaternion.identity);
        }

        private Vector2 GetSubsystemPosition()
        {
            var spawnPos = new Vector2(
                Random.Range(TopLeft.x + beaconEdgeOffset, 
                BottomRight.x - beaconEdgeOffset), 
                
                Random.Range(BottomRight.y + beaconEdgeOffset, 
                TopLeft.y - beaconEdgeOffset));

            return spawnPos;
        }
    }
}
