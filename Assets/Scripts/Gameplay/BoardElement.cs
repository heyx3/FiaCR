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

				SpriteRender.sprite = (isCursed ?
										  Sprite_Cursed :
										  Sprite_Friendly);
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

		public Sprite Sprite_Friendly, Sprite_Cursed;
		
		[SerializeField]
		private bool isCursed;
		[SerializeField]
		private Vector2i pos;


		public SpriteRenderer SpriteRender { get; private set; }


		private void Awake()
		{
			SpriteRender = GetComponent<SpriteRenderer>();
		}
	}
}
