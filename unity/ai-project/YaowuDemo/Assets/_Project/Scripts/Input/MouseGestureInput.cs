using System;
using System.Collections.Generic;
using UnityEngine;

public sealed class MouseGestureInput : MonoBehaviour
{
    public event Action<SkillType, string> SkillRequested;

    private readonly List<Vector2> points = new List<Vector2>();

    private GestureTrailView trailView;
    private Vector2 virtualPointer;

    public bool IsRecording
    {
        get;
        private set;
    }

    public void Initialize(GestureTrailView view)
    {
        trailView = view;
        virtualPointer = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
    }

    private void Update()
    {
        UpdateVirtualPointer();

        if (Input.GetMouseButtonDown(0))
        {
            BeginGesture();
        }

        if (IsRecording && Input.GetMouseButton(0))
        {
            AppendPoint(GetPointerPosition());
        }

        if (IsRecording && Input.GetMouseButtonUp(0))
        {
            EndGesture();
        }

        if (Input.GetMouseButtonDown(1))
        {
            SkillRequested?.Invoke(SkillType.DeitySummon, "鼠标右键请神");
        }
    }

    private void BeginGesture()
    {
        points.Clear();
        IsRecording = true;
        trailView.Begin();
        AppendPoint(GetPointerPosition());
        SimpleEventBus.RaiseRecognitionStatusChanged("记录中: 鼠标左键画符");
    }

    private void UpdateVirtualPointer()
    {
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            virtualPointer += new Vector2(Input.GetAxisRaw("Mouse X"), Input.GetAxisRaw("Mouse Y")) * 18f;
            virtualPointer.x = Mathf.Clamp(virtualPointer.x, 0f, Screen.width);
            virtualPointer.y = Mathf.Clamp(virtualPointer.y, 0f, Screen.height);
            return;
        }

        virtualPointer = Input.mousePosition;
    }

    private Vector2 GetPointerPosition()
    {
        return Cursor.lockState == CursorLockMode.Locked ? virtualPointer : (Vector2)Input.mousePosition;
    }

    private void AppendPoint(Vector2 point)
    {
        if (points.Count > 0 && Vector2.Distance(points[points.Count - 1], point) < 8f)
        {
            return;
        }

        points.Add(point);
        trailView.Append(point);
    }

    private void EndGesture()
    {
        IsRecording = false;
        trailView.End();

        SkillType detectedSkill;
        if (TryDetectSwipe(out detectedSkill) || TryDetectCircle(out detectedSkill))
        {
            string label = detectedSkill == SkillType.SwordQi ? "横划剑气" : "画圈火符";
            SkillRequested?.Invoke(detectedSkill, label);
            return;
        }

        SimpleEventBus.RaiseRecognitionStatusChanged("识别失败: 请重新画符");
    }

    private bool TryDetectSwipe(out SkillType skill)
    {
        skill = SkillType.SwordQi;
        if (points.Count < 4)
        {
            return false;
        }

        Vector2 start = points[0];
        Vector2 end = points[points.Count - 1];
        float deltaX = end.x - start.x;
        float deltaY = end.y - start.y;
        float pathLength = GetPathLength();

        if (Mathf.Abs(deltaX) > 130f && Mathf.Abs(deltaX) > Mathf.Abs(deltaY) * 2.5f && pathLength > Mathf.Abs(deltaX) * 1.05f)
        {
            return true;
        }

        return false;
    }

    private bool TryDetectCircle(out SkillType skill)
    {
        skill = SkillType.FireTalisman;
        if (points.Count < 10)
        {
            return false;
        }

        Vector2 min = points[0];
        Vector2 max = points[0];
        for (int index = 1; index < points.Count; index++)
        {
            min = Vector2.Min(min, points[index]);
            max = Vector2.Max(max, points[index]);
        }

        Vector2 size = max - min;
        float minExtent = Mathf.Min(size.x, size.y);
        float maxExtent = Mathf.Max(size.x, size.y);
        if (minExtent < 70f || maxExtent / minExtent > 1.55f)
        {
            return false;
        }

        float closingDistance = Vector2.Distance(points[0], points[points.Count - 1]);
        if (closingDistance > minExtent * 0.55f)
        {
            return false;
        }

        float accumulatedTurn = 0f;
        for (int index = 1; index < points.Count - 1; index++)
        {
            Vector2 previous = (points[index] - points[index - 1]).normalized;
            Vector2 next = (points[index + 1] - points[index]).normalized;
            if (previous.sqrMagnitude < 0.001f || next.sqrMagnitude < 0.001f)
            {
                continue;
            }

            accumulatedTurn += Mathf.Abs(Vector2.SignedAngle(previous, next));
        }

        return accumulatedTurn > 250f;
    }

    private float GetPathLength()
    {
        float length = 0f;
        for (int index = 1; index < points.Count; index++)
        {
            length += Vector2.Distance(points[index - 1], points[index]);
        }

        return length;
    }
}
