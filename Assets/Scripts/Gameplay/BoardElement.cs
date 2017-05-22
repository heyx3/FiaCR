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
				pos = value;

				var tr = transform;
				tr.position = new Vector3(pos.x + 0.5f, pos.y + 0.5f, tr.position.z);
			}
		}

		
		public GameObject ChildSprite_Friendly, ChildSprite_Cursed;
		
		private bool isCursed;
		private Vector2i pos;


		public void Reset(Teams team, Vector2i pos)
		{
			Team = team;
			Pos = pos;
		}
	}
}
