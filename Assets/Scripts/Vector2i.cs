public struct Vector2i : System.IEquatable<Vector2i>
{
	public static Vector2i Zero { get { return new Vector2i(0, 0); } }

	public int x, y;

	public Vector2i(int _x, int _y) { x = _x; y = _y; }

	public Vector2i LessX { get { return new Vector2i(x - 1, y); } }
	public Vector2i LessY { get { return new Vector2i(x, y - 1); } }
	public Vector2i MoreX { get { return new Vector2i(x + 1, y); } }
	public Vector2i MoreY { get { return new Vector2i(x, y + 1); } }

	public static Vector2i operator +(Vector2i a, Vector2i b) { return new Vector2i(a.x + b.x, a.y + b.y); }
	public static Vector2i operator +(Vector2i a, int b) { return new Vector2i(a.x + b, a.y + b); }
	public static Vector2i operator -(Vector2i a, Vector2i b) { return new Vector2i(a.x - b.x, a.y - b.y); }
	public static Vector2i operator -(Vector2i a, int b) { return new Vector2i(a.x - b, a.y - b); }
	public static Vector2i operator *(Vector2i a, int b) { return new Vector2i(a.x * b, a.y * b); }
	public static Vector2i operator /(Vector2i a, int b) { return new Vector2i(a.x / b, a.y / b); }
	public static Vector2i operator -(Vector2i a) { return new Vector2i(-a.x, -a.y); }

	public static bool operator ==(Vector2i a, Vector2i b) { return a.x == b.x && a.y == b.y; }
	public static bool operator !=(Vector2i a, Vector2i b) { return !(a == b); }


	public override string ToString()
	{
		return "{" + x + ", " + y + "}";
	}
	public override int GetHashCode()
	{
		return (x * 73856093) ^ (y * 19349663);
	}
	public override bool Equals(object obj)
	{
		return (obj is Vector2i) && ((Vector2i)obj) == this;
	}
	public bool Equals(Vector2i v) { return v == this; }


	#region Iterator definition
	public struct Iterator
	{
		public Vector2i MinInclusive { get { return minInclusive; } }
		public Vector2i MaxExclusive { get { return maxExclusive; } }
		public Vector2i Current { get { return current; } }

		private Vector2i minInclusive, maxExclusive, current;

		public Iterator(Vector2i maxExclusive) : this(Vector2i.Zero, maxExclusive) { }
		public Iterator(Vector2i _minInclusive, Vector2i _maxExclusive)
		{
			minInclusive = _minInclusive;
			maxExclusive = _maxExclusive;

			current = Vector2i.Zero; //Just to make the compiler shut up
			Reset();
		}

		public bool MoveNext()
		{
			current.x += 1;
			if (current.x >= maxExclusive.x)
				current = new Vector2i(minInclusive.x, current.y + 1);

			return (current.y < maxExclusive.y);
		}
		public void Reset() { current = new Vector2i(minInclusive.x - 1, minInclusive.y); }
		public void Dispose() { }

		public Iterator GetEnumerator() { return this; }
		public System.Collections.Generic.IEnumerable<Vector2i> Enumerable()
		{
			foreach (Vector2i v in this)
				yield return v;
		}
	}
	/// <summary>
	/// All 4 orthogonal neighbors to a given position.
	/// </summary>
	public struct Neighbors
	{
		public static readonly Vector2i[] LocalPoses = {
			new Vector2i(-1, 0), new Vector2i(1, 0),
			new Vector2i(0, -1), new Vector2i(0, 1)
		};

		public Vector2i SourcePos { get { return sourcePos; } }
		public int CurrentIndex { get { return currentIndex; } }
		public Vector2i Current { get { return LocalPoses[currentIndex]; } }

		private Vector2i sourcePos;
		private int currentIndex;

		public Neighbors(Vector2i _sourcePos)
		{
			sourcePos = _sourcePos;
			currentIndex = -1;
		}

		public bool MoveNext()
		{
			currentIndex += 1;
			return (currentIndex < 4);
		}
		public void Reset() { currentIndex = -1; }
		public void Dispose() { }

		public Neighbors GetEnumerator() { return this; }
	}
	#endregion
}