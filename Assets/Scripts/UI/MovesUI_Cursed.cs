using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class MovesUI_Cursed : Singleton<MovesUI_Cursed>
{
	public float WaitInterval = 0.75f;
	public Color MovedColor = Color.gray;

	private Coroutine makeMovesCoroutine = null;

	private Gameplay.Logic Logic { get { return Gameplay.Logic.Instance; } }
	private Gameplay.Board Board { get { return Gameplay.Board.Instance; } }


	private void Start()
	{
		Logic.OnTurnChanged += Callback_TurnChanged;
	}

	private void Callback_TurnChanged()
	{
		//Stop Cursed turn logic.
		if (makeMovesCoroutine != null)
			StopCoroutine(makeMovesCoroutine);

		//Undo any modifications to piece color.
		foreach (var piece in Board.AllPieces)
			piece.ActiveSpriteObj.GetComponent<SpriteRenderer>().color = Color.white;

		//If it's the Cursed's turn, start Cursed turn logic.
		if (Logic.CurrentPlayer == Gameplay.Players.Curse)
			makeMovesCoroutine = StartCoroutine(MakeMoves());
	}
	private System.Collections.IEnumerator MakeMoves()
	{
		//Get all available moves.
		List<List<Gameplay.Move_Curse>> moveOptionsPerPiece = new List<List<Gameplay.Move_Curse>>();
		Gameplay.Move_Curse.GetMoves(Board, moveOptionsPerPiece);

		//Randomly choose some moves to apply.
		System.Random rng = new System.Random(unchecked(Board.Seed * (Logic.TurnIndex + 1)));
		float moveChance = Gameplay.GameConsts.ChanceCurseMoveByBoardSize[Board.BoardSize];
		foreach (var pieceMoveOptions in moveOptionsPerPiece)
		{
			if (pieceMoveOptions.Count > 0 && rng.NextDouble() < moveChance)
			{
				var move = pieceMoveOptions[rng.Next(pieceMoveOptions.Count)];

				var results = new Gameplay.MovementResults(Board, move);
				Board.MoveElement(false, move.Piece.Pos, move.NewPos);
				Board.Apply(results, Gameplay.Teams.Cursed);

				move.Piece.ActiveSpriteObj.GetComponent<SpriteRenderer>().color = MovedColor;

				yield return new WaitForSeconds(WaitInterval);

				//Note that decrementing this value may end the turn,
				//    which automatically kills this coroutine.
				Logic.MovesLeftThisTurn -= 1;
			}

			//Wait a frame, in case the turn just ended.
			yield return null;
		}

		yield return new WaitForSeconds(WaitInterval);

		//If we got this far, we've finished the turn before the number of movable pieces ran out.
		Logic.AdvanceTurn();
	}
}