using UnityEngine;
using System;


public enum CurveType { Hard90, Soft90, Soft180, Wave, Straight, Hard45, LeanA, LeanB, LeanC,  }

public class BezierSpline : MonoBehaviour {

	[SerializeField]
	private Vector3[] points;

	[SerializeField]
	private BezierControlPointMode[] modes;

	[SerializeField]
	private bool loop;


	public bool Loop {
		get {
			return loop;
		}
		set {
			loop = value;
			if (value == true) {
				modes[modes.Length - 1] = modes[0];
				SetControlPoint(0, points[0]);
			}
		}
	}

	public int ControlPointCount {
		get {
			return points.Length;
		}
	}

	public Vector3 GetControlPoint (int index) {
		return points[index];
	}

	public void SetControlPoint (int index, Vector3 point) {
		if (index % 3 == 0) {
			Vector3 delta = point - points[index];
			if (loop) {
				if (index == 0) {
					points[1] += delta;
					points[points.Length - 2] += delta;
					points[points.Length - 1] = point;
				}
				else if (index == points.Length - 1) {
					points[0] = point;
					points[1] += delta;
					points[index - 1] += delta;
				}
				else {
					points[index - 1] += delta;
					points[index + 1] += delta;
				}
			}
			else {
				if (index > 0) {
					points[index - 1] += delta;
				}
				if (index + 1 < points.Length) {
					points[index + 1] += delta;
				}
			}
		}
		points[index] = point;
		EnforceMode(index);
	}

	public BezierControlPointMode GetControlPointMode (int index) {
		return modes[(index + 1) / 3];
	}

	public void SetControlPointMode (int index, BezierControlPointMode mode) {
		int modeIndex = (index + 1) / 3;
		modes[modeIndex] = mode;
		if (loop) {
			if (modeIndex == 0) {
				modes[modes.Length - 1] = mode;
			}
			else if (modeIndex == modes.Length - 1) {
				modes[0] = mode;
			}
		}
		EnforceMode(index);
	}

	private void EnforceMode (int index) {
		int modeIndex = (index + 1) / 3;
		BezierControlPointMode mode = modes[modeIndex];
		if (mode == BezierControlPointMode.Free || !loop && (modeIndex == 0 || modeIndex == modes.Length - 1)) {
			return;
		}

		int middleIndex = modeIndex * 3;
		int fixedIndex, enforcedIndex;
		if (index <= middleIndex) {
			fixedIndex = middleIndex - 1;
			if (fixedIndex < 0) {
				fixedIndex = points.Length - 2;
			}
			enforcedIndex = middleIndex + 1;
			if (enforcedIndex >= points.Length) {
				enforcedIndex = 1;
			}
		}
		else {
			fixedIndex = middleIndex + 1;
			if (fixedIndex >= points.Length) {
				fixedIndex = 1;
			}
			enforcedIndex = middleIndex - 1;
			if (enforcedIndex < 0) {
				enforcedIndex = points.Length - 2;
			}
		}

		Vector3 middle = points[middleIndex];
		Vector3 enforcedTangent = middle - points[fixedIndex];
		if (mode == BezierControlPointMode.Aligned) {
			enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
		}
		points[enforcedIndex] = middle + enforcedTangent;
	}

	public int CurveCount {
		get {
			return (points.Length - 1) / 3;
		}
	}

	public Vector3 GetPoint (float t) {
		int i;
		if (t >= 1f) {
			t = 1f;
			i = points.Length - 4;
		}
		else {
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
        //return transform.TransformPoint(Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t));
        return Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t);
    }
	
