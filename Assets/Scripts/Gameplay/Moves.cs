using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace Gameplay
{
	//TODO: Maybe constrain julia placements to adjacent friendly pieces?
	//TODO: What to do if the block of pieces is directly centered over a host block? Ignore if it's friendly, overwrite if it's enemy?

	/// <summary>
	/// Placing a new friendly piece onto the field.
	/// </summary>
	public struct Move_Julia
	{
		public static void GetMoves(Board board, List<Move_Julia> outMoves)
		{
			foreach (Vector2i pos in board.AllPoses)
				if (board.Pieces.Get(pos) == null)
					outMoves.Add(new Move_Julia(pos));
		}

		public Vector2i Pos;
		public Move_Julia(Vector2i pos) { Pos = pos; }
	}
	/// <summary>
	/// Moving a friendly piece to another position.
	/// </summary>
	public struct Move_Billy
	{
		public static void GetMoves(Board board, List<Move_Billy> outMoves)
		{
			foreach (var friendlyPiece in board.AllPieces.Where(p => !p.IsCursed))
				GetMoves(board, friendlyPiece, outMoves);
		}
		public static void GetMoves(Board board, BoardElement piece, List<Move_Billy> outMoves)
		{
			//Breadth-first search up to a certain distance away.
			//Each "hop" is a valid move as long as the space is empty.
			//Store a "hop" as the current position plus the number of hops left.
			Queue<KeyValuePair<Vector2i, int>> hopsToCheck = new Queue<KeyValuePair<Vector2i, int>>();
			HashSet<Vector2i> alreadyChecked = new HashSet<Vector2i>();
			hopsToCheck.Enqueue(new KeyValuePair<Vector2i, int>(piece.Pos,
																GameConsts.NSpacesPerBillyMove));
			while (hopsToCheck.Count > 0)
			{
				var hop = hopsToCheck.Dequeue();
				alreadyChecked.Add(hop.Key);

				if (board.Pieces.Get(hop.Key) == null)
					outMoves.Add(new Move_Billy(piece, hop.Key));

				//Make more hops.
				if (hop.Value > 0)
				{
					int nextHop = hop.Value - 1;
					foreach (Vector2i neighborPos in new Vector2i.Neighbors(hop.Key))
					{
						if (board.IsInRange(neighborPos) && board.Pieces.Get(neighborPos) == null &&
							!alreadyChecked.Contains(neighborPos))
						{
							hopsToCheck.Enqueue(new KeyValuePair<Vector2i, int>(neighborPos,
																				nextHop));
						}
					}
				}
			}
		}

		public BoardElement Piece;
		public Vector2i NewPos;
		public Move_Billy(BoardElement piece, Vector2i newPos)
		{
			Piece = piece;
			NewPos = newPos;

			UnityEngine.Assertions.Assert.IsFalse(piece.IsCursed);
		}
	}
	/// <summary>
	/// Moving a cursed piece to another position.
	/// </summary>
	public struct Move_Curse
	{
		public static void GetMoves(Board board, List<List<Move_Curse>> out_MovesPerPiece)
		{
			foreach (var cursedPiece in board.AllPieces.Where(p => p.IsCursed))
			{
				List<Move_Curse> list = null;
				foreach (Vector2i neighborPos in new Vector2i.Neighbors(cursedPiece.Pos))
				{
					if (board.IsInRange(neighborPos) && board.Pieces.Get(neighborPos) == null)
					{
						if (list == null)
							list = new List<Move_Curse>(4);
						list.Add(new Move_Curse(cursedPiece, neighborPos));
					}
				}
				if (list != null)
					out_MovesPerPiece.Add(list);
			}
		}

		public BoardElement Piece;
		public Vector2i NewPos;
		public Move_Curse(BoardElement piece, Vector2i newPos)
		{
			Piece = piece;
			NewPos = newPos;

			UnityEngine.Assertions.Assert.IsTrue(piece.IsCursed);
		}

		/// <summary>
		/// Gets whether this move is still valid on the given board.
		/// </summary>
		public bool IsStillValid(Board board)
		{
			return board.IsInRange(NewPos) && board.Pieces.Get(NewPos) == null;
		}
	}

	/// <summary>
	/// The results of doing one of the above moves.
	/// </summary>
	public class MovementResults
	{
		/// <summary>
		/// If the given piece placement/movement creates a block of identical pieces,
		///     returns the min corner of that block.
		/// Otherwise, returns null.
		/// </summary>
		/// <param name="movedPiecePos">The position of the piece after its movement.</param>
		/// <param name="previousPiecePos">
		/// The position of the piece before its movement,
		///     or "null" if the piece is being placed onto the board.
		/// </param>
		public static Vector2i? GetHostBlock(Board board, Vector2i movedPiecePos,
											 Vector2i? previousPiecePos, Teams team)
		{
			//Assume the movement is made, and create a getter for the new board.

			//First, figure out what happens at the piece's previous position (if it exists).
			Teams? pieceAtPreviousPos = null;
			if (previousPiecePos.HasValue)
			{
				//If a piece moves off of a host, the host creates a new piece in its place.
				var host = board.Hosts.Get(previousPiecePos.Value);
				if (host != null)
					pieceAtPreviousPos = host.Team;
			}

			Func<Vector2i, Teams?> getPieceAt = (boardPos) =>
			{
				//Special cases:
				//Previous position of the moving piece.
				if (previousPiecePos.HasValue && boardPos == previousPiecePos.Value)
					return pieceAtPreviousPos;
				//New position of the moving piece.
				else if (boardPos == movedPiecePos)
					return team;
				//Outside the board.
				else if (!board.IsInRange(boardPos))
					return null;

				var piece = board.Pieces.Get(boardPos);
				return (piece == null ?
					        new Teams?() :
							piece.Team);
			};

			//Try to find a square of identical pieces around the moved piece.
			var minCornerRegion = new Vector2i.Iterator(movedPiecePos - GameConsts.HostBlockSize + 1,
														movedPiecePos + 1);
			foreach (Vector2i minCorner in minCornerRegion)
			{
				bool failed = false;

				var blockRegion = new Vector2i.Iterator(minCorner,
														minCorner + GameConsts.HostBlockSize);
				foreach (Vector2i blockPos in blockRegion)
				{
					Teams? blockTeam = getPieceAt(blockPos);
					if (!blockTeam.HasValue || blockTeam.Value != team)
					{
						failed = true;
						break;
					}
				}

				if (!failed)
					return minCorner;
			}

			//Nothing was found.
			return null;
		}

		/// <summary>
		/// Gets all captures from placing/moving the given piece.
		/// </summary>
		/// <param name="previousPos">
		/// If moving a piece, this is the previous position of that piece.
		/// </param>
		/// <param name="newPos">The new position the piece will occupy.</param>
		/// <param name="ignoreHostBlock">
		/// If not null, this specifies the start of a block of pieces to ignore when checking captures.
		/// The size of the block is the size requried to create a host.
		/// </param>
		public static void GetCaptures(Board board, HashSet<BoardElement> outCaptures,
									   Vector2i? previousPos, Vector2i newPos,
									   Teams pieceTeam, Vector2i? ignoreHostBlockMin)
		{
			//Assume the movement is made, and create a getter for the new board.

			//First, figure out what happens at the piece's previous position (if it exists).
			Teams? pieceAtPreviousPos = null;
			if (previousPos.HasValue)
			{
				//If a piece moves off of a host, the host creates a new piece in its place.
				var host = board.Hosts.Get(previousPos.Value);
				if (host != null)
					pieceAtPreviousPos = host.Team;
			}

			Func<Vector2i, Teams?> getPieceAt = (boardPos) =>
			{
				//Special cases:
				//Part of a host block.
				if (ignoreHostBlockMin.HasValue && IsInHostBlock(boardPos, ignoreHostBlockMin.Value))
					return null;
				//Previous position of the moving piece.
				if (previousPos.HasValue && boardPos == previousPos.Value)
					return pieceAtPreviousPos;
				//New position of the moving piece.
				else if (boardPos == newPos)
					return pieceTeam;
				//Outside the board.
				else if (!board.IsInRange(boardPos))
					return null;

				var piece = board.Pieces.Get(boardPos);
				return (piece == null ?
					        new Teams?() :
							piece.Team);
			};

			//Check all orthogonal directions away from the piece to find a line of enemies.
			foreach (Vector2i lineDir in Vector2i.Neighbors.LocalPoses)
			{
				//If there is a line of one or more enemies, followed by a friendly piece,
				//    then this is a capture.
				Vector2i lineStart = newPos + lineDir;
				if (getPieceAt(lineStart) == pieceTeam.Enemy())
				{
					//Get the end of the line.
					Vector2i lineEnd = lineStart;
					while (getPieceAt(lineEnd + lineDir) == pieceTeam.Enemy())
						lineEnd += lineDir;

					//If the next pos after the line has a friendly piece, we've got a capture.
					Vector2i afterLine = lineEnd + lineDir;
					if (getPieceAt(afterLine) == pieceTeam)
					{
						for (Vector2i enemyPos = lineStart; enemyPos != afterLine; enemyPos += lineDir)
							outCaptures.Add(board.Pieces.Get(enemyPos));
					}
				}
			}
		}
		private static bool IsInHostBlock(Vector2i pos, Vector2i blockMinCorner)
		{
			Vector2i toPos = pos - blockMinCorner;
			return (toPos.x >= 0 && toPos.x < GameConsts.HostBlockSize &&
					toPos.y >= 0 && toPos.y < GameConsts.HostBlockSize);
		}


		/// <summary>
		/// The min corner of the square block of pieces that will be destroyed and replaced with a host.
		/// </summary>
		public Vector2i? HostBlockMinCorner = null;
		/// <summary>
		/// The pieces that will be converted to the other team.
		/// </summary>
		public HashSet<BoardElement> Captures = new HashSet<BoardElement>();


		public MovementResults() { }
		public MovementResults(Board board, Move_Julia move)
		{
			HostBlockMinCorner = GetHostBlock(board, move.Pos, null, Teams.Friendly);
			GetCaptures(board, Captures, null, move.Pos, Teams.Friendly, HostBlockMinCorner);
		}
		public MovementResults(Board board, Move_Billy move)
		{
			HostBlockMinCorner = GetHostBlock(board, move.NewPos, move.Piece.Pos, Teams.Friendly);
			GetCaptures(board, Captures, move.Piece.Pos, move.NewPos,
						Teams.Friendly, HostBlockMinCorner);
		}
		public MovementResults(Board board, Move_Curse move)
		{
			HostBlockMinCorner = GetHostBlock(board, move.NewPos, move.Piece.Pos, Teams.Cursed);
			GetCaptures(board, Captures, move.Piece.Pos, move.NewPos, Teams.Cursed, HostBlockMinCorner);
		}
	}
}
