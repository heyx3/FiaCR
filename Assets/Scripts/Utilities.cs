using System;
using System.Collections.Generic;
using UnityEngine;


public static class Utilities
{
	/// <summary>
	/// Creates a GameObject with a sprite at the given position.
	/// </summary>
	public static SpriteRenderer CreateSprite(Sprite spr, string name = "Sprite",
											  Vector3? pos = null, Transform parent = null,
											  int sortOrder = 0)
	{
		GameObject go = new GameObject(name);
		Transform tr = go.transform;
		tr.parent = parent;

		if (pos.HasValue)
			tr.position = pos.Value;

		SpriteRenderer sprR = go.AddComponent<SpriteRenderer>();
		sprR.sprite = spr;
		sprR.sortingOrder = sortOrder;

		return sprR;
	}


	#region 2D Array extensions

	public static T Get<T>(this T[,] array, Vector2i pos)
    {
        return array[pos.x, pos.y];
    }
    public static void Set<T>(this T[,] array, Vector2i pos, T newVal)
    {
        array[pos.x, pos.y] = newVal;
    }

    public static bool IsInRange<T>(this T[,] array, Vector2i pos)
    {
        return pos.x >= 0 && pos.y >= 0 &&
               pos.x < array.GetLength(0) && pos.y < array.GetLength(1);
    }

    public static int SizeX(this Array array) { return array.GetLength(0); }
    public static int SizeY(this Array array) { return array.GetLength(1); }
    public static Vector2i SizeXY<T>(this T[,] array) { return new Vector2i(array.SizeX(), array.SizeY()); }

    public static Vector2i.Iterator AllIndices<T>(this T[,] array) { return new Vector2i.Iterator(array.SizeXY()); }
	public static IEnumerable<T> AllValues<T>(this T[,] array)
	{
		foreach (Vector2i pos in array.AllIndices())
			yield return array.Get(pos);
	}

	#endregion
}