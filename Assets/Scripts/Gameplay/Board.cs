﻿using System;
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

		public Sprite Sprite_Piece_Friendly, Sprite_Piece_Cursed,
					  Sprite_Host_Friendly, Sprite_Host_Cursed;
		public int SpriteDepth_Piece = 1,
				   SpriteDepth_Host = 2;

		
		/// <summary>
		/// Adds an element with the given properties to the board.
		/// </summary>
		/// <param name="isHost">
		/// If true, a host will be created. Otherwise, a piece will be created.
		/// </param>
		public BoardElement AddElement(bool isHost, Vector2i pos, Teams team)
		{
			GameObject go = new GameObject(isHost ? "Host" : "Piece");
			var element = go.AddComponent<BoardElement>();

			if (isHost)
			{
				element.Sprite_Friendly = Sprite_Host_Friendly;
				element.Sprite_Cursed = Sprite_Host_Cursed;
				element.SpriteRender.sortingOrder = SpriteDepth_Host;

				UnityEngine.Assertions.Assert.IsNull(Hosts.Get(pos));
				Hosts.Set(pos, element);
			}
			else
			{
				element.Sprite_Friendly = Sprite_Piece_Friendly;
				element.Sprite_Cursed = Sprite_Piece_Cursed;
				element.SpriteRender.sortingOrder = SpriteDepth_Piece;

				UnityEngine.Assertions.Assert.IsNull(Pieces.Get(pos));
				Pieces.Set(pos, element);
			}

			element.Team = team;
			element.Pos = pos;

			return element;
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
				Destroy(Hosts.Get(pos));
				Hosts.Set(pos, null);
			}
			else
			{
				Destroy(Pieces.Get(pos));
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

		private void ResetArrays()
		{
			int size = (int)BoardSize;
			Pieces = new BoardElement[size, size];
			Hosts = new BoardElement[size, size];
			foreach (Vector2i boardPos in Pieces.AllIndices())
			{
				Pieces.Set(boardPos, null);
				Hosts.Set(boardPos, null);
			}
		}

		private void Start()
		{
			ResetArrays();
		}
	}
}