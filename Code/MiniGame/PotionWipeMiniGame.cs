using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PotionWipeMiniGame : BaseMiniGame, IDragHandler
{
    [SerializeField] private RectTransform potion; //포션 오브젝트 위치
    [SerializeField] private GameObject particlePrefab; //드래그 시 생성할 파티클 이펙트 프리팹
    [SerializeField] List<RectTransform> dustRects = new List<RectTransform>(); //먼지 리스트
    private Dictionary<RectTransform, int> dustHitCount = new Dictionary<RectTransform, int>(); //먼지 닦인 횟수

    private List<GameObject> spawnedParticles = new List<GameObject>(); //게임 중 생성된 파티클 오브젝트를 저장(게임 종료 시 삭제)
    private float particleCooldown = 0.05f; //최소 간격
    private float lastParticleTime = 0f; //가장 최근에 파티클이 생성된 게임 시작

    private Dictionary<RectTransform, float> dustLastTouchedTime = new Dictionary<RectTransform, float>();
    private float dustCooldown = 0.2f;

    private int dustCount = 2;
    private float oneDustScore = 50;

    public override void StartMiniGame()
    {
        base.StartMiniGame();

        //먼지 위치 랜덤
        float potionHalfWidth = potion.rect.width / 2f;
        float potionHalfHeight = potion.rect.height / 2f;

        List<Rect> placedDustRects = new List<Rect>(); //중복되지 않도록 배치한 먼지 리스트
        foreach(var data in dustRects)
        {
            data.gameObject.SetActive(false);
        }
        for (int i = 0; i < dustCount; i++) //초기화
        {
            RectTransform dust = dustRects[i];
            dust.gameObject.SetActive(true); //먼지 활성화
            dustHitCount[dust] = 0; //먼지가 닦인 횟수 초기화

            var img = dust.GetComponent<UnityEngine.UI.Image>();
            if (img != null)
            {
                Color c = img.color;
                c.a = 1f;
                img.color = c;
            }

            dustLastTouchedTime[dust] = -999f;
        }

        for (int i = 0; i < dustCount; i++) //먼지 배치
        {
            RectTransform dust = dustRects[i];
            float dustHalfWidth = dust.rect.width / 4f;
            float dustHalfHeight = dust.rect.height / 4f;

            bool placed = false;
            int tryCount = 0;

            while (!placed && tryCount < 100)
            {
                tryCount++;

                // 임의 위치 지정
                float randomX = Random.Range(-potionHalfWidth + dustHalfWidth, potionHalfWidth - dustHalfWidth);
                float randomY = Random.Range(-potionHalfHeight + dustHalfHeight, potionHalfHeight - dustHalfHeight);

                Vector2 candidatePos = new Vector2(randomX, randomY);
                dust.anchoredPosition = candidatePos;

                Bounds bounds = RectTransformUtility.CalculateRelativeRectTransformBounds(potion, dust);
                Rect candidateRect = new Rect(bounds.min, bounds.size);

                bool overlaps = false;
                foreach (var otherRect in placedDustRects)
                {
                    if (candidateRect.Overlaps(otherRect))
                    {
                        overlaps = true;
                        break;
                    }
                }

                if (!overlaps)
                {
                    placedDustRects.Add(candidateRect);
                    placed = true;
                }
            }

            if (!placed)
            {
                Debug.LogWarning($"{dust.name} 위치 미중복으로 배치 실패");
            }
        }
        oneDustScore = (float)100/dustCount;
    }
    protected override void SetDifficulty()
    {
        isHardMode = false;
        switch (difficulty)
        {
            case Difficulty.Easy:
                SetTimer(10f);
                dustCount = 2;
                break;
            case Difficulty.Normal:
                SetTimer(5f);
                dustCount = 3;
                break;
            case Difficulty.Hard:
                SetTimer(5f);
                dustCount = 5;
                isHardMode = true;
                break;
        }
    }
    public override void UpdateMiniGame()
    {
        base.UpdateMiniGame(); //시간이 종료되면 알아서 점수 가져감
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 localPointerPos;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(potion, eventData.position, eventData.pressEventCamera, out localPointerPos)) return;
        //드래그 위치를 로컬좌표로 변환하지 못했으면 나가기
                
        if (Time.time - lastParticleTime >= particleCooldown) //마지막 파티클 생성 후 0.05초 지났는지 검사
        {
            lastParticleTime = Time.time;

            //파티클 생성
            Vector3 worldPos;
            if (!RectTransformUtility.ScreenPointToWorldPointInRectangle(potion, eventData.position, eventData.pressEventCamera, out worldPos)) return;
            SpawnParticle(worldPos);
        }

        //먼지 충돌 판정
        for (int i = 0; i < dustCount; i++)
        {
            RectTransform dust = dustRects[i];
            if (!dust.gameObject.activeSelf) continue;

            Vector2 dustCenter = dust.anchoredPosition;
            float distance = Vector2.Distance(localPointerPos, dustCenter);
            float effectiveRange = Mathf.Min(dust.rect.width, dust.rect.height) / 5f;

            if (distance < effectiveRange)
            {
                if (Time.time - dustLastTouchedTime[dust] < dustCooldown) continue;
                dustLastTouchedTime[dust] = Time.time;

                dustHitCount[dust]++;

                float newAlpha = Mathf.Clamp01(1f - (dustHitCount[dust] / 4f));

                var img = dust.GetComponent<UnityEngine.UI.Image>();
                if (img != null)
                {
                    Color c = img.color;
                    c.a = newAlpha;
                    img.color = c;
                }

                // 4번 닦였으면 먼지 제거 및 점수 증가
                if (dustHitCount[dust] >= 4)
                {
                    dust.gameObject.SetActive(false);
                    Score += oneDustScore;
                    Debug.Log($"현재 점수: {Score}");
                }
                break;
            }
        }

        if (Score >= 100)
        {
            StartCoroutine(DelayGameComplete());
        }
    }

    private void SpawnParticle(Vector3 position)
    {
        GameObject particle = Instantiate(particlePrefab, position, Quaternion.identity, potion);
        ParticleSystem ps = particle.GetComponent<ParticleSystem>();
        if (ps != null)
        {
            var main = ps.main;
            main.startSize = Random.Range(0.1f, 0.5f);

            ps.Play();
            Destroy(particle, ps.main.duration + ps.main.startLifetime.constantMax);
        }

        spawnedParticles.Add(particle);
    }

    private IEnumerator DelayGameComplete() //바로 끝나니까 파티클이 안 예뻐서
    {
        yield return new WaitForSeconds(0.5f);

        foreach (var particle in spawnedParticles)
        {
            if (particle != null)
            {
                Destroy(particle);
            }
        }
        spawnedParticles.Clear();

        OnGameComplete?.Invoke(Score);
    }
}