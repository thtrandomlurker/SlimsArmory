using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaCLib.IO
{
    public class EndianBinaryWriter : BinaryWriter
    {
        public Endianness Endian;

        public override void Write(sbyte value)
        {
            base.Write(value);
        }

        public override void Write(byte value)
        {
            base.Write(value);
        }

        public override void Write(short value)
        {
            switch (Endian)
            {
                case Endianness.Little:
                    base.Write(value);
                    break;
                case Endianness.Big:
                    base.Write(BinaryPrimitives.ReverseEndianness(value));
                    break;
            }
        }

        public override void Write(ushort value)
        {
            switch (Endian)
            {
                case Endianness.Little:
                    base.Write(value);
                    break;
                case Endianness.Big:
                    base.Write(BinaryPrimitives.ReverseEndianness(value));
                    break;
            }
        }

        public override void Write(int value)
        {
            switch (Endian)
            {
                case Endianness.Little:
                    base.Write(value);
                    break;
                case Endianness.Big:
                    base.Write(BinaryPrimitives.ReverseEndianness(value));
                    break;
            }
        }

        public override void Write(uint value)
        {
            switch (Endian)
            {
                case Endianness.Little:
                    base.Write(value);
                    break;
                case Endianness.Big:
                    base.Write(BinaryPrimitives.ReverseEndianness(value));
                    break;
            }
        }

        public override void Write(long value)
        {
            switch (Endian)
            {
                case Endianness.Little:
                    base.Write(value);
                    break;
                case Endianness.Big:
                    base.Write(BinaryPrimitives.ReverseEndianness(value));
                    break;
            }
        }

        public override void Write(ulong value)
        {
            switch (Endian)
            {
                case Endianness.Little:
                    base.Write(value);
                    break;
                case Endianness.Big:
                    base.Write(BinaryPrimitives.ReverseEndianness(value));
                    break;
            }
        }

        public override void Write(float value)
        {
            switch (Endian)
            {
                case Endianness.Little:
                    base.Write(value);
                    break;
                case Endianness.Big:
                    base.Write(BinaryPrimitives.ReverseEndianness(BitConverter.SingleToInt32Bits(value)));
                    break;
            }
        }

        public override void Write(double value)
        {
            switch (Endian)
            {
                case Endianness.Little:
                    base.Write(value);
                    break;
                case Endianness.Big:
                    base.Write(BinaryPrimitives.ReverseEndianness(BitConverter.DoubleToInt64Bits(value)));
                    break;
            }
        }

        public long Seek(long offset, SeekOrigin origin)
        {
            return this.BaseStream.Seek(offset, origin);
        }

        public long Tell()
        {
            return this.BaseStream.Position;
        }

        public void WriteNulls(long count)
        {
            byte[] buf = new byte[count];
            Write(buf);
        }

        public void Align(int alignment)
        {
            if (Tell() % alignment != 0)
            {
                WriteNulls(alignment - (Tell() % alignment));
            }
        }

        public EndianBinaryWriter(Stream output) : base(output)
        {
            return;
        }

        public EndianBinaryWriter(Stream output, Endianness endian) : base(output)
        {
            Endian = endian;
            return;
        }

        public EndianBinaryWriter(Stream output, Encoding encoding) : base(output, encoding)
        {
            return;
        }

        public EndianBinaryWriter(Stream output, Encoding encoding, Endianness endian) : base(output, encoding)
        {
            Endian = endian;
            return;
        }

        public EndianBinaryWriter(Stream output, Encoding encoding, bool leaveOpen) : base(output, encoding, leaveOpen)
        {
            return;
        }

        public EndianBinaryWriter(Stream output, Encoding encoding, bool leaveOpen, Endianness endian) : base(output, encoding, leaveOpen)
        {
            Endian = endian;
            return;
        }
    }
}
