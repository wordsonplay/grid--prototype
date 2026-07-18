using UnityEngine;
using System.Collections;

/**
 * Extension methods for the Vector2 class
 */

namespace WordsOnPlay.Utils {
public static class Vector2Extensions  {

	/**
	 * Test if vector v1 is on the left of v2
	 */

	public static bool IsOnLeft(this Vector2 v1, Vector2 v2) {
		return v1.x * v2.y < v1.y * v2.x;
	}

	/**
	 * Test if vector P1->P2 is on the left of P1->P3
	 */

	public static bool IsOnLeft(this Vector2 p1, Vector2 p2, Vector2 p3) {
		Vector2 v12 = p2-p1;
		Vector2 v13 = p3-p1;
		return v12.IsOnLeft(v13);
	}

	/**
	 * Rotate a 2D vector anticlockwise by the given angle (in degrees)
	 */

	public static Vector2 Rotate(this Vector2 v, float angle) {
		angle *= Mathf.Deg2Rad;
	    return new Vector2(
	        v.x * Mathf.Cos(angle) - v.y * Mathf.Sin(angle),
    	    v.x * Mathf.Sin(angle) + v.y * Mathf.Cos(angle));
	}

	/**
	 * Rotate a 2D vector anticlockwise by 90 degrees
	 */

	public static Vector2 Perp(this Vector2 v) {		
		return new Vector2(-v.y, v.x);
	}

	/**
	 * Project this vector onto another
	 */
	public static Vector2 Project(this Vector2 v, Vector2 onto) {
		Vector2 u = onto.normalized;
		return Vector2.Dot(v, u) * u;
	}
}
}