using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class MovesUI_Julia : MonoBehaviour
{
	public Sprite OptionSprite;
	public Color SpriteCol = Color.white;
	public float SpriteScale = 0.9f;

	private List<SpriteRenderer> moveSprites;
	private List<Gameplay.Moves_Julia> moveOptions = new List<Gameplay.Moves_Julia>();


	private void Start()
	{
		Gameplay.Logic.Instance.OnTurnChanged += Callback_TurnChanged;
	}

	private void Callback_TurnChanged()
	{
		if (Gameplay.Logic.Instance.CurrentPlayer == Gameplay.Players.Julia)
		{
			//Get and display the moves.

			moveOptions.Clear();
			Gameplay.Moves_Julia.GetMoves(Gameplay.Board.Instance, moveOptions);

			moveSprites = SpritePool.Instance.AllocateSprites(moveOptions.Count, OptionSprite,
															  1, null, "Julia Move");

			for (int i = 0; i < moveSprites.Count; ++i)
			{
				moveSprites[i].transform.position = new Vector3(moveOptions[i].Pos.x + 0.5f,
																moveOptions[i].Pos.y + 0.5f,
																0.0f);
				moveSprites[i].transform.localScale = new Vector3(SpriteScale, SpriteScale, 1.0f);
				moveSprites[i].color = SpriteCol;

				var responder = moveSprites[i].gameObject.AddComponent<InputResponder>();
				AddResponse(responder, moveOptions[i]);
			}
		}
		else
		{
			//Destroy the moves.
			SpritePool.Instance.DeallocateSprites(moveSprites);
			moveSprites.Clear();
		}
	}

	private void AddResponse(InputResponder responder, Gameplay.Moves_Julia move)
	{
		responder.OnStopClick += (_responder, mousePos) =>
		{
			Gameplay.Board.Instance.AddElement(false, move.Pos, Gameplay.Teams.Friendly);

			Gameplay.Logic.Instance.MovesLeftThisTurn -= 1;
			if (Gameplay.Logic.Instance.MovesLeftThisTurn == 0)
			{
				Gameplay.Logic.Instance.AdvanceTurn();
			}
		};
	}
}