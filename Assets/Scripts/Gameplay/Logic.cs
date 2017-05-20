using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;


namespace Gameplay
{
	public class Logic : Singleton<Logic>
	{
		public Players CurrentPlayer = (Players)0;


		public static bool DidHumansWin(Board board)
		{
			//The humans win if there are no more cursed pieces,
			//    and all cursed hosts are occupied by friendly pieces.
			return board.AllPieces.All(piece => !piece.IsCursed) &&
				   board.AllHosts.All(host => !host.IsCursed ||
											  (board.Pieces.Get(host.Pos) != null &&
											   !board.Pieces.Get(host.Pos).IsCursed));
		}
		public static bool DidCurseWin(Board board)
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
	}
}
