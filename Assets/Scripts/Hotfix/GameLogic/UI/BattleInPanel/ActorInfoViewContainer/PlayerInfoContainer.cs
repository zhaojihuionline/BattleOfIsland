/****************************************************************************
 * 2025.10 DESKTOP-HUQFF5N
 ****************************************************************************/

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using QFramework;

namespace QFramework.UI
{
	public partial class PlayerInfoContainer : UIComponent
	{
		private void Awake()
		{

		}

		public void Init(Player player)
		{
			playerName.text = player.playerData.PlayerName;
			WoodCountLabel.text = player.playerData.resourceData[0].Quantity.ToString();	
            StoneCountLabel.text = player.playerData.resourceData[1].Quantity.ToString();
            MeatCountLabel.text = player.playerData.resourceData[2].Quantity.ToString();
        }

        public void Init(PlayerOpponentData opponentData)
        {
            playerName.text = opponentData.playerName;
            WoodCountLabel.text = opponentData.WoodCount.ToString();
            StoneCountLabel.text = opponentData.StoneCount.ToString();
            MeatCountLabel.text = opponentData.MeatCount.ToString();
        }


        protected override void OnBeforeDestroy()
		{
		}
	}
}