using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using UnityEngine;


namespace Gameplay
{
	/// <summary>
	/// The game board.
	/// </summary>
	public class Board : Singleton<Board>
	{
		public Vector2i.Iterator AllPoses { get { return Pieces.AllIndices(); } }
		public IEnumerable<BoardElement> AllPieces
		{
			get { return Pieces.AllValues().Where(piece => (piece != null)); }
		}
		public IEnumerable<BoardElement> AllHosts
		{
			get { return Hosts.AllValues().Where(host => (host != null)); }
		}


		public BoardElement[,] Pieces, Hosts;
		public int Seed;
		public BoardSizes BoardSize = BoardSizes.Six;

		public GameObject PiecePrefab, HostPrefab;
		public Sprite GridCellSprite;
		public int GridCellSortLayer = 0;

		private System.Random rng;
		private GameObject gridCellsContainer = null;

		
		/// <summary>
		/// Adds an element with the given properties to the board.
		/// </summary>
		/// <param name="isHost">
		/// If true, a host will be created. Otherwise, a piece will be created.
		/// </param>
		public BoardElement AddElement(bool isHost, Vector2i pos, Teams team)
		{
			GameObject go = Instantiate(isHost ? HostPrefab : PiecePrefab);

			var element = go.GetComponent<BoardElement>();
			element.Reset(team, pos);

			if (isHost)
			{
				UnityEngine.Assertions.Assert.IsNull(Hosts.Get(pos));
				Hosts.Set(pos, element);
			}
			else
			{
				UnityEngine.Assertions.Assert.IsNull(Pieces.Get(pos));
				Pieces.Set(pos, element);
			}

			return element;
		}
		/// <summary>
		/// Moves the element at the given position to the new given position.
		/// </summary>
		public void MoveElement(bool isHost, Vector2i oldPos, Vector2i newPos)
		{
			if (isHost)
			{
				Hosts.Get(oldPos).Pos = newPos;

				Hosts.Set(newPos, Hosts.Get(oldPos));
				Hosts.Set(oldPos, null);
			}
			else
			{
				var piece = Pieces.Get(oldPos);
				var host = Hosts.Get(oldPos);

				piece.Pos = newPos;
				Pieces.Set(newPos, piece);
				Pieces.Set(oldPos, null);

				//If the piece just moved off a host, create a new piece on the host.
				if (host != null)
					AddElement(false, oldPos, host.Team);
			}
		}
		/// <summary>
		/// Removes the given piece/host at the given position.
		/// </summary>
		/// <param name="isHost">
		/// If true, will remove the host at the position.
		/// Otherwise, will remove the piece at the position.
		/// </param>
		public void RemoveElement(bool isHost, Vector2i pos)
		{
			if (isHost)
			{
				Destroy(Hosts.Get(pos).gameObject);
				Hosts.Set(pos, null);
			}
			else
			{
				Destroy(Pieces.Get(pos).gameObject);
				Pieces.Set(pos, null);
			}
		}
		/// <summary>
		/// Removes all pieces and hosts from this board.
		/// </summary>
		public void ClearBoard()
		{
			foreach (var piece in AllPieces)
				RemoveElement(false, piece.Pos);
			foreach (var host in AllHosts)
				RemoveElement(true, host.Pos);
		}

		/// <summary>
		/// Applies the given results of a move.
		/// </summary>
		public void Apply(MovementResults moveResults, Teams team)
		{
			//Capture the pieces.
			foreach (var piece in moveResults.Captures)
				piece.Team = team;

			//Convert a block of pieces to a host.
			if (moveResults.HostBlockMinCorner.HasValue)
			{
				//Remove the block of pieces making a host.
				foreach (Vector2i p in new Vector2i.Iterator(new Vector2i(GameConsts.HostBlockSize,
																		  GameConsts.HostBlockSize)))
				{
					Vector2i tilePos = moveResults.HostBlockMinCorner.Value + p;
					if (Pieces.Get(tilePos) != null)
						RemoveElement(false, tilePos);
				}

				//Make the host.
				Vector2i hostPos = moveResults.HostBlockMinCorner.Value +
								   (GameConsts.HostBlockSize / 2);
				if (Hosts.Get(hostPos) != null)
					RemoveElement(true, hostPos);
				AddElement(true, hostPos, team);
			}
		}

