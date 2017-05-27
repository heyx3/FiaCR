using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


[RequireComponent(typeof(TextMesh))]
public class DisplayMovesLeft : MonoBehaviour
{
	private TextMesh text;

	private void Awake()
	{
		text = GetComponent<TextMesh>();
	}
	private void Start()
	{
		Gameplay.Logic.Instance.OnMovesLeftChanged += Callback_MovesLeftChanged;
	}
	private void OnDestroy()
	{
		Gameplay.Logic.Instance.OnMovesLeftChanged -= Callback_MovesLeftChanged;
	}

	private void Callback_MovesLeftChanged()
	{
		text.text = Gameplay.Logic.Instance.MovesLeftThisTurn.ToString();
	}
}