using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


namespace Gameplay
{
	/// <summary>
	/// The different available board sizes.
	/// </summary>
	public enum BoardSizes
	{
		Six = 6,
		Seven = 7,
		Eight = 8,
		Nine = 9,
	}

	/// <summary>
	/// The players of this game, in the order they play in.
	/// </summary>
	public enum Players
	{
		Julia,
		Billy,
		Curse
	}

	/// <summary>
	/// The teams in this game.
	/// </summary>
	public enum Teams
	{
		Friendly,
		Cursed,
	}

	public static class GameConsts
	{
		public static Teams Enemy(this Teams t)
		{
			switch (t)
			{
				case Teams.Cursed: return Teams.Friendly;
				case Teams.Friendly: return Teams.Cursed;
				default: throw new NotImplementedException(t.ToString());
			}
		}


		/// <summary>
		/// The number of "hops" a friendly piece can move in one turn.
		/// </summary>
		public static readonly int NSpacesPerBillyMove = 2;
		/// <summary>
		/// The size of the block of identical pieces needed to create a host.
		/// </summary>
		public static readonly int HostBlockSize = 3;
			
		public static readonly Dictionary<BoardSizes, uint> NHostsByBoardSize =
			new Dictionary<BoardSizes, uint>()
			{
				{ BoardSizes.Six, 2 },
				{ BoardSizes.Seven, 3 },
				{ BoardSizes.Eight, 4 },
				{ BoardSizes.Nine, 5 },
			};
		public static readonly Dictionary<BoardSizes, uint> NJuliaMovesByBoardSize =
			new Dictionary<BoardSizes, uint>()
			{
				{ BoardSizes.Six, 3 },
				{ BoardSizes.Seven, 4 },
				{ BoardSizes.Eight, 6 },
				{ BoardSizes.Nine, 9 },
			};
		public static readonly Dictionary<BoardSizes, uint> NBillyMovesByBoardSize =
			new Dictionary<BoardSizes, uint>()
			{
				{ BoardSizes.Six, 3 },
				{ BoardSizes.Seven, 4 },
				{ BoardSizes.Eight, 6 },
				{ BoardSizes.Nine, 9 },
			};
		public static readonly Dictionary<BoardSizes, float> ChanceCurseMoveByBoardSize =
			new Dictionary<BoardSizes, float>()
			{
				{ BoardSizes.Six, 0.85f },
				{ BoardSizes.Seven, 0.75f },
				{ BoardSizes.Eight, 0.65f },
				{ BoardSizes.Nine, 0.55f },
			};
	}
}
