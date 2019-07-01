using Photon.Pun;
using UnityEngine;

public class PlayerID : MonoBehaviour
{
	public int playerID = -1;
	public string PlayerName => GetRandomPlayerName();
	public int team = 1;

	private string playerName;

	string[] names = 
	{
		//"Akzotus",
		//"Techn0",
		//"Spike",
		//"Wired",
		//"Orion",
		//"Alex",
		//"Chirs",
		//"eyeCube",
		//"Max",
		//"Music Man",
		//"Nona",
		//"Pigeon Postmaster",
		//"SpecialK",
		//"Ted",
		//"TheComposer",
		//"teehee",
		"Romeo",
		"Gadget",
		"Eagle Eye",
		"Beetle",
		"Skyler",
		"Hotshot",
		"Stubby",
	};

	string GetRandomPlayerName()
	{
		playerName = names[Random.Range(0, names.Length)];
		foreach(Photon.Realtime.Player p in PhotonNetwork.PlayerList)
		{
			if(playerName.Equals(p.NickName))
			{
				NetD.PrintS(p.NickName + " is already taken.");
				return GetRandomPlayerName();
			}
		}
		NetD.PrintS(playerName + " is now assigned.");
		return playerName;
	}
}
