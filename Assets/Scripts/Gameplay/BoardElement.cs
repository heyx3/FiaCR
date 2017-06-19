using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;


namespace Gameplay
{
	/// <summary>
	/// A piece or host on the game board.
	/// </summary>
	public class BoardElement : MonoBehaviour
	{
		public Teams Team
		{
			get { return isCursed ? Teams.Cursed : Teams.Friendly; }
			set
			{
				switch (value)
				{
					case Teams.Cursed: IsCursed = true; break;
					case Teams.Friendly: IsCursed = false; break;
					default: throw new NotImplementedException(value.ToString());
				}
			}
		}

		public bool IsCursed
		{
			get { return isCursed; }
			set
			{
				isCursed = value;

				ChildSprite_Friendly.SetActive(!isCursed);
				ChildSprite_Cursed.SetActive(isCursed);
			}
		}
		public Vector2i Pos
		{
			get { return pos; }
			set
			{
				var tr = transform;
				tr.position = new Vector3(pos.x + 0.5f, pos.y + 0.5f, tr.position.z);

				pos = value;

				//Play the move animation.
				if (moveCoroutine != null)
					StopCoroutine(moveCoroutine);
				moveCoroutine = StartCoroutine(Coroutine_MoveToPos(pos));
			}
		}

		public GameObject ActiveSpriteObj
		{
			get { return (isCursed ? ChildSprite_Cursed : ChildSprite_Friendly); }
		}


		public GameObject ChildSprite_Friendly, ChildSprite_Cursed;
		public float MoveSpeed = 3.5f;

		private bool isCursed;
		private Vector2i pos;
		private Coroutine moveCoroutine = null;


		public void Reset(Teams team, Vector2i _pos)
		{
			Team = team;
			pos = _pos;

			transform.position = new Vector3(pos.x + 0.5f, pos.y + 0.5f, transform.position.z);
		}

		private System.Collections.IEnumerator Coroutine_MoveToPos(Vector2i end)
		{
			Transform tr = transform;
			Vector2 worldEnd = new Vector2(end.x + 0.5f, end.y + 0.5f);

			//Move at a constant speed every frame until we're close to the destination.
			Vector2 moveDir = (worldEnd - (Vector2)tr.position).normalized;
			float delta = 0.0f;
			while (Vector2.Distance(tr.position, worldEnd) > (delta * Time.deltaTime))
			{
				tr.position += (Vector3)(moveDir * delta * Time.deltaTime);
				delta = MoveSpeed;
				yield return null;
			}

			//We're close enough to snap to the destination position.
			tr.position = worldEnd;
		}
	}
}
