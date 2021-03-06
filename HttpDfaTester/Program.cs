﻿using System;
using System.Collections.Generic;
using System.Text;
using Http.Message;

namespace HttpDfaTester
{
	class Program
	{
		static void Main(string[] args)
		{
			Console.Write("Loading...");
			int start = Environment.TickCount;

			int loadTablesDelay = Environment.TickCount - start;
			HttpMessageReader.LoadTables(@"..\..\..\Http.Message\bin\Release");
			start = Environment.TickCount;
			var dfa = new HttpMessageReader();
			dfa.SetDefaultValue();
			dfa.Parse(new byte[] { 0 }, 0, 1);
			Console.WriteLine("Done (LoadTables {0} ms + JIT {1} ms)", loadTablesDelay, Environment.TickCount - start);

			var utf = new UTF8Encoding();

			var message0 = utf.GetBytes(
				"POST /enlighten/calais.asmx/Enlighten HTTP/1.1\r\n" +
				"Test: test\r\n" +
				"Host: api.opencalais.com:9000\r\n" +
				"Content-Type: application/x-www-form-urlencoded; charset=UTF-8\r\n" +
				//"Content-Type: application/x-www-form-urlencoded\r\n" +
				"Content-Length: 123\r\n" +
				"Upgrade: first, websocket, websocketex, websocket/v15, second/567,     third\r\n" +
				"Referer: http://localhost/#ffff\r\n" +
				//"Referer: referer.com\r\n" +
				"Origin: http://example.com\r\n" +
				"Sec-WebSocket-Key: dGhlIHNhbXBsZSBub25jZQ==\r\n" +
				"Sec-WebSocket-Protocol: chat  ,   superchat\r\n" +
				"Cookie: session-id1=1; session-id2=2\r\n" +
				"Cookie: session-id3=3; session-id4=4\r\n" +
				"Sec-WebSocket-Version: 13\r\n" +
				"If-Match: \"01234567\"\r\n" +
				"\r\n");

			dfa.SetDefaultValue();
			int proccessed = dfa.Parse(message0, 0, message0.Length);
			dfa.SetArray(message0);

			Console.WriteLine("Total: {0}", message0.Length);
			Console.WriteLine("Proccessed: {0}", proccessed);
			Console.WriteLine("Final: {0}", dfa.IsFinal);
			Console.WriteLine("Error: {0}", dfa.IsError);

			Console.WriteLine("Method: {0}", dfa.Method);
			Console.WriteLine("Request-URI: |{0}|", dfa.RequestUri.ToString());
			Console.WriteLine("Host: |{0}| : {1}", dfa.Host.Host.ToString(), dfa.Host.Port);
			Console.WriteLine("Content-Type: |{0}|", dfa.ContentType.Value.ToString());
			Console.WriteLine("Content-Length: {0}", dfa.ContentLength);
			Console.WriteLine("Referer: |{0}|", dfa.Referer.ToString());

			Console.WriteLine("Cookie");
			for (int i = 0; i < dfa.Count.Cookie; i++)
				Console.WriteLine("  #{0} |{1}| : |{2}|", i, dfa.Cookie[i].Name.ToString(), dfa.Cookie[i].Value.ToString());

			//Console.WriteLine("Upgrade");
			//for (int i = 0; i < dfa.Count.UpgradeCount; i++)
			//    Console.WriteLine("  #{0} |{1}| + |{2}| = |{3}| ({4})", i, dfa.Upgrades[i].Name.ToString(), dfa.Upgrades[i].Version.ToString(), dfa.Upgrades[i].Value.ToString(), dfa.Upgrades[i].Upgrate);

			Console.Write("Upgrade: ");
			for (int i = 0; i < dfa.Count.Upgrade; i++)
				Console.Write("{0}, ", dfa.Upgrade[i]);
			Console.WriteLine();

			Console.Write("If-Match: ");
			for (int i = 0; i < dfa.Count.IfMatches; i++)
				Console.Write("{0}, ", dfa.IfMatches[i].ToString());
			Console.WriteLine();

			Console.WriteLine("Sec-WebSocket-Key: |{0}|", dfa.SecWebSocketKey.ToString());
			Console.Write("Sec-WebSocket-Protocol: ");
			for (int i = 0; i < dfa.Count.SecWebSocketProtocol; i++)
				Console.Write("|{0}|, ", dfa.SecWebSocketProtocol[i].ToString());
			Console.WriteLine();
			Console.WriteLine("Sec-WebSocket-Version: {0}", dfa.SecWebSocketVersion);

			var message1 = utf.GetBytes(
				"GET /api/role?time=1343516363983&sig=4c25f6162d70ede434b37571cbe23201 HTTP/1.1\r\n" +
				"Accept: */*\r\n" +
				"Content-Type: application/json\r\n" +
				"X-Requested-With: XMLHttpRequest\r\n" +
				"Referer: http://localhost/#\r\n" +
				"Accept-Language: ru\r\n" +
				"Accept-Encoding: gzip, deflate\r\n" +
				"User-Agent: Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.0; Trident/5.0)\r\n" +
				"Host: localhost\r\n" +
				"Connection: Keep-Alive\r\n" +
				"\r\n");


			//TestSpeed(dfa, message0);
		}

		private static void TestSpeed(HttpMessageReader dfa, byte[] message)
		{
			Console.WriteLine("Testing speed");

			int repeat = 1000000;
			int start2 = Environment.TickCount;
			for (int i = 0; i < repeat; i++)
			{
				dfa.SetDefaultValue();
				dfa.Parse(message, 0, message.Length);
				dfa.SetArray(message);
			}
			int spent = Environment.TickCount - start2;

			Console.WriteLine("Parsed {0} times, {1} ms", repeat, spent);
		}
	}
}
