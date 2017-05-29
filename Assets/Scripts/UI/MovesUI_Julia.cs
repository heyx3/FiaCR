using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class MovesUI_Julia : Singleton<MovesUI_Julia>
{
	public Sprite OptionSprite;
	public Color SpriteCol = Color.white;
	public float SpriteScale = 0.9f;
	public int SpriteLayer = 2;

	private List<SpriteRenderer> moveSprites;
	private List<Gameplay.Move_Julia> moveOptions = new List<Gameplay.Move_Julia>();
	private Dictionary<Vector2i, int> moveSpriteIndexAtPos;

	private Gameplay.Logic Logic { get { return Gameplay.Logic.Instance; } }
	private Gameplay.Board Board { get { return Gameplay.Board.Instance; } }


	private void Start()
	{
		//Note: we're assuming that Logic.Start() has already been called.
		Logic.OnTurnChanged += Callback_TurnChanged;
		Callback_TurnChanged();
	}

	private void Callback_TurnChanged()
	{
		if (Logic.CurrentPlayer == Gameplay.Players.Julia)
		{
			//Get and display the moves.

			moveOptions.Clear();
			Gameplay.Move_Julia.GetMoves(Board, moveOptions);

			moveSpriteIndexAtPos = new Dictionary<Vector2i, int>();
			moveSprites = SpritePool.Instance.AllocateSprites(moveOptions.Count, OptionSprite,
															  SpriteLayer, null, "Julia Move");

			for (int i = 0; i < moveSprites.Count; ++i)
			{
				moveSpriteIndexAtPos.Add(moveOptions[i].Pos, i);

				moveSprites[i].transform.position = new Vector3(moveOptions[i].Pos.x + 0.5f,
																moveOptions[i].Pos.y + 0.5f,
																0.0f);
				moveSprites[i].transform.localScale = new Vector3(SpriteScale, SpriteScale, 1.0f);
				moveSprites[i].color = SpriteCol;

				var collider = moveSprites[i].gameObject.AddComponent<BoxCollider2D>();

				var responder = moveSprites[i].gameObject.AddComponent<InputResponder>();
				AddResponse(responder, moveOptions[i]);
			}
		}
		else
		{
			//Destroy the moves.
			if (moveSprites != null)
			{
				SpritePool.Instance.DeallocateSprites(moveSprites);
				moveSprites.Clear();
				moveSpriteIndexAtPos = null;
			}
			moveOptions.Clear();
		}
	}

	private void AddResponse(InputResponder responder, Gameplay.Move_Julia move)
	{
		responder.OnStopClick += (_responder, mousePos) =>
		{
			//Apply the move.
			var results = new Gameplay.MovementResults(Board, move);
			Board.AddElement(false, move.Pos, Gameplay.Teams.Friendly);
			Board.Apply(results, Gameplay.Teams.Friendly);

			//Remove this movement option.
			int moveIndex = moveSpriteIndexAtPos[move.Pos];
			SpritePool.Instance.DeallocateSprite(moveSprites[moveIndex]);
			moveSprites.RemoveAt(moveIndex);
			moveOptions.RemoveAt(moveIndex);
			foreach (Vector2i key in moveSpriteIndexAtPos.Keys.ToList())
				if (moveSpriteIndexAtPos[key] > moveIndex)
					moveSpriteIndexAtPos[key] -= 1;


			Logic.MovesLeftThisTurn -= 1;
		};
	}
}