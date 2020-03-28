using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
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

		protected override void OnOpen()
		{
			base.OnOpen();
			Program.Users.Add(this);
		}

		protected override void OnClose(CloseEventArgs e)
		{
			base.OnClose(e);
			Program.Users.Remove(this);
		}

		protected override void OnMessage(MessageEventArgs e)
		{
			Console.WriteLine(e.Data);
			dynamic pk = JObject.Parse(e.Data);
			Console.WriteLine(pk.id == "GET_ALL_NOTES");
			switch ((string)pk.id)
			{
				case "STORE_NOTE":
				{
					var text = (string)pk.Text;
					var x = (int)pk.X;
					var y = (int)pk.Y;
					var isDead = (bool)pk.IsDead;
					var dungeonId = (int)pk.DungeonId;
					Console.WriteLine($"storing note {text} at [{x};{y}] in dungeon id {dungeonId}");
					var file = JArray.Parse(File.ReadAllText("data.json"));
					foreach(var j in file)
					{
						if((int)j["X"] == x && (int)j["Y"] == y)
						{
							return;
						}
					}
					var note = new Note
					{
						Text = text,
						X = x,
						Y = y,
						DungeonId = dungeonId,
						IsDead = isDead
					};
					file.Add(JObject.FromObject(note));
					File.WriteAllText("data.json", file.ToString());
					var r = new JObject();
					r["id"] = "ALL_NOTES";
					r["notes"] = new JArray
					{
						note
					};
					Console.WriteLine(r.ToString());

					Send(r.ToString());
					foreach (var i in Program.Users)
					{
						if (i != this)
						{
							i.Send(r.ToString());
						}
					}
					break;
				}
				case "GET_ALL_NOTES":
				{
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
	}

	public class Program
	{
		public static List<CollabDungeon> Users = new List<CollabDungeon>();
		public static void Main(string[] args)
		{
			if(!File.Exists("data.json"))
			{
				File.WriteAllText("data.json", "[]");
			}
			var wssv = new WebSocketServer(0x1337);
			wssv.AddWebSocketService<CollabDungeon>("/");
			wssv.Start();
			Console.ReadKey(true);
			wssv.Stop();
		}
	}
}
