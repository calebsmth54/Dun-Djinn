using UnityEngine;


public static class MathUtil
{
	// expects angle in the range 0 to 360
	// expects min and max in the range -180 to 180
	// returns the clamped angle in the range 0 to 360
	public static float ClampAngle(float angle, float min, float max) 
	{
		if (angle > 180f) // remap 0 - 360 --> -180 - 180
			angle -= 360f;
		angle = Mathf.Clamp(angle, min, max);
		if (angle < 0f) // map back to 0 - 360
			angle += 360f;
		return angle;
	}
}
