using System;
using Unity.VisualScripting;
using UnityEngine;

public class AttackTrailController : MonoBehaviour
{
    public enum Part 
    { 
        RightHand,
        LeftHand,
        RightFoot,
        LeftFoot
    }

    [Serializable]
    private class TrailEntry
    {
        public Part part;
        public TrailRenderer trail;
    }

    [SerializeField]
    private TrailEntry[] trails;

    private void Awake()
    {
        foreach(var t in trails)
        {
            t.trail.emitting = false;
        }
    }

    //AnimationEventから呼ぶ
    public void TrailOn(string partName)
    {
        if (!Enum.TryParse(partName, out Part part)) return;
        foreach(var t in trails)
        {
            if (t.part != part) continue;
            t.trail.Clear();    //前回の軌跡を消す
            t.trail.emitting = true;
        }
    }

    public void TrailOff(string partName)
    {
        if (!Enum.TryParse(partName, out Part part)) return;
        foreach(var t in trails)
        {
            if(t.part == part)
            {
                t.trail.emitting = false;
            }
        }
    }

    // 被弾・キャンセル時などに全停止
    public void TrailOffAll()
    {
        foreach (var t in trails)
            t.trail.emitting = false;
    }

}
