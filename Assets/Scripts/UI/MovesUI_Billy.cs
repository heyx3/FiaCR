using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class MovesUI_Billy : Singleton<MovesUI_Billy>
{
	public Sprite OptionSprite;
	public Color SpriteCol = Color.white;
	public float SpriteScale = 0.9f;
	public int SpriteLayer = 2;

	private List<SpriteRenderer> moveSprites = null;
	private List<Gameplay.Move_Billy> moveOptions = new List<Gameplay.Move_Billy>();

	private Gameplay.Logic Logic { get { return Gameplay.Logic.Instance; } }
	private Gameplay.Board Board { get { return Gameplay.Board.Instance; } }
	

	private void Start()
	{
		Logic.OnTurnChanged += Callback_TurnChanged;
	}

	private void Callback_TurnChanged()
	{
		if (Logic.CurrentPlayer == Gameplay.Players.Billy)
		{
			//Make each piece a responder that can be clicked on.
			foreach (var piece in Board.AllPieces)
			{
				UnityEngine.Assertions.Assert.IsNotNull(piece.gameObject.GetComponent<Collider2D>(),
														"Every piece must have a collider!");
				var responder = piece.gameObject.AddComponent<InputResponder>();
				responder.OnStopClick += Callback_PieceClicked;
			}
		}
		else
		{
			//Clean up.
			if (moveSprites != null)
			{
				SpritePool.Instance.DeallocateSprites(moveSprites);
				moveSprites = null;

				moveOptions.Clear();

				foreach (var piece in Board.AllPieces)
					Destroy(piece.GetComponent<InputResponder>());
			}
		}
	}

	private void Callback_PieceClicked(InputResponder piece, Vector2 worldMousePos)
	{
		//Clean up any previous moves being displayed.
		if (moveSprites != null)
			SpritePool.Instance.DeallocateSprites(moveSprites);
		moveOptions.Clear();

		//TODO: This is all weird.
		Gameplay.Move_Billy.GetMoves(Board, piece.GetComponent<Gameplay.BoardElement>(), moveOptions);
		moveSprites = SpritePool.Instance.AllocateSprites(moveOptions.Count, OptionSprite,
														  SpriteLayer, null, "Billy Move");
		for (int i = 0; i < moveSprites.Count; ++i)
		{
			moveSprites[i].transform.position = new Vector3(moveOptions[i].NewPos.x + 0.5f,
															moveOptions[i].NewPos.y + 0.5f,
															0.0f);
			moveSprites[i].transform.localScale = new Vector3(SpriteScale, SpriteScale, 1.0f);
			moveSprites[i].color = SpriteCol;

			var collider = moveSprites[i].gameObject.AddComponent<BoxCollider2D>();

			var responder = moveSprites[i].gameObject.AddComponent<InputResponder>();
			AddResponse(responder, moveOptions[i]);
		}
	}
	private void AddResponse(InputResponder responder, Gameplay.Move_Billy move)
	{
		responder.OnStopClick += (_responder, mousePos) =>
		{
			//Apply the move.
			var results = new Gameplay.MovementResults(Board, move);
			Board.MoveElement(false, move.Piece.Pos, move.NewPos);
			Board.Apply(results, Gameplay.Teams.Friendly);

			//Stop this piece from being movable again.
			Destroy(responder);

			Logic.MovesLeftThisTurn -= 1;
		};
	}
}