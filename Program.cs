using Newtonsoft.Json.Linq;
using System;
using System.IO;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace CollabDungeonServer
{
	public class Note
	{
		public string Text;
		public int X;
		public int Y;
		public int DungeonId;
		public bool IsDead;
	}
	public class CollabDungeon : WebSocketBehavior
	{
		protected override void OnMessage(MessageEventArgs e)
		{
			Console.WriteLine(e.Data);
			dynamic pk = JObject.Parse(e.Data);
			Console.WriteLine(pk.id == "GET_ALL_NOTES");
			switch ((string)pk.id)
			{
				case "STORE_NOTE":
					var text = (string)pk.Text;
					var x = (int)pk.X;
					var y = (int)pk.Y;
					var isDead = (bool)pk.IsDead;
					var dungeonId = (int)pk.DungeonId;
					Console.WriteLine($"storing note {text} at [{x};{y}] in dungeon id {dungeonId}");
					var file = JArray.Parse(File.ReadAllText("data.json"));
					file.Add(JObject.FromObject(new Note
					{
						Text = text,
						X = x,
						Y = y,
						DungeonId = dungeonId,
						IsDead = isDead
					}));
					File.WriteAllText("data.json", file.ToString());
					break;
				case "GET_ALL_NOTES":
					Console.WriteLine("WOW");
					Console.WriteLine("Get all notes");
					var r = new JObject();
					r["id"] = "ALL_NOTES";
					r["notes"] = JArray.Parse(File.ReadAllText("data.json"));
					Console.WriteLine(r.ToString());

					Send(r.ToString());
					break;
			}
		}
	}

	public class Program
	{
		public static void Main(string[] args)
		{
			if(!File.Exists("data.json"))
			{
				File.Create("data.json").Close();
			}
			var wssv = new WebSocketServer(0x1337);
			wssv.AddWebSocketService<CollabDungeon>("/");
			wssv.Start();
			Console.ReadKey(true);
			wssv.Stop();
		}
	}
}