		/// <summary>
		/// Gets whether the given position is inside this board.
		/// </summary>
		public bool IsInRange(Vector2i pos)
		{
			return pos.x >= 0 && pos.y >= 0 &&
				   pos.x < (int)BoardSize &&
				   pos.y < (int)BoardSize;
		}
			
		public void ToStream(BinaryWriter stream)
		{
			stream.Write(Seed);
			stream.Write((int)BoardSize);

			//Write the pieces.
			stream.Write(Pieces.AllValues().Count(piece => (piece != null)));
			foreach (var piece in AllPieces)
			{
				stream.Write((int)piece.Team);
				stream.Write(piece.Pos.x);
				stream.Write(piece.Pos.y);
			}

			//Write the hosts.
			stream.Write(Hosts.AllValues().Count(host => (host != null)));
			foreach (var host in AllHosts)
			{
				stream.Write((int)host.Team);
				stream.Write(host.Pos.x);
				stream.Write(host.Pos.y);
			}
		}
		public void FromStream(BinaryReader stream)
		{
			ClearBoard();

			Seed = stream.ReadInt32();
			BoardSize = (BoardSizes)stream.ReadInt32();

			//If the board size is different, resize the arrays.
			if (Pieces.GetLength(0) != (int)BoardSize)
				ResetArrays();

			//Read the pieces.
			int nPieces = stream.ReadInt32();
			for (int i = 0; i < nPieces; ++i)
			{
				Teams team = (Teams)stream.ReadInt32();
				Vector2i pos = new Vector2i(stream.ReadInt32(), stream.ReadInt32());
				AddElement(false, pos, team);
			}
			
			//Read the hosts.
			int nHosts = stream.ReadInt32();
			for (int i = 0; i < nHosts; ++i)
			{
				Teams team = (Teams)stream.ReadInt32();
				Vector2i pos = new Vector2i(stream.ReadInt32(), stream.ReadInt32());
				AddElement(true, pos, team);
			}
		}

		private bool initYet = false;
		public void Start()
		{
			//If this isn't the first time Start() is called, clear the current board.
			if (initYet)
				ClearBoard();
			initYet = true;

			ResetArrays();


			//Generate some cursed hosts/pieces.
			rng = new System.Random(Seed);
			for (int i = 0; i < GameConsts.NHostsByBoardSize[BoardSize]; ++i)
			{
				//Keep generating host positions until we find one that isn't close to another host.
				Vector2i newPos = new Vector2i(rng.Next((int)BoardSize),
											   rng.Next((int)BoardSize));
				while (AllHosts.Any(host => host.Pos.ManhattanDistance(newPos) < 2))
					newPos = new Vector2i(rng.Next((int)BoardSize), rng.Next((int)BoardSize));

				AddElement(true, newPos, Teams.Cursed);
				AddElement(false, newPos, Teams.Cursed);
			}
		}

		private void ResetArrays()
		{
			//Reset the actual arrays.
			int size = (int)BoardSize;
			Pieces = new BoardElement[size, size];
			Hosts = new BoardElement[size, size];
			foreach (Vector2i boardPos in Pieces.AllIndices())
			{
				Pieces.Set(boardPos, null);
				Hosts.Set(boardPos, null);
			}

			//Also reset the grid cell sprites.
			if (gridCellsContainer != null)
				Destroy(gridCellsContainer);
			gridCellsContainer = new GameObject("Grid Cells");
			foreach (Vector2i boardPos in AllPoses)
			{
				GameObject cell = new GameObject(boardPos.ToString());
				cell.transform.parent = gridCellsContainer.transform;
				cell.transform.position = new Vector3(boardPos.x + 0.5f, boardPos.y + 0.5f, 0.0f);

				var spr = cell.AddComponent<SpriteRenderer>();
				spr.sprite = GridCellSprite;
				spr.sortingOrder = GridCellSortLayer;
			}
		}
	}
}