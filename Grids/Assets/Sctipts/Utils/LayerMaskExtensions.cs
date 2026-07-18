using UnityEngine;
using System.Collections;

/**
 * Extensions to Unity's LayerMask class
 * 
 */
namespace WordsOnPlay.Utils {

public static class LayerMaskExtensions  {

	/**
	 * Check if a particular gameobject is included in the layermask
	 */

	public static bool Contains(this LayerMask layerMask, GameObject gameObject) {
		return (layerMask.value & (1 << gameObject.layer)) != 0;
	}

	/**
	 * Check if a particular 3D collider is included in the layermask
	 */

	public static bool Contains(this LayerMask layerMask, Collider collider) {
		return (layerMask.value & (1 << collider.gameObject.layer)) != 0;
	}

	/**
	 * Check if a particular 2D collider is included in the layermask
	 */

	public static bool Contains(this LayerMask layerMask, Collider2D collider) {
		return (layerMask.value & (1 << collider.gameObject.layer)) != 0;
	}

}
}