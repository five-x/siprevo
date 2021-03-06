﻿using System;
using System.Net;
using System.Text;
using Sip.Message;
using Base.Message;
using NUnit.Framework;

namespace SipMessageTest
{
	[TestFixture]
	public class ByteArrayWriterTest
	{
		class ByteArrayWriterImplementaion
			: ByteArrayWriter
		{
			public ByteArrayWriterImplementaion(int reservAtBegin, ArraySegment<byte> segment)
				: base(reservAtBegin, segment)
			{
			}

			protected override void Reallocate(ref ArraySegment<byte> segment, int extraSize)
			{
				int newLength = segment.Array.Length + extraSize;
				var array = segment.Array;

				Array.Resize<byte>(ref array, newLength);

				segment = new ArraySegment<byte>(array, 0, newLength);
			}
		}

		private ByteArrayWriter CreateWriter(int size)
		{
			return CreateWriter(0, size);
		}

		private ByteArrayWriter CreateWriter(int reservAtBegin, int size)
		{
			return new ByteArrayWriterImplementaion(reservAtBegin, new ArraySegment<byte>(new byte[size], 0, size));
		}

		[Test]
		public void It_should_write_byte_array()
		{
			var writer = CreateWriter(2);

			var block1 = new byte[] { 0, 1, };
			writer.Write(block1);

			Assert.AreEqual(writer.Segment.Array, block1);

			writer.Write(block1);
			Assert.AreEqual(writer.Segment.Array, new byte[] { 0, 1, 0, 1, });
			Assert.AreEqual(writer.Count, 4);
		}

		[Test]
		public void It_should_write_ByteArrayPart()
		{
			var writer = CreateWriter(2);

			writer.Write(new ByteArrayPart(new byte[] { 0, 1, 2, 3, 4, 5, }, 2, 2));

			Assert.AreEqual(writer.Segment.Array, new byte[] { 2, 3, });
			Assert.AreEqual(writer.Count, 2);
		}

		[Test]
		public void It_should_write_UInt32()
		{
			var writer = CreateWriter(11);

			writer.Write(UInt32.MinValue);
			writer.Write(UInt32.MaxValue);

			var actual = GetWritedArrayPart(writer);
			Assert.AreEqual(new byte[] { 48, 52, 50, 57, 52, 57, 54, 55, 50, 57, 53, }, actual);

			var writer2 = CreateWriter(11);
			writer2.Write(100);
			Assert.AreEqual(new byte[] { 49, 48, 48, }, GetWritedArrayPart(writer2));
		}

		[Test]
		public void It_should_write_Int32()
		{
			var expeted = Encoding.UTF8.GetBytes("-214748364802147483647-1234567890");

			var writer = CreateWriter(expeted.Length);

			writer.Write(Int32.MinValue);
			writer.Write(0);
			writer.Write(Int32.MaxValue);
			writer.Write(-1234567890);

			var actual = GetWritedArrayPart(writer);
			Assert.AreEqual(expeted, actual);
		}

		[Test]
		public void It_should_write_Int32_as_Hex8()
		{
			var expeted = Encoding.UTF8.GetBytes("80000000ffffffff000000007fffffff1234567800abcdef");

			var writer = CreateWriter(expeted.Length);

			writer.WriteAsHex8(Int32.MinValue);
			writer.WriteAsHex8(-1);
			writer.WriteAsHex8(0);
			writer.WriteAsHex8(Int32.MaxValue);
			writer.WriteAsHex8(0x12345678);
			writer.WriteAsHex8(0x00abcdef);

			var actual = GetWritedArrayPart(writer);
			Assert.AreEqual(expeted, actual);
		}

		[Test]
		public void It_should_write_to_top_Int32()
		{
			var expeted = Encoding.UTF8.GetBytes("-214748364802147483647-1234567890");

			var writer = CreateWriter(256, 1024);

			writer.WriteToTop(-1234567890);
			writer.WriteToTop(Int32.MaxValue);
			writer.WriteToTop(0);
			writer.WriteToTop(Int32.MinValue);

			var actual = GetWritedArrayPart(writer);
			Assert.AreEqual(expeted, actual);
		}

		[Test]
		public void It_should_write_IPAddressV4()
		{
			var expeted = Encoding.UTF8.GetBytes("192.168.1.2");

			var writer = CreateWriter(expeted.Length);

			writer.Write(IPAddress.Parse("192.168.1.2"));

			var actual = GetWritedArrayPart(writer);
			Assert.AreEqual(expeted, actual);
		}

		[Test]
		public void It_should_write_IPAddressV6()
		{
			var expeted = Encoding.UTF8.GetBytes("fe80::202:b3ff:fe1e:8329");

			var writer = CreateWriter(expeted.Length);

			writer.Write(IPAddress.Parse("fe80::202:b3ff:fe1e:8329"));

			var actual = GetWritedArrayPart(writer);
			Assert.AreEqual(expeted, actual);
		}

		[Test]
		public void It_should_write_byte()
		{
			var expeted = Encoding.UTF8.GetBytes("0123255");

			var writer = CreateWriter(1024);

			writer.Write((byte)byte.MinValue);
			writer.Write((byte)123);
			writer.Write((byte)byte.MaxValue);

			var actual = GetWritedArrayPart(writer);
			Assert.AreEqual(expeted, actual);
		}

		[Test]
		public void It_should_increase_size_of_buffer()
		{
			var writer = CreateWriter(1);

			var block1 = new byte[] { 1, };
			var block2 = new byte[] { 2, };

			writer.Write(block1);
			Assert.AreEqual(writer.Segment.Count, 1);

			writer.Write(block2);
			Assert.AreEqual(writer.Segment.Count, 2);

			writer.Write(block1);
			Assert.AreEqual(writer.Segment.Count, 3);

			writer.Write(block2);
			Assert.AreEqual(writer.Segment.Count, 4);

			writer.Write(block1);
			Assert.AreEqual(writer.Segment.Count, 5);

			var actual = GetWritedArrayPart(writer);
			Assert.AreEqual(new byte[] { 1, 2, 1, 2, 1 }, actual);
		}

		private byte[] GetWritedArrayPart(ByteArrayWriter writer)
		{
			var bytes = new byte[writer.Count];

			Buffer.BlockCopy(writer.Segment.Array, writer.Offset, bytes, 0, bytes.Length);

			return bytes;
		}
	}
}
