using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;


namespace Gameplay
{
	/// <summary>
	/// Manages game logic, including enabling/disabling UI overlays and determining who won.
	/// </summary>
	public class Logic : Singleton<Logic>
	{
		public GameObject TurnUI_Julia, TurnUI_Billy, TurnUI_Curse;
		public GameObject Winner_Friends, Winner_Cursed;
		public ScaleCameraToFit CamRegion;


		public Players CurrentPlayer
		{
			get { return currentPlayer; }
			set
			{
				currentPlayer = value;
				TurnUI_Julia.SetActive(currentPlayer == Players.Julia);
				TurnUI_Billy.SetActive(currentPlayer == Players.Billy);
				TurnUI_Curse.SetActive(currentPlayer == Players.Curse);
			}
		}
		private Players currentPlayer;


		public bool DidHumansWin(Board board)
		{
			//The humans win if there are no more cursed pieces,
			//    and all cursed hosts are occupied by friendly pieces.
			return board.AllPieces.All(piece => !piece.IsCursed) &&
				   board.AllHosts.All(host => !host.IsCursed ||
											  (board.Pieces.Get(host.Pos) != null &&
											   !board.Pieces.Get(host.Pos).IsCursed));
		}
		public bool DidCurseWin(Board board)
		{
			//The curse wins if there are no empty spaces to place new pieces,
			//    and any existing friendly pieces have no movement options.
			List<Moves_Billy> friendlyMoves = new List<Moves_Billy>();
			return board.AllPoses.Enumerable().All(pos => board.Pieces.Get(pos) != null) &&
				   board.AllPieces.All(piece =>
				   {
					   if (piece.IsCursed)
						   return true;
					   else
					   {
						   friendlyMoves.Clear();
						   Moves_Billy.GetMoves(board, piece, friendlyMoves);
						   return friendlyMoves.Count == 0;
					   }
				   });
		}

		public void EndGame(Teams winner)
		{
			TurnUI_Julia.SetActive(false);
			TurnUI_Billy.SetActive(false);
			TurnUI_Curse.SetActive(false);

			Winner_Friends.SetActive(winner == Teams.Friendly);
			Winner_Cursed.SetActive(winner == Teams.Cursed);
		}


		protected override void Awake()
		{
			base.Awake();

			TurnUI_Julia.SetActive(false);
			TurnUI_Billy.SetActive(false);
			TurnUI_Curse.SetActive(false);
		}
		private void Start()
		{
			CurrentPlayer = (Players)0;

			float boardSize = (int)Board.Instance.BoardSize;
			CamRegion.RegionToFit = new Rect(0.0f, 0.0f, boardSize, boardSize);

			//TODO: Figure out how to position the UI elements. Get code from board games repo.
		}
	}
}
