using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace RaCLib.IO
{
    public class EndianBinaryReader : BinaryReader
    {
        public Endianness Endian;

        public override sbyte ReadSByte()
        {
            return base.ReadSByte();
        }

        public override byte ReadByte()
        {
            return base.ReadByte();
        }

        public override short ReadInt16()
        {
            return Endian == Endianness.Little ? base.ReadInt16() : BinaryPrimitives.ReverseEndianness(base.ReadInt16());
        }

        public override ushort ReadUInt16()
        {
            return Endian == Endianness.Little ? base.ReadUInt16() : BinaryPrimitives.ReverseEndianness(base.ReadUInt16());
        }

        public override int ReadInt32()
        {
            return Endian == Endianness.Little ? base.ReadInt32() : BinaryPrimitives.ReverseEndianness(base.ReadInt32());
        }

        public override uint ReadUInt32()
        {
            return Endian == Endianness.Little ? base.ReadUInt32() : BinaryPrimitives.ReverseEndianness(base.ReadUInt32());
        }

        public override long ReadInt64()
        {
            return Endian == Endianness.Little ? base.ReadInt16() : BinaryPrimitives.ReverseEndianness(base.ReadInt64());
        }

        public override ulong ReadUInt64()
        {
            return Endian == Endianness.Little ? base.ReadUInt16() : BinaryPrimitives.ReverseEndianness(base.ReadUInt64());
        }

        public override float ReadSingle() => BitConverter.Int32BitsToSingle(ReadInt32());

        public override double ReadDouble() => BitConverter.Int64BitsToDouble(ReadInt64());

        public Vector2 ReadVec2() => new Vector2(ReadSingle(), ReadSingle());
        public Vector3 ReadVec3() => new Vector3(ReadSingle(), ReadSingle(), ReadSingle());
        public Vector4 ReadVec4() => new Vector4(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());

        public Vector2 ReadVec2(VectorBinaryFormat fmt)
        {
            switch (fmt)
            {
                case VectorBinaryFormat.SInt8Normalized:
                    return new Vector2((float)ReadSByte() / 127.0f, (float)ReadSByte() / 127.0f);
                case VectorBinaryFormat.SInt8:
                    return new Vector2((float)ReadSByte(), (float)ReadSByte());
                case VectorBinaryFormat.UInt8Normalized:
                    return new Vector2((float)ReadByte() / 255.0f, (float)ReadByte() / 255.0f);
                case VectorBinaryFormat.UInt8:
                    return new Vector2((float)ReadByte(), (float)ReadByte());
                case VectorBinaryFormat.SInt16Normalized:
                    return new Vector2((float)ReadInt16() / 32767.0f, (float)ReadInt16() / 32767.0f);
                case VectorBinaryFormat.SInt16:
                    return new Vector2((float)ReadInt16(), (float)ReadInt16());
                case VectorBinaryFormat.UInt16Normalized:
                    return new Vector2((float)ReadUInt16() / 65535.0f, (float)ReadUInt16() / 65535.0f);
                case VectorBinaryFormat.UInt16:
                    return new Vector2((float)ReadUInt16(), (float)ReadUInt16());
                case VectorBinaryFormat.Single:
                    return new Vector2(ReadSingle(), ReadSingle());
                default:
                    return new Vector2(ReadSingle(), ReadSingle());
            }
        }

        public Vector3 ReadVec3(VectorBinaryFormat fmt)
        {
            switch (fmt)
            {
                case VectorBinaryFormat.SInt8Normalized:
                    return new Vector3((float)ReadSByte() / 127.0f, (float)ReadSByte() / 127.0f, (float)ReadSByte() / 127.0f);
                case VectorBinaryFormat.SInt8:
                    return new Vector3((float)ReadSByte(), (float)ReadSByte(), (float)ReadSByte());
                case VectorBinaryFormat.UInt8Normalized:
                    return new Vector3((float)ReadByte() / 255.0f, (float)ReadByte() / 255.0f, (float)ReadByte() / 255.0f);
                case VectorBinaryFormat.UInt8:
                    return new Vector3((float)ReadByte(), (float)ReadByte(), (float)ReadByte());
                case VectorBinaryFormat.SInt16Normalized:
                    return new Vector3((float)ReadInt16() / 32767.0f, (float)ReadInt16() / 32767.0f, (float)ReadInt16() / 32767.0f);
                case VectorBinaryFormat.SInt16:
                    return new Vector3((float)ReadInt16(), (float)ReadInt16(), (float)ReadInt16());
                case VectorBinaryFormat.UInt16Normalized:
                    return new Vector3((float)ReadUInt16() / 65535.0f, (float)ReadUInt16() / 65535.0f, (float)ReadUInt16() / 65535.0f);
                case VectorBinaryFormat.UInt16:
                    return new Vector3((float)ReadUInt16(), (float)ReadUInt16(), (float)ReadUInt16());
                case VectorBinaryFormat.Single:
                    return new Vector3(ReadSingle(), ReadSingle(), ReadSingle());
                default:
                    return new Vector3(ReadSingle(), ReadSingle(), ReadSingle());
            }
        }

        public Vector4 ReadVec4(VectorBinaryFormat fmt)
        {
            switch (fmt)
            {
                case VectorBinaryFormat.SInt8Normalized:
                    return new Vector4((float)ReadSByte() / 127.0f, (float)ReadSByte() / 127.0f, (float)ReadSByte() / 127.0f, (float)ReadSByte() / 127.0f);
                case VectorBinaryFormat.SInt8:
                    return new Vector4((float)ReadSByte(), (float)ReadSByte(), (float)ReadSByte(), (float)ReadSByte());
                case VectorBinaryFormat.UInt8Normalized:
                    return new Vector4((float)ReadByte() / 255.0f, (float)ReadByte() / 255.0f, (float)ReadByte() / 255.0f, (float)ReadByte() / 255.0f);
                case VectorBinaryFormat.UInt8:
                    return new Vector4((float)ReadByte(), (float)ReadByte(), (float)ReadByte(), (float)ReadByte());
                case VectorBinaryFormat.SInt16Normalized:
                    return new Vector4((float)ReadInt16() / 32767.0f, (float)ReadInt16() / 32767.0f, (float)ReadInt16() / 32767.0f, (float)ReadInt16() / 32767.0f);
                case VectorBinaryFormat.SInt16:
                    return new Vector4((float)ReadInt16(), (float)ReadInt16(), (float)ReadInt16(), (float)ReadInt16());
                case VectorBinaryFormat.UInt16Normalized:
                    return new Vector4((float)ReadUInt16() / 65535.0f, (float)ReadUInt16() / 65535.0f, (float)ReadUInt16() / 65535.0f, (float)ReadUInt16() / 65535.0f);
                case VectorBinaryFormat.UInt16:
                    return new Vector4((float)ReadUInt16(), (float)ReadUInt16(), (float)ReadUInt16(), (float)ReadUInt16());
                case VectorBinaryFormat.Single:
                    return new Vector4(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
                default:
                    return new Vector4(ReadSingle(), ReadSingle(), ReadSingle(), ReadSingle());
            }
        }

        public long Seek(long position, SeekOrigin origin)
        {
            return this.BaseStream.Seek(position, origin);
        }

        public EndianBinaryReader(Stream input) : base(input)
        {
            return;
        }

        public EndianBinaryReader(Stream input, Endianness endian) : base(input)
        {
            Endian = endian;
            return;
        }

        public EndianBinaryReader(Stream input, Encoding encoding) : base(input, encoding)
        {
            return;
        }

        public EndianBinaryReader(Stream input, Encoding encoding, Endianness endian) : base(input, encoding)
        {
            Endian = endian;
            return;
        }

        public EndianBinaryReader(Stream input, Encoding encoding, bool leaveOpen) : base(input, encoding, leaveOpen)
        {
            return;
        }

        public EndianBinaryReader(Stream input, Encoding encoding, bool leaveOpen, Endianness endian) : base(input, encoding, leaveOpen)
        {
            Endian = endian;
            return;
        }
    }
}
