using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(BoxCollider))]
public class Trigger : MonoBehaviour
{
    [HideInInspector] public int setId = 0;

    public int[] targetObjectNumbers = { 1 };

    private bool hasTriggered = false;
    private List<Pedestrian> cachedTargets = null; // null = 아직 캐싱 안 됨

    void Start()
    {
        GetComponent<BoxCollider>().isTrigger = true;
    }

    void CacheTargets()
    {
        cachedTargets = new List<Pedestrian>();
        Pedestrian[] all = FindObjectsOfType<Pedestrian>();

        foreach (int number in targetObjectNumbers)
        {
            bool found = false;
            foreach (Pedestrian p in all)
            {
                if (p.setId == setId && p.objectNumber == number)
                {
                    cachedTargets.Add(p);
                    found = true;
                    break;
                }
            }
            if (!found)
                Debug.LogWarning($"[Trigger] Set {setId} / 번호 {number}인 Pedestrian을 찾지 못했습니다.");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        if (hasTriggered) return;

        if (cachedTargets == null)
            CacheTargets();

        hasTriggered = true;

        foreach (Pedestrian target in cachedTargets)
        {
            target.Activate();
            Debug.Log($"[Trigger] Set {setId} / Pedestrian {target.objectNumber} 활성화");
        }
    }

}