	public Vector3 GetVelocity (float t) {
		int i;
		if (t >= 1f) {
			t = 1f;
			i = points.Length - 4;
		}
		else {
			t = Mathf.Clamp01(t) * CurveCount;
			i = (int)t;
			t -= i;
			i *= 3;
		}
        //return transform.TransformPoint(Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;
        return Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t);
    }
	
	public Vector3 GetDirection (float t) {
		return GetVelocity(t).normalized;
	}

    public void AddCurve(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        AddCurve(p0, p1, p2, Quaternion.identity);
    }

    public void AddCurve(Vector3 p0, Vector3 p1, Vector3 p2, Quaternion rotation, float size = 1f)
    {
        Vector3 point = points[points.Length - 1];
		Array.Resize(ref points, points.Length + 3);
		point += rotation * p0 * size;
		points[points.Length - 3] = point;
        point += rotation * p1 * size;
        points[points.Length - 2] = point;
        point += rotation * p2 * size;
        points[points.Length - 1] = point;

		Array.Resize(ref modes, modes.Length + 1);
		modes[modes.Length - 1] = modes[modes.Length - 2];
		EnforceMode(points.Length - 4);
    }

	public void AddCurve () {
		Vector3 point = points[points.Length - 1];
		Array.Resize(ref points, points.Length + 3);
		point.x += 1f;
		points[points.Length - 3] = point;
		point.x += 1f;
		points[points.Length - 2] = point;
		point.x += 1f;
		points[points.Length - 1] = point;

		Array.Resize(ref modes, modes.Length + 1);
		modes[modes.Length - 1] = modes[modes.Length - 2];
		EnforceMode(points.Length - 4);

		if (loop) {
			points[points.Length - 1] = points[0];
			modes[modes.Length - 1] = modes[0];
			EnforceMode(0);
		}
	}

    public void Setup(BezierControlPointMode mode, params Vector3[] points)
    {

        this.points = points;
        modes = new BezierControlPointMode[] { mode };
    }
	
	public void Reset () {
		points = new Vector3[] {
			new Vector3(1f, 0f, 0f),
			new Vector3(2f, 0f, 0f),
			new Vector3(3f, 0f, 0f),
			new Vector3(4f, 0f, 0f)
		};
		modes = new BezierControlPointMode[] {
			BezierControlPointMode.Free,
			BezierControlPointMode.Free
		};
	}

    private static Vector3 zero = Vector3.zero;
    private static Vector3 up = Vector3.up;
    private static Vector3 down = Vector3.down;
    private static Vector3 left = Vector3.left;
    private static Vector3 right = Vector3.right;
    private static Vector3 forward = Vector3.forward;
    private static Vector3 back = Vector3.back;

    private Vector3 v(float x, float y, float z)
    {
        return new Vector3(x, y, z);
    }

    Quaternion YLookRotation(Vector3 dir, Vector3 up)
    {

        Quaternion upToForward = Quaternion.Euler(90.0f, 0f, 0f);
        Quaternion forwardToTarget = Quaternion.LookRotation(dir, up);

        return forwardToTarget * upToForward;
    }

    Vector3 latestUp = Vector3.back;

    public void AddCurveType(CurveType type, Quaternion rotation, float size = 1f, bool applyLatestUp = false)
    {
        //if (applyLatestUp)
        //{
        //    latestUp = rotation * latestUp;
        //    rotation = rotation * YLookRotation(GetDirection(1), latestUp);
        //}
        switch (type)
        {
            case CurveType.Hard90:
                AddCurve(up, zero, right, rotation, size);
                break;
            case CurveType.Soft90:
                AddCurve(up*0.5f, (up * 0.5f) + (right * 0.5f), right * 0.5f, rotation, size);
                break;
            case CurveType.Soft180:
                break;
            case CurveType.Wave:
                AddCurve(up, down + right * 0.5f, up, rotation, size);
                break;
            case CurveType.Straight:
                //AddCurve(up * 0.5f, zero, up * 0.5f, rotation, size);
                AddCurve(zero, up * 0.5f, zero, rotation, size);
                break;
            case CurveType.Hard45:
                break;
            case CurveType.LeanA:
                AddCurve(up, up, up + v(0.25f, 0, 0), rotation, size);
                break;
            case CurveType.LeanB:
                break;
            case CurveType.LeanC:
                break;
            default:
                break;
        }

    }
    
}